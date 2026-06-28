using DependencyPropertyGenerator;
using RadianTools.UI.WPF.Imaging;
using RadianTools.UI.WPF.IO;
using RadianTools.UI.WPF.Logging;
using RadianTools.UI.WPF.Storage;
using RadianTools.UI.WPF.Threading;
using RadianTools.UI.WPF.ViewModels;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace RadianTools.UI.WPF.Controls;

/// <summary>
/// 画像やサムネイルをリスト表示するための ListView コントロール。
/// 可視範囲内のアイテムのみをロードする最適化処理を含んでいる。
/// </summary>
[DependencyProperty<double>("ThumbnailWidth", DefaultValue = 120.0)]
[DependencyProperty<double>("ThumbnailHeight", DefaultValue = 120.0)]
[DependencyProperty<double>("TextHeight", DefaultValue = 35.0)]
[DependencyProperty<string>("Folder")]
[DependencyProperty<int>("LoadThreads", DefaultValueExpression = "System.Environment.ProcessorCount / 2")]
public partial class ThumbnailListView : UserControl
{
    private ObservableCollection<ThumbnailItemViewModel> _items = new();

    /// <summary>リスト内の ScrollViewer を保持。</summary>
    private ScrollViewer? _scrollViewer;

    /// <summary>初回ロードが完了したかどうか。</summary>
    private bool _initialLoadDone;

    /// <summary>ロード処理に使用する CancellationTokenSource。</summary>
    private CancellationTokenSource? _loadCts;

    /// <summary>ロード処理のバージョン番号（キャンセル判定用）。</summary>
    private int _loadVersion;

    /// <summary>並列ロード数を制御する Semaphore。</summary>
    private SemaphoreSlim _loadSemaphore;

    /// <summary>スレッド数毎セマフォのキャッシュ。（大した数にはならないのでDisposeしない設計）</summary>
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> _semaphoreCache = new();

    /// <summary>Dispose 済みかどうか。</summary>
    private bool _disposed;

    /// <summary>サムネイル生成ファクトリ。</summary>
    private readonly IThumbnailFactory _thumbnailFactory = new RsImageThumbnailFactory();

    /// <summary>スクロール停止後タイマー</summary>
    private readonly DispatcherTimer _scrollStopTimer =
        new() { Interval = TimeSpan.FromMilliseconds(100) };

    /// <summary>
    /// スレッド数変更時
    /// </summary>
    /// <param name="oldValue">以前の値</param>
    /// <param name="newValue">新しい値</param>
    partial void OnLoadThreadsChanged(int oldValue, int newValue)
    {
        if (oldValue == newValue)
            return;

        UpdateSemaphore();
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ThumbnailListView()
    {
        InitializeComponent();

        _loadSemaphore = GetCurrentSemaphore();

        PART_ListBox.ItemsSource = _items;

        Loaded += ThumbnailListView_Loaded;
        Unloaded += ThumbnailListView_Unloaded;

        _scrollStopTimer.Tick += OnScrollStopped;
    }

    /// <summary>
    /// ViewModel の Folder 変更イベントハンドラ
    /// </summary>
    partial void OnFolderChanged(string? oldValue, string? newValue)
    {
        if (oldValue == newValue)
            return;

        Logger.Shared.Debug($"OnFolderChanged oldValue:{oldValue} newValue:{newValue}");
        _ = ReloadFolderAsync();
    }

    /// <summary>
    /// スクロール停止後処理
    /// </summary>
    private async void OnScrollStopped(object? sender, EventArgs e)
    {
        _scrollStopTimer.Stop();
        try
        {
            await LoadVisibleItemsAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// フォルダ再読み込み
    /// </summary>
    private async Task ReloadFolderAsync()
    {
        CancelLoading(null);
        UpdateSemaphore();
        ResetScrollToTop();
        LoadFolder();

        if (_items.Count == 0)
            return;

        await LoadVisibleItemsAsync();
    }

    /// <summary>
    /// スクロールを最上部に移動
    /// </summary>
    private void ResetScrollToTop()
    {
        if (_scrollViewer == null)
            return;

        _scrollViewer.ScrollToVerticalOffset(0);
    }

    /// <summary>
    /// ロード後イベント
    /// </summary>
    private async void ThumbnailListView_Loaded(object sender, RoutedEventArgs e)
    {
        // ListBox を取得
        var listBox = PART_ListBox;

        // ScrollViewer を検出（テンプレート内または親ツリー内）
        _scrollViewer = listBox.Template.FindName("PART_ScrollViewer", listBox) as ScrollViewer;
        _scrollViewer ??= FindScrollViewerInParent(this);

        Logger.Shared.Debug($"ScrollViewer found: {_scrollViewer != null}");
        if (_scrollViewer != null)
        {
            Logger.Shared.Debug(
                $"Initial: Offset={_scrollViewer.VerticalOffset}, Viewport={_scrollViewer.ViewportHeight}, Extent={_scrollViewer.ExtentHeight}");
            // スクロールイベントの登録
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        // 初回ロード処理（1 回だけ実行）
        if (!_initialLoadDone)
        {
            _initialLoadDone = true;
            Logger.Shared.Debug("Initial LoadVisible_itemsAsync called");
            await LoadVisibleItemsAsync();
        }
    }

    /// <summary>
    /// アンロード後イベント
    /// </summary>
    private void ThumbnailListView_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_scrollViewer != null)
            _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;

        _scrollStopTimer.Stop();
    }

    /// <summary>
    /// 指定した DependencyObject の子ツリーを探索し、<see cref="ScrollViewer"/> を発見します。
    /// </summary>
    /// <param name="start">探索開始の DependencyObject。</param>
    /// <returns>発見した <see cref="ScrollViewer"/>。存在しない場合は <see langword="null"/></returns>
    private static ScrollViewer? FindScrollViewerInParent(DependencyObject start)
    {
        var count = VisualTreeHelper.GetChildrenCount(start);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(start, i);
            if (child is ScrollViewer sv)
                return sv;

            // 子ツリーを再帰的に探索
            var found = FindScrollViewerInParent(child);
            if (found != null)
                return found;
        }
        return null;
    }

    /// <summary>
    /// ScrollViewer スクロールイベント
    /// </summary>
    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // 小さな変化は無視
        if (Math.Abs(e.VerticalChange) < 0.1 &&
            Math.Abs(e.HorizontalChange) < 0.1 &&
            Math.Abs(e.ViewportHeightChange) < 0.1 &&
            Math.Abs(e.ViewportWidthChange) < 0.1)
            return;

        // 現在のロードをキャンセル
        CancelLoading(null);

        // スクロール停止待ち
        _scrollStopTimer.Stop();
        _scrollStopTimer.Start();
    }

    /// <summary>
    /// 現在 ScrollViewer で可視となっているアイテムのインデックス範囲を計算します。
    /// </summary>
    /// <returns>
    /// <tuple>
    ///   <member name="startIndex">開始インデックス</member>
    ///   <member name="endIndex">終了インデックス</member>
    /// </tuple>
    /// </returns>
    public (int startIndex, int endIndex) GetVisibleIndices()
    {
        if (_items.Count == 0)
            return (0, -1);

        if (_scrollViewer == null)
        {
            // ScrollViewer が未取得の場合はダミー範囲を返す
            var visibleCount = Math.Min(10, _items.Count);
            return (0, visibleCount - 1);
        }

        // ScrollViewer のビューポート矩形
        var listBox = PART_ListBox;
        var viewportRect = new Rect(0, 0, _scrollViewer.ViewportWidth, _scrollViewer.ViewportHeight);

        int? first = null;
        int? last = null;

        // 各アイテムの矩形を ScrollViewer 座標系に変換し、ビューポートと交差するか確認
        for (int i = 0; i < _items.Count; i++)
        {
            if (listBox.ItemContainerGenerator.ContainerFromIndex(i) is not ListBoxItem container)
                continue;

            try
            {
                // ScrollViewer への変換を取得
                var transform = container.TransformToAncestor(_scrollViewer);
                var itemRect = transform.TransformBounds(new Rect(new Point(0, 0), container.RenderSize));

                if (!itemRect.IntersectsWith(viewportRect))
                    continue;

                // 最初の可視アイテムと最後の可視アイテムを記録
                first ??= i;
                last = i;
            }
            catch
            {
                // 変換失敗などは無視
            }
        }

        if (first == null || last == null)
        {
            // 可視アイテムが未取得の場合はダミー範囲を返す
            var visibleCount = Math.Min(10, _items.Count);
            return (0, visibleCount - 1);
        }

        // 可視範囲の前後にリザーブ領域を追加
        var reserve = 10;
        var start = Math.Max(0, first.Value - reserve);
        var end = Math.Min(_items.Count - 1, last.Value + reserve);

        Logger.Shared.Debug($"Measured visible indices: first={first.Value}, last={last.Value}, start={start}, end={end}");
        return (start, end);
    }

    /// <summary>
    /// 可視範囲内のアイテムをロードします。
    /// ViewModel 経由で、指定インデックス範囲のデータを取得します。
    /// </summary>
    /// <param name="token">キャンセル用の <see cref="CancellationToken"/>.</param>
    public async Task LoadVisibleItemsAsync(CancellationToken token = default)
    {
        if (_items.Count == 0)
            return;

        // 可視インデックス範囲を計算
        var (startIndex, endIndex) = GetVisibleIndices();
        Logger.Shared.Debug($"LoadVisible_itemsAsync: startIndex={startIndex}, endIndex={endIndex}, total={_items.Count}");

        // 指定範囲のロードを要求
        await LoadVisibleRangeAsync(startIndex, endIndex, token);
    }

    /// <summary>
    /// 指定フォルダ内の画像ファイルを検出し、<see cref="_items"/> に登録します。
    /// 既存のロード処理はキャンセルされ、アイテムコレクションはクリアされます。
    /// </summary>
    private void LoadFolder()
    {
        // 既存のロード処理をキャンセル
        CancelLoading(null);
        _items.Clear();

        //// フォルダが無効または存在しない場合は終了
        //if (string.IsNullOrWhiteSpace(Folder) || !Directory.Exists(Folder))
        //    return;

        // フォルダ内のファイルを走査
        var enumerator = FileEnumeratorFactory.Create(Folder);
        foreach (var file in enumerator.Enumerate())
        {
            // サムネイル生成可能なファイルのみ追加
            if (!_thumbnailFactory.CanCreate(file.LogicalPath))
                continue;

            _items.Add(new ThumbnailItemViewModel
            {
                FileEntry = file
            });
        }

        // レイアウト生成を強制する
        PART_ListBox.UpdateLayout();
    }

    /// <summary>
    /// 指定インデックス範囲（可視範囲）内のサムネイルをロードします。
    /// 既存のロード処理はキャンセルされ、新しいバージョンで再実行されます。
    /// </summary>
    /// <param name="startIndex">開始インデックス（0 以上）。</param>
    /// <param name="endIndex">終了インデックス（startIndex 以上）。</param>
    /// <param name="token">キャンセル用の <see cref="CancellationToken"/>.</param>
    public async Task LoadVisibleRangeAsync(int startIndex, int endIndex, CancellationToken token = default)
    {
        // Dispose 済みまたはアイテムが存在しない場合は終了
        if (_disposed || _items.Count == 0)
            return;

        // インデックス範囲が不正の場合は終了
        if (startIndex < 0 || endIndex < startIndex)
            return;

        // インデックスを範囲内に制限
        startIndex = Math.Max(0, startIndex);
        endIndex = Math.Min(_items.Count - 1, endIndex);

        // 既存のロードをキャンセルし、新しいバージョンで開始
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        CancelLoading(linkedCts);

        var linkedToken = linkedCts.Token;
        var version = _loadVersion;

        // サムネイル未生成のアイテムをターゲットとして抽出
        var targets = _items
            .Skip(startIndex)
            .Take(endIndex - startIndex + 1)
            .Where(x => x.Thumbnail == null)
            .ToArray();

        // サムネイルサイズを構成
        var size = new Size(ThumbnailWidth, ThumbnailHeight);

        // 各アイテムに対してサムネイルロードタスクを生成し、並列実行
        var tasks = targets.Select(item => LoadThumbnailAsync(item, size, linkedToken, version));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// 指定アイテムのサムネイルを生成します。
    /// Semaphore で並列数を制御し、キャンセル・バージョンチェックを行います。
    /// </summary>
    /// <param name="item">サムネイルを生成するアイテム。</param>
    /// <param name="size">サムネイルサイズ。</param>
    /// <param name="token">キャンセル用の <see cref="CancellationToken"/>.</param>
    /// <param name="version">当前ロードバージョン。</param>
    private async Task LoadThumbnailAsync(
        ThumbnailItemViewModel item,
        Size size,
        CancellationToken token,
        int version)
    {
        // Dispose 済み、キャンセル、またはバージョン不一致の場合は終了
        if (_disposed || token.IsCancellationRequested || version != _loadVersion)
            return;

        try
        {
            // Semaphore で並列数を制御
            var semaphore = _loadSemaphore;
            await semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                // セマフォ取得後にも再度チェック
                if (_disposed || token.IsCancellationRequested || version != _loadVersion)
                    return;

                if (item.FileEntry == null)
                    return;

                // サムネイルを生成
                var thumbnail = await _thumbnailFactory.CreateAsync(
                    await item.FileEntry.ReadAllBytesAsync(),
                    size, 
                    token).ConfigureAwait(false);

                // 再度キャンセル・バージョンチェック
                if (_disposed || token.IsCancellationRequested || version != _loadVersion)
                    return;

                if (thumbnail != null)
                {
                    // Dispatcher で UI 側にセット
                    await SafeDispatcher.InvokeAsync(() =>
                    {
                        if (version != _loadVersion)
                            return;

                        if (!_disposed && item.Thumbnail == null)
                            item.Thumbnail = thumbnail;
                    });
                }
            }
            finally
            {
                // セマフォを解放
                semaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセル時は何もしない
        }
        catch (ObjectDisposedException)
        {
            // Dispose 済み時は何もしない
        }
    }

    /// <summary>
    /// 現在のロード処理をキャンセルし、バージョン番号をインクリメントします。
    /// </summary>
    /// <param name="newCts">新しいCancellationTokenSource</param>
    private void CancelLoading(CancellationTokenSource? newCts)
    {
        Interlocked.Increment(ref _loadVersion);
        var oldCts = Interlocked.Exchange(ref _loadCts, newCts);
        if (oldCts == null)
            return;

        try
        {
            oldCts.Cancel();
            oldCts.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    /// <summary>
    /// リソースを解放し、現在のロード処理をキャンセルします。
    /// </summary>
    public void Dispose()
    {
        var disposed = Interlocked.Exchange(ref _disposed, true);
        if (disposed)
            return;

        _scrollStopTimer.Stop();
        _scrollStopTimer.Tick -= OnScrollStopped;

        Loaded -= ThumbnailListView_Loaded;
        Unloaded -= ThumbnailListView_Unloaded;

        if (_scrollViewer != null)
            _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;

        CancelLoading(null);
    }

    /// <summary>
    /// Semaphore のスレッド数を更新します。
    /// フォルダが光学ドライブかHDD 場合は1、その他は <see cref="LoadThreads"/> を使用します。
    /// </summary>
    private void UpdateSemaphore()
    {
        CancelLoading(null);
        _loadSemaphore = GetCurrentSemaphore();
    }

    /// <summary>
    /// 現在のスレッド数に対応するセマフォを取得する。
    /// </summary>
    /// <returns>セマフォオブジェクト</returns>
    private SemaphoreSlim GetCurrentSemaphore()
    {
        var threads = Math.Min(
            LoadThreads,
            Environment.ProcessorCount);

        if( !string.IsNullOrEmpty(Folder) )
        {
            var diskInfo = DiskAnalyzer.GetDiskInfoFromPath(Folder);
            if (diskInfo.IsOptical ||
                diskInfo.StorageMediaType == StorageMediaType.HDD)
            {
                threads = 1;
            }
        }

        return _semaphoreCache.GetOrAdd(
            threads,
            static count => new SemaphoreSlim(count, count));
    }
}