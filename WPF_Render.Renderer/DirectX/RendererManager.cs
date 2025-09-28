using System;
using System.Diagnostics;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Vortice.Direct3D9;

namespace WPF_Render.Renderer.DirectX;

public class RendererManager
{
    private IDirect3D9? _d3d9;
    private IDirect3D9Ex? _d3d9Ex;

    private uint _adapters;
    private Renderer[] _rgRenderers;
    private Renderer _currentRenderer;

    private IntPtr _hwnd;
    
    private uint _width;
    private uint _height;
    private uint _numSamples;
    private bool _isUseAlpha;
    private bool _surfaceSettingsChanged;

    private RendererManager() { }
    
    public void SetSize(uint width, uint height)
    {
        if (width == _width && height == _height) return;
        
        _width = width;
        _height = height;
        _surfaceSettingsChanged = true;
    }

    public void SetAlpha(bool useAlpha)
    {
        if (_isUseAlpha == useAlpha) return;
        
        _isUseAlpha = useAlpha;
        _surfaceSettingsChanged = true;
    }

    public void SetNumDesiredSamples(uint numSamples)
    {
        if (_numSamples == numSamples) return;

        _numSamples = numSamples;
        _surfaceSettingsChanged = true;
    }

    public void GetBackBufferNoRef(out IDirect3DSurface9? pSurface)
    {
        CleanupInvalidDevices();
        
        EnsureD3DObjects();

        EnsureRenderers();
        
        if (_surfaceSettingsChanged)
        {
            for (uint i = 0; i < _adapters; i++)
            {
                _rgRenderers[i].CreateSurface(_width, _height, _isUseAlpha, _numSamples);
            }

            _surfaceSettingsChanged = false;
        }

        pSurface = _currentRenderer.Surface;
    }

    [DllImport("User32.dll")]
    private static extern IntPtr MonitorFromPoint([In]Point pt, [In]uint dwFlags);
    
    public void SetAdapter(Point screenSpacePoint)
    {
        CleanupInvalidDevices();
        
        if (_d3d9 is null || _rgRenderers.Length is not 0) return;
        
        var hMon = MonitorFromPoint(screenSpacePoint, 0);
        for (uint i = 0; i < _adapters; i++)
        {
            if (hMon == _d3d9?.GetAdapterMonitor(i))
            {
                _currentRenderer = _rgRenderers[i];
                break;
            }
        }
    }
    
    public void Render() => _currentRenderer.Render();
    
    private void EnsureD3DObjects()
    {
        _d3d9Ex ??= D3D9.Direct3DCreate9Ex();
        _d3d9 ??= _d3d9Ex.QueryInterface<IDirect3D9>();

        _adapters = _d3d9.AdapterCount;
    }
    
    private void EnsureRenderers()
    {
        Debug.Assert(_adapters is not 0);
        _rgRenderers = new Renderer[_adapters];

        for (uint i = 0; i < _adapters; i++)
        {
            TriangleRenderer.Create(_d3d9, _d3d9Ex, _hwnd, i, out var renderer);
            _rgRenderers[i] = renderer;
        }

        _currentRenderer = _rgRenderers[0];
    }

    private void CleanupInvalidDevices()
    {
        for (uint i = 0; i < _adapters; i++)
        {
            if (!_rgRenderers[i].CheckDeviceState()) continue;
            
            DestroyResources();
            break;
        }
    }

    private void DestroyResources()
    {
        _d3d9?.Release();
        _d3d9Ex?.Release();

        _adapters = 0;
        _surfaceSettingsChanged = true;
    }

    public static RendererManager Create(Window window)
    {
        var rendererManager = new RendererManager
        {
            _hwnd = ((HwndSource?)PresentationSource.FromVisual(window)).Handle,
        };

        return rendererManager;
    }
}