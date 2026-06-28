namespace RadianTools.UI.WPF.IO;

using System;

public sealed record FileSourceSpec(
    FileSourceKind Kind,
    string RootPath,
    string? InnerPath);

public static class FileSourceFactory
{
    public static FileSourceSpec Create(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
            throw new ArgumentException("Folder is empty.", nameof(folder));

        var normalized = folder.Replace('\\', '/');

        var zipIndex = normalized.IndexOf(".zip/", StringComparison.OrdinalIgnoreCase);
        if (zipIndex >= 0)
        {
            var zipPath = normalized[..(zipIndex + 4)].Replace('/', '\\');
            var inner = normalized[(zipIndex + 5)..].Trim('/');
            return new FileSourceSpec(FileSourceKind.ZipArchiveDirectory, zipPath, inner);
        }

        if (normalized.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return new FileSourceSpec(FileSourceKind.ZipArchive, folder, null);

        return new FileSourceSpec(FileSourceKind.Directory, folder, null);
    }
}