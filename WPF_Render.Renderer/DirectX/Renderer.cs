using System;
using System.Diagnostics;
using Vortice.Direct3D9;

namespace WPF_Render.Renderer.DirectX;

public abstract class Renderer
{
    protected IDirect3DDevice9? _d3dDevice;
    protected IDirect3DDevice9Ex? _d3dDeviceEx;
    protected IDirect3DSurface9? _d3dRTS;

    public IDirect3DSurface9? Surface => _d3dRTS;
    
    protected Renderer() { }
    public abstract void Render();

    public bool CheckDeviceState() => _d3dDevice is not null && _d3dDeviceEx is not null;
    
    
    public void CreateSurface(uint width, uint height, bool isAlpha, uint uNumSamples)
    {
        _d3dRTS?.Release();

        _d3dRTS = _d3dDevice?.CreateRenderTarget(width, height, isAlpha ? Format.A8R8G8B8 : Format.X8R8G8B8,
            (MultisampleType)uNumSamples, 0,
            _d3dDeviceEx is null);
        
        _d3dDevice?.SetRenderTarget(0, _d3dRTS);
    }
    
    protected virtual void Init(IDirect3D9? d3D, IDirect3D9Ex? d3DEx, IntPtr hwnd, uint uAdapter)
    {
        var d3dPresent = new PresentParameters
        {
            Windowed = true,
            BackBufferFormat = Format.Unknown,
            BackBufferHeight = 1,
            BackBufferWidth = 1,
            SwapEffect = SwapEffect.Discard,
        };

        var cpas = d3D.GetDeviceCaps(uAdapter, DeviceType.Hardware);
        var vertexProcessing = CreateFlags.HardwareVertexProcessing;
        if ((cpas.DeviceCaps & DeviceCaps.HWRasterization) is not DeviceCaps.HWRasterization)
        {
            vertexProcessing = CreateFlags.SoftwareVertexProcessing;
        }

        if (d3DEx is not null)
        {
            _d3dDeviceEx = d3DEx.CreateDeviceEx(uAdapter, DeviceType.Hardware, hwnd,
                vertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                d3dPresent);

            _d3dDevice = _d3dDeviceEx.QueryInterface<IDirect3DDevice9>();
        }
        else
        {
            Debug.Assert(d3D is not null);

            _d3dDevice = d3D.CreateDevice(uAdapter, DeviceType.Hardware, hwnd,
                vertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, d3dPresent);
        }
    }
}