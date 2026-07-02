namespace RadianTools.UI.WPF;

using RadianTools.Interop.Windows;
using RadianTools.UI.WPF.Common;

/// <summary>
/// WindowsシェルのPIDL（項目識別子リスト）を保持し、フォルダ情報を抽象化するアイテムクラス。
/// </summary>
public class WindowsFolderItem : IFolderItem
{
    // <inheritdoc />
    public IFolderItem? Parent { get; }

    // <inheritdoc />
    public string TreePath { get; }

    /// <summary>
    /// フォルダのPIDL
    /// </summary>
    private readonly SafePIDL _pidl;

    // アイコンの重複生成を防ぐためのキャッシュ
    private static readonly Lazy<WpfShellIconCache> _iconFactory = new(() => new WpfShellIconCache(SHIL.SMALL));

    /// <summary>
    /// 遅延読み込み用などの「ダミーアイテム」を生成するためのプライベートコンストラクタ。
    /// </summary>
    internal WindowsFolderItem()
    {
        IsDummy = true;
        _pidl = SafePIDL.Null;
        TreePath = "";
    }

    /// <summary>
    /// 指定されたPIDLからフォルダアイテムを生成します。
    /// </summary>
    /// <param name="parent">親フォルダアイテム。</param>
    /// <param name="pidl">WindowsシェルのPIDL。</param>
    internal WindowsFolderItem(WindowsFolderItem? parent, SafePIDL pidl)
    {
        IsDummy = false;
        _pidl = pidl;
        Parent = parent;
        TreePath = this.MakeTreePath(parent);

        if (pidl.IsNull)
            return;

        // アイコンキャッシュから対応するアイコンを取得
        _icon = _iconFactory.Value.GetIcon(pidl.IconIndex);
    }

    // <inheritdoc />
    public string FilePath => IsDummy ? "" : _pidl.FilePath;

    // <inheritdoc />
    public string DisplayName => IsDummy ? "" : _pidl.DisplayName;

    // <inheritdoc />
    public bool IsFolder => IsDummy ? false : _pidl.IsFolder;

    // <inheritdoc />
    public bool HasSubFolder => IsDummy ? false : _pidl.HasSubFolder;

    // <inheritdoc />
    public object? Icon => _icon;
    private object? _icon;

    // <inheritdoc />
    public bool IsDummy { get; }

    // <inheritdoc />
    public IEnumerable<IFolderItem> EnumFolders()
    {
        using var cts = new CancellationTokenSource();

        var children = Task.Run(() =>
        {
            return _pidl.EnumFoldersAsync(cts.Token);
        }).GetAwaiter().GetResult();

        return children.Select(p => new WindowsFolderItem(this, p));
    }

    // <inheritdoc />
    public IEnumerable<IFolderItem> EnumFiles()
        => _pidl.EnumFiles().Select(p => new WindowsFolderItem(this, p));

    // <inheritdoc />
    public IEnumerable<IFolderItem> EnumAllChilds()
        => _pidl.EnumAllChilds().Select(p => new WindowsFolderItem(this, p));

    // <inheritdoc />
    public void Dispose() => _pidl.Dispose();
}