using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D9;
using Vortice.Mathematics;

namespace WPF_Render.Renderer.DirectX;

public class TriangleRenderer: Renderer
{
    private IDirect3DVertexBuffer9? _d3dVertexBuffer;

    private TriangleRenderer() { }

    public override void Render()
    {
        var iTime = (uint)(Environment.TickCount64 % 1000);
        float fAngle = iTime * (2.0f * MathF.PI) / 1000.0f;
        var matWorld = Matrix4x4.CreateRotationY(fAngle);

        _d3dDevice?.BeginScene();
        
        _d3dDevice?.Clear(ClearFlags.Target, new Color(0), 1.0f, 0);
        
        _d3dDevice?.SetTransform(0, matWorld);
        
        _d3dDevice?.DrawPrimitive(PrimitiveType.TriangleList, 0, 1);
        
        _d3dDevice?.EndScene();
    }
    
    protected override unsafe void Init(IDirect3D9? d3D, IDirect3D9Ex? d3DEx, IntPtr hwnd, uint uAdapter)
    {
        var vEyePt = new Vector3(0.0f, 0.0f, -0.5f);
        var vLookPt = new Vector3(0.0f);
        var vUpVec = new Vector3(0.0f, 1.0f, 0.0f);

        CustomVertex[] vertices = [
            new(-1.0f, -1.0f, 0.0f, 0xffff0000),
            new( 1.0f, -1.0f, 0.0f, 0xff00ff00),
            new( 0.0f,  0.0f, 0.0f, 0xff00ffff)
        ];
        
        base.Init(d3D, d3DEx, hwnd, uAdapter);
        
        _d3dVertexBuffer = _d3dDevice?.CreateVertexBuffer((uint)(sizeof(CustomVertex) * vertices.Length), 0,
            VertexFormat.Position | VertexFormat.Diffuse, Pool.Default);
        
         fixed (void* verticesPtr1 = _d3dVertexBuffer!.Lock<CustomVertex>(0, (uint)(sizeof(CustomVertex) * vertices.Length)))
         fixed (void* verticesPtr2 = vertices)
         {
             var numBytesToCopy = vertices.Length * sizeof(CustomVertex);
             Buffer.MemoryCopy(verticesPtr1, verticesPtr2, numBytesToCopy, numBytesToCopy);
         }

        
        _d3dVertexBuffer?.Unlock();

        var matView = Matrix4x4.CreateLookAtLeftHanded(vEyePt, vLookPt, vUpVec);
        _d3dDevice?.SetTransform(TransformState.View, matView);
        var matProj = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(MathF.PI / 4, 1.0f, 1.0f, 100.0f);
        _d3dDevice?.SetTransform(TransformState.Projection, matProj);
        
        _d3dDevice?.SetRenderState(RenderState.CullMode, 1);
        _d3dDevice?.SetRenderState(RenderState.Lighting, false);
        _d3dDevice?.SetStreamSource(0, _d3dVertexBuffer, 0, (uint)sizeof(CustomVertex));
    }

    public static void Create(IDirect3D9? d3D, IDirect3D9Ex? d3DEx, IntPtr hwnd, uint uAdapter, out Renderer renderer)
    {
        var triangleRenderer = new TriangleRenderer();
        triangleRenderer.Init(d3D, d3DEx, hwnd, uAdapter);
        renderer = triangleRenderer;
    }
}

public struct CustomVertex(float X, float Y, float Z, uint Color)
{
    public float X = X;
    public float Y = Y;
    public float Z = Z;
    public uint Color = Color;
}