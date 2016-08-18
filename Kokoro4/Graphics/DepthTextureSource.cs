using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Kokoro.Graphics
{
    public class DepthTextureSource : ITextureSource
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public PixelInternalFormat InternalFormat { get; set; }

        public DepthTextureSource(int width, int height)
        {
            this.Width = width;
            this.Height = height;
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
            return PixelFormat.DepthComponent;
        }

        public int GetHeight()
        {
            return Height;
        }

        public PixelInternalFormat GetInternalFormat()
        {
            return InternalFormat;
        }

        public int GetLevels()
        {
            return 0;
        }

        public IntPtr GetPixelData()
        {
            return IntPtr.Zero;
        }

        public TextureTarget GetTextureTarget()
        {
            return TextureTarget.Texture2D;
        }

        public int GetWidth()
        {
            return Width;
        }

        PixelType ITextureSource.GetType()
        {
            return PixelType.Float;
        }
    }
}
