using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif

namespace Kokoro.Engine.Graphics
{
    public class BitmapTextureSource : RawTextureSource
    {
        Bitmap srcBmp;
        BitmapData bmpData;

        public BitmapTextureSource(Bitmap bmp, int mipmapLevels) : base(2, bmp.Width, bmp.Height, 0, mipmapLevels, PixelFormat.Bgra, PixelInternalFormat.Rgba8, TextureTarget.Texture2D, PixelType.UnsignedByte)
        {
            srcBmp = (Bitmap)bmp.Clone();
            srcBmp.RotateFlip(RotateFlipType.Rotate180FlipX);

            bmpData = srcBmp.LockBits(new Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public BitmapTextureSource(string path, int mipmapLevels) : this(new Bitmap(path), mipmapLevels) { }
        
        public override IntPtr GetPixelData(int level)
        {
            return bmpData.Scan0;
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
