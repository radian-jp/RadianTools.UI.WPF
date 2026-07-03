namespace RadianTools.UI.WPF.Controls;

using DependencyPropertyGenerator;
using RadianTools.UI.WPF.Common;
using RadianTools.UI.WPF.Extentions;
using RadianTools.UI.WPF.ViewModels;
using RadianTools.UI.WPF.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO;

/// <summary>
/// ファイルシステムや仮想フォルダ階層を表示・操作するためのツリービューコントロール。
/// </summary>
[DependencyProperty<FolderRootMode>("RootMode", DefaultValue = FolderRootMode.DesktopVirtualFolders)]
[DependencyProperty<IFolderItem>("SelectedItem", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<string>("SelectedTreePath", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<double>("IconSize", DefaultValue = 16.0)]
[DependencyProperty<Thickness>("IconMargin", DefaultValueExpression = "new System.Windows.Thickness(2)")]
[DependencyProperty<Thickness>("TextMargin", DefaultValueExpression = "new System.Windows.Thickness(2)")]
public partial class FolderTreeView : UserControl, IDisposable
{
    /// <summary>
    /// 選択アイテム変更時のイベントハンドラ。
    /// </summary>
    public event EventHandler<FolderItemEventArgs>? SelectedItemChanged;

    private FolderTreeItemViewModel? _loadingTreeItemVm = null;

    private IFolderItemFactory _factory;

    /// <summary>初期化済みかどうか</summary>
    private bool _initialized;

    /// <summary>
    /// ツリーの最上位に表示されるアイテムのコレクション。
    /// </summary>
    public ObservableCollection<FolderTreeItemViewModel> _rootItems = new ();

    /// <summary>
    /// <see cref="FolderTreeView"/> のインスタンスを初期化します。
    /// </summary>
    public FolderTreeView()
    {
        InitializeComponent();

        _factory = new WindowsFolderItemFactory { RootMode = RootMode };
        treeView.ItemsSource = _rootItems;

        CreateRootItems();

        Loaded += OnLoaded;
    }

    /// <summary>
    /// ロード時イベント
    /// </summary>
    /// <param name="sender">送信元</param>
    /// <param name="e">イベント引数</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
            return;

        _initialized = true;

        if (!string.IsNullOrEmpty(SelectedTreePath) && SelectItemFromTreePath(SelectedTreePath))
            treeView.ScrollFromItemData(SelectedItem);
    }

    partial void OnSelectedItemChanged()
    {
        SelectedTreePath = SelectedItem?.TreePath;
        SelectedItemChanged?.Invoke(
            this,
            new FolderItemEventArgs(SelectedItem));
    }

    /// <summary>
    /// 指定されたファイルパスに基づいて、ツリー内の対応するアイテムを選択・展開します。
    /// </summary>
    /// <param name="filePath">選択するターゲットのファイルパス。</param>
    public void SelectItemFromFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        IEnumerable<FolderTreeItemViewModel> drives = Array.Empty<FolderTreeItemViewModel>();

        // ルートモードに応じた探索開始位置の設定
        switch (RootMode)
        {
            case FolderRootMode.DesktopVirtualFolders:
                var pcNode = _rootItems.FirstOrDefault(item => item.Item.DisplayName == "PC");
                if (pcNode == null) return;
                if (!pcNode.IsExpanded) pcNode.IsExpanded = true;
                drives = pcNode.Children;
                break;
            default:
                drives = _rootItems;
                break;
        }

        // 再帰的にパスを探索
        var current = SearchChildFromFilePath(filePath, drives);
        if (current == null) return;

        SelectedItem = current.Item;
        SelectedTreePath = current?.Item.TreePath ?? "";

        // UIツリー上でアイテムが見つかれば表示・選択
        var treeViewItem = treeView.FindItem(current);
        if (treeViewItem != null)
        {
            treeViewItem.BringIntoView();
            treeViewItem.IsSelected = true;
        }
        else
        {
            // 未ロードの場合はロード完了時に選択処理を行うようフラグを立てる
            _loadingTreeItemVm = current;
        }

        SelectedItemChanged?.Invoke(
            this,
            new FolderItemEventArgs(SelectedItem));
    }

    /// <summary>
    /// 指定されたアイテムリストからパスに一致するノードを再帰的に検索します。
    /// </summary>
    private FolderTreeItemViewModel? SearchChildFromFilePath(string filePath, IEnumerable<FolderTreeItemViewModel> items)
    {
        foreach (var child in items)
        {
            var childPath = child.Item.FilePath;
            if (childPath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
            {
                child.IsExpanded = true;
                return child;
            }

            // パスの階層を掘り下げて探索
            if (filePath.StartsWith(childPath, StringComparison.OrdinalIgnoreCase))
            {
                child.IsExpanded = true;
                var result = SearchChildFromFilePath(filePath, child.Children);
                if (result != null) return result;
            }
        }
        return null;
    }

    #region IDisposable

    private bool _disposed = false;

    /// <inheritdoc/>
    public void Dispose()
    {
        var disposed = Interlocked.Exchange(ref _disposed, true);
        if (disposed)
            return;

        Loaded -= OnLoaded;

        // ViewModelリソース破棄
        foreach (var item in _rootItems)
            DisposeRecursive(item);

        treeView.DataContext = null;
        _loadingTreeItemVm = null;
    }

    /// <summary>
    /// 再帰的にフォルダツリーのアイテムViewModelを解放する。
    /// </summary>
    /// <param name="vmItem">フォルダツリーアイテムのViewModel</param>
    private void DisposeRecursive(FolderTreeItemViewModel vmItem)
    {
        vmItem.Item.Dispose();
        foreach (var child in vmItem.Children)
            DisposeRecursive(child);
    }

    #endregion

    /// <summary>
    /// WPFのTreeView選択変更をViewModelへ通知します。
    /// </summary>
    /// <param name="sender">送信元</param>
    /// <param name="e">イベント引数</param>
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var treeView = (TreeView)sender;
        var vmItem = treeView.SelectedItem as FolderTreeItemViewModel;

        SelectedItem = vmItem?.Item;
        SelectedTreePath = vmItem?.Item.TreePath ?? "";
    }

    /// <summary>
    /// RootMode変更時
    /// </summary>
    partial void OnRootModeChanged()
    {
        CreateRootItems();
    }

    /// <summary>
    /// 新しいViewModelインスタンスを生成し、イベント紐付けを行います。
    /// </summary>
    private void CreateRootItems()
    {
        _factory = new WindowsFolderItemFactory { RootMode = RootMode };

        // ルートアイテムの初期化：ファクトリから取得したアイテムをViewModelでラップする
        var items = _factory.GetRootItems()
            .DistinctBy(x => x.DisplayName)
            .Select(
                item => new FolderTreeItemViewModel(_factory, item, null)
            );

        _rootItems.Clear();
        _rootItems.AddRange(items);
    }

    /// <summary>
    /// UIのロード完了時に、必要なアイテムを自動的に表示・選択します。
    /// </summary>
    private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        var element = (sender as FrameworkElement)!;
        var textBlock = sender as TextBlock;
        var dataContext = textBlock?.DataContext as FolderTreeItemViewModel;

        if (_loadingTreeItemVm == null || textBlock == null || dataContext == null || dataContext != _loadingTreeItemVm)
            return;

        var treeViewItem = element.FindAncestor<TreeViewItem>();
        if (dataContext == _loadingTreeItemVm && treeViewItem != null)
        {
            treeViewItem.BringIntoView();
            treeViewItem.IsSelected = true;
        }
        _loadingTreeItemVm = null;
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
        var currentItems = _rootItems;
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
            SelectedItem = current.Item;
            current.IsSelected = true;
            return true;
        }

        return false;
    }
}

/// <summary>
/// フォルダ選択変更イベントの引数クラス。
/// </summary>
public class FolderItemEventArgs
{
    public IFolderItem? Item { get; }

    public FolderItemEventArgs(IFolderItem? item)
    {
        Item = item;
    }
}