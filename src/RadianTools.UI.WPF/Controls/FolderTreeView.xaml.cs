namespace RadianTools.UI.WPF.Controls;

using DependencyPropertyGenerator;
using RadianTools.UI.WPF.Common;
using RadianTools.UI.WPF.Extentions;
using RadianTools.UI.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

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

    private FolderTreeViewModel _vm;
    private int _disposed = 0;
    private FolderTreeItemViewModel? _loadingTreeItemVm = null;

    /// <summary>
    /// <see cref="FolderTreeView"/> のインスタンスを初期化します。
    /// </summary>
    public FolderTreeView()
    {
        InitializeComponent();
        _vm = CreateViewModel();

        Loaded += OnLoaded;
    }

    /// <summary>
    /// ロード時イベント処理
    /// </summary>
    /// <param name="sender">送信元</param>
    /// <param name="e">イベント引数</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedTreePath) && _vm.SelectItemFromTreePath(SelectedTreePath))
            treeView.ScrollFromItemData(_vm.SelectedItem);
    }

    /// <summary>
    /// ViewModelのプロパティ変更を監視し、Viewと同期します。
    /// </summary>
    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_vm.SelectedItem == null || e.PropertyName != nameof(FolderTreeViewModel.SelectedItem))
            return;

        SelectedItem = _vm.SelectedItem.Item;
        SelectedTreePath = _vm.SelectedItem.Item.TreePath;

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
        if (string.IsNullOrEmpty(filePath) || _vm == null)
            return;

        IEnumerable<FolderTreeItemViewModel> drives = Array.Empty<FolderTreeItemViewModel>();

        // ルートモードに応じた探索開始位置の設定
        switch (RootMode)
        {
            case FolderRootMode.DesktopVirtualFolders:
                var pcNode = _vm.RootItems.FirstOrDefault(item => item.Item.DisplayName == "PC");
                if (pcNode == null) return;
                if (!pcNode.IsExpanded) pcNode.IsExpanded = true;
                drives = pcNode.Children;
                break;
            default:
                drives = _vm.RootItems;
                break;
        }

        // 再帰的にパスを探索
        var current = SearchChildFromFilePath(filePath, drives);
        if (current == null) return;

        _vm.SelectedItem = current;
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

    /// <summary>
    /// リソースを破棄し、イベントハンドラの購読を解除します。
    /// </summary>
    public void Dispose()
    {
        var disposed = Interlocked.Exchange(ref _disposed, 1);
        if (disposed == 1)
            return;

        // イベントハンドラ解除
        Loaded -= OnLoaded;
        _vm.PropertyChanged -= OnVmPropertyChanged!;

        // ViewModelリソース破棄
        foreach (var item in _vm.RootItems)
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

    /// <summary>
    /// WPFのTreeView選択変更をViewModelへ通知します。
    /// </summary>
    /// <param name="sender">送信元</param>
    /// <param name="e">イベント引数</param>
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var treeView = (TreeView)sender;
        var vmItem = treeView.SelectedItem as FolderTreeItemViewModel;

        if (_vm != null)
            _vm.SelectedItem = vmItem;

        this.SelectedItem = vmItem?.Item;
        this.SelectedTreePath = vmItem?.Item.TreePath ?? "";
    }

    /// <summary>
    /// RootMode変更時
    /// </summary>
    partial void OnRootModeChanged()
    {
        this._vm = this.CreateViewModel();
    }

    /// <summary>
    /// 新しいViewModelインスタンスを生成し、イベント紐付けを行います。
    /// </summary>
    private FolderTreeViewModel CreateViewModel()
    {
        if (_vm != null)
            _vm.PropertyChanged -= OnVmPropertyChanged!;

        var factory = new WindowsFolderItemFactory { RootMode = RootMode };
        var vmNew = new FolderTreeViewModel(factory);
        treeView.DataContext = vmNew;

        vmNew.PropertyChanged += OnVmPropertyChanged!;

        if (!string.IsNullOrEmpty(SelectedTreePath) && vmNew.SelectItemFromTreePath(SelectedTreePath))
        {
            treeView.ScrollFromItemData(vmNew.SelectedItem);
        }

        return vmNew;
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
}

/// <summary>
/// フォルダ選択変更イベントの引数クラス。
/// </summary>
public class FolderItemEventArgs
{
    public IFolderItem Item { get; }

    public FolderItemEventArgs(IFolderItem item)
    {
        Item = item;
    }
}