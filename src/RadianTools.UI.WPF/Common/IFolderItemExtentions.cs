using System.IO;

namespace RadianTools.UI.WPF.Common;

public static class IFolderItemExtentions
{
    public static string MakeTreePath(this IFolderItem source, IFolderItem? parent)
        => parent == null ? source.DisplayName : $"{parent.TreePath}{Path.DirectorySeparatorChar}{source.DisplayName}";
}
