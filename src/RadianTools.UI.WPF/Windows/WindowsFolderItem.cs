using RadianTools.Interop.Windows;
using RadianTools.UI.WPF.Common;

namespace RadianTools.UI.WPF;

/// <summary>
/// WindowsシェルのPIDL（項目識別子リスト）を保持し、フォルダ情報を抽象化するアイテムクラス。
/// </summary>
public class WindowsFolderItem : IFolderItem
{
    /// <summary>
    /// 親フォルダのアイテム。ルートの場合はnull。
    /// </summary>
    public IFolderItem? Parent { get; }

    /// <summary>
    /// ツリー構造上のパス表現。
    /// </summary>
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

    /// <summary>
    /// ファイルパス
    /// </summary>
    public string FilePath => IsDummy ? "" : _pidl.FilePath;

    /// <summary>
    /// 表示名
    /// </summary>
    public string DisplayName => IsDummy ? "" : _pidl.DisplayName;

    /// <summary>
    /// 実フォルダかどうか
    /// </summary>
    public bool IsFolder => IsDummy ? false : _pidl.IsFolder;

    /// <summary>
    /// サブフォルダ有無
    /// </summary>
    public bool HasSubFolder => IsDummy ? false : _pidl.HasSubFolder;

    /// <summary>
    /// アイコン
    /// </summary>
    public object? Icon => _icon;
    private object? _icon;

    /// <summary>実体を持たないプレースホルダーかどうか。</summary>
    public bool IsDummy { get; }

    /// <summary>
    /// フォルダ内のサブフォルダ一覧取得
    /// </summary>
    public IEnumerable<IFolderItem> EnumFolders()
    {
        using var cts = new CancellationTokenSource();

        var children = Task.Run(() =>
        {
            var t = _pidl.EnumFoldersAsync(cts.Token);
            t.Wait();
            return t.Result;
        }).GetAwaiter().GetResult();

        return children.Select(p => new WindowsFolderItem(this, p));
    }

    /// <summary>フォルダ内のファイル一覧を取得します。</summary>
    public IEnumerable<IFolderItem> EnumFiles()
        => _pidl.EnumFiles().Select(p => new WindowsFolderItem(this, p));

    /// <summary>フォルダ内の全てのアイテム（ファイル・フォルダ両方）を取得します。</summary>
    public IEnumerable<IFolderItem> EnumAllChilds()
        => _pidl.EnumAllChilds().Select(p => new WindowsFolderItem(this, p));

    /// <summary>保持しているPIDLリソースを解放します。</summary>
    public void Dispose() => _pidl.Dispose();
}