using RadianTools.UI.WPF.Logging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static RadianTools.UI.WPF.Imaging.RsImage;

namespace RadianTools.UI.WPF.Imaging;

public sealed class RsImageFactory : IImageFactory
{
    public static RsImageFactory Shared { get; } = new RsImageFactory();

    public bool CanCreate(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".tif" or ".tiff" or ".webp" or ".avif" or ".dds";
    }

    public Task<ImageSource?> CreateThumbnailAsync(byte[] image, System.Windows.Size thumbnailSize, CancellationToken cancellationToken)
    {
        Logger.Shared.Debug($"{nameof(WpfImageFactory)}.{nameof(CreateThumbnailAsync)} start: Length={image.Length}");

        return Task.Run<ImageSource?>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var decoded = DecodeImageWithResize(image, thumbnailSize);
                if (decoded == null)
                    return null;

                try
                {
                    var bitmap = CreateBitmapSourceFromRsImage(decoded.Value);
                    if (bitmap.CanFreeze)
                        bitmap.Freeze();

                    return bitmap;
                }
                finally
                {
                    rsimage_free(decoded.Value.image_data);
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logger.Shared.Debug(ex.ToString());
                return null;
            }
        }, cancellationToken);
    }

    public ImageSource? CreateThumbnail(byte[] image, System.Windows.Size thumbnailSize)
    {
        Logger.Shared.Debug($"{nameof(WpfImageFactory)}.{nameof(CreateThumbnail)} start: Length={image.Length}");

        try
        {
            var decoded = DecodeImageWithResize(image, thumbnailSize);
            if (decoded == null)
                return null;

            try
            {
                var bitmap = CreateBitmapSourceFromRsImage(decoded.Value);
                if (bitmap.CanFreeze)
                    bitmap.Freeze();

                return bitmap;
            }
            finally
            {
                rsimage_free(decoded.Value.image_data);
            }
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Logger.Shared.Debug(ex.ToString());
            return null;
        }
    }

    private static RSIDecodedImage? DecodeImageWithResize(byte[] image, System.Windows.Size size)
    {
        var decoded = new RSIDecodedImage();
        unsafe
        {
            fixed(byte* pImage = &image[0])
            {
                var result = rsimage_generic_decode_resize_memory(
                    image_data: (IntPtr)pImage,
                    image_data_size: (uint)image.Length,
                    (uint)size.Width,
                    (uint)size.Height,
                    RSIResizeFilter.Triangle,
                    format: RSIPixelFormat.BGRA, 
                    allocator: IntPtr.Zero, 
                    output: ref decoded
                );

                if (result != RSIDecodeResult.Ok)
                {
                    Logger.Shared.Debug($"RsImage decode error: {result}");
                    return null;
                }
            }
        }

        return decoded;
    }


    private static RSIDecodedImage? DecodeImage(byte[] image)
    {
        var decoded = new RSIDecodedImage();
        unsafe
        {
            fixed (byte* pImage = &image[0])
            {
                var result = rsimage_generic_decode_memory(
                    image_data: (IntPtr)pImage,
                    image_data_size: (uint)image.Length,
                    format: RSIPixelFormat.BGRA,
                    allocator: IntPtr.Zero,      // rsimage_alloc を使う
                    output: ref decoded
                );

                if (result != RSIDecodeResult.Ok)
                {
                    Logger.Shared.Debug($"RsImage decode error: {result}");
                    return null;
                }
            }
        }

        return decoded;
    }

    private static BitmapSource CreateBitmapSourceFromRsImage(RSIDecodedImage decoded)
    {
        var srcW = decoded.width;
        var srcH = decoded.height;

        if (srcW <= 0 || srcH <= 0)
            throw new InvalidOperationException("Invalid image size from ");

        // ストレイド（1 行のバイト数）: 32bit なので 4 バイト/ピクセル
        int rawStride = (int)srcW * 4;

        BitmapSource frame = BitmapSource.Create(
            (int)srcW,
            (int)srcH,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            decoded.image_data,
            (int)decoded.size,
            rawStride
        );

        return frame;
    }

    public Task<ImageSource?> CreateImageAsync(byte[] image, CancellationToken cancellationToken)
    {
        Logger.Shared.Debug($"{nameof(WpfImageFactory)}.{nameof(CreateThumbnailAsync)} start: Length={image.Length}");

        return Task.Run<ImageSource?>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var decoded = DecodeImage(image);
                if (decoded == null)
                    return null;

                try
                {
                    var bitmap = CreateBitmapSourceFromRsImage(decoded.Value);
                    if (bitmap.CanFreeze)
                        bitmap.Freeze();

                    return bitmap;
                }
                finally
                {
                    rsimage_free(decoded.Value.image_data);
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logger.Shared.Debug(ex.ToString());
                return null;
            }
        }, cancellationToken);
    }

    public ImageSource? CreateImage(byte[] image)
    {
        Logger.Shared.Debug($"{nameof(WpfImageFactory)}.{nameof(CreateImage)} start: Length={image.Length}");

        try
        {
            var decoded = DecodeImage(image);
            if (decoded == null)
                return null;

            try
            {
                var bitmap = CreateBitmapSourceFromRsImage(decoded.Value);
                if (bitmap.CanFreeze)
                    bitmap.Freeze();

                return bitmap;
            }
            finally
            {
                rsimage_free(decoded.Value.image_data);
            }
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            Logger.Shared.Debug(ex.ToString());
            return null;
        }
    }
}