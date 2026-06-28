namespace RadianTools.UI.WPF.IO;

using System;

public static class FileEnumeratorFactory
{
    public static IFileEnumerator Create(string folder)
    {
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