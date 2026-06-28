using RadianTools.UI.WPF.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace RadianTools.UI.WPF.ViewModels;

public class ThumbnailItemViewModel : INotifyPropertyChanged
{
    private ImageSource? _thumbnail;

    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";

    public ImageSource? Thumbnail
    {
        get => _thumbnail;
        set
        {
            if (_thumbnail == value) return;
            _thumbnail = value;
            Logger.Shared.Debug($"Thumbnail changed: {FileName}, Value={value != null}");
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}