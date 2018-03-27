using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class RawTextureSource : ITextureSource
    {
        public virtual int Width { get; protected set; }
        public virtual int Height { get; protected set; }
        public virtual int Depth { get; protected set; }
        public virtual int Levels { get; protected set; }
        public virtual int Dimensions { get; protected set; }
        public virtual int Bpp { get; protected set; }
        public virtual PixelFormat PixelFormat { get; protected set; }
        public virtual PixelInternalFormat InternalFormat { get; protected set; }
        public virtual TextureTarget Target { get; protected set; }
        public virtual PixelType PixelType { get; protected set; }

        public RawTextureSource(int dim, int width, int height, int depth, int levels, PixelFormat pFormat, PixelInternalFormat iFormat, TextureTarget target, PixelType pType)
        {
            if (levels < 1)
                throw new ArgumentException("levels must be at least 1!");
            Bpp = 4;
            Dimensions = dim;
            Width = width;
            Height = height;
            Depth = depth;
            Levels = levels;
            PixelFormat = pFormat;
            InternalFormat = iFormat;
            Target = target;
            PixelType = pType;
        }

        public int GetDepth()
        {
            return Depth;
        }

        public int GetDimensions()
        {
            return Dimensions;
        }

        public PixelFormat GetFormat()
        {
            return PixelFormat;
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
            return Levels;
        }

        public virtual IntPtr GetPixelData(int level)
        {
            return IntPtr.Zero;
        }

        public TextureTarget GetTextureTarget()
        {
            return Target;
        }

        public int GetWidth()
        {
            return Width;
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
            return Bpp;
        }
    }
}
