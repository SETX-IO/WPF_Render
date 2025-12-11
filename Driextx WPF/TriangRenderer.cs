using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Mathematics;
using Vortice.Direct3D9;

namespace Driextx_WPF;

public class TriangRenderer : Renderer
{
    private IDirect3DVertexBuffer9 _vertexBuffer = null!;
    
    public static void Create(IDirect3D9 d3d, IDirect3D9Ex d3dEx, nint hwnd, uint uAdapter, out Renderer renderer)
    {
        var tempRender = new TriangRenderer();
        tempRender.Init(d3d,d3dEx, hwnd, uAdapter);

        renderer = tempRender;
    }

    protected override void Init(IDirect3D9 d3d, IDirect3D9Ex? d3dEx, nint hwnd, uint uAdapter)
    {
        Vertex[] vertices =
        [
            new(-1.0f, -1.0f, 0.0f, 0xffff0000),
            new(1.0f, -1.0f, 0.0f, 0xff00ff00),
            new(0.0f, 1.0f, 0.0f, 0xff00ffff)
        ];
        var verticesLength = (uint)(Marshal.SizeOf<Vertex>() * vertices.Length);
        var eye = new Vector3(0, 0, -5);
        var lookAt = new Vector3(0);
        var up = new Vector3(0, 1, 0);

        base.Init(d3d, d3dEx, hwnd, uAdapter);

        _vertexBuffer = D3dDevice!.CreateVertexBuffer(
            verticesLength,
            Usage.None,
            VertexFormat.Position | VertexFormat.Diffuse,
            Pool.Default);
        
        vertices.CopyTo(_vertexBuffer.Lock<Vertex>(0, (uint)vertices.Length));
        _vertexBuffer.Unlock();

        var matView = Matrix4x4.CreateLookAtLeftHanded(eye, lookAt, up);
        D3dDevice.SetTransform(TransformState.View, matView);
        var matproj = Matrix4x4.CreatePerspectiveLeftHanded(MathF.PI / 4, 1, 1, 100);
        D3dDevice.SetTransform(TransformState.Projection, matproj);
        
        D3dDevice.SetRenderState(RenderState.CullMode, Cull.None);
        D3dDevice.SetRenderState(RenderState.Lighting, false);
        D3dDevice.SetStreamSource(0, _vertexBuffer, 0, (uint)Marshal.SizeOf(vertices[0]));
        D3dDevice.VertexFormat = VertexFormat.Position | VertexFormat.Diffuse;
    }
    
    public override void Render()
    {
        D3dDevice!.BeginScene();
        D3dDevice.Clear(ClearFlags.Target, new Color(128, 0, 0, 256), 1.0f, 0);

        var iTime = (uint)(Environment.TickCount64 % 1000);
        var fAngle = iTime * (2.0f * MathF.PI) / 1000.0f;
        var matWorld = Matrix4x4.CreateRotationY(fAngle);
        D3dDevice.SetTransform((TransformState)256, matWorld);
        
        D3dDevice.DrawPrimitive(PrimitiveType.TriangleList, 0, 1);
        
        D3dDevice.EndScene();
    }
}
