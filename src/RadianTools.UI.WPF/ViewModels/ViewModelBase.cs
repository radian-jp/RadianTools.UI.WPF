using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RadianTools.UI.WPF.ViewModels;

/// <summary>
/// 全てのViewModelの基底クラス。
/// INotifyPropertyChangedインターフェースを実装し、プロパティ変更通知機能を提供します。
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// プロパティ値が変更されたときに発生するイベント。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// プロパティ変更を通知します。
    /// </summary>
    /// <param name="propertyName">呼び出し元のプロパティ名。CallerMemberName属性により自動補完されます。</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// プロパティの値を更新し、値が変更された場合のみ変更通知イベントを発行します。
    /// </summary>
    /// <typeparam name="T">プロパティの型。</typeparam>
    /// <param name="storage">現在の値が格納されているフィールドへの参照。</param>
    /// <param name="value">設定する新しい値。</param>
    /// <param name="propertyName">プロパティ名（自動設定）。</param>
    /// <returns>値が変更された場合は true、変更がなかった場合は false。</returns>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        // 現在の値と新しい値が等しければ何もしない
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        // 値を更新して通知を行う
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
