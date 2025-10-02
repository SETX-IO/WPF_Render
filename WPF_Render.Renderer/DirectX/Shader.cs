using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vortice.D3DCompiler;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace WPF_Render.Renderer.DirectX;

public class Shader
{
    private string _shaderPath = String.Empty;
    
    private ID3D11VertexShader? _vertexShader;
    private ID3D11PixelShader? _pixelShader;
    private ID3D11InputLayout? _inputLayout;

    private ID3D11DeviceContext1 _context = null!;

    private Shader() { }

    private void Init((string semanticName, Description format)[] inputElement)
    {
        var shaderCode = CompileBytecode(_shaderPath, "PSMain", "ps_4_0");
        _pixelShader = _context.Device.CreatePixelShader(shaderCode.Span);
        
        shaderCode = CompileBytecode(_shaderPath, "VSMain", "vs_4_0");
        _vertexShader = _context.Device.CreateVertexShader(shaderCode.Span);

        var descriptions = new List<InputElementDescription>();
        uint offset = 0;
        for (int i = 0; i < inputElement.Length; i++)
        {
            var valueTuple = inputElement[i];
            descriptions.Add(new InputElementDescription(valueTuple.semanticName, 0, valueTuple.format.Format, offset, 0));
            offset += valueTuple.format.Size;
        }

        _inputLayout = _context.Device.CreateInputLayout(descriptions.ToArray() ,shaderCode.Span);
    }
    
    public void Use()
    {
        _context.VSSetShader(_vertexShader);
        _context.PSSetShader(_pixelShader);
        _context.IASetInputLayout(_inputLayout);
    }

    public void Dispose()
    {
        _vertexShader?.Dispose();
        _pixelShader?.Dispose();
        _inputLayout?.Dispose();
    }
    
    private ReadOnlyMemory<byte> CompileBytecode(string shaderName, string entryPoint, string profile)
    {
        string assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        string shaderFile = Path.Combine(assetsPath, shaderName);
        //string shaderSource = File.ReadAllText(Path.Combine(assetsPath, shaderName));

        return Compiler.CompileFromFile(shaderFile, entryPoint, profile);
    }

    public static Shader Create(ID3D11DeviceContext1 context, string fileName, (string, Description)[] inputElement)
    {
        var shader = new Shader
        {
            _context = context,
            _shaderPath = fileName
        };
        
        shader.Init(inputElement);
        
        return shader;
    }
}