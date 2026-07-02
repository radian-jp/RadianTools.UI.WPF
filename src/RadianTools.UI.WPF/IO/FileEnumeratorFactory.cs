namespace RadianTools.UI.WPF.IO;

using System;

public static class FileEnumeratorFactory
{
    private class EmptyFileEnumerator : IFileEnumerator
    {
        public IEnumerable<IFileEntry> Enumerate()
            => Array.Empty<IFileEntry>();
    }

    private static EmptyFileEnumerator _emptyEnumerator = new ();

    public static IFileEnumerator Create(string? folder)
    {
        if (folder == null)
            return _emptyEnumerator;

        var spec = FileSourceFactory.Create(folder);

        return spec.Kind switch
        {
            FileSourceKind.Directory => new DirectoryFileEnumerator(spec.RootPath),
            FileSourceKind.ZipArchive => new ZipFileEnumerator(spec.RootPath, ""),
            FileSourceKind.ZipArchiveDirectory => new ZipFileEnumerator(spec.RootPath, spec.InnerPath ?? ""),
            _ => throw new NotSupportedException()
        };
    }
}