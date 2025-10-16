using RadianTools.UI.WPF.Common;
using System.Collections.ObjectModel;
using System.IO;

namespace RadianTools.UI.WPF.ViewModels;

public class FolderTreeViewModel : ViewModelBase
{
    private readonly IFolderItemFactory _factory;

    public FolderTreeViewModel(WindowsFolderItemFactory factory)
    {
        _factory = factory;
        RootItems = new ObservableCollection<FolderTreeItemViewModel>(
            _factory.GetRootItems().Select(
                item => new FolderTreeItemViewModel(_factory, item, null)
            )
        );
    }

    public ObservableCollection<FolderTreeItemViewModel> RootItems { get; }

    private FolderTreeItemViewModel? _selectedItem;
    public FolderTreeItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public bool SelectItemFromTreePath(string treePath)
    {
        if (string.IsNullOrEmpty(treePath))
            return false;

        var parts = treePath.Split(Path.DirectorySeparatorChar);
        var currentItems = RootItems;
        FolderTreeItemViewModel? current = null;

        foreach (var part in parts)
        {
            var next = currentItems.FirstOrDefault(x => x.Name == part);
            if (next == null)
                break; // ここで終了 → 途中までしか見つからなかった

            current = next;
            current.IsExpanded = true;
            currentItems = current.Childrens;
        }

        if (current != null)
        {
            SelectedItem = current;
            return true;
        }

        return false;
    }
}
