using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Kokoro.Graphics
{
    public class FramebufferTextureSource : ITextureSource
    {
        private int width, height, levels;

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
            return PixelInternalFormat.Rgba;
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

        PixelType ITextureSource.GetType()
        {
            return PixelType.UnsignedByte;
        }
    }
}
