using System.Windows;
using System.Windows.Media;

namespace RadianTools.UI.WPF.Extentions;

public static class DependencyObjectExtentions
{
    public static T? FindAncestor<T>(this DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T target)
                return target;

            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
