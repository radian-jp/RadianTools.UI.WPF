using System.IO;
using System.Management;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RadianTools.UI.WPF.Storage;

/// <summary>
/// 接続方式(MSFT_PhysicalDisk の仕様に準拠)
/// </summary>
public enum StorageBusType : ushort
{
    Unknown = 0,
    SCSI = 1,
    ATAPI = 2,
    ATA = 3,
    IEEE1394 = 4,
    SSA = 5,
    FibreChannel = 6,
    USB = 7,
    RAID = 8,
    iSCSI = 9,
    SAS = 14,
    SATA = 11,
    SD = 12,
    MMC = 13,
    FileBackedVirtual = 15,
    StorageSpaces = 16,
    NVMe = 17,
    SCM = 18,
    UFS = 19
}

/// <summary>
/// ストレージ用メディアタイプ(MSFT_PhysicalDisk の仕様に準拠)
/// </summary>
public enum StorageMediaType : int
{
    NotSet = -1,
    Unspecified = 0,
    HDD = 3,
    SSD = 4,
    SCM = 5
}

/// <summary>
/// 光学ドライブ用メディアタイプ
/// </summary>
public enum OpticalMediaType : int
{
    NotSet = -1,
    Unspecified = 0,
    CD = 1,
    DVD = 2,
    BD = 3
}

/// <summary>
/// 解析された物理ディスクの構造体情報を保持するクラスです。
/// </summary>
public class DiskInfo
{
    /// <summary>JSONシリアライズ用のオプション</summary>
    private static readonly JsonSerializerOptions _serializerOption;

    /// <summary>
    /// <see cref="DiskInfo"/> クラスの静的初期化子。JSONの出力設定を行います。
    /// </summary>
    static DiskInfo()
    {
        _serializerOption = new JsonSerializerOptions() { WriteIndented = true };
        _serializerOption.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>OSが割り当てているディスクのインデックス番号を取得または設定します。</summary>
    public uint DiskNumber { get; set; }

    /// <summary>ディスクのバス接続方式を取得または設定します。</summary>
    public StorageBusType BusType { get; set; } = StorageBusType.Unknown;

    /// <summary>通常のストレージ（HDD/SSDなど）のメディア種別を取得または設定します。</summary>
    public StorageMediaType StorageMediaType { get; set; } = StorageMediaType.NotSet;

    /// <summary>光学ドライブのメディア種別を取得または設定します。</summary>
    public OpticalMediaType OpticalMediaType { get; set; } = OpticalMediaType.NotSet;

    /// <summary>対象のパスに対応するドライブレター（A-Z）を取得または設定します。</summary>
    public char DriveLetter { get; set; }

    /// <summary>OSから取得したデバイスのモデル名（型番）を取得または設定します。</summary>
    public string DeviceName { get; set; } = "";

    /// <summary>ディスクの総容量（バイト単位）を取得または設定します。</summary>
    public ulong Size { get; set; }

    /// <summary>このディスクが通常のストレージ（HDD/SSD等）として判定されているかを取得します。</summary>
    public bool IsStorage => StorageMediaType != StorageMediaType.NotSet;

    /// <summary>このディスクが光学ドライブとして判定されているかを取得します。</summary>
    public bool IsOptical => OpticalMediaType != OpticalMediaType.NotSet;

    /// <summary>
    /// メディアのシリアルナンバー
    /// </summary>
    public string SerialNumber { get; set; } = "";

    /// <summary>
    /// 現在のインスタンスの情報をインデントされたJSON文字列にシリアライズして返します。
    /// </summary>
    /// <returns>プロパティ情報を含むJSON文字列</returns>
    public override string ToString() => JsonSerializer.Serialize(this, _serializerOption);
}

/// <summary>
/// ディスク情報解析クラス
/// </summary>
public static class DiskAnalyzer
{
    /// <summary>
    /// 指定されたパス（フォルダまたはファイル）が所属する物理ディスクの詳細情報を取得します。
    /// </summary>
    /// <param name="path">調査対象のフォルダやファイルのパス（相対パス・絶対パス双方に対応）</param>
    /// <returns>解析されたディスク情報オブジェクト。情報が特定できなかった場合は <see langword="null"/> を返します。</returns>
    /// <exception cref="ArgumentException">パスが空であるか、有効なドライブレターが検出されなかった場合にスローされます。</exception>
    public static DiskInfo GetDiskInfoFromPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("path is empty.");

        // 相対パスを絶対パスに補完
        var absolutePath = Path.GetFullPath(path);
        var pathRoot = Path.GetPathRoot(absolutePath);
        if (string.IsNullOrEmpty(pathRoot))
            throw new ArgumentException($"DriveLetter not found. (path={path})");

        // ドライブレターの抽出とA-Zガード判定
        char driveLetter = char.ToUpper(pathRoot[0]);
        if (driveLetter < 'A' || driveLetter > 'Z')
            throw new ArgumentException($"DriveLetter not found. (path={path})");

        var targetDrive = driveLetter + ":";

        try
        {
            // ドライブレターから論理ディスク（Win32_LogicalDisk）を検索
            string logicalDriveQuery = $"SELECT __PATH FROM Win32_LogicalDisk WHERE Name = '{targetDrive}'";
            using (var logicalSearcher = new ManagementObjectSearcher(@"root\cimv2", logicalDriveQuery))
            {
                foreach (ManagementObject logicalDisk in logicalSearcher.Get())
                {
                    string logicalFullPath = logicalDisk.Path.Path;

                    // --- 通常ストレージ（HDD/SSD/USB）ルート ---
                    // 論理ディスク -> パーティション（Win32_DiskPartition）への関連付け
                    string partitionQuery = $"ASSOCIATORS OF {{{logicalFullPath}}} WHERE AssocClass = Win32_LogicalDiskToPartition";
                    using (var partitionSearcher = new ManagementObjectSearcher(@"root\cimv2", partitionQuery))
                    using (var partitions = partitionSearcher.Get())
                    {
                        if (partitions.Count > 0)
                        {
                            foreach (ManagementObject partition in partitions)
                            {
                                string partitionFullPath = partition.Path.Path;

                                // パーティション -> 物理ディスク（Win32_DiskDrive）への関連付け
                                string diskDriveQuery = $"ASSOCIATORS OF {{{partitionFullPath}}} WHERE AssocClass = Win32_DiskDriveToDiskPartition";

                                using (var diskSearcher = new ManagementObjectSearcher(@"root\cimv2", diskDriveQuery))
                                {
                                    foreach (ManagementObject disk in diskSearcher.Get())
                                    {
                                        var info = new DiskInfo
                                        {
                                            DiskNumber = (uint)(disk["Index"] ?? 0),
                                            DeviceName = disk["Model"]?.ToString() ?? "Unknown Device",
                                            DriveLetter = driveLetter,
                                            StorageMediaType = StorageMediaType.Unspecified,
                                            SerialNumber = disk["SerialNumber"]?.ToString()?.Trim() ?? ""
                                        };

                                        if (disk["Size"] != null) info.Size = (ulong)disk["Size"];

                                        // MSFT_PhysicalDisk からバス接続（SATA/NVMe等）とストレージ固有メディア（HDD/SSD）の情報を補完
                                        EnrichStorageDetails(info);
                                        return info;
                                    }
                                }
                            }
                        }
                    }

                    // --- 光学ドライブルート ---
                    // 通常ストレージルートでヒットしなかった場合、CDROMDriveクラスをドライブレターでダイレクト検索
                    string cdromQuery = $"SELECT Index, Model, Name, Size, InterfaceType, MediaType FROM Win32_CDROMDrive WHERE Drive = '{targetDrive}\\'";
                    using (var cdromSearcher = new ManagementObjectSearcher(@"root\cimv2", cdromQuery))
                    using (var cdroms = cdromSearcher.Get())
                    {
                        foreach (ManagementObject cdrom in cdroms)
                        {
                            var info = new DiskInfo
                            {
                                DiskNumber = (uint)(cdrom["Index"] ?? 0),
                                DeviceName = cdrom["Model"]?.ToString() ?? cdrom["Name"]?.ToString() ?? "Unknown Optical Drive",
                                DriveLetter = driveLetter,
                                Size = cdrom["Size"] != null ? (ulong)cdrom["Size"] : 0,

                                // 接続方式の解析
                                BusType = ParseOpticalBusType(cdrom["InterfaceType"]?.ToString()),
                                // 光学専用メディア（CD/DVD/BD）の解析
                                OpticalMediaType = ParseOpticalMediaType(cdrom["MediaType"]?.ToString())
                            };

                            return info;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DiskInfoService error]: {ex.Message}");
        }

        return new DiskInfo();
    }

    /// <summary>
    /// Storage名前空間（Root\Microsoft\Windows\Storage）の MSFT_PhysicalDisk クラスを参照し、
    /// バス接続方式（BusType）および物理メディアの判定値（MediaType）を補完します。
    /// </summary>
    /// <param name="info">補完対象の <see cref="DiskInfo"/> インスタンス</param>
    private static void EnrichStorageDetails(DiskInfo info)
    {
        try
        {
            string escapedSerial = info.SerialNumber.Replace("'", "''");
            string storageQuery = $"SELECT BusType, MediaType, SerialNumber FROM MSFT_PhysicalDisk WHERE SerialNumber = '{escapedSerial}'";
            using (var storageSearcher = new ManagementObjectSearcher(@"Root\Microsoft\Windows\Storage", storageQuery))
            {
                foreach (ManagementObject storage in storageSearcher.Get())
                {
                    if (storage["BusType"] != null)
                    {
                        ushort busTypeValue = (ushort)storage["BusType"];
                        info.BusType = Enum.IsDefined(typeof(StorageBusType), busTypeValue) ? (StorageBusType)busTypeValue : StorageBusType.Unknown;
                    }
                    if (storage["MediaType"] != null)
                    {
                        ushort mediaTypeValue = (ushort)storage["MediaType"];
                        info.StorageMediaType = Enum.IsDefined(typeof(StorageMediaType), mediaTypeValue) ? (StorageMediaType)mediaTypeValue : StorageMediaType.Unspecified;
                    }
                    break;
                }
            }
        }
        catch { /* 権限不足エラー等は安全に握りつぶして初期値のまま通す */ }
    }

    /// <summary>
    /// 光学ドライブオブジェクトから取得したインターフェース文字列を解析し、接続方式（BusType）を割り出します。
    /// </summary>
    /// <param name="interfaceType">WMIから取得したインターフェース型文字列</param>
    /// <returns>対応する <see cref="StorageBusType"/></returns>
    private static StorageBusType ParseOpticalBusType(string? interfaceType)
    {
        if (string.IsNullOrEmpty(interfaceType))
            return StorageBusType.Unknown;

        if (interfaceType.Contains("SCSI", StringComparison.OrdinalIgnoreCase)) return StorageBusType.SCSI;
        if (interfaceType.Contains("ATAPI", StringComparison.OrdinalIgnoreCase)) return StorageBusType.ATAPI;
        if (interfaceType.Contains("ATA", StringComparison.OrdinalIgnoreCase)) return StorageBusType.ATA;
        if (interfaceType.Contains("USB", StringComparison.OrdinalIgnoreCase)) return StorageBusType.USB;
        if (interfaceType.Contains("SATA", StringComparison.OrdinalIgnoreCase)) return StorageBusType.SATA;

        return StorageBusType.Unknown;
    }

    /// <summary>
    /// 光学ドライブオブジェクトから取得したメディアタイプ文字列を解析し、光学専用メディア種別を割り出します。
    /// </summary>
    /// <param name="mediaTypeStr">WMIから取得したメディアタイプ型文字列</param>
    /// <returns>対応する <see cref="OpticalMediaType"/></returns>
    private static OpticalMediaType ParseOpticalMediaType(string? mediaTypeStr)
    {
        if (string.IsNullOrEmpty(mediaTypeStr))
            return OpticalMediaType.Unspecified;

        if (mediaTypeStr.Contains("DVD", StringComparison.OrdinalIgnoreCase)) return OpticalMediaType.DVD;
        if (mediaTypeStr.Contains("CD", StringComparison.OrdinalIgnoreCase)) return OpticalMediaType.CD;
        if (mediaTypeStr.Contains("BLU", StringComparison.OrdinalIgnoreCase) || mediaTypeStr.Contains("BD", StringComparison.OrdinalIgnoreCase)) return OpticalMediaType.BD;

        return OpticalMediaType.Unspecified;
    }
}