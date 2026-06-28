namespace RadianTools.UI.WPF.IO;

using System.Collections.Generic;
using System.IO;

public sealed class DirectoryFileEnumerator : IFileEnumerator
{
    private readonly string _folder;

    public DirectoryFileEnumerator(string folder)
    {
        _folder = folder;
    }

    public IEnumerable<IFileEntry> Enumerate()
    {
        if (!Directory.Exists(_folder))
            yield break;

        foreach (var file in Directory.EnumerateFiles(_folder))
            yield return new DirectoryFileEntry(file);
    }
}