using System.Runtime.InteropServices;

namespace Driextx_WPF;

public class Utilis
{
    [DllImport("User32.dll")]
    public static extern IntPtr MonitorFromPoint([In]System.Windows.Point pt, [In]uint dwFlags);
}