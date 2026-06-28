namespace RadianTools.UI.WPF.IO;

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

public sealed class ZipEntryStreamWrapper : Stream
{
    private readonly Stream _inner;
    private readonly ZipArchive _archive;
    private bool _disposed;

    public ZipEntryStreamWrapper(Stream inner, ZipArchive archive)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _archive = archive ?? throw new ArgumentNullException(nameof(archive));
    }

    public override bool CanRead => !_disposed && _inner.CanRead;
    public override bool CanSeek => !_disposed && _inner.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        await _inner.DisposeAsync().ConfigureAwait(false);
        _archive.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;
        if (disposing)
        {
            _inner.Dispose();
            _archive.Dispose();
        }
        base.Dispose(disposing);
    }
}