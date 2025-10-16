using System.Windows.Controls;

namespace RadianTools.UI.WPF.Extentions;

/// <summary>
/// TreeView に関する拡張メソッドを提供します。
/// </summary>
public static class TreeViewExtensions
{
    /// <summary>
    /// 指定されたデータアイテムに対応する TreeViewItem を検索します。
    /// </summary>
    /// <param name="treeView">検索対象の TreeView。</param>
    /// <param name="itemData">TreeView.ItemsSource中の検索したい要素。</param>
    /// <returns>対応する TreeViewItem。見つからない場合は null。</returns>
    public static TreeViewItem? FindItem(this TreeView treeView, object? itemData)
    {
        if (itemData == null) 
            return null;

        // ルートレベルで探す
        var rootContainer = treeView.ItemContainerGenerator.ContainerFromItem(itemData) as TreeViewItem;
        if (rootContainer != null)
            return rootContainer;

        // 再帰的に探索
        for (int i = 0; i < treeView.Items.Count; i++)
        {
            var root = treeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
            var found = FindItemInSubtree(root, itemData);
            if (found != null) 
                return found;
        }

        return null;
    }

    /// <summary>
    /// 指定された TreeViewItem の下位階層から、データアイテムに対応する TreeViewItem を再帰的に検索します。
    /// </summary>
    /// <param name="parentContainer">探索を開始する親の TreeViewItem。</param>
    /// <param name="targetItem">TreeView.ItemsSource中の検索したい要素。</param>
    /// <returns>対応する TreeViewItem。見つからない場合は null。</returns>
    private static TreeViewItem? FindItemInSubtree(TreeViewItem? parentContainer, object targetItem)
    {
        if (parentContainer == null) 
            return null;

        var direct = parentContainer.ItemContainerGenerator.ContainerFromItem(targetItem) as TreeViewItem;
        if (direct != null) return direct;

        var children = parentContainer.Items;
        for (int i = 0; i < children.Count; i++)
        {
            var childContainer = parentContainer.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
            var found = FindItemInSubtree(childContainer, targetItem);
            if (found != null) return found;
        }

        return null;
    }

    /// <summary>
    /// 指定されたデータアイテムに対応する TreeViewItem を検索し、見つかればスクロールして表示します。
    /// </summary>
    /// <param name="treeView">対象の TreeView。</param>
    /// <param name="itemData">表示したいデータアイテム。</param>
    /// <returns>スクロールされたかどうか。</returns>
    public static bool ScrollFromItemData(this TreeView treeView, object? itemData)
    {
        var container = treeView.FindItem(itemData);
        if (container != null)
        {
            container.BringIntoView();
            return true;
        }
        return false;
    }
}
