namespace RadianTools.UI.WPF.IO;

using System.IO;

public sealed class DirectoryFileEntry : IFileEntry
{
    private readonly string _filePath;

    public string DisplayName { get; }
    public string LogicalPath => _filePath;

    public DirectoryFileEntry(string filePath)
    {
        _filePath = filePath;
        DisplayName = Path.GetFileName(filePath);
    }

    public Task<Stream> OpenReadAsync(CancellationToken token = default)
    {
        Stream stream = File.OpenRead(_filePath);
        return Task.FromResult(stream);
    }
}