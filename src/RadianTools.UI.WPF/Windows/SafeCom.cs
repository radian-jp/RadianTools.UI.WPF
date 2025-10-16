using System.Runtime.InteropServices;

namespace RadianTools.UI.WPF;

public class SafeCom
{
    public static SafeCom<T> Create<T>(T instance) where T : class
        => new SafeCom<T>(instance);
}

public class SafeCom<T> : IDisposable where T : class
{
    protected T Instance => _Instance;

    private T _Instance;

    public SafeCom(T instance)
        => _Instance = instance;

    public void Dispose()
    {
        var value = Interlocked.Exchange(ref _Instance!, null);
        if (value != null)
            Marshal.ReleaseComObject(value);
    }
}

public struct CoTaskMemArray<T> : IDisposable where T : unmanaged
{
    private IntPtr _value;
    public int Count { get; }

    public IntPtr Value => _value;

    public CoTaskMemArray(IntPtr value, int count)
    {
        _value = value;
        Count = count;
    }

    public Span<T> AsSpan()
    {
        var ptr = _value;
        if (ptr == IntPtr.Zero || Count == 0) 
            return Span<T>.Empty;

        unsafe
        {
            return new Span<T>((void*)ptr, Count);
        }
    }

    public T[] ToArray() => AsSpan().ToArray();

    public void Dispose()
    {
        var ptr = Interlocked.Exchange(ref _value, IntPtr.Zero);
        if (ptr != IntPtr.Zero)
            Marshal.FreeCoTaskMem(ptr);
    }
}
