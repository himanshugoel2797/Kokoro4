using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Messier.Graphics
{
    public interface ITextureSource
    {
        TextureTarget GetTextureTarget();
        int GetDimensions();
        int GetWidth();
        int GetHeight();
        int GetDepth();
        int GetLevels();
        PixelInternalFormat GetInternalFormat();
        PixelFormat GetFormat();
        PixelType GetType();
        IntPtr GetPixelData();
    }
}
