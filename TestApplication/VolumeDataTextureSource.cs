using Kokoro.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface
{
    class VolumeDataTextureSource : RawTextureSource, IDisposable
    {
        public IntPtr Data { get; private set; }

        public static VolumeDataTextureSource Load(string dir, int w, int h, int d)
        {
            var vol = new VolumeDataTextureSource(w, h, d, 2, PixelFormat.Red, PixelInternalFormat.R16, PixelType.UnsignedShort);

            ushort max_den = 0;
            for (int i = 0; i < d; i++)
            {
                var data = File.ReadAllBytes(dir + (i + 1));


                for (int j = 0; j < data.Length; j += 2)
                {
                    ushort density = (ushort)(data[j] << 8 | data[j + 1]);
                    density = (ushort)(((uint)density * ushort.MaxValue) / 4096);

                    data[j] = (byte)(density & 0xff);
                    data[j + 1] = (byte)(density >> 8);
                }

                Marshal.Copy(data, 0, vol.Data + (i * w * h * 2), data.Length);
            }

            return vol;
        }

        private VolumeDataTextureSource(int width, int height, int depth, int Bpp, PixelFormat pFormat, PixelInternalFormat iFormat, PixelType pType) : base(3, width, height, depth, 1, pFormat, iFormat, TextureTarget.Texture3D, pType)
        {
            this.Data = Marshal.AllocHGlobal(width * height * depth * Bpp);
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
        ~VolumeDataTextureSource()
        {
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
