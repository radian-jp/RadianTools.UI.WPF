using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RadianTools.UI.WPF;

internal class ShellImageList
{
    public delegate object Converter(int width, int height, int stride, nint src);

    private readonly Dictionary<int, object> _cache = new();

    private IImageList ImageList { get; }
    public nint Pointer { get; }

    private Converter _converter;

    public ShellImageList(SHIL shil, Converter converter)
    {
        ImageList = GetSysImageList((uint)shil);
        Pointer = Marshal.GetComInterfaceForObject<IImageList, IImageList>(ImageList);
        _converter = converter;
    }

    public object GetIcon(int iconIndex)
    {
        if (_cache.TryGetValue(iconIndex, out var image))
            return image;

        if (ImageList.GetIcon(iconIndex, 0, out var hIcon).IsNotOK)
            throw new Win32Exception($"{nameof(IImageList)}.{nameof(IImageList.GetIcon)} failed.");

        var bmp = IconToBitmapWithAlpha(hIcon);
        hIcon.Dispose();
        var bm = bmp.LockBits(
            new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);
        var t = _converter(bm.Width, bm.Height, bm.Stride, bm.Scan0);
        bmp.UnlockBits(bm);
        _cache[iconIndex] = t;
        return t;
    }

    private static IImageList GetSysImageList(uint shil)
    {
        var iid = typeof(IImageList).GUID;
        Shell32.SHGetImageList((int)shil, in iid, out var oImageList).ThrowOnFailure();
        return (IImageList)oImageList;
    }

    public static System.Drawing.Bitmap IconToBitmapWithAlpha(nint hIcon)
    {
        using var icon = System.Drawing.Icon.FromHandle(hIcon);
        var bmp = new System.Drawing.Bitmap(
            icon.Width, icon.Height,
            PixelFormat.Format32bppArgb);

        using (var g = System.Drawing.Graphics.FromImage(bmp))
        {
            g.Clear(System.Drawing.Color.Transparent);
            g.DrawIcon(icon, 0, 0);
        }
        return bmp;
    }

}