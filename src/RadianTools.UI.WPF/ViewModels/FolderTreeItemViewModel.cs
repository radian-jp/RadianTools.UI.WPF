using RadianTools.UI.WPF.Common;
using System.Collections.ObjectModel;

namespace RadianTools.UI.WPF.ViewModels;

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
    /// フォルダの展開状態を取得・設定します。
    /// 展開時に子階層の遅延読み込みをトリガーします。
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
            var folders = Item.EnumFolders();

            // 親が存在する場合（ルート以外）はフォルダ優先で名前順に並び替え
            if (Item.Parent != null && Item.Parent.IsFolder)
                folders = folders
                    .OrderBy(x => x.IsFolder)
                    .ThenBy(x => x.DisplayName);

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
