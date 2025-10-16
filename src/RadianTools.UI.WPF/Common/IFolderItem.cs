namespace RadianTools.UI.WPF.Common;

public interface IFolderItem : IDisposable
{
    IFolderItem? Parent { get; }
    string FilePath { get; }
    string TreePath { get; }
    string DisplayName { get; }
    bool IsFolder { get; }
    bool HasSubFolder { get; }
    object? Icon { get; }
    bool IsDummy { get; }

    IEnumerable<IFolderItem> EnumFolders();
    IEnumerable<IFolderItem> EnumFiles();
    IEnumerable<IFolderItem> EnumAllChilds();
}