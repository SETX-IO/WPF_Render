using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Size = System.Windows.Size;

namespace Driextx_WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IDisposable
{
    private DispatcherTimer? _sizeTimer;
    private DispatcherTimer? _adapterTimer;
    private TimeSpan _lastRender;
    
    private RendererManager _manager = null!;
    // private ID2D1Factory _factory;
    // private ID2D1HwndRenderTarget _renderTarget;
    // private ID2D1SolidColorBrush _colorbrush;

    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);

        // _factory = D2D1.D2D1CreateFactory<ID2D1Factory>();
        // _renderTarget = _factory.CreateHwndRenderTarget(new RenderTargetProperties(), new HwndRenderTargetProperties { Hwnd = helper.Handle, PixelSize = new SizeI((int)Width, (int)Height)});
        // _colorbrush = _renderTarget.CreateSolidColorBrush(new Color(1, 0, 0));

        RendererManager.Create(helper.Handle, out _manager);
        
        _manager.UseAlpha = true;
        _manager.Size = new Size(Width * 2, Height * 2);
        _manager.NumSamles = 4;
        
        CompositionTarget.Rendering += CompositionTarget_Rundering;
        
        _adapterTimer = new DispatcherTimer();
        _adapterTimer.Tick += AdapterTimerOnTick;
        _adapterTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
        _adapterTimer.Start();
        
        _sizeTimer = new DispatcherTimer();
        _sizeTimer.Tick += SizeTimerOnTick;
        _sizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
        _sizeTimer.Start();
    }
    
    private void CompositionTarget_Rundering(object? sender, EventArgs e)
    {
        var args = (RenderingEventArgs)e;
        
        if (!d3dimg.IsFrontBufferAvailable || _lastRender == args.RenderingTime) return;
        
        _manager.GetBackBufferNoRef(out var pSurface);
        
        if (pSurface == IntPtr.Zero) return;
        d3dimg.Lock();
        d3dimg.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface);
        _manager.Render();
        d3dimg.AddDirtyRect(new Int32Rect(0, 0, d3dimg.PixelWidth, d3dimg.PixelHeight));
        d3dimg.Unlock();
        
        _lastRender = args.RenderingTime;
        
        // _renderTarget.BeginDraw();
        //
        // _renderTarget.Clear(new Color(1, 1, 1, 1));
        // _renderTarget.DrawRectangle(new Rect(100f, 100f, 100f, 100f), _colorbrush);
        //
        // _renderTarget.EndDraw();
    }
    
    private void SizeTimerOnTick(object? sender, EventArgs e)
    {
        var actualHeight = (uint)imgelt.ActualHeight;
        var actualWidth = (uint)imgelt.ActualWidth;
        if (actualWidth > 0 && actualHeight > 0 && (actualWidth != (uint)d3dimg.Width || actualHeight != (uint)d3dimg.Height))
        {
            _manager.Size = new Size(actualWidth, actualHeight);
        }
    }
    
    private void AdapterTimerOnTick(object? sender, EventArgs e)
    {
        var point = imgelt.PointToScreen(new Point(0, 0));
        _manager.SetAdapters(point);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // _manager.Release();
        // _renderTarget.Release();
        // _factory.Release();
    }
}