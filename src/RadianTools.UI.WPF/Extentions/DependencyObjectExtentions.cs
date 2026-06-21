using System.Windows;
using System.Windows.Media;

namespace RadianTools.UI.WPF.Extentions;

/// <summary>
/// <see cref="DependencyObject"/> 拡張メソッド群。
/// </summary>
public static class DependencyObjectExtentions
{
    /// <summary>
    /// ビジュアルツリーを遡り、指定した型に一致する最初の祖先要素を検索します。
    /// </summary>
    /// <typeparam name="T">検索対象の型。</typeparam>
    /// <param name="current">検索を開始する起点となるオブジェクト。</param>
    /// <returns>見つかった場合はその要素、見つからなかった場合は null。</returns>
    public static T? FindAncestor<T>(this DependencyObject current) where T : DependencyObject
    {
        // ビジュアルツリーのルートに到達するまで親を辿る
        while (current != null)
        {
            if (current is T target)
                return target;

            current = VisualTreeHelper.GetParent(current);
        }

        // 該当する型が見つからなかった場合
        return null;
    }
}
