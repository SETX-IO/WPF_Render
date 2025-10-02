using Vortice.DXGI;

namespace WPF_Render.Renderer.DirectX;

public abstract record DescriptionSize
{
    public const uint Float32 = 4;
    public const uint Float64 = 8;
    public const uint Float96 = 12;
    public const uint Float128 = 16;
}

public struct Description(Format format, uint size)
{
    public readonly Format Format = format;
    public readonly uint Size = size;

    public static Description Float32 = new Description(Format.R32_Float, DescriptionSize.Float32);
    public static Description Float64 = new Description(Format.R32G32_Float, DescriptionSize.Float64);
    public static Description Float96 = new Description(Format.R32G32B32_Float, DescriptionSize.Float96);
    public static Description Float128 = new Description(Format.R32G32B32A32_Float, DescriptionSize.Float128);
}