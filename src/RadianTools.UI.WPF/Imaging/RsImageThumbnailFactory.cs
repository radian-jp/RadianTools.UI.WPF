using RadianTools.UI.WPF.Logging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static RadianTools.UI.WPF.Imaging.RsImage;

namespace RadianTools.UI.WPF.Imaging;

public sealed class RsImageThumbnailFactory : IThumbnailFactory
{
    public bool CanCreate(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".tif" or ".tiff" or ".webp" or ".avif" or ".dds";
    }

    public async Task<ImageSource?> CreateAsync(string filePath, System.Windows.Size thumbnailSize, CancellationToken cancellationToken)
    {
        Logger.Shared.Debug($"{nameof(WpfThumbnailFactory)}.{nameof(CreateAsync)} start: {filePath}");

        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(filePath))
        {
            Logger.Shared.Debug($"File not found: {filePath}");
            return null;
        }

        var bytes = File.ReadAllBytes(filePath);
        return await CreateAsync(bytes, thumbnailSize, cancellationToken);
    }

    public Task<ImageSource?> CreateAsync(byte[] bytes, System.Windows.Size thumbnailSize, CancellationToken cancellationToken)
    {
        Logger.Shared.Debug($"{nameof(WpfThumbnailFactory)}.{nameof(CreateAsync)} start: Length={bytes.Length}");

        return Task.Run<ImageSource?>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var decoded = DecodeImageWithRsImage(bytes, thumbnailSize);
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

    public ImageSource? Create(string filePath, System.Windows.Size thumbnailSize)
    {
        Logger.Shared.Debug($"{nameof(WpfThumbnailFactory)}.{nameof(CreateAsync)} start: {filePath}");

        if (!File.Exists(filePath))
        {
            Logger.Shared.Debug($"File not found: {filePath}");
            return null;
        }

        var bytes = File.ReadAllBytes(filePath);

        return Create(bytes, thumbnailSize);
    }

    public ImageSource? Create(byte[] bytes, System.Windows.Size thumbnailSize)
    {
        Logger.Shared.Debug($"{nameof(WpfThumbnailFactory)}.{nameof(CreateAsync)} start: Length={bytes.Length}");

        try
        {
            var decoded = DecodeImageWithRsImage(bytes, thumbnailSize);
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

    private static RSIDecodedImage? DecodeImageWithRsImage(byte[] bytes, System.Windows.Size thumbnailSize)
    {
        var decoded = new RSIDecodedImage();
        unsafe
        {
            fixed(byte* pImage = &bytes[0])
            {
                var result = rsimage_generic_decode_resize_memory(
                    image_data: (IntPtr)pImage,
                    image_data_size: (uint)bytes.Length,
                    (uint)thumbnailSize.Width,
                    (uint)thumbnailSize.Height,
                    RSIResizeFilter.Triangle,
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
}