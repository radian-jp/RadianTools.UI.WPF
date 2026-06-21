namespace RadianTools.UI.WPF.Common;

/// <summary>
/// フォルダルートモード
/// </summary>
public enum FolderRootMode
{
    /// <summary>
    /// 仮想デスクトップフォルダ
    /// </summary>
    DesktopVirtualFolders,

    /// <summary>
    /// 論理ドライブ
    /// </summary>
    LogicalDrives
}

/// <summary>
/// フォルダアイテムファクトリインターフェース
/// </summary>
public interface IFolderItemFactory
{
    /// <summary>
    /// ルートモード
    /// </summary>
    FolderRootMode RootMode { get; set; }

    /// <summary>
    /// ルート直下アイテム取得
    /// </summary>
    IEnumerable<IFolderItem> GetRootItems();

    /// <summary>
    /// ダミーアイテム取得
    /// </summary>
    IFolderItem GetDummyItem();
}
