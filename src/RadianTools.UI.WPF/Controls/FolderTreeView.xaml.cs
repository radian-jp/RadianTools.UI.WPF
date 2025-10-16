using RadianTools.UI.WPF.Common;
using RadianTools.UI.WPF.Extentions;
using RadianTools.UI.WPF.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RadianTools.UI.WPF.Controls;

public partial class FolderTreeView : UserControl, IDisposable
{
    public static readonly DependencyProperty RootModeProperty =
        DependencyProperty.Register(
            nameof(RootMode),
            typeof(FolderRootMode),
            typeof(FolderTreeView),
            new PropertyMetadata(FolderRootMode.DesktopVirtualFolders, OnRootModeChanged));

    public FolderRootMode RootMode
    {
        get => (FolderRootMode)GetValue(RootModeProperty);
        set => SetValue(RootModeProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(IFolderItem),
            typeof(FolderTreeView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public IFolderItem? SelectedItem
    {
        get => (IFolderItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedTreePathProperty =
        DependencyProperty.Register(
            nameof(SelectedTreePath),
            typeof(string), 
            typeof(FolderTreeView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string SelectedTreePath
    {
        get => (string)GetValue(SelectedTreePathProperty);
        set => SetValue(SelectedTreePathProperty, value);
    }

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(FolderTreeView),
            new PropertyMetadata(16.0));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly DependencyProperty IconMarginProperty =
        DependencyProperty.Register(nameof(IconMargin), typeof(Thickness), typeof(FolderTreeView),
            new PropertyMetadata(new Thickness(2)));

    public Thickness IconMargin
    {
        get => (Thickness)GetValue(IconMarginProperty);
        set => SetValue(IconMarginProperty, value);
    }

    public static readonly DependencyProperty TextMarginProperty =
        DependencyProperty.Register(nameof(TextMargin), typeof(Thickness), typeof(FolderTreeView),
            new PropertyMetadata(new Thickness(2)));

    public Thickness TextMargin
    {
        get => (Thickness)GetValue(TextMarginProperty);
        set => SetValue(TextMarginProperty, value);
    }

    public static readonly RoutedEvent SelectedItemChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectedItemChanged), RoutingStrategy.Bubble,
            typeof(EventHandler<SelectedItemChangedEventArgs>), typeof(FolderTreeView));

    public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged
    {
        add => AddHandler(SelectedItemChangedEvent, value);
        remove => RemoveHandler(SelectedItemChangedEvent, value);
    }

    private FolderTreeViewModel _vm;
    private bool _disposed;
    private FolderTreeItemViewModel? _loadingTreeItemVm = null;

    public FolderTreeView()
    {
        InitializeComponent();
        _vm = CreateViewModel();

        Loaded += (s, e) =>
        {
            if (!string.IsNullOrEmpty(SelectedTreePath) && _vm.SelectItemFromTreePath(SelectedTreePath))
            {
                treeView.ScrollFromItemData(_vm.SelectedItem);
            }
        };
    }

    private void OnVmPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_vm.SelectedItem==null || e.PropertyName != nameof(FolderTreeViewModel.SelectedItem))
            return;

        SelectedItem = _vm.SelectedItem.Item;
        SelectedTreePath = _vm.SelectedItem.Item.TreePath;

        if (SelectedItem != null)
            RaiseEvent(new SelectedItemChangedEventArgs(SelectedItemChangedEvent, SelectedItem));
    }

    public void SelectItemFromFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || _vm == null)
            return;

        char dsp = Path.DirectorySeparatorChar;

        IEnumerable<FolderTreeItemViewModel> drives = Array.Empty<FolderTreeItemViewModel>();
        switch (RootMode)
        {
            case FolderRootMode.DesktopVirtualFolders:
                // "PC" ノードを探す
                var pcNode = _vm.RootItems.FirstOrDefault(item => item.Item.DisplayName == "PC");
                if (pcNode == null)
                    return;

                if (!pcNode.IsExpanded)
                    pcNode.IsExpanded = true;

                drives = pcNode.Childrens;
                break;

            default:
                // ルートに直接ドライブがある
                drives = _vm.RootItems;
                break;
        }

        var current = SearchChildFromFilePath(filePath, drives);
        if (current == null)
            return;

        _vm.SelectedItem = current;
        SelectedItem = current.Item;
        SelectedTreePath = current?.Item.TreePath ?? "";

        var treeViewItem = treeView.FindItem(current);
        if (treeViewItem != null)
        {
            // すでにロード済み
            treeViewItem.BringIntoView();
            treeViewItem.IsSelected = true;
        }
        else
        {
            // まだロードされていない
            _loadingTreeItemVm = current;
        }

        RaiseEvent(new SelectedItemChangedEventArgs(SelectedItemChangedEvent, SelectedItem));
    }

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

            if (filePath.StartsWith(childPath, StringComparison.OrdinalIgnoreCase))
            {
                child.IsExpanded = true;
                var result = SearchChildFromFilePath(filePath, child.Childrens);
                if (result != null)
                    return result;
            }
        }

        return null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var item in _vm.RootItems)
            DisposeRecursive(item);

        _vm.PropertyChanged -= OnVmPropertyChanged!;
    }

    private void DisposeRecursive(FolderTreeItemViewModel vmItem)
    {
        vmItem.Item.Dispose();
        foreach (var child in vmItem.Childrens)
            DisposeRecursive(child);
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var treeView = (TreeView)sender;
        var vmItem = treeView.SelectedItem as FolderTreeItemViewModel;

        if (_vm != null)
            _vm.SelectedItem = vmItem;

        this.SelectedItem = vmItem?.Item;
        this.SelectedTreePath = vmItem?.Item.TreePath ?? "";
    }


    private static void OnRootModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FolderTreeView view)
            view._vm = view.CreateViewModel();
    }

    private FolderTreeViewModel CreateViewModel()
    {
        if (_vm != null)
            _vm.PropertyChanged -= OnVmPropertyChanged!;

        var factory = new WindowsFolderItemFactory();
        factory.RootMode = RootMode;
        var vmNew = new FolderTreeViewModel(factory);
        treeView.DataContext = vmNew;

        vmNew.PropertyChanged += OnVmPropertyChanged!;

        if (!string.IsNullOrEmpty(SelectedTreePath) && vmNew.SelectItemFromTreePath(SelectedTreePath))
        {
            treeView.ScrollFromItemData(vmNew.SelectedItem);
        }

        return vmNew;
    }

    private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        var element = (sender as FrameworkElement)!;
        var textBlock = sender as TextBlock;
        var dataContext = textBlock?.DataContext as FolderTreeItemViewModel;
        if (_loadingTreeItemVm == null || textBlock == null || dataContext == null || dataContext != _loadingTreeItemVm)
            return;

        var treeViewItem = element.FindAncestor<TreeViewItem>();
        var vm = element.DataContext as FolderTreeItemViewModel;
        if (vm == _loadingTreeItemVm && treeViewItem != null)
        {
            treeViewItem.BringIntoView();
            treeViewItem.IsSelected = true;
        }
        _loadingTreeItemVm = null;
    }
}

public class SelectedItemChangedEventArgs : RoutedEventArgs
{
    public IFolderItem Item { get; }

    public SelectedItemChangedEventArgs(RoutedEvent routedEvent, IFolderItem item)
        : base(routedEvent)
    {
        Item = item;
    }
}
