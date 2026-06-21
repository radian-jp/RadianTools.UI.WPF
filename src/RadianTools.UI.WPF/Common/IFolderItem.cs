using System;
using System.Collections.Generic;

namespace RadianTools.UI.WPF.Common;

/// <summary>
/// フォルダを表現するための共通インターフェース。
/// </summary>
public interface IFolderItem : IDisposable
{
    /// <summary>親階層のアイテム。ルートの場合はnull。</summary>
    IFolderItem? Parent { get; }

    /// <summary>ファイルシステム上の絶対パス。</summary>
    string FilePath { get; }

    /// <summary>ツリー表示用の論理パス（階層を識別するための文字列）。</summary>
    string TreePath { get; }

    /// <summary>ユーザーに表示する名前。</summary>
    string DisplayName { get; }

    /// <summary>フォルダであるかどうかを示します。</summary>
    bool IsFolder { get; }

    /// <summary>サブフォルダを保持している可能性があるかどうかを示します。</summary>
    bool HasSubFolder { get; }

    /// <summary>フォルダやファイルに関連付けられたアイコンオブジェクト。</summary>
    object? Icon { get; }

    /// <summary>遅延読み込み用のプレースホルダー（ダミー）かどうかを示します。</summary>
    bool IsDummy { get; }

    /// <summary>直下のサブフォルダ一覧を取得します。</summary>
    IEnumerable<IFolderItem> EnumFolders();

    /// <summary>直下のファイル一覧を取得します。</summary>
    IEnumerable<IFolderItem> EnumFiles();

    /// <summary>直下の全アイテム（ファイルおよびフォルダ）を取得します。</summary>
    IEnumerable<IFolderItem> EnumAllChilds();
}