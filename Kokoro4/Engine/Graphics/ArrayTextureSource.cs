using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class ArrayTextureSource : ITextureSource
    {
        private ITextureSource[] src;
        private int w, h, layers, levels;
        private PixelFormat fmt;
        private PixelType type;
        private int curLayer = 0;

        public ArrayTextureSource(int w, int h, int layers, int levels, PixelFormat fmt, PixelType type, params ITextureSource[] srcs)
        {
            src = srcs;
            this.w = w;
            this.h = h;
            this.layers = layers;
            this.levels = levels;
            this.fmt = fmt;
            this.type = type;
        }

        bool inited = false; 
        public int GetDepth()
        {
            if (!inited)
            {
                inited = true;
                return layers;
            }
            else
            {
                return 1;
            }
        }

        public int GetDimensions()
        {
            return 3;
        }

        public PixelFormat GetFormat()
        {
            return fmt;
        }

        public int GetHeight()
        {
            return h;
        }

        public PixelInternalFormat GetInternalFormat()
        {
            return src[curLayer].GetInternalFormat();
        }

        public int GetLevels()
        {
            return levels;
        }

        public void SetCurrentLayerIndex(int layer)
        {
            curLayer = layer;
        }

        public IntPtr GetPixelData(int level)
        {
            if (curLayer > src.Length)
            {
                return IntPtr.Zero;
            }

            return src[curLayer++].GetPixelData(level);
        }

        public TextureTarget GetTextureTarget()
        {
            return TextureTarget.Texture2DArray;
        }

        public int GetWidth()
        {
            return w;
        }

        PixelType ITextureSource.GetPixelType()
        {
            return type;
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
            return curLayer;
        }

        public int GetBpp()
        {
            return 4; //TODO
        }
    }
}
