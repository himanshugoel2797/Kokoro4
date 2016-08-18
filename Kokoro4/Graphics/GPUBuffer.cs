using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace Kokoro.Graphics
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
            id = GL.GenBuffer();
            this.target = target;
            GraphicsDevice.Cleanup += Dispose;
            addr = IntPtr.Zero;
        }

        public GPUBuffer(BufferTarget target, int size, bool read)
        {
            id = GL.GenBuffer();
            this.target = target;

            this.size = size;

            GPUStateMachine.BindBuffer(target, id);
            GL.BufferStorage(target, (IntPtr)size, IntPtr.Zero, BufferStorageFlags.MapPersistentBit | BufferStorageFlags.MapWriteBit | (read ? BufferStorageFlags.MapReadBit : 0));

            addr = GL.MapBufferRange(target, IntPtr.Zero, (IntPtr)size, BufferAccessMask.MapPersistentBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapInvalidateBufferBit | BufferAccessMask.MapFlushExplicitBit | BufferAccessMask.MapWriteBit | (read ? BufferAccessMask.MapReadBit : 0));
            GPUStateMachine.UnbindBuffer(target);
        }

        public void BufferData<T>(int offset, T[] data, BufferUsageHint hint) where T : struct
        {
            //if (data.Length == 0) return;

            dataLen = data.Length;

            if (data.Length != 0) size = (Marshal.SizeOf(data[0]) * data.Length);

            if (addr == IntPtr.Zero)
            {
                if (data.Length < 1) throw new Exception("Buffer is empty!");
                GPUStateMachine.BindBuffer(target, id);

                GL.BufferData(target, (IntPtr)size, data, hint);

                GPUStateMachine.UnbindBuffer(target);
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
            GPUStateMachine.BindBuffer(target, id);
            GL.UnmapBuffer(target);
            GPUStateMachine.UnbindBuffer(target);
        }

        public void MapBuffer(bool read)
        {
            GPUStateMachine.BindBuffer(target, id);
            addr = GL.MapBufferRange(target, IntPtr.Zero, (IntPtr)size, BufferAccessMask.MapPersistentBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit | BufferAccessMask.MapInvalidateBufferBit | BufferAccessMask.MapWriteBit | (read ? BufferAccessMask.MapReadBit : 0));
            GPUStateMachine.UnbindBuffer(target);
        }

        public void MapBuffer(bool read, int offset, int size)
        {
            GPUStateMachine.BindBuffer(target, id);
            addr = GL.MapBufferRange(target, (IntPtr)offset, (IntPtr)size, BufferAccessMask.MapPersistentBit | BufferAccessMask.MapUnsynchronizedBit | BufferAccessMask.MapFlushExplicitBit | BufferAccessMask.MapInvalidateBufferBit | BufferAccessMask.MapWriteBit | (read ? BufferAccessMask.MapReadBit : 0));
            GPUStateMachine.UnbindBuffer(target);
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
                        GL.BindBuffer(target, id);
                        GL.UnmapBuffer(target);
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
