using RadianTools.UI.WPF.Common;
using System.IO;
using RadianTools.Interop.Windows;

namespace RadianTools.UI.WPF;

public class WindowsFolderItemFactory : IFolderItemFactory
{
    public FolderRootMode RootMode { get; set; } = FolderRootMode.DesktopVirtualFolders;

    private static readonly Guid[] _IgnoreItems = {
        FOLDERID.UsersLibraries,
        FOLDERID.UsersFiles,
        FOLDERID.NetworkFolder,
    };

    public IEnumerable<IFolderItem> GetRootItems()
    {
        return RootMode switch
        {
            FolderRootMode.DesktopVirtualFolders => GetDesktopChildren(),
            FolderRootMode.LogicalDrives => GetLogicalDrives(),
            _ => new List<IFolderItem>()
        };
    }

    public IFolderItem GetDummyItem()
        => new WindowsFolderItem();

    private IEnumerable<IFolderItem> GetDesktopChildren()
    {
        var pidls = KnownFolderPIDL.Desktop.Value.EnumFolders();
        foreach (var child in pidls.Where(x => x.IsKnownFolder && !_IgnoreItems.Contains(x.KnownFolderId)))
        {
            yield return new WindowsFolderItem(null, child);
        }
    }

    private IEnumerable<IFolderItem> GetLogicalDrives()
    {
        return DriveInfo.GetDrives()
            .Where(d => d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Removable)
            .Select(d => SafePIDL.FromFilePath(d.Name))
            .Select(pidl => new WindowsFolderItem(null, pidl));
    }
}
