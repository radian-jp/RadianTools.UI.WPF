using System;
using System.Runtime.InteropServices;

namespace RadianTools.UI.WPF.Imaging;

internal static partial class RsImage
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RSIDecodedImage
    {
        public uint width;
        public uint height;
        public ulong size;
        public IntPtr image_data;
    }

    public enum RSIDecodeResult : int
    {
        Ok = 0,
        InvalidInput = -1,
        HeaderParseFailure = -2,
        FrameDecodeFailure = -3,
        AllocationFailure = -4,
    }

    public enum RSIResizeFilter : int
    {
        /// <summary>最近傍補間</summary>
        Nearest = 0,

        /// <summary>双一次補間 (バイリニア)</summary>
        Triangle = 1,

        /// <summary>双三次補間 (Catmull-Rom)</summary>
        CatmullRom = 2,

        /// <summary>ガウス補間</summary>
        Gaussian = 3,

        /// <summary>ランツォス補間 (ウィンドウ 3)</summary>
        Lanczos3 = 4,
    }

    public enum RSIPixelFormat : int
    {
        RGBA = 0,
        BGRA = 1,
    }

    [LibraryImport("rsimage.dll")]
    public static partial IntPtr rsimage_alloc(ulong size);

    // rsimage_free
    [LibraryImport("rsimage.dll")]
    public static partial void rsimage_free(IntPtr image_data);

    [LibraryImport("rsimage.dll")]
    public static partial RSIDecodeResult rsimage_generic_decode_memory(
        IntPtr image_data,
        uint image_data_size,
        RSIPixelFormat format,
        IntPtr allocator,
        ref RSIDecodedImage output
    );

    [LibraryImport("rsimage.dll")]
    public static partial RSIDecodeResult rsimage_generic_decode_resize_memory(
        IntPtr image_data,
        uint image_data_size,
        uint image_width,
        uint image_height,
        RSIResizeFilter filter,
        RSIPixelFormat format,
        IntPtr allocator,
        ref RSIDecodedImage output
    );

    [LibraryImport("rsimage.dll")]
    public static partial RSIDecodeResult rsimage_png_decode_memory(
        IntPtr png_data,
        uint png_data_size,
        RSIPixelFormat format,
        IntPtr allocator,
        ref RSIDecodedImage output
    );
}
