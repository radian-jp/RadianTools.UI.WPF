using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RadianTools.Interop.Windows;

namespace RadianTools.UI.WPF;

internal class WindowsFolderIconCache : ShellImageList
{
    private static object WPFBitmapConverter(int width, int height, int stride, IntPtr src)
    {
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null) ;
        var rect = new Int32Rect(0, 0, width, height);
        bmp.WritePixels(rect, src, stride * height, stride);
        return bmp;
    }

    public WindowsFolderIconCache(SHIL shil) : base(shil, WPFBitmapConverter)
    {
    }
}