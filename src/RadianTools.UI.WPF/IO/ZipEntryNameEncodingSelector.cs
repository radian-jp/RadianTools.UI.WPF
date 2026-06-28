namespace RadianTools.UI.WPF.IO;

using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading;

public static class ZipEntryNameEncodingSelector
{
    private static int _registered;

    public static Encoding DefaultEncoding { get; set; } = Encoding.GetEncoding(932);

    private static void EnsureCodePagesRegistered()
    {
        if (Interlocked.Exchange(ref _registered, 1) == 1)
            return;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static Encoding GetEncoding(string zipPath)
    {
        EnsureCodePagesRegistered();

        return HasUtf8Flag(zipPath)
            ? Encoding.UTF8
            : DefaultEncoding;
    }

    private const uint LocalFileHeaderSignature = 0x04034b50;

    public static bool HasUtf8Flag(string zipPath)
    {
        using var fs = File.OpenRead(zipPath);

        Span<byte> header = stackalloc byte[30];

        while (fs.Position + header.Length <= fs.Length)
        {
            fs.ReadExactly(header);

            // Local File Header signature確認
            if (BinaryPrimitives.ReadUInt32LittleEndian(header) != LocalFileHeaderSignature)
                break;

            ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(6));

            // UTF-8フラグが1つでもあればUTF-8扱い
            if ((flags & 0x0800) != 0)
                return true;

            ushort fileNameLength = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(26));
            ushort extraFieldLength = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(28));
            uint compressedSize = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(18));

            long skip =
                (long)fileNameLength +
                extraFieldLength +
                compressedSize;

            fs.Position += skip;
        }

        return false;
    }
}