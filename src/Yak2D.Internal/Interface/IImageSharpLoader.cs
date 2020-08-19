using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Veldrid;

namespace Yak2D.Internal
{
    public interface IImageSharpLoader
    {
        Texture GenerateVeldridTextureFromStream(Stream stream, bool mipMap);
        Texture GenerateRgbaVeldridTextureFromPixelData(Rgba32[] data, uint width, uint height);
        Texture GenerateFloat32VeldridTextureFromPixelData(float[] data, uint width, uint height);
        Texture GenerateSingleWhitePixel();
    }
}