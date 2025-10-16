using RadianTools.UI.WPF.Common;
using System.IO;

namespace RadianTools.UI.WPF;

public class WindowsFolderItem : IFolderItem
{
    public IFolderItem? Parent { get; }
    public string TreePath { get; }

    private readonly SafePIDL _pidl;
    private static readonly Lazy<WindowsFolderIconCache> _iconFactory = new Lazy<WindowsFolderIconCache>(() => new WindowsFolderIconCache(SHIL.SMALL));

    internal WindowsFolderItem()
    {
        IsDummy = true;
        _pidl = SafePIDL.Null;
        TreePath = "";
    }

    internal WindowsFolderItem(WindowsFolderItem? parent, SafePIDL pidl)
    {
        IsDummy = false;
        _pidl = pidl;
        Parent = parent;
        TreePath = this.MakeTreePath(parent);
        if (pidl.IsNull)
            return;

        _icon = _iconFactory.Value.GetIcon(pidl.IconIndex);
    }

    public string FilePath => IsDummy ? "" : _pidl.FilePath;
    public string DisplayName => IsDummy ? "" : _pidl.DisplayName;
    public bool IsFolder => IsDummy ? false : _pidl.IsFolder;
    public bool HasSubFolder => IsDummy ? false : _pidl.HasSubFolder;
    public object? Icon => _icon;
    private object? _icon;
    public bool IsDummy { get; }

    public IEnumerable<IFolderItem> EnumFolders()
    {
        using var cts = new CancellationTokenSource();
        var t = _pidl.EnumFoldersAsync(cts.Token);
        t.ConfigureAwait(false);
        t.Wait();            
        return t.Result.Select(p => new WindowsFolderItem(this, p));
    }


    public IEnumerable<IFolderItem> EnumFiles()
        => _pidl.EnumFiles().Select(p => new WindowsFolderItem(this, p));

    public IEnumerable<IFolderItem> EnumAllChilds()
        => _pidl.EnumAllChilds().Select(p => new WindowsFolderItem(this, p));

    public void Dispose() => _pidl.Dispose();
}