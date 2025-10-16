using System.Collections.Concurrent;

namespace RadianTools.UI.WPF.Windows;

public class STAThreadPool : IDisposable
{
    private readonly BlockingCollection<Action> _queue = new();
    private readonly List<Thread> _threads = new();

    public STAThreadPool(int threadCount)
    {
        for (int i = 0; i < threadCount; i++)
        {
            var thread = new Thread(() =>
            {
                foreach (var action in _queue.GetConsumingEnumerable())
                {
                    try { action(); }
                    catch { }
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            _threads.Add(thread);
        }
    }

    public async Task<T> RunAsync<T>(Func<CancellationToken, T> func, CancellationToken token)
    {
        var tcs = new TaskCompletionSource<T>();

        if (token.IsCancellationRequested)
        {
            tcs.SetCanceled();
            return await tcs.Task.ConfigureAwait(false);
        }

        _queue.Add(() =>
        {
            if (token.IsCancellationRequested)
            {
                tcs.SetCanceled();
                return;
            }

            try
            {
                var result = func(token);
                tcs.SetResult(result);
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return await tcs.Task.ConfigureAwait(false);
    }

    public void Dispose()
    {
        _queue.CompleteAdding();
    }
}
