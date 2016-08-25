using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Kokoro.Graphics.OpenGL
{
    public class GPUBuffer : IDisposable
    {
        internal int id;
        internal BufferTarget target;
        internal int size;
        public int dataLen;

        private IntPtr addr;

        public GPUBuffer(BufferTarget target)
        {
            GL.CreateBuffers(1, out id);
            this.target = target;
            GraphicsDevice.Cleanup += Dispose;
            addr = IntPtr.Zero;
        }

        public GPUBuffer(BufferTarget target, int size, bool read)
        {
            GL.CreateBuffers(1, out id);
            this.target = target;

            this.size = size;
            
            GL.NamedBufferStorage(id, size, IntPtr.Zero, BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapWriteBit | (read ? BufferStorageFlags.MapReadBit : 0));
            addr = GL.MapNamedBufferRange(id, IntPtr.Zero, size, BufferAccessMask.MapPersistentBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit | BufferAccessMask.MapWriteBit | (read ? BufferAccessMask.MapReadBit : 0));
        }

        public void BufferData<T>(int offset, T[] data, BufferUsageHint hint) where T : struct
        {
            //if (data.Length == 0) return;

            dataLen = data.Length;

            if (data.Length != 0) size = (Marshal.SizeOf(data[0]) * data.Length);

            if (addr == IntPtr.Zero)
            {
                if (data.Length < 1) throw new Exception("Buffer is empty!");
                GL.NamedBufferData(id, size, data, hint);
            }
            else
            {
                throw new Exception("This buffer is mapped!");
            }
        }

        public IntPtr GetPtr()
        {
            return addr;
        }

        public static void FlushAll()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.ClientMappedBufferBarrierBit);
        }

        public void UnMapBuffer()
        {
            GL.UnmapNamedBuffer(id);
        }

        public void MapBuffer(bool read)
        {
            addr = GL.MapNamedBufferRange(id, IntPtr.Zero, size, BufferAccessMask.MapPersistentBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit | BufferAccessMask.MapWriteBit | (read ? BufferAccessMask.MapReadBit : 0));
        }

        public void MapBuffer(bool read, int offset, int size)
        {
            addr = GL.MapNamedBufferRange(id, (IntPtr)offset, size, BufferAccessMask.MapPersistentBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit | BufferAccessMask.MapWriteBit | (read ? BufferAccessMask.MapReadBit : 0));
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        public bool Disposed { get { return disposedValue; } }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                if (addr != IntPtr.Zero)
                {
                    addr = IntPtr.Zero;
                    try
                    {
                        GL.UnmapNamedBuffer(id);
                    }
                    catch (Exception)
                    {

                    }
                }
                GL.DeleteBuffer(id);
                id = 0;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        ~GPUBuffer()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
