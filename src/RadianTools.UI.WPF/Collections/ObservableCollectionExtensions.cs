namespace RadianTools.UI.WPF.Collections;

using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class ObservableCollectionExtensions
{
    /// <summary>
    /// 複数の要素を追加する。
    /// </summary>
    /// <typeparam name="T">データ型</typeparam>
    /// <param name="collection">追加対象コレクション</param>
    /// <param name="items">追加する要素</param>
    public static void AddRange<T>(
        this ObservableCollection<T> collection,
        IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
