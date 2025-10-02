using System.Numerics;
using System.Windows;
using Vortice.Direct3D11;
using Vortice.Mathematics;
using Vortice.Wpf;
using Vortice.DXGI;
using System.IO;
using System.Runtime.InteropServices;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using WPF_Render.Renderer.DirectX;

namespace WPF_Render.SandBox;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private ID3D11Buffer? _vertexBuffer;
    private Shader? _shader;
    
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void DrawingSurface_LoadContent(object? sender, DrawingSurfaceEventArgs e)
    {
        ReadOnlySpan<VertexPositionColor> triangleVertices =
        [
            new VertexPositionColor(new Vector3(0f, 0.5f, 0.0f), new Color4(1.0f, 0.0f, 0.0f)),
            new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.0f), new Color4(0.0f, 1.0f, 0.0f)),
            new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.0f), new Color4(0.0f, 0.0f, 1.0f))
        ];
        
        _vertexBuffer = e.Device.CreateBuffer(triangleVertices, BindFlags.VertexBuffer);

        (string , Description)[] inputElement = 
        [
            ("POSITION", Description.Float96),
            ("COLOR", Description.Float128)
        ];
        
        _shader = Shader.Create(e.Context, "Triangle.hlsl", inputElement);
    }

    private void DrawingSurface_UnloadContent(object sender, DrawingSurfaceEventArgs e)
    {
        _vertexBuffer?.Dispose();
        
        _shader?.Dispose();
    }

    private void DrawingSurface_Draw(object? sender, DrawEventArgs e)
    {
        e.Context.OMSetBlendState(null);
        e.Context.ClearRenderTargetView(e.Surface.ColorTextureView, Colors.Transparent);

        if (e.Surface.DepthStencilView != null)
        {
            e.Context.ClearDepthStencilView(e.Surface.DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        e.Context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        
        _shader?.Use();
        
        e.Context.IASetVertexBuffer(0, _vertexBuffer, VertexPositionColor.SizeInBytes);
        e.Context.Draw(3, 0);
    }
}

struct VertexPositionColor (Vector3 pos, Color4 color)
{
    public Vector3 Posion = pos;
    public Color4 Color = color;

    public static readonly uint SizeInBytes = (uint)Marshal.SizeOf<VertexPositionColor>();
}