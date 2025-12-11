using System.Diagnostics;
using Vortice.Direct3D9;

namespace Driextx_WPF;

public abstract class Renderer
{
    public IDirect3DSurface9? Surface { get; protected set; }

    protected IDirect3DDevice9Ex?  D3dDeviceEx;
    protected IDirect3DDevice9?    D3dDevice;

    public abstract void Render();

    public bool CheckDeviceSate()
    {
        var resault = true;
        if (D3dDeviceEx is not null)
        {
            D3dDeviceEx.CheckDeviceState(0);
        }
        else if (D3dDevice is not null)
        {
            D3dDevice.TestCooperativeLevel();
        }
        else
        {
            resault = false;
        }

        return resault;
    }

    public void CreateSuface(uint width, uint height, bool useAlpha, uint numSamples)
    {
        if (D3dDevice is null) throw new NullReferenceException("Device is null.");
        
        Surface?.Release();
        Surface = D3dDevice.CreateRenderTarget(
            width, height,
            useAlpha ? Format.A8R8G8B8 : Format.X8R8G8B8,
            (MultisampleType)numSamples,
            0, 
            D3dDeviceEx is null
        );
        
        D3dDevice.SetRenderTarget(0, Surface);
    }

    protected virtual void Init(IDirect3D9 d3d, IDirect3D9Ex? d3dEx, IntPtr hwnd, uint uAdapter)
    {
        var present = new PresentParameters
        {
            Windowed = true,
            BackBufferFormat = Format.Unknown,
            BackBufferHeight = 1,
            BackBufferWidth = 1,
            SwapEffect = SwapEffect.Discard
        };

        var cap = d3d.GetDeviceCaps(uAdapter, DeviceType.Hardware);
        var processing = ((cap.DeviceCaps & DeviceCaps.HWTransformAndLight) == DeviceCaps.HWTransformAndLight) ? 
            CreateFlags.HardwareVertexProcessing : CreateFlags.SoftwareVertexProcessing;


        if (d3dEx is not null)
        {
            D3dDeviceEx = d3dEx.CreateDeviceEx(
                uAdapter,
                DeviceType.Hardware,
                hwnd,
                processing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                present);

            D3dDevice = D3dDeviceEx.QueryInterface<IDirect3DDevice9>();
        }
        else
        {
            Debug.Assert(d3d is not null, "Dirext3D9 is null");

            D3dDevice = d3d.CreateDevice(
                uAdapter,
                DeviceType.Hardware,
                hwnd,
                processing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                present);
        }
    }

    public void Release()
    {
        D3dDevice?.Release();
        D3dDeviceEx?.Release();
        Surface?.Release();
    }
}
