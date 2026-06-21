using System.IO;

namespace RadianTools.UI.WPF.Common;

/// <summary>
/// <see cref="IFolderItem"/> に関する拡張メソッド群。
/// </summary>
public static class IFolderItemExtentions
{
    /// <summary>
    /// 親アイテムのパスと自身の名前を連結し、ツリー表示用のスを生成します。
    /// </summary>
    /// <param name="source">現在のフォルダアイテム。</param>
    /// <param name="parent">親フォルダアイテム（ルートの場合は null）。</param>
    /// <returns>区切り文字で連結された階層パス文字列。</returns>
    public static string MakeTreePath(this IFolderItem source, IFolderItem? parent)
        => parent == null
            ? source.DisplayName
            : $"{parent.TreePath}{Path.DirectorySeparatorChar}{source.DisplayName}";
}