using Kokoro.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface
{
    class KinectFrameTextureSource : RawTextureSource, IDisposable
    {
        public IntPtr Data { get; private set; }

        public KinectFrameTextureSource(int width, int height, int Bpp, PixelFormat pFormat, PixelInternalFormat iFormat, PixelType pType) : base(2, width, height, 0, 1, pFormat, iFormat, TextureTarget.Texture2D, pType)
        {
            this.Data = Marshal.AllocHGlobal(width * height * Bpp);
            this.Bpp = Bpp;
        }

        public override IntPtr GetPixelData(int level)
        {
            return Data;
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
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                Marshal.FreeHGlobal(Data);

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
         ~KinectFrameTextureSource() {
           // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
         }

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
