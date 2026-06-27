using System.Windows;
using System.Windows.Threading;

namespace RadianTools.UI.WPF.Threading;

/// <summary>
/// WPF UIスレッド安全実行ラッパー
/// - Application.Current依存を内部に閉じ込める
/// - shutdown時の例外を吸収
/// </summary>
public static class SafeDispatcher
{
    /// <summary>
    /// UIスレッドで安全に実行（戻り値なし）
    /// </summary>
    public static Task InvokeAsync(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null)
            return Task.CompletedTask;

        if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
            return Task.CompletedTask;

        return InvokeInternal(dispatcher, action);
    }

    private static async Task InvokeInternal(Dispatcher dispatcher, Action action)
    {
        try
        {
            await dispatcher.InvokeAsync(action);
        }
        catch (TaskCanceledException)
        {
            // shutdown時の典型例なので無視
        }
    }

    /// <summary>
    /// 戻り値あり版
    /// </summary>
    public static Task<T?> InvokeAsync<T>(Func<T> func)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null)
            return Task.FromResult<T?>(default);

        if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
            return Task.FromResult<T?>(default);

        return InvokeInternal(dispatcher, func);
    }

    private static async Task<T?> InvokeInternal<T>(Dispatcher dispatcher, Func<T> func)
    {
        try
        {
            return await dispatcher.InvokeAsync(func);
        }
        catch (TaskCanceledException)
        {
            return default;
        }
    }
}