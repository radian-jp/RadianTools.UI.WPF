namespace RadianTools.UI.WPF.IO;

using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class ZipFileEntry : IFileEntry
{
    private readonly string _zipPath;
    private readonly string _entryFullName;
    private readonly Encoding _encoding;

    public string DisplayName { get; }
    public string LogicalPath => $"{_zipPath}/{_entryFullName}";

    public ZipFileEntry(string zipPath, string entryFullName, Encoding encoding)
    {
        _zipPath = zipPath;
        _entryFullName = entryFullName;
        _encoding = encoding;
        DisplayName = Path.GetFileName(entryFullName);
    }

    public Task<Stream> OpenReadAsync(CancellationToken token = default)
    {
        var archive = new ZipArchive(File.OpenRead(_zipPath), ZipArchiveMode.Read, leaveOpen: false, _encoding);
        var entry = archive.GetEntry(_entryFullName)
            ?? throw new FileNotFoundException(_entryFullName);

        Stream baseStream = entry.Open();
        return Task.FromResult<Stream>(new ZipEntryStreamWrapper(baseStream, archive));
    }
}