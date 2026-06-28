namespace RadianTools.UI.WPF.IO;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

public enum FileSourceKind
{
    Directory,
    ZipArchive,
    ZipArchiveDirectory
}

public interface IFileEntry
{
    string DisplayName { get; }
    string LogicalPath { get; }
    Task<Stream> OpenReadAsync(CancellationToken token = default);
}

public static class FileEntryExtensions
{
    public static async Task<byte[]> ReadAllBytesAsync(
        this IFileEntry entry,
        CancellationToken token = default)
    {
        await using var stream = await entry.OpenReadAsync(token).ConfigureAwait(false);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, token).ConfigureAwait(false);
        return ms.ToArray();
    }
}