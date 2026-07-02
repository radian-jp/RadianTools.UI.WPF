namespace RadianTools.UI.WPF.ViewModels;

using RadianTools.UI.WPF.Common;
using System.Collections.ObjectModel;
using RadianTools.Interop.Windows.Utility;
using System.IO;

/// <summary>
/// ツリービューの各階層におけるフォルダアイテムのビューモデル。
/// </summary>
public class FolderTreeItemViewModel : ViewModelBase
{
    /// <summary>親階層のアイテム。ルートの場合はnull。</summary>
    public FolderTreeItemViewModel? Parent { get; }

    /// <summary>内部データとしてのフォルダアイテム本体。</summary>
    public IFolderItem Item { get; }

    /// <summary>子アイテムを生成するためのファクトリ。</summary>
    public IFolderItemFactory ItemFactory { get; }

    public string Name => Item.DisplayName;
    public string FilePath => Item.FilePath;
    public string TreePath => Item.TreePath;
    public object? Icon => Item.Icon;

    /// <summary>子階層のアイテムリスト。</summary>
    public ObservableCollection<FolderTreeItemViewModel> Children => _children;
    private ObservableCollection<FolderTreeItemViewModel> _children = new();

    /// <summary>
    /// フォルダの展開状態
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value) && _isExpanded)
                LoadChildren();
        }
    }
    private bool _isExpanded;

    /// <summary>
    /// フォルダ選択有無
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    private bool _isSelected;

    private bool _isLoaded;

    /// <summary>
    /// <see cref="FolderTreeItemViewModel"/> のインスタンスを初期化します。
    /// </summary>
    /// <param name="itemFactory">アイテム生成用ファクトリ</param>
    /// <param name="item">保持するデータモデル</param>
    /// <param name="parent">親ビューモデル</param>
    public FolderTreeItemViewModel(IFolderItemFactory itemFactory, IFolderItem item, FolderTreeItemViewModel? parent)
    {
        ItemFactory = itemFactory;
        Item = item;
        Parent = parent;

        // 子フォルダを持つ可能性がある場合、ダミーアイテムを追加して「展開矢印」を表示させる
        if (Item.HasSubFolder)
            _children.Add(new FolderTreeItemViewModel(itemFactory, itemFactory.GetDummyItem(), parent));
    }

    /// <summary>
    /// 子階層のアイテムを読み込みます（遅延読み込み）。
    /// </summary>
    private void LoadChildren()
    {
        if (_isLoaded)
            return;

        _isLoaded = true;

        // 読み込み開始前にダミーアイテムを削除
        if (_children.Count == 1 && _children[0].Item.IsDummy)
            _children.Clear();

        try
        {
            // 実データ取得とソート処理
            // フォルダ優先で名前順に並び替え
            var folders = Item.EnumFolders();
            folders = folders
                .OrderBy(x => string.IsNullOrEmpty(x.FilePath))
                .ThenBy(x => !Directory.Exists(x.FilePath))
                .ThenBy(x => x.FilePath, NaturalStringComparer.Shared)
                .ThenBy(x => x.DisplayName, NaturalStringComparer.Shared);

            // 取得したデータを子リストに追加
            foreach (var folder in folders)
                _children.Add(new FolderTreeItemViewModel(ItemFactory, folder, this));
        }
        catch
        {
            // アクセス権限エラー等を想定、特に処理はしない
        }
    }
}
