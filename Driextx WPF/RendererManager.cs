using System.Diagnostics;
using System.Windows;
using Vortice.Direct3D9;

namespace Driextx_WPF;

public class RendererManager
{
    private IDirect3D9? _d3d;
    private IDirect3D9Ex? _d3dEx;
    private nint _hwnd;
    
    private Renderer? _currentRenderer;
    private Renderer[]? _renderers;
    
    private bool _sufaceSettingsChanged;

    public bool UseAlpha
    {
        private get;
        set
        {
            if (field.Equals(value)) return;
            field = value;
            _sufaceSettingsChanged = true;
        }
    }

    public Size Size
    {
        private get;
        set
        {
            if (field.Equals(value)) return;
            field = value;
            _sufaceSettingsChanged = true;
        }
    }

    public uint NumSamles
    {
        private get;
        set
        {
            if (field.Equals(value)) return;
            {
                field = value;
                _sufaceSettingsChanged = true;
            }
        }
    }

    private uint _adapters;

    public void SetAdapters(Point point)
    {
        if (_d3d is null) return;
        
        CleanupInvalidDevices();
        
        nint a = Utilis.MonitorFromPoint(point, 2);
        for (uint i = 0; i < _adapters; i++)
        {
            if (a == _d3d.GetAdapterMonitor(i))
            {
                _currentRenderer = _renderers?[i];
            }
        }
    }

    public static void Create(nint windowHwnd, out RendererManager manager)
    {
        manager = new RendererManager
        {
            _hwnd = windowHwnd
        };
    }

    private void EnsureRenderers()
    {
        if (_renderers != null) return;
        
        _renderers = new Renderer[_adapters];

        for (uint i = 0; i < _adapters; ++i)
        {
            TriangRenderer.Create(_d3d!, _d3dEx!, _hwnd, i, out _renderers[i]);
        }

        _currentRenderer = _renderers[0];

    }
    
    private void CleanupInvalidDevices()
    {
        for (uint i = 0; i < _adapters; ++i)
        {
            if (_renderers != null && _renderers[i].CheckDeviceSate()) continue;
            
            _renderers?[i].Release();
            break;
        }
    }
    
    private void EnsureD3dObjects()
    {
        _d3dEx ??= D3D9.Direct3DCreate9Ex();
        _d3d ??= D3D9.Direct3DCreate9();

        _adapters = _d3d.AdapterCount;
    }
    
    public void GetBackBufferNoRef(out IntPtr pSurface)
    {
        pSurface = IntPtr.Zero;
        
        CleanupInvalidDevices();

        EnsureD3dObjects();
        
        EnsureRenderers();
        
        if (_sufaceSettingsChanged)
        {
            if (!TestSurfaceSttings())
            {
                throw new NullReferenceException("你的GPU不支持DX平面9");
            }
            
            foreach (var renderer in _renderers!)
            {
                renderer.CreateSuface((uint)Size.Width, (uint)Size.Height, UseAlpha, NumSamles);
            }
            
            _sufaceSettingsChanged = false;
        }
        
        pSurface = (IntPtr)_currentRenderer?.Surface;
    }

    private bool TestSurfaceSttings()
    {
        bool resault = true;
        var fmt = UseAlpha ? Format.A8R8G8B8 : Format.X8R8G8B8;

        for (uint i = 0; i < _adapters; ++i)
        {
            resault = _d3d!.CheckDeviceType(
                i,
                DeviceType.Hardware,
                Format.X8R8G8B8,
                fmt,
                true
                ).Success &&
                
            _d3d.CheckDeviceFormat(
                i,
                DeviceType.Hardware,
                Format.X8R8G8B8, 
                (int)(Usage.RenderTarget | Usage.Dynamic),
                ResourceType.Surface,
                fmt).Success;
            
            if (_d3dEx is not null && NumSamles > 1)
            {
                Debug.Assert(NumSamles <= 16);

                if (!_d3d.CheckDeviceMultiSampleType(i, DeviceType.Hardware, fmt, true, (MultisampleType)NumSamles, out _).Success)
                {
                    NumSamles = 0;
                }
            }
            else
            {
                NumSamles = 0;
            }
        
        }
        
        return resault;
    }
    
    public void Render()
    {
        _currentRenderer?.Render();
    }
    
    public void Release()
    {
        _d3d?.Release();
        _d3dEx?.Release();

        if (_renderers != null)
            foreach (var renderer in _renderers)
            {
                renderer.Release();
            }

        _sufaceSettingsChanged = true;
    }
}
