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
    public class FramebufferTextureSource : ITextureSource
    {
        private int width, height, levels;

        public PixelType PixelType { get; set; }
        public PixelInternalFormat InternalFormat { get; set; }

        public FramebufferTextureSource(int width, int height, int levels)
        {
            this.width = width;
            this.height = height;
            this.levels = levels;
        }

        public int GetDepth()
        {
            return 0;
        }

        public int GetDimensions()
        {
            return 2;
        }

        public PixelFormat GetFormat()
        {
            return PixelFormat.Bgra;
        }

        public int GetHeight()
        {
            return height;
        }

        public PixelInternalFormat GetInternalFormat()
        {
            return InternalFormat;
        }

        public int GetLevels()
        {
            return levels;
        }

        public IntPtr GetPixelData(int level)
        {
            return IntPtr.Zero;
        }

        public TextureTarget GetTextureTarget()
        {
            return TextureTarget.Texture2D;
        }

        public int GetWidth()
        {
            return width;
        }

        PixelType ITextureSource.GetPixelType()
        {
            return PixelType;
        }

        public int GetBaseWidth()
        {
            return 0;
        }

        public int GetBaseHeight()
        {
            return 0;
        }

        public int GetBaseDepth()
        {
            return 0;
        }

        public int GetBpp()
        {
            return 4; //TODO
        }
    }
}
