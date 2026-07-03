namespace RadianTools.UI.WPF.Collections;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

/// <summary>
/// 複数追加用ObservableCollection
/// </summary>
/// <typeparam name="T">データ型</typeparam>
public class RangeObservableCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// 複数の要素を追加した後に通知を行う。（1要素毎には通知しない）
    /// </summary>
    /// <param name="items">追加する要素</param>
    public void AddRangeDeferred(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        bool added = false;

        foreach (var item in items)
        {
            Items.Add(item);
            added = true;
        }

        if (!added)
            return;

        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
