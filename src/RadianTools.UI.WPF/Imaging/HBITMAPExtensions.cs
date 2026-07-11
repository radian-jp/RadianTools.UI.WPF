namespace RadianTools.UI.WPF.Imaging;

using RadianTools.Interop.Windows;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

public static class HBITMAPExtensions
{
    /// <summary>
    /// HBITMAPをBitmapSourceに変換する。（元のHBITMAPは削除される）
    /// </summary>
    /// <param name="hBmp">元になるHBITMAP</param>
    /// <returns>BitmapSourceオブジェクト</returns>
    public static BitmapSource? AsBitmapSource(this HBITMAP hBmp)
    {
        if (hBmp == HBITMAP.Null)
            return null;

        try
        {
            var source = Imaging.CreateBitmapSourceFromHBitmap(
                hBmp,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            source.Freeze();
            return source;
        }
        finally
        {
            Gdi32.DeleteObject(hBmp);
        }
    }
}
