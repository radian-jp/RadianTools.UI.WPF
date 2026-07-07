namespace RadianTools.UI.WPF.Imaging;

using System.Buffers.Binary;

public enum ImageFormat
{
    Unknown,
    Gif,
    Jpeg,
    Png,
    Bmp,
    Webp,
    Avif,
    Tiff,
    Dds
}

public static class ImageFormatDetector
{
    public static ImageFormat GetFormat(ReadOnlySpan<byte> header)
    {
        if (IsJpeg(header))
            return ImageFormat.Jpeg;

        if (IsPng(header))
            return ImageFormat.Png;

        if (IsGif(header))
            return ImageFormat.Gif;

        if (IsBmp(header))
            return ImageFormat.Bmp;

        if (IsWebp(header))
            return ImageFormat.Webp;

        if (IsAvif(header))
            return ImageFormat.Avif;

        if (IsTiff(header))
            return ImageFormat.Tiff;

        if (IsDds(header))
            return ImageFormat.Dds;

        return ImageFormat.Unknown;
    }

    public static bool IsGif(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            return false;

        const uint Magic = 0x38464947; // GIF8

        return BinaryPrimitives.ReadUInt32LittleEndian(data)
            == Magic;
    }

    public static bool IsJpeg(ReadOnlySpan<byte> data)
    {
        if (data.Length < 3)
            return false;

        const uint Magic = 0xFFD8FF;

        uint value =
            BinaryPrimitives.ReadUInt32LittleEndian(data);

        return (value & 0x00FFFFFF) == Magic;
    }

    public static bool IsPng(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
            return false;

        const ulong Magic = 0x0A1A0A0D474E5089;

        return BinaryPrimitives.ReadUInt64LittleEndian(data) == Magic;
    }

    public static bool IsBmp(ReadOnlySpan<byte> data)
    {
        if (data.Length < 2)
            return false;

        const ushort Magic = 0x4D42; // BM

        return BinaryPrimitives.ReadUInt16LittleEndian(data) == Magic;
    }

    public static bool IsWebp(ReadOnlySpan<byte> data)
    {
        if (data.Length < 12)
            return false;

        const uint Riff = 0x46464952; // RIFF
        const uint Webp = 0x50424557; // WEBP

        return BinaryPrimitives.ReadUInt32LittleEndian(data) == Riff
            && BinaryPrimitives.ReadUInt32LittleEndian(data[8..]) == Webp;
    }

    public static bool IsAvif(ReadOnlySpan<byte> data)
    {
        if (data.Length < 12)
            return false;

        const uint Ftyp = 0x70797466; // ftyp
        const uint Avif = 0x66697661; // avif

        return BinaryPrimitives.ReadUInt32LittleEndian(data[4..]) == Ftyp
            && BinaryPrimitives.ReadUInt32LittleEndian(data[8..]) == Avif;
    }

    public static bool IsTiff(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            return false;
        
        const uint Little = 0x002A4949; // II*\0        
        const uint Big = 0x2A004D4D; // MM\0*

        uint value = BinaryPrimitives.ReadUInt32LittleEndian(data);
        return value == Little ||
               value == Big;
    }

    public static bool IsDds(ReadOnlySpan<byte> data)
    {
        if (data.Length < 4)
            return false;

        const uint Magic = 0x20534444; // "DDS "
        return BinaryPrimitives.ReadUInt32LittleEndian(data) == Magic;
    }
}