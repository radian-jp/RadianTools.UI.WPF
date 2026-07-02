namespace RadianTools.UI.WPF.ViewModels;

using RadianTools.UI.WPF.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using RadianTools.UI.WPF.IO;

public class ThumbnailItemViewModel : INotifyPropertyChanged
{
    private ImageSource? _thumbnail;

    public IFileEntry? FileEntry { get; set; }
    public string DisplayName => FileEntry?.DisplayName ?? string.Empty;

    public ImageSource? Thumbnail
    {
        get => _thumbnail;
        set
        {
            if (_thumbnail == value)
                return;

            _thumbnail = value;
            Logger.Shared.Debug($"Thumbnail changed: {DisplayName}");
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}