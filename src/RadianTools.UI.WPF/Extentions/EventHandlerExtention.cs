using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RadianTools.UI.WPF.Extentions;

/// <summary>
/// EventHandlerデバッグ補助拡張メソッド群。
/// </summary>
public static class EventHandlerExtensions
{
    /// <summary>
    /// <see cref="EventHandler"/> を安全に呼び出し、呼び出し元と実行されたメソッドをデバッグ出力します。
    /// </summary>
    /// <param name="handler">呼び出すイベントハンドラ。</param>
    /// <param name="sender">イベント送信元オブジェクト。</param>
    /// <param name="args">イベント引数（nullの場合は Empty を使用）。</param>
    /// <param name="caller">呼び出し元のメンバー名（自動設定）。</param>
    public static void InvokeEx(this EventHandler? handler, object? sender, EventArgs? args = null, [CallerMemberName] string? caller = null)
    {
        var senderName = sender?.GetType().ToString() ?? "null";

        // ハンドラが登録されていない場合はデバッグログを出して終了
        if (handler == null)
        {
            Debug.WriteLine($"{nameof(InvokeEx)} was called, but the handler was null. (sender: {senderName}, caller: {caller})");
            return;
        }

        args ??= EventArgs.Empty;

        // マルチキャストデリゲートを個別に展開し、呼び出し履歴とメソッド名をトレース
        foreach (EventHandler h in handler.GetInvocationList())
        {
            Debug.WriteLine($"{h.Method.Name} Invoked. (sender: {senderName}, caller: {caller})");
            h(sender, args);
        }
    }

    /// <summary>
    /// ジェネリックな <see cref="EventHandler{TEventArgs}"/> を安全に呼び出し、デバッグ出力を行います。
    /// </summary>
    /// <typeparam name="TEventArgs">イベント引数の型。</typeparam>
    /// <param name="handler">呼び出すイベントハンドラ。</param>
    /// <param name="sender">イベント送信元オブジェクト。</param>
    /// <param name="args">イベント引数。</param>
    /// <param name="caller">呼び出し元のメンバー名（自動設定）。</param>
    public static void InvokeEx<TEventArgs>(this EventHandler<TEventArgs>? handler, object? sender, TEventArgs args, [CallerMemberName] string? caller = null) where TEventArgs : EventArgs
    {
        var senderName = sender?.GetType().ToString() ?? "null";

        // ハンドラが登録されていない場合はデバッグログを出して終了
        if (handler == null)
        {
            Debug.WriteLine($"{nameof(InvokeEx)} was called, but the handler was null. (sender: {senderName}, caller: {caller})");
            return;
        }

        // 登録されている全てのハンドラを順次実行し、そのメソッド名をデバッグ出力
        foreach (EventHandler<TEventArgs> h in handler.GetInvocationList())
        {
            Debug.WriteLine($"{h.Method.Name} Invoked. (sender: {senderName}, caller: {caller})");
            h(sender, args);
        }
    }
}