using RadianTools.UI.WPF.Logging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RadianTools.UI.WPF.Imaging;

public sealed class WpfImageFactory : IImageFactory
{
    public bool CanCreate(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".tif" or ".tiff";
    }

    public Task<ImageSource?> CreateThumbnailAsync(byte[] bytes, Size thumbnailSize, CancellationToken cancellationToken)
    {
        Logger.Shared.Debug($"{nameof(WpfImageFactory)}.{nameof(CreateThumbnailAsync)} start: Length={bytes.Length}");

        return Task.Run<ImageSource?>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var stream = new MemoryStream(bytes);
                var decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.DelayCreation,
                    BitmapCacheOption.None);

                var frame = decoder.Frames[0];

                int srcW = frame.PixelWidth;
                int srcH = frame.PixelHeight;

                int targetW = (int)thumbnailSize.Width;
                int targetH = (int)thumbnailSize.Height;

                double scale = Math.Min(
                    (double)targetW / srcW,
                    (double)targetH / srcH);

                int decodeWidth = Math.Max(1, (int)(srcW * scale));

                using var stream2 = new MemoryStream(bytes);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = decodeWidth;
                bitmap.StreamSource = stream2;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
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

    public ImageSource? CreateThumbnail(byte[] bytes, Size thumbnailSize)
    {
        Logger.Shared.Debug($"{nameof(WpfImageFactory)}.{nameof(CreateThumbnailAsync)} start: Length={bytes.Length}");

        try
        {
            using var stream = new MemoryStream(bytes);
            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.DelayCreation,
                BitmapCacheOption.None);

            var frame = decoder.Frames[0];

            int srcW = frame.PixelWidth;
            int srcH = frame.PixelHeight;

            int targetW = (int)thumbnailSize.Width;
            int targetH = (int)thumbnailSize.Height;

            double scale = Math.Min(
                (double)targetW / srcW,
                (double)targetH / srcH);

            int decodeWidth = Math.Max(1, (int)(srcW * scale));

            using var stream2 = new MemoryStream(bytes);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = decodeWidth;
            bitmap.StreamSource = stream2;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        catch (Exception ex)
        {
            Logger.Shared.Debug(ex.ToString());
            return null;
        }
    }

    public Task<ImageSource?> CreateImageAsync(byte[] image, CancellationToken cancellationToken)
    {
        Logger.Shared.Debug($"{nameof(WpfImageFactory)}.{nameof(CreateThumbnailAsync)} start: Length={image.Length}");

        return Task.Run<ImageSource?>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var stream = new MemoryStream(image);
                var decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.DelayCreation,
                    BitmapCacheOption.None);

                var frame = decoder.Frames[0];
                return frame;
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
        Logger.Shared.Debug($"{nameof(WpfImageFactory)}.{nameof(CreateThumbnailAsync)} start: Length={image.Length}");

        try
        {
            using var stream = new MemoryStream(image);
            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.DelayCreation,
                BitmapCacheOption.None);

            var frame = decoder.Frames[0];
            return frame;
        }
        catch (Exception ex)
        {
            Logger.Shared.Debug(ex.ToString());
            return null;
        }
    }
}