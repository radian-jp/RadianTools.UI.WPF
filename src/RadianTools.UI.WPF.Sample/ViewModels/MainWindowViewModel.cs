using RadianTools.UI.WPF.Common;

namespace RadianTools.UI.WPF.Sample.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string _selectedTreePath = "";
    public string SelectedTreePath
    {
        get => _selectedTreePath;
        set => SetProperty(ref _selectedTreePath, value);
    }

    private IFolderItem? _selectedItem;
    public IFolderItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }
}
