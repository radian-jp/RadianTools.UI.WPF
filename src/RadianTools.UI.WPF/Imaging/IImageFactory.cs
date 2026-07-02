namespace RadianTools.UI.WPF.Imaging;

using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

public interface IImageFactory
{
    bool CanCreate(string filePath);
    Task<ImageSource?> CreateThumbnailAsync(byte[] image, Size thumbnailSize, CancellationToken cancellationToken);
    Task<ImageSource?> CreateImageAsync(byte[] image, CancellationToken cancellationToken);
    ImageSource? CreateThumbnail(byte[] image, Size thumbnailSize);
    ImageSource? CreateImage(byte[] image);
}
