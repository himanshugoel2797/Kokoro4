using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif

namespace Kokoro.Engine.Graphics
{
    public interface ITextureSource
    {
        TextureTarget GetTextureTarget();
        int GetDimensions();
        int GetWidth();
        int GetHeight();
        int GetDepth();
        int GetLayersCount();
        int GetLevels();
        TextureTilingMode GetTilingMode();
        PixelInternalFormat GetInternalFormat();
        PixelFormat GetFormat();
        PixelType GetType();
        IntPtr GetPixelData(int level, int layer);
    }
}
