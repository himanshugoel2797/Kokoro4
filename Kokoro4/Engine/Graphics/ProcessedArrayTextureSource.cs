using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    class ProcessedArrayTextureSource : ITextureSource
    {
        private int w, h, layers, levels;
        private PixelFormat fmt;
        private PixelType type;
        private PixelInternalFormat iFmt;
        private IntPtr data;
        
        public ProcessedArrayTextureSource(string file)
        {
            //TODO parse the file to directly obtain the required data to directly upload
        }

        public int GetDepth()
        {
            return layers;
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
            return iFmt;
        }

        public int GetLevels()
        {
            return levels;
        }

        public IntPtr GetPixelData(int level)
        {
            return data;
        }

        public TextureTarget GetTextureTarget()
        {
            return TextureTarget.Texture2DArray;
        }

        public int GetWidth()
        {
            return w;
        }

        PixelType ITextureSource.GetType()
        {
            return type;
        }
    }
}
