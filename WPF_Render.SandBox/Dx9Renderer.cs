using System.Runtime.InteropServices;
using System.Windows;

namespace WPF_Render.SandBox;

public class Dx9Renderer
{
    [DllImport("WPF_Render.Renderder.dll")]
    public static extern int GetBackBufferNoRef(out IntPtr pSurface);

    [DllImport("WPF_Render.Renderder")]
    public static extern int SetSize(uint width, uint height);

    [DllImport("WPF_Render.Renderder.dll")]
    public static extern int SetAlpha(bool useAlpha);

    [DllImport("WPF_Render.Renderder.dll")]
    public static extern int SetNumDesiredSamples(uint numSamples);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public POINT(Point p)
        {
            x = (int)p.X;
            y = (int)p.Y;
        }

        public int x;
        public int y;
    }

    [DllImport("WPF_Render.Renderder.dll")]
    public static extern int SetAdapter(POINT screenSpacePoint);

    [DllImport("WPF_Render.Renderder.dll")]
    public static extern int Render();

    [DllImport("WPF_Render.Renderder.dll")]
    public static extern void Destroy();
}

public static class HRESULT
{
    public static void Check(int hr)
    {
        Marshal.ThrowExceptionForHR(hr);
    }
}
