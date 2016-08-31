using Cloo;
using Kokoro.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Kokoro.Engine.Graphics
{
    public class AsyncComputeProgram : IDisposable
    {
        internal ComputeKernel kern;
        internal List<ComputeMemory> Objects;

        public AsyncComputeProgram(AsyncComputeSource src, string entryPoint)
        {
            kern = src.prog.CreateKernel(entryPoint);
            GraphicsDevice.Cleanup += Dispose;
            Objects = new List<ComputeMemory>();
        }

        public void Set(int index, TextureHandle value, bool read, bool write, int miplevel)
        {
            ComputeMemoryFlags flags = ComputeMemoryFlags.None;
            if (read && write) flags |= ComputeMemoryFlags.ReadWrite;
            else if (read && !write) flags |= ComputeMemoryFlags.ReadOnly;
            else if (!read && write) flags |= ComputeMemoryFlags.WriteOnly;

            while(Objects.Count <= index)
            {
                Objects.Add(null);
            }


            kern.SetMemoryArgument(index, value.GetImageForCompute(flags, miplevel));
            Objects[index] = (value.GetImageForCompute(flags, miplevel));
        }

        public void Set(int index, GPUBuffer value, bool read, bool write)
        {
            ComputeMemoryFlags flags = ComputeMemoryFlags.None;
            if (read && write) flags |= ComputeMemoryFlags.ReadWrite;
            else if (read && !write) flags |= ComputeMemoryFlags.ReadOnly;
            else if (!read && write) flags |= ComputeMemoryFlags.WriteOnly;

            while (Objects.Count <= index)
            {
                Objects.Add(null);
            }

            kern.SetMemoryArgument(index, value.GetComputeBuffer(flags));
            Objects[index] = (value.GetComputeBuffer(flags));
        }

        public void Set(int index, UniformBuffer value, bool read, bool write)
        {
            ComputeMemoryFlags flags = ComputeMemoryFlags.None;
            if (read && write) flags |= ComputeMemoryFlags.ReadWrite;
            else if (read && !write) flags |= ComputeMemoryFlags.ReadOnly;
            else if (!read && write) flags |= ComputeMemoryFlags.WriteOnly;

            while (Objects.Count <= index)
            {
                Objects.Add(null);
            }

            kern.SetMemoryArgument(index, value.buf.GetComputeBuffer(flags));
            Objects[index] = (value.buf.GetComputeBuffer(flags));
        }

        public void Set(int index, ShaderStorageBuffer value, bool read, bool write)
        {
            ComputeMemoryFlags flags = ComputeMemoryFlags.None;
            if (read && write) flags |= ComputeMemoryFlags.ReadWrite;
            else if (read && !write) flags |= ComputeMemoryFlags.ReadOnly;
            else if (!read && write) flags |= ComputeMemoryFlags.WriteOnly;

            while (Objects.Count <= index)
            {
                Objects.Add(null);
            }

            kern.SetMemoryArgument(index, value.buf.GetComputeBuffer(flags));
            Objects[index] = (value.buf.GetComputeBuffer(flags));
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
                    kern.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AsyncComputeProgram() {
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