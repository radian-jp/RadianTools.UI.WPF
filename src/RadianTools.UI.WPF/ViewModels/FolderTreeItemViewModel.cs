using RadianTools.UI.WPF.Common;
using System.Collections.ObjectModel;

namespace RadianTools.UI.WPF.ViewModels;

public class FolderTreeItemViewModel : ViewModelBase
{
    public FolderTreeItemViewModel? Parent { get; }
    public IFolderItem Item { get; }
    public IFolderItemFactory ItemFactory { get; }
    public string Name => Item.DisplayName;
    public string FilePath => Item.FilePath;
    public string TreePath => Item.TreePath;
    public object? Icon => Item.Icon;

    public ObservableCollection<FolderTreeItemViewModel> Childrens => _childrens;
    private ObservableCollection<FolderTreeItemViewModel> _childrens = new();

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

    public FolderTreeItemViewModel(IFolderItemFactory itemFactory, IFolderItem item, FolderTreeItemViewModel? parent)
    {
        ItemFactory = itemFactory;
        Item = item;
        Parent = parent;
        if (Item.HasSubFolder)
            _childrens.Add(new FolderTreeItemViewModel(itemFactory, itemFactory.GetDummyItem(), parent));
    }

    private void LoadChildren()
    {
        if (_isLoaded)
            return;

        _isLoaded = true;

        var childrens = new ObservableCollection<FolderTreeItemViewModel>();
        if (_childrens.Count == 1 && _childrens[0].Item.IsDummy)
            _childrens.Clear();

        try
        {
            var folders = Item.EnumFolders();
            if (Item.Parent != null && Item.Parent.IsFolder)
                folders = folders
                    .OrderBy(x=>x.IsFolder)
                    .ThenBy(x=>x.DisplayName);

            foreach (var folder in folders)
                _childrens.Add(new FolderTreeItemViewModel(ItemFactory, folder, this));
        }
        catch
        {
        }
    }
}
