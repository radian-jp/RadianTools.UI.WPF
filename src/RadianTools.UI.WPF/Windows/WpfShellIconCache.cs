using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RadianTools.Interop.Windows;
using RadianTools.Interop.Windows.Utility;

namespace RadianTools.UI.WPF;

/// <summary>
/// WPFシェルアイコンキャッシュクラス。
/// </summary>
internal class WpfShellIconCache
{
    private ShellImageList _shellImageList;

    /// <summary>
    /// 指定された画像解像度サイズでキャッシュを初期化します。
    /// </summary>
    /// <param name="shil">アイコンのサイズ指定（SHIL.SMALLなど）。</param>
    public WpfShellIconCache(SHIL shil)
    {
        _shellImageList = new ShellImageList(shil, WpfBitmapConverter);
    }

    /// <summary>
    /// アイコンを取得する
    /// </summary>
    /// <param name="index">ShellImageListのアイコンのインデックス。</param>
    /// <returns>アイコンオブジェクト。(WriteableBitmap)</returns>
    public object GetIcon(int index) => _shellImageList.GetIcon(index);

    /// <summary>
    /// ネイティブのアイコンポインタからWPFの <see cref="WriteableBitmap"/> を生成します。
    /// </summary>
    /// <param name="width">アイコンの幅。</param>
    /// <param name="height">アイコンの高さ。</param>
    /// <param name="stride">ビットマップのストライド。</param>
    /// <param name="src">ネイティブ画像データのポインタ。</param>
    /// <returns>変換されたWPF用ビットマップオブジェクト。</returns>
    private static object WpfBitmapConverter(int width, int height, int stride, IntPtr src)
    {
        // 96 DPIでWriteableBitmapを初期化し、メモリ上のピクセルデータを書き込む
        var bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        var rect = new Int32Rect(0, 0, width, height);

        // ネイティブメモリからWPFのビットマップ領域へピクセルデータをコピー
        bmp.WritePixels(rect, src, stride * height, stride);

        return bmp;
    }
}