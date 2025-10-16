namespace RadianTools.UI.WPF.Common;

public enum FolderRootMode
{
    DesktopVirtualFolders,
    LogicalDrives
}

public interface IFolderItemFactory
{
    FolderRootMode RootMode { get; set; }
    IEnumerable<IFolderItem> GetRootItems();
    IFolderItem GetDummyItem();
}
