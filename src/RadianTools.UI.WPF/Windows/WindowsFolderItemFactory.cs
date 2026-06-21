using RadianTools.Interop.Windows;
using RadianTools.UI.WPF.Common;
using System.IO;

namespace RadianTools.UI.WPF;

/// <summary>
/// Windows用フォルダアイテムファクトリクラス。
/// </summary>
public class WindowsFolderItemFactory : IFolderItemFactory
{
    /// <summary>ルート階層の表示モード（デフォルトは仮想フォルダ表示）。</summary>
    public FolderRootMode RootMode { get; set; } = FolderRootMode.DesktopVirtualFolders;

    // ツリーに表示させない不要なシステムフォルダのIDリスト
    private static readonly Guid[] _IgnoreItems = {
        FOLDERID.UsersLibraries,
        FOLDERID.UsersFiles,
        FOLDERID.NetworkFolder,
    };

    /// <summary>
    /// 現在の <see cref="RootMode"/> に応じて、ルートとなるフォルダアイテムを取得します。
    /// </summary>
    public IEnumerable<IFolderItem> GetRootItems()
    {
        return RootMode switch
        {
            FolderRootMode.DesktopVirtualFolders => GetDesktopChildren(),
            FolderRootMode.LogicalDrives => GetLogicalDrives(),
            _ => new List<IFolderItem>()
        };
    }

    /// <summary>
    /// 遅延読み込み用のダミーアイテムを生成します。
    /// </summary>
    public IFolderItem GetDummyItem()
        => new WindowsFolderItem();

    /// <summary>
    /// デスクトップ配下の仮想フォルダ（PCなど）を取得します。
    /// </summary>
    private IEnumerable<IFolderItem> GetDesktopChildren()
    {
        var pidls = KnownFolderPIDL.Desktop.Value.EnumFolders();
        // 除外リストに含まれない既知のフォルダのみを抽出
        foreach (var child in pidls.Where(x => x.IsKnownFolder && !_IgnoreItems.Contains(x.KnownFolderId)))
        {
            yield return new WindowsFolderItem(null, child);
        }
    }

    /// <summary>
    /// 固定ドライブおよびリムーバブルドライブのリストを取得します。
    /// </summary>
    private IEnumerable<IFolderItem> GetLogicalDrives()
    {
        return DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable)
            .Select(d => SafePIDL.FromFilePath(d.Name))
            .Select(pidl => new WindowsFolderItem(null, pidl));
    }
}