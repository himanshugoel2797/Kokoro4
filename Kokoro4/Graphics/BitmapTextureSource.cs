using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kokoro.Graphics
{
    public class BitmapTextureSource : ITextureSource, IDisposable
    {
        Bitmap srcBmp;
        BitmapData bmpData;

        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int Levels { get; set; }

        public BitmapTextureSource(Bitmap bmp, int mipmapLevels)
        {
            srcBmp = (Bitmap)bmp.Clone();
            srcBmp.RotateFlip(RotateFlipType.Rotate180FlipX);
            Width = bmp.Width;
            Height = bmp.Height;
            Levels = mipmapLevels;

            bmpData = srcBmp.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public BitmapTextureSource(string path, int mipmapLevels) : this(new Bitmap(path), mipmapLevels) { }


        public int GetDepth()
        {
            return 0;
        }

        public int GetDimensions()
        {
            return 2;
        }

        public OpenTK.Graphics.OpenGL.PixelFormat GetFormat()
        {
            return OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
        }

        public int GetHeight()
        {
            return Height;
        }

        public PixelInternalFormat GetInternalFormat()
        {
            return PixelInternalFormat.Rgba8;
        }

        public int GetLevels()
        {
            return Levels;
        }

        public IntPtr GetPixelData(int level)
        {
            return bmpData.Scan0;
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
            return PixelType.UnsignedByte;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    srcBmp.UnlockBits(bmpData);
                    srcBmp.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BitmapTextureSource() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
