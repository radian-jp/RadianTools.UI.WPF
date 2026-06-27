using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RadianTools.UI.WPF.Imaging;

public interface IThumbnailFactory
{
    bool CanCreate(string filePath);
    Task<ImageSource?> CreateAsync(string filePath, Size thumbnailSize, CancellationToken cancellationToken);
    Task<ImageSource?> CreateAsync(byte[] image, Size thumbnailSize, CancellationToken cancellationToken);
    ImageSource? Create(string filePath, Size thumbnailSize);
    ImageSource? Create(byte[] image, Size thumbnailSize);
}
