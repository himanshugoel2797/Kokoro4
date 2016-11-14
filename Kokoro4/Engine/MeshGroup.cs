using Kokoro.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif


namespace Kokoro.Engine
{
    public class MeshGroup : IDisposable
    {
        GPUBuffer vertices, uvs, normals, indices;
        internal VertexArray varray;
        private int offset, vertex_cnt, index_cnt;
        
        public enum IntPtrIndex
        {
            Index = 0,
            Vertex = 1,
            UV = 2,
            Normal = 3
        }

        public MeshGroup(int vertex_cnt, int index_cnt)
        {
            vertices = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer, vertex_cnt * 3 * sizeof(float), false);
            uvs = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer, vertex_cnt * 2 * sizeof(ushort), false);
            normals = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer, vertex_cnt * sizeof(uint), false);
            indices = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ElementArrayBuffer, index_cnt * sizeof(ushort), false);

            varray = new VertexArray();
            varray.SetBufferObject(0, vertices, 3, OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float, false);
            varray.SetBufferObject(1, uvs, 2, OpenTK.Graphics.OpenGL.VertexAttribPointerType.UnsignedShort, false);
            varray.SetBufferObject(2, normals, 2, OpenTK.Graphics.OpenGL.VertexAttribPointerType.Short, false);
            varray.SetElementBufferObject(indices);

            offset = 0;
            this.vertex_cnt = vertex_cnt;
            this.index_cnt = index_cnt;

            //NOTE: Meshes are automatically expelled from memory as needed, the engine knows about which meshes should be visible based on the player's position in the scene, thus it can handle evicting meshes as necessary
            //Have multiple block sizes, 65536, 16384, 4096, 1024, 256, 64, 16
        }

        public IntPtr[] AllocateMemory(int cnt, out int offset)
        {
            if (this.offset + cnt >= vertex_cnt | this.offset + cnt >= index_cnt)
                throw new OutOfMemoryException();

            offset = this.offset;
            this.offset += cnt;

            IntPtr[] result = new IntPtr[4];
            result[(int)IntPtrIndex.Index] = indices.GetPtr() + (offset * sizeof(ushort));
            result[(int)IntPtrIndex.Vertex] = vertices.GetPtr() + (offset * 3 * sizeof(float));
            result[(int)IntPtrIndex.UV] = uvs.GetPtr() + (offset * 2 * sizeof(float));
            result[(int)IntPtrIndex.Normal] = normals.GetPtr() + (offset * sizeof(uint));

            return result;
        }

        public void FlushBuffer(IntPtrIndex idx, int offset, int size)
        {
            switch (idx)
            {
                case IntPtrIndex.Index:
                    indices.FlushBuffer(indices.GetPtr() + (offset * sizeof(ushort)), size * sizeof(ushort));
                    break;
                case IntPtrIndex.Normal:
                    normals.FlushBuffer(normals.GetPtr() + (offset * sizeof(uint)), size * sizeof(uint));
                    break;
                case IntPtrIndex.UV:
                    uvs.FlushBuffer(uvs.GetPtr() + (offset * 2 * sizeof(float)), size * sizeof(float) * 2);
                    break;
                case IntPtrIndex.Vertex:
                    vertices.FlushBuffer(vertices.GetPtr() + (offset * 3 * sizeof(float)), size * sizeof(float) * 3);
                    break;
            }

            //Place a fence here to allow freeing to be deferred to when the resource is no longer in use
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
        // ~MeshGroup() {
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
