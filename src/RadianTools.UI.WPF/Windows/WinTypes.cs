using RadianTools.Generators.UnmanagedStructGenerator;
using System.Runtime.InteropServices;

namespace RadianTools.UI.WPF;

internal enum KF_CATEGORY
{
    KF_CATEGORY_VIRTUAL = 1,
    KF_CATEGORY_FIXED = 2,
    KF_CATEGORY_COMMON = 3,
    KF_CATEGORY_PERUSER = 4,
}

internal enum FFFP_MODE
{
    FFFP_EXACTMATCH = 0,
    FFFP_NEARESTPARENTMATCH = 1,
}

internal enum GPFIDL_FLAGS : uint
{
    GPFIDL_DEFAULT = 0U,
    GPFIDL_ALTNAME = 1U,
    GPFIDL_UNCPRINTER = 2U,
}

internal enum SHIL : uint
{
    LARGE = 0,
    SMALL,
    EXTRALARGE,
    SYSSMALL,
    JUMBO,
}

internal enum STRRET_TYPE
{
    STRRET_WSTR = 0,
    STRRET_OFFSET = 1,
    STRRET_CSTR = 2,
}

[Flags]
internal enum SHGDNF : uint
{
    SHGDN_NORMAL = 0U,
    SHGDN_INFOLDER = 1U,
    SHGDN_FOREDITING = 4096U,
    SHGDN_FORADDRESSBAR = 16384U,
    SHGDN_FORPARSING = 32768U,
}

[Flags]
internal enum _SHCONTF
{
    SHCONTF_CHECKING_FOR_CHILDREN = 16,
    SHCONTF_FOLDERS = 32,
    SHCONTF_NONFOLDERS = 64,
    SHCONTF_INCLUDEHIDDEN = 128,
    SHCONTF_INIT_ON_FIRST_NEXT = 256,
    SHCONTF_NETPRINTERSRCH = 512,
    SHCONTF_SHAREABLE = 1024,
    SHCONTF_STORAGE = 2048,
    SHCONTF_NAVIGATION_ENUM = 4096,
    SHCONTF_FASTITEMS = 8192,
    SHCONTF_FLATLIST = 16384,
    SHCONTF_ENABLE_ASYNC = 32768,
    SHCONTF_INCLUDESUPERHIDDEN = 65536,
}

[Flags]
internal enum SFGAO_FLAGS : uint
{
    SFGAO_CANCOPY = 0x00000001,
    SFGAO_CANMOVE = 0x00000002,
    SFGAO_CANLINK = 0x00000004,
    SFGAO_STORAGE = 0x00000008,
    SFGAO_CANRENAME = 0x00000010,
    SFGAO_CANDELETE = 0x00000020,
    SFGAO_HASPROPSHEET = 0x00000040,
    SFGAO_DROPTARGET = 0x00000100,
    SFGAO_CAPABILITYMASK = 0x00000177,
    SFGAO_PLACEHOLDER = 0x00000800,
    SFGAO_SYSTEM = 0x00001000,
    SFGAO_ENCRYPTED = 0x00002000,
    SFGAO_ISSLOW = 0x00004000,
    SFGAO_GHOSTED = 0x00008000,
    SFGAO_LINK = 0x00010000,
    SFGAO_SHARE = 0x00020000,
    SFGAO_READONLY = 0x00040000,
    SFGAO_HIDDEN = 0x00080000,
    SFGAO_DISPLAYATTRMASK = 0x000FC000,
    SFGAO_FILESYSANCESTOR = 0x10000000,
    SFGAO_FOLDER = 0x20000000,
    SFGAO_FILESYSTEM = 0x40000000,
    SFGAO_HASSUBFOLDER = 0x80000000,
    SFGAO_CONTENTSMASK = 0x80000000,
    SFGAO_VALIDATE = 0x01000000,
    SFGAO_REMOVABLE = 0x02000000,
    SFGAO_COMPRESSED = 0x04000000,
    SFGAO_BROWSABLE = 0x08000000,
    SFGAO_NONENUMERATED = 0x00100000,
    SFGAO_NEWCONTENT = 0x00200000,
    SFGAO_CANMONIKER = 0x00400000,
    SFGAO_HASSTORAGE = 0x00400000,
    SFGAO_STREAM = 0x00400000,
    SFGAO_STORAGEANCESTOR = 0x00800000,
    SFGAO_STORAGECAPMASK = 0x70C50008,
    SFGAO_PKEYSFGAOMASK = 0x81044000,
}

[Flags]
internal enum FILE_FLAGS_AND_ATTRIBUTES : uint
{
    FILE_ATTRIBUTE_READONLY = 0x00000001,
    FILE_ATTRIBUTE_HIDDEN = 0x00000002,
    FILE_ATTRIBUTE_SYSTEM = 0x00000004,
    FILE_ATTRIBUTE_DIRECTORY = 0x00000010,
    FILE_ATTRIBUTE_ARCHIVE = 0x00000020,
    FILE_ATTRIBUTE_DEVICE = 0x00000040,
    FILE_ATTRIBUTE_NORMAL = 0x00000080,
    FILE_ATTRIBUTE_TEMPORARY = 0x00000100,
    FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,
    FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,
    FILE_ATTRIBUTE_COMPRESSED = 0x00000800,
    FILE_ATTRIBUTE_OFFLINE = 0x00001000,
    FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,
    FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,
    FILE_ATTRIBUTE_INTEGRITY_STREAM = 0x00008000,
    FILE_ATTRIBUTE_VIRTUAL = 0x00010000,
    FILE_ATTRIBUTE_NO_SCRUB_DATA = 0x00020000,
    FILE_ATTRIBUTE_EA = 0x00040000,
    FILE_ATTRIBUTE_PINNED = 0x00080000,
    FILE_ATTRIBUTE_UNPINNED = 0x00100000,
    FILE_ATTRIBUTE_RECALL_ON_OPEN = 0x00040000,
    FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS = 0x00400000,
    FILE_FLAG_WRITE_THROUGH = 0x80000000,
    FILE_FLAG_OVERLAPPED = 0x40000000,
    FILE_FLAG_NO_BUFFERING = 0x20000000,
    FILE_FLAG_RANDOM_ACCESS = 0x10000000,
    FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,
    FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,
    FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,
    FILE_FLAG_POSIX_SEMANTICS = 0x01000000,
    FILE_FLAG_SESSION_AWARE = 0x00800000,
    FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,
    FILE_FLAG_OPEN_NO_RECALL = 0x00100000,
    FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,
    PIPE_ACCESS_DUPLEX = 0x00000003,
    PIPE_ACCESS_INBOUND = 0x00000001,
    PIPE_ACCESS_OUTBOUND = 0x00000002,
    SECURITY_ANONYMOUS = 0x00000000,
    SECURITY_IDENTIFICATION = 0x00010000,
    SECURITY_IMPERSONATION = 0x00020000,
    SECURITY_DELEGATION = 0x00030000,
    SECURITY_CONTEXT_TRACKING = 0x00040000,
    SECURITY_EFFECTIVE_ONLY = 0x00080000,
    SECURITY_SQOS_PRESENT = 0x00100000,
    SECURITY_VALID_SQOS_FLAGS = 0x001F0000,
}

[Flags]
internal enum SHGFI_FLAGS : uint
{
    SHGFI_ADDOVERLAYS = 0x00000020,
    SHGFI_ATTR_SPECIFIED = 0x00020000,
    SHGFI_ATTRIBUTES = 0x00000800,
    SHGFI_DISPLAYNAME = 0x00000200,
    SHGFI_EXETYPE = 0x00002000,
    SHGFI_ICON = 0x00000100,
    SHGFI_ICONLOCATION = 0x00001000,
    SHGFI_LARGEICON = 0x00000000,
    SHGFI_LINKOVERLAY = 0x00008000,
    SHGFI_OPENICON = 0x00000002,
    SHGFI_OVERLAYINDEX = 0x00000040,
    SHGFI_PIDL = 0x00000008,
    SHGFI_SELECTED = 0x00010000,
    SHGFI_SHELLICONSIZE = 0x00000004,
    SHGFI_SMALLICON = 0x00000001,
    SHGFI_SYSICONINDEX = 0x00004000,
    SHGFI_TYPENAME = 0x00000400,
    SHGFI_USEFILEATTRIBUTES = 0x00000010,
}

[StructLayout(LayoutKind.Sequential)]
internal struct HRESULT
{
    private int _value;
    int Value => _value;

    internal bool Succeeded => this.Value >= 0;
    internal bool Failed => this.Value < 0;
    internal bool IsOK => this.Value == 0;
    internal bool IsNotOK => this.Value != 0;

    internal HRESULT ThrowOnFailure(IntPtr errorInfo = default)
    {
        Marshal.ThrowExceptionForHR(this.Value, errorInfo);
        return this;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct BOOL : IEquatable<BOOL>
{
    int _intValue;
    internal BOOL(bool value) => _intValue = value ? 1 : 0;
    internal BOOL(int value) => _intValue = value > 0 ? 1 : 0;

    public bool Equals(BOOL other) => _intValue == other._intValue;

    public static implicit operator bool(BOOL value) => value._intValue > 0;
}

[StructLayout(LayoutKind.Sequential)]
internal struct COLORREF
{
    internal uint Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    internal int left;
    internal int top;
    internal int right;
    internal int bottom;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    internal int x;
    internal int y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct IMAGEINFO
{
    internal HBITMAP hbmImage;
    internal HBITMAP hbmMask;
    internal int Unused1;
    internal int Unused2;
    internal RECT rcImage;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct SHFILEINFOW
{
    internal HICON hIcon;
    internal int iIcon;
    internal uint dwAttributes;
    internal CHARS_MAX_PATH szDisplayName;
    internal CHARS_80 szTypeName;
}

[StructLayout(LayoutKind.Sequential)]
internal struct KNOWNFOLDER_DEFINITION
{
    internal KF_CATEGORY category;
    internal IntPtr pszName;
    internal IntPtr pszDescription;
    internal Guid fidParent;
    internal IntPtr pszRelativePath;
    internal IntPtr pszParsingName;
    internal IntPtr pszTooltip;
    internal IntPtr pszLocalizedName;
    internal IntPtr pszIcon;
    internal IntPtr pszSecurity;
    internal uint dwAttributes;
    internal uint kfdFlags;
    internal Guid ftidType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SHITEMID
{
    internal ushort cb;
    internal byte abID;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct ITEMIDLIST
{
    internal SHITEMID mkid;
}

[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
internal struct STRRET
{
    [FieldOffset(0)] internal uint uType;
    [FieldOffset(4)] internal IntPtr pOleStr;
    [FieldOffset(4)] internal uint uOffset;
    [FieldOffset(4)] internal CHARS_MAX_PATH cStr;
}

[StructLayout(LayoutKind.Sequential)]
internal struct IMAGELISTDRAWPARAMS
{
    internal uint cbSize;
    internal HIMAGELIST himl;
    internal int i;
    internal HDC hdcDst;
    internal int x;
    internal int y;
    internal int cx;
    internal int cy;
    internal int xBitmap;
    internal int yBitmap;
    internal COLORREF rgbBk;
    internal COLORREF rgbFg;
    internal uint fStyle;
    internal uint dwRop;
    internal uint fState;
    internal uint Frame;
    internal COLORREF crEffect;
}

internal static class FileSystem
{
    internal const int MAX_PATH = 260;
}

internal static class FOLDERID
{
    internal static readonly Guid Computer = new Guid(0x0AC0837C, 0xBBF8, 0x452A, 0x85, 0x0D, 0x79, 0xD0, 0x8E, 0x66, 0x7C, 0xA7);
    internal static readonly Guid Desktop = new Guid(0xB4BFCC3A, 0xDB2C, 0x424C, 0xB0, 0x29, 0x7F, 0xE9, 0x9A, 0x87, 0xC6, 0x41);
    internal static readonly Guid NetworkFolder = new Guid(0xD20BEEC4, 0x5CA8, 0x4905, 0xAE, 0x3B, 0xBF, 0x25, 0x1E, 0xA0, 0x9B, 0x53);
    internal static readonly Guid UsersLibraries = new Guid(0xA302545D, 0xDEFF, 0x464B, 0xAB, 0xE8, 0x61, 0xC8, 0x64, 0x8D, 0x93, 0x9B);
    internal static readonly Guid UsersFiles = new Guid(0xF3CE0F7C, 0x4901, 0x4ACC, 0x86, 0x48, 0xD5, 0xD4, 0x4B, 0x04, 0xEF, 0x8F);
}

internal class KnownFolderPIDL
{
    internal static readonly Lazy<SafePIDL> Desktop = new(() => GetKnownFolderPIDL(FOLDERID.Desktop));
    internal static readonly Lazy<SafePIDL> NetworkFolder = new(() => GetKnownFolderPIDL(FOLDERID.NetworkFolder));
    internal static readonly Lazy<SafePIDL> UsersLibraries = new(() => GetKnownFolderPIDL(FOLDERID.UsersLibraries));
    internal static readonly Lazy<SafePIDL> UsersFiles = new(GetKnownFolderPIDL(FOLDERID.UsersFiles));

    internal static SafePIDL GetKnownFolderPIDL(in Guid rfid)
    {
        var hr = Shell32.SHGetKnownFolderIDList(in rfid, 0, HANDLE.Null, out var pidl);
        return hr.IsOK ? new SafePIDL(pidl, false) : SafePIDL.Null;
    }
}

internal static class Shell32
{
    [DllImport("SHELL32.dll")]
    internal static extern uint ILGetSize(PIDL pidl);

    [DllImport("SHELL32.dll")]
    internal static extern BOOL ILIsEqual(PIDL pidl1, PIDL pidl2);

    [DllImport("SHELL32.dll")]
    internal static extern PIDL ILCombine(PIDL pidl1, PIDL pidl2);

    [DllImport("SHELL32.dll", CharSet = CharSet.Unicode)]
    internal static extern PIDL ILCreateFromPath(string pszPath);

    [DllImport("SHELL32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SHGetFileInfo(string pszPath, FILE_FLAGS_AND_ATTRIBUTES dwFileAttributes, ref SHFILEINFOW psfi, uint cbFileInfo, SHGFI_FLAGS uFlags);

    [DllImport("SHELL32.dll", CharSet = CharSet.Unicode)]
    internal static extern IntPtr SHGetFileInfo(PIDL pidl, FILE_FLAGS_AND_ATTRIBUTES dwFileAttributes, ref SHFILEINFOW psfi, uint cbFileInfo, SHGFI_FLAGS uFlags);

    [DllImport("SHELL32.dll")]
    internal static extern HRESULT SHBindToObject(IntPtr psf, PIDL pidl, IntPtr pbc, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

    [DllImport("SHELL32.dll")]
    internal static extern HRESULT SHGetImageList(int iImageList, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppvObj);

    [DllImport("SHELL32.dll", CharSet = CharSet.Unicode, EntryPoint = "SHGetPathFromIDListEx")]
    internal static extern BOOL SHGetPathFromIDListEx(PIDL pidl, out CHARS_MAX_PATH pszPath, uint cchPath, GPFIDL_FLAGS uOpts);

    [DllImport("SHELL32.dll")]
    internal static extern HRESULT SHGetKnownFolderIDList(in Guid rfid, uint dwFlags, HANDLE hToken, out PIDL ppidl);

}

[Guid("4DF0C730-DF9D-4AE3-9153-AA6B82E9795A"), ComImport()]
internal partial class KnownFolderManager
{
}

[Guid("8BE2D872-86AA-4D47-B776-32CCA40C7018"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComImport()]
internal interface IKnownFolderManager
{
    void FolderIdFromCsidl(int nCsidl, out Guid pfid);
    void FolderIdToCsidl(in Guid rfid, out int pnCsidl);
    [PreserveSig()] HRESULT GetFolderIds(out IntPtr ppKFId, out uint pCount);
    void GetFolder(in Guid rfid, out IKnownFolder ppkf);
    void GetFolderByName([MarshalAs(UnmanagedType.LPWStr)] string pszCanonicalName, out IKnownFolder ppkf);
    void RegisterFolder(in Guid rfid, in KNOWNFOLDER_DEFINITION pKFD);
    void UnregisterFolder(in Guid rfid);
    void FindFolderFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath, FFFP_MODE mode, out IKnownFolder ppkf);
    [PreserveSig()] HRESULT FindFolderFromIDList(PIDL pidl, out IKnownFolder ppkf);
    void Redirect(in Guid rfid, HWND hwnd, uint flags, [MarshalAs(UnmanagedType.LPWStr)] string pszTargetPath, uint cFolders, [Optional] IntPtr pExclusion, [Optional] IntPtr ppszError);
}

internal static class IKnownFolderManagerExtentions
{
    internal static Guid[] GetFolderIdsSafe(this IKnownFolderManager mgr)
    {
        if( mgr.GetFolderIds(out var pGuids, out var count).IsNotOK )
            return Array.Empty<Guid>();

        using var coArray = new CoTaskMemArray<Guid>(pGuids, (int)count);
        return coArray.ToArray();
    }
}

[Guid("000214F2-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComImport()]
internal interface IEnumIDList
{
    [PreserveSig()] HRESULT Next(uint celt, out PIDL rgelt, [Optional] IntPtr pceltFetched);
    [PreserveSig()] HRESULT Skip(uint celt);
    [PreserveSig()] HRESULT Reset();
    [PreserveSig()] HRESULT Clone(out IEnumIDList ppenum);
}

[Guid("3AA7AF7E-9B36-420C-A8E3-F77D4674A488"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComImport()]
internal interface IKnownFolder
{
    void GetId(out Guid pkfid);
    void GetCategory(out KF_CATEGORY pCategory);
    void GetShellItem(uint dwFlags, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    void GetPath(uint dwFlags, out CHARS_MAX_PATH ppszPath);
    void SetPath(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszPath);
    void GetIDList(uint dwFlags, out PIDL ppidl);
    void GetFolderType(out Guid pftid);
    void GetRedirectionCapabilities(out uint pCapabilities);
    void GetFolderDefinition(out KNOWNFOLDER_DEFINITION pKFD);
}

[Guid("0000000E-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComImport()]
internal interface IBindCtx
{
    void RegisterObjectBound([MarshalAs(UnmanagedType.IUnknown)] object punk);
    void RevokeObjectBound([MarshalAs(UnmanagedType.IUnknown)] object punk);
    void ReleaseBoundObjects();
    void SetBindOptions(IntPtr pbindopts);
    void GetBindOptions(IntPtr pbindopts);
    void GetRunningObjectTable([MarshalAs(UnmanagedType.IUnknown)] out object pprot);
    void RegisterObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey, [MarshalAs(UnmanagedType.IUnknown)] object punk);
    void GetObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
    void EnumObjectParam(out IEnumString ppenum);
    void RevokeObjectParam([MarshalAs(UnmanagedType.LPWStr)] string pszKey);
}

[Guid("00000101-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComImport()]
internal interface IEnumString
{
    [PreserveSig()] HRESULT Next(uint celt, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 0), Out] string[] rgelt, [Optional] out uint pceltFetched);
    [PreserveSig()] HRESULT Skip(uint celt);
    void Reset();
    void Clone(out IEnumString ppenum);
}

[Guid("000214E6-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComImport()]
internal interface IShellFolder
{
    void ParseDisplayName(HWND hwnd, IBindCtx pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, [Optional] out uint pchEaten, out PIDL ppidl, ref uint pdwAttributes);
    [PreserveSig()] HRESULT EnumObjects(HWND hwnd, uint grfFlags, out IEnumIDList ppenumIDList);
    void BindToObject(PIDL pidl, IBindCtx pbc, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    void BindToStorage(PIDL pidl, IBindCtx pbc, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    [PreserveSig()] HRESULT CompareIDs(IntPtr lParam, PIDL pidl1, PIDL pidl2);
    void CreateViewObject(HWND hwndOwner, in Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    void GetAttributesOf(uint cidl, in PIDL apidl, ref uint rgfInOut);
    void GetUIObjectOf(HWND hwndOwner, uint cidl, out PIDL apidl, in Guid riid, [Optional] out uint rgfReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    void GetDisplayNameOf(PIDL pidl, SHGDNF uFlags, out STRRET pName);
    void SetNameOf(HWND hwnd, PIDL pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, SHGDNF uFlags, [Optional] out PIDL ppidlOut);
}

[ComImport]
[Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IImageList
{
    [PreserveSig] HRESULT Add(HBITMAP hbmImage, HBITMAP hbmMask, out int pi);
    [PreserveSig] HRESULT ReplaceIcon(int i, HICON hicon, out int pi);
    [PreserveSig] HRESULT SetOverlayImage(int iImage, int iOverlay);
    [PreserveSig] HRESULT Replace(int i, HBITMAP hbmImage, HBITMAP hbmMask);
    [PreserveSig] HRESULT AddMasked(HBITMAP hbmImage, COLORREF crMask, out int pi);
    [PreserveSig] HRESULT Draw(ref IMAGELISTDRAWPARAMS pimldp);
    [PreserveSig] HRESULT Remove(int i);
    [PreserveSig] HRESULT GetIcon(int i, uint flags, out HICON picon);
    [PreserveSig] HRESULT GetImageInfo(int i, out IMAGEINFO pImageInfo);
    [PreserveSig] HRESULT Copy(int iDst, [MarshalAs(UnmanagedType.IUnknown)] object punkSrc, int iSrc, uint uFlags);
    [PreserveSig] HRESULT Merge(int i1, [MarshalAs(UnmanagedType.IUnknown)] object punk2, int i2, int dx, int dy, ref Guid riid, out nint ppv);
    [PreserveSig] HRESULT Clone(ref Guid riid, out nint ppv);
    [PreserveSig] HRESULT GetImageRect(int i, out RECT prc);
    [PreserveSig] HRESULT GetIconSize(out int cx, out int cy);
    [PreserveSig] HRESULT SetIconSize(int cx, int cy);
    [PreserveSig] HRESULT GetImageCount(out int pi);
    [PreserveSig] HRESULT SetImageCount(uint uNewCount);
    [PreserveSig] HRESULT SetBkColor(COLORREF clrBk, out COLORREF pclr);
    [PreserveSig] HRESULT GetBkColor(out COLORREF pclr);
    [PreserveSig] HRESULT BeginDrag(int iTrack, int dxHotspot, int dyHotspot);
    [PreserveSig] HRESULT EndDrag();
    [PreserveSig] HRESULT DragEnter(HWND hwndLock, int x, int y);
    [PreserveSig] HRESULT DragLeave(HWND hwndLock);
    [PreserveSig] HRESULT DragMove(int x, int y);
    [PreserveSig] HRESULT SetDragCursorImage([MarshalAs(UnmanagedType.IUnknown)] object punk, int iDrag, int dxHotspot, int dyHotspot);
    [PreserveSig] HRESULT DragShowNolock(BOOL fShow);
    [PreserveSig] HRESULT GetDragImage(out POINT ppt, out POINT pptHotspot, ref Guid riid, out nint ppv);
    [PreserveSig] HRESULT GetItemFlags(int i, out uint dwFlags);
    [PreserveSig] HRESULT GetOverlayImage(int iOverlay, out int piIndex);
}

#region UnmanagedStructGenerator

[FixedChars(FileSystem.MAX_PATH)]
internal partial struct CHARS_MAX_PATH
{
}

[FixedChars(80)]
internal partial struct CHARS_80
{
}

[NativeHandle]
internal partial struct HBITMAP
{
}

[NativeHandle]
internal partial struct HWND
{
}

[NativeHandle]
internal partial struct HANDLE
{
}

[NativeHandle]
internal partial struct HDC
{
}

[NativeHandle]
internal partial struct HIMAGELIST
{
}

[NativeHandle]
internal partial struct HICON : IDisposable
{
    [DllImport("USER32.dll")]
    private static extern BOOL DestroyIcon(IntPtr hIcon);

    public void Dispose()
    {
        var value = Interlocked.Exchange(ref this._value, IntPtr.Zero);
        if (value != IntPtr.Zero)
            DestroyIcon(value);
    }
}

[NativeHandle(typeof(ITEMIDLIST*))]
internal partial struct PIDL
{
}

#endregion