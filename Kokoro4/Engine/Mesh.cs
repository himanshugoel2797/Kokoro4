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
    /// <summary>
    /// Represents a collection of vertices, indices, normals and UVs
    /// </summary>
    public class Mesh : IDisposable
    {
        [System.Security.SuppressUnmanagedCodeSecurity]
        [System.Runtime.InteropServices.DllImport("Kokoro4.Native.dll", EntryPoint = "LoadMesh")]
        extern static int LoadMesh(string file, IntPtr[] ptrs);//CAl

        public Math.BoundingBox Bounds { get; set; }

        public int IndexCount { get; private set; }
        public int StartOffset { get { return offset; } }

        public MeshGroup Parent { get; private set; }

        private int offset = 0;
        private bool lock_changes = false;

        public Mesh(MeshGroup parent, int vertex_count, int index_count, string file)
        {
            IndexCount = index_count;
            Parent = parent;

            int alloc_size = global::System.Math.Max(vertex_count, index_count);
            if (LoadMesh(file, Parent.AllocateMemory(alloc_size, out offset)) != 0)
                throw new Exception("Failed to load mesh");
        }

        public Mesh(MeshGroup parent, float[] vertices, float[] uv, float[] norm, ushort[] indice)
        {
            //TODO: A mesh shouldn't expect the raw data, it should expect the offset, length and a mesh group object
            //A group object is 
            //A mesh group object provides the backing storage for the data
            //This arrangement allows for cleaner batching of meshes when possible
            //Add a new class that can take individual mesh objects and bucket them based on the rendering properties
            //These buckets can then be individually submitted for drawing


            //These allocations can't be managed by the scene manager, best to keep them limited
            IndexCount = indice.Length;
            Parent = parent;

            int alloc_size = global::System.Math.Max(vertices.Length, indice.Length);
            IntPtr[] ptrs = Parent.AllocateMemory(alloc_size, out offset);

            short[] temp = new short[indice.Length];
            System.Buffer.BlockCopy(indice, 0, temp, 0, temp.Length * 2);

            System.Runtime.InteropServices.Marshal.Copy(temp, 0, ptrs[(int)MeshGroup.IntPtrIndex.Index], indice.Length);
            System.Runtime.InteropServices.Marshal.Copy(uv, 0, ptrs[(int)MeshGroup.IntPtrIndex.UV], uv.Length);
            System.Runtime.InteropServices.Marshal.Copy(norm, 0, ptrs[(int)MeshGroup.IntPtrIndex.Normal], norm.Length);
            System.Runtime.InteropServices.Marshal.Copy(vertices, 0, ptrs[(int)MeshGroup.IntPtrIndex.Vertex], vertices.Length);
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
