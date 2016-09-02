using Kokoro.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Engine.Graphics;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif

namespace Kokoro.Engine
{
    public class Mesh : IDisposable
    {
        [System.Security.SuppressUnmanagedCodeSecurity]
        [System.Runtime.InteropServices.DllImport("Kokoro4.Native.dll", EntryPoint = "LoadMesh")]
        extern static int LoadMesh(string file, IntPtr[] ptrs);//CAl

        public Math.BoundingBox Bounds { get; set; }

        public int IndexCount { get; private set; }

        private int offset = 0;
        private bool lock_changes = false;

        public Mesh(int vertex_count, int index_count, string file)
        {
            IndexCount = index_count;

            int alloc_size = global::System.Math.Max(vertex_count, index_count);
            if (LoadMesh(file, MemoryAllocator.AllocateMemory(alloc_size, out offset)) != 0)
                throw new Exception("Failed to load mesh");
        }

        public Mesh(float[] vertices, float[] uv, float[] norm, ushort[] indice)
        {
            //These allocations can't be managed by the scene manager, best to keep them limited
            IndexCount = indice.Length;

            int alloc_size = global::System.Math.Max(vertices.Length, indice.Length);
            IntPtr[] ptrs = MemoryAllocator.AllocateMemory(alloc_size, out offset);

            short[] temp = new short[indice.Length];
            System.Buffer.BlockCopy(indice, 0, temp, 0, temp.Length * 2);

            System.Runtime.InteropServices.Marshal.Copy(temp, 0, ptrs[(int)MemoryAllocator.IntPtrIndex.Index], indice.Length);
            System.Runtime.InteropServices.Marshal.Copy(uv, 0, ptrs[(int)MemoryAllocator.IntPtrIndex.UV], uv.Length);
            System.Runtime.InteropServices.Marshal.Copy(norm, 0, ptrs[(int)MemoryAllocator.IntPtrIndex.Normal], norm.Length);
            System.Runtime.InteropServices.Marshal.Copy(vertices, 0, ptrs[(int)MemoryAllocator.IntPtrIndex.Vertex], vertices.Length);
        }

        public Mesh(Mesh src, bool lockChanges)
        {
            IndexCount = src.IndexCount;
            
            lock_changes = lockChanges;
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

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EngineObject() {
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
