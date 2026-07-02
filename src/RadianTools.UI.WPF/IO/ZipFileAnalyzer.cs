namespace RadianTools.UI.WPF.IO;

using System.Buffers.Binary;
using System.IO;

public readonly record struct ZipFileInfo(
    bool IsValid,
    bool HasUtf8Flag);

public static class ZipFileAnalyzer
{
    private const uint LocalFileHeaderSignature = 0x04034B50;

    public static ZipFileInfo Analyze(string zipPath)
    {
        if( !File.Exists(zipPath) )
        {
            return new ZipFileInfo(
                IsValid: false,
                HasUtf8Flag: false);
        }

        using var fs = File.OpenRead(zipPath);
        Span<byte> header = stackalloc byte[30];

        var isValid = false;
        var hasUtf8Flag = false;
        do
        {
            if (fs.Length < header.Length)
                break;

            fs.ReadExactly(header);
            if (BinaryPrimitives.ReadUInt32LittleEndian(header) != LocalFileHeaderSignature)
                break;

            isValid = true;
            ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(6));
            if ((flags & 0x0800) != 0)
                hasUtf8Flag = true;

        } while (false);

        return new ZipFileInfo(
            IsValid: isValid,
            HasUtf8Flag: hasUtf8Flag);
    }
}