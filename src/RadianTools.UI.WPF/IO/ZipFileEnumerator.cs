namespace RadianTools.UI.WPF.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

public sealed class ZipFileEnumerator : IFileEnumerator
{
    private readonly string _zipPath;
    private readonly string _innerFolder;
    private readonly Encoding _encoding;

    public ZipFileEnumerator(string zipPath, string innerFolder)
    {
        _zipPath = zipPath;
        _innerFolder = Normalize(innerFolder);
        _encoding = ZipEntryNameEncodingSelector.GetEncoding(zipPath);
    }

    public IEnumerable<IFileEntry> Enumerate()
    {
        if (!File.Exists(_zipPath))
            yield break;

        using var fs = File.OpenRead(_zipPath);
        using var archive = new ZipArchive(fs, ZipArchiveMode.Read, leaveOpen: false, _encoding);

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))
                continue;

            if (!IsTarget(entry.FullName))
                continue;

            yield return new ZipFileEntry(_zipPath, entry.FullName, _encoding);
        }
    }

    private bool IsTarget(string fullName)
    {
        var normalized = Normalize(fullName);

        if (string.IsNullOrEmpty(_innerFolder))
        {
            return normalized.IndexOf('/') < 0;
        }

        if (!normalized.StartsWith(_innerFolder + "/", StringComparison.OrdinalIgnoreCase))
            return false;

        // innerFolder/ を除いた残り
        var relative = normalized.Substring(_innerFolder.Length + 1);

        // さらに '/' があればサブフォルダ配下
        return relative.IndexOf('/') < 0;
    }

    private static string Normalize(string path)
        => path.Replace('\\', '/').Trim('/');
}
