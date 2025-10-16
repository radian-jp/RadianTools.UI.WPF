using RadianTools.UI.WPF.Windows;
using System.Runtime.InteropServices;

namespace RadianTools.UI.WPF;

internal class SafePIDL : IDisposable, IEquatable<SafePIDL>
{
    private nint _Value;
    private bool _mustNotFree;
    private Guid? _KnownFolderId;
    private int? _cachedHashCode;
    private static readonly STAThreadPool _staPool = new STAThreadPool(threadCount: 2);

    public string FilePath { get; }
    public string DisplayName { get; }
    public bool IsFolder { get; }
    public bool IsFileSystem { get; }
    public bool HasSubFolder { get; }
    public int IconIndex { get; }

    public Guid KnownFolderId
    {
        get
        {
            if (_KnownFolderId.HasValue)
                return _KnownFolderId.Value;

            _KnownFolderId = Guid.Empty;
            using var knownFolderManager = new SafeKnownFolderManager();
            if (knownFolderManager.FindFolderFromIDList(_Value, out var folder).IsNotOK)
                return _KnownFolderId.Value;

            try
            {
                folder.GetId(out var folderId);
                _KnownFolderId = folderId;
                return _KnownFolderId.Value;
            }
            finally
            {
                Marshal.ReleaseComObject(folder);
            }
        }
    }

    public bool IsKnownFolder
        => KnownFolderId != Guid.Empty;

    public SafePIDL(PIDL pidl, bool mustNotFree)
    {
        _Value = pidl;
        _mustNotFree = mustNotFree;
        FilePath = GetFilePath();
        IsFileSystem = !string.IsNullOrEmpty(FilePath);
        DisplayName = string.Empty;

        if (IsNull)
        {
            return;
        }

        var fileInfo = GetFileInfo(
            SFGAO_FLAGS.SFGAO_FOLDER | SFGAO_FLAGS.SFGAO_HASSUBFOLDER,
            SHGFI_FLAGS.SHGFI_DISPLAYNAME |
            SHGFI_FLAGS.SHGFI_SMALLICON |
            SHGFI_FLAGS.SHGFI_SYSICONINDEX |
            SHGFI_FLAGS.SHGFI_ATTR_SPECIFIED |
            SHGFI_FLAGS.SHGFI_ATTRIBUTES |
            SHGFI_FLAGS.SHGFI_PIDL
            );
        if (!fileInfo.HasValue)
        {
            return;
        }

        DisplayName = fileInfo.Value.szDisplayName.ToString();
        IconIndex = fileInfo.Value.iIcon;
        IsFolder = (fileInfo.Value.dwAttributes & (uint)SFGAO_FLAGS.SFGAO_FOLDER) != 0;
        HasSubFolder = (fileInfo.Value.dwAttributes & (uint)SFGAO_FLAGS.SFGAO_HASSUBFOLDER) != 0;
    }

    public SafePIDL(IntPtr pidl) : this(pidl, true) { }

    protected virtual void Dispose(bool disposing)
    {
        if (_mustNotFree)
            return;

        var p = Interlocked.Exchange(ref _Value, nint.Zero);
        if (p != nint.Zero)
            Marshal.FreeCoTaskMem(p);
    }

    ~SafePIDL()
        => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public PIDL Value => (PIDL)_Value;

    public static implicit operator PIDL(SafePIDL p)
        => p.Value;

    public static SafePIDL Null { get; } = new SafePIDL(nint.Zero, false);

    public bool IsNull
        => _Value == nint.Zero;

    public SHFILEINFOW? GetFileInfo(SFGAO_FLAGS attrFlags, SHGFI_FLAGS getFlags)
    {
        var fileInfo = new SHFILEINFOW();
        fileInfo.dwAttributes = (uint)attrFlags;
        var result = Shell32.SHGetFileInfo(Value, 0, ref fileInfo, (uint)Marshal.SizeOf(fileInfo), getFlags);
        return result == 0 ? null : fileInfo;
    }

    public IEnumerable<SafePIDL> EnumFolders(CancellationToken? token = null)
        => InternalEnumChildsAsync(_SHCONTF.SHCONTF_FOLDERS, token);

    public IEnumerable<SafePIDL> EnumFiles(CancellationToken? token = null)
        => InternalEnumChildsAsync(_SHCONTF.SHCONTF_NONFOLDERS, token);

    public IEnumerable<SafePIDL> EnumAllChilds(CancellationToken? token = null)
        => InternalEnumChildsAsync(_SHCONTF.SHCONTF_FOLDERS | _SHCONTF.SHCONTF_NONFOLDERS, token);

    public Task<IEnumerable<SafePIDL>> EnumFoldersAsync(CancellationToken token)
        => RunSTA(ct => EnumFolders(token), token);

    public Task<IEnumerable<SafePIDL>> EnumFilesAsync(CancellationToken token)
        => RunSTA(ct => EnumFiles(token), token);

    public Task<IEnumerable<SafePIDL>> EnumAllChildsAsync(CancellationToken token)
        => RunSTA(ct => EnumAllChilds(token), token);

    private IEnumerable<SafePIDL> InternalEnumChildsAsync(_SHCONTF flags, CancellationToken? token)
    {
        using var shellFolder = CreateShellFolder();
        if (shellFolder == null)
            yield break;

        using var enumIDList = shellFolder.EnumObjects(HWND.Null, flags);
        if (enumIDList == null)
            yield break;

        while (enumIDList.Next(this, out var childPidl))
        {
            if(token.HasValue && token.Value.IsCancellationRequested)
                yield break;

            yield return childPidl;
        }
    }

    public static SafePIDL FromFilePath(string filePath)
    {
        return new SafePIDL(Shell32.ILCreateFromPath(filePath));
    }

    public bool Equals(SafePIDL? other)
    {
        if (other is null || IsNull || other.IsNull)
            return false;

        return Shell32.ILIsEqual(Value, other.Value);
    }

    public override int GetHashCode()
    {
        if (_cachedHashCode.HasValue)
            return _cachedHashCode.Value;

        if (IsNull)
        {
            _cachedHashCode = 0;
            return 0;
        }

        unsafe
        {
            var size = Shell32.ILGetSize(Value);
            if (size <= 0)
            {
                _cachedHashCode = 0;
                return 0;
            }

            byte* ptr = (byte*)_Value;
            int hash = 17;
            for (int i = 0; i < size; i++)
            {
                hash = hash * 31 ^ ptr[i];
            }

            _cachedHashCode = hash;
            return hash;
        }
    }

    private class SafeShellFolder : SafeCom<IShellFolder>
    {
        public SafeShellFolder(IShellFolder value) : base(value) { }

        public SafeEnumIDList? EnumObjects(HWND hwnd, _SHCONTF grfFlags)
        {
            if (Instance.EnumObjects(hwnd, (uint)grfFlags, out var enumIDList).IsNotOK)
                return null;

            return new SafeEnumIDList(enumIDList);
        }
    }

    private class SafeEnumIDList : SafeCom<IEnumIDList>
    {
        public SafeEnumIDList(IEnumIDList value) : base(value) { }

        public bool Next(SafePIDL parent, out SafePIDL pidl)
        {
            if (Instance.Next(1, out var childPidl).IsNotOK)
            {
                pidl = Null;
                return false;
            }

            pidl = Combine(parent, childPidl);
            Marshal.FreeCoTaskMem((nint)childPidl);
            return true;
        }
    }

    private class SafeKnownFolderManager : SafeCom<IKnownFolderManager>
    {
        public SafeKnownFolderManager() : this((IKnownFolderManager)new KnownFolderManager()) { }
        private SafeKnownFolderManager(IKnownFolderManager value) : base(value) { }

        public HRESULT FindFolderFromIDList(PIDL pidl, out IKnownFolder ppkf) 
            => this.Instance.FindFolderFromIDList(pidl, out ppkf);
    }

    private string GetFilePath()
    {
        var result = Shell32.SHGetPathFromIDListEx(Value, out var path, FileSystem.MAX_PATH, GPFIDL_FLAGS.GPFIDL_DEFAULT);
        return result ? path.ToString() : "";
    }


    private SafeShellFolder? CreateShellFolder()
    {
        var iid = typeof(IShellFolder).GUID;
        if (Shell32.SHBindToObject(IntPtr.Zero, Value, IntPtr.Zero, in iid, out var oShell).IsNotOK)
            return null;

        return new SafeShellFolder((IShellFolder)oShell);
    }

    private static SafePIDL Combine(PIDL pidl1, PIDL pidl2)
        => new SafePIDL(Shell32.ILCombine(pidl1, pidl2));

    public static Task<T> RunSTA<T>(Func<CancellationToken, T> func, CancellationToken token)
        => _staPool.RunAsync(func, token);
}
