namespace RadianTools.UI.WPF.ViewModels;

using RadianTools.UI.WPF.Common;
using System.Collections.ObjectModel;
using System.IO;

/// <summary>
/// ツリービュー全体のルート階層と選択状態を管理するメインのViewModel。
/// </summary>
public class FolderTreeViewModel : ViewModelBase
{
    private readonly IFolderItemFactory _factory;

    /// <summary>
    /// <see cref="FolderTreeViewModel"/> のインスタンスを初期化し、ルートアイテムを生成します。
    /// </summary>
    /// <param name="factory">フォルダアイテムの生成に使用するファクトリ。</param>
    public FolderTreeViewModel(WindowsFolderItemFactory factory)
    {
        _factory = factory;

        // ルートアイテムの初期化：ファクトリから取得したアイテムをViewModelでラップする
        RootItems = new ObservableCollection<FolderTreeItemViewModel>(
            _factory.GetRootItems()
            .DistinctBy(x=>x.DisplayName)
            .Select(
                item => new FolderTreeItemViewModel(_factory, item, null)
            )
        );
    }

    /// <summary>
    /// ツリーの最上位に表示されるアイテムのコレクション。
    /// </summary>
    public ObservableCollection<FolderTreeItemViewModel> RootItems { get; }

    private FolderTreeItemViewModel? _selectedItem;

    /// <summary>
    /// 現在選択されているツリーアイテムを取得または設定します。
    /// </summary>
    public FolderTreeItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    /// <summary>
    /// 指定されたツリーパスに基づいて、該当するアイテムを再帰的に探索し選択状態にします。
    /// </summary>
    /// <param name="treePath">対象のツリーパス（ディレクトリ区切り文字で連結された文字列）。</param>
    /// <returns>アイテムが見つかり、選択が完了した場合は true。</returns>
    public bool SelectItemFromTreePath(string treePath)
    {
        if (string.IsNullOrEmpty(treePath))
            return false;

        // パスを分解してルートから順に辿る
        var parts = treePath.Split(Path.DirectorySeparatorChar);
        var currentItems = RootItems;
        FolderTreeItemViewModel? current = null;

        foreach (var part in parts)
        {
            // 現在の階層から該当する名前のアイテムを探す
            var next = currentItems.FirstOrDefault(x => x.Name == part);
            if (next == null)
                break; // 途中の階層が見つからない場合は探索終了

            current = next;

            // 探索経路を自動的に展開する
            current.IsExpanded = true;

            // 次の階層へ移動（子アイテムコレクションを参照）
            currentItems = current.Children;
        }

        // 最終到達地点を選択状態にする
        if (current != null)
        {
            SelectedItem = current;
            current.IsSelected = true;
            return true;
        }

        return false;
    }
}
