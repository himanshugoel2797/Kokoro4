﻿using Kokoro.Engine.Graphics;
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
    public enum MeshGroupVertexFormat
    {
        X32F_Y32F_Z32F,
        X10UI_Y10UI_Z10UI_MISC2UI
    }

    public class MeshGroup : IDisposable
    {
        internal GPUBuffer vertices, uvs, normals, indices;
        internal Fence verticeF, uvF, normalF, indiceF;

        internal VertexArray varray;
        private int offset, vertex_cnt, index_cnt;

        private int vertex_comp_cnt = 0;
        private int vertex_size = 0;

        public int VertexCount
        {
            get
            {
                return vertex_cnt;
            }
        }

        public int IndexCount
        {
            get
            {
                return index_cnt;
            }
        }

        public bool IsReady
        {
            get
            {
                return verticeF.Raised(1) && uvF.Raised(1) && normalF.Raised(1) && indiceF.Raised(1);
            }
        }

        public enum IntPtrIndex
        {
            Index = 0,
            Vertex = 1,
            UV = 2,
            Normal = 3
        }

        public MeshGroup(MeshGroupVertexFormat vFormat, int vertex_cnt, int index_cnt = 0)
        {
            OpenTK.Graphics.OpenGL.VertexAttribPointerType vAttribPtrType = 0;
            switch (vFormat)
            {
                case MeshGroupVertexFormat.X32F_Y32F_Z32F:
                    vertex_comp_cnt = 3;
                    vertex_size = 3 * sizeof(float);
                    vAttribPtrType = OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float;
                    break;
                case MeshGroupVertexFormat.X10UI_Y10UI_Z10UI_MISC2UI:
                    vertex_comp_cnt = 1;
                    vertex_size = sizeof(uint);
                    vAttribPtrType = OpenTK.Graphics.OpenGL.VertexAttribPointerType.UnsignedInt2101010Rev;
                    break;
            }


            vertices = new GPUBuffer(Kokoro.Engine.Graphics.BufferTarget.ArrayBuffer, vertex_cnt * vertex_size, false);
            uvs = new GPUBuffer(Kokoro.Engine.Graphics.BufferTarget.ArrayBuffer, vertex_cnt * 2 * sizeof(float), false);
            normals = new GPUBuffer(Kokoro.Engine.Graphics.BufferTarget.ArrayBuffer, vertex_cnt * sizeof(uint), false);
            if(index_cnt != 0) indices = new GPUBuffer(Kokoro.Engine.Graphics.BufferTarget.ElementArrayBuffer, index_cnt * sizeof(ushort), false);

            verticeF = new Fence();
            uvF = new Fence();
            normalF = new Fence();
            if (index_cnt != 0) indiceF = new Fence();

            verticeF.PlaceFence();
            uvF.PlaceFence();
            normalF.PlaceFence();
            if (index_cnt != 0) indiceF.PlaceFence();

            varray = new VertexArray();
            varray.SetBufferObject(0, vertices, vertex_comp_cnt, vAttribPtrType, false);
            varray.SetBufferObject(1, uvs, 2, OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float, false);
            varray.SetBufferObject(2, normals, 2, OpenTK.Graphics.OpenGL.VertexAttribPointerType.Short, false);
            if (index_cnt != 0) varray.SetElementBufferObject(indices);

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
            if (index_cnt != 0) result[(int)IntPtrIndex.Index] = indices.GetPtr() + (offset * sizeof(ushort));
            result[(int)IntPtrIndex.Vertex] = vertices.GetPtr() + (offset * vertex_size);
            result[(int)IntPtrIndex.UV] = uvs.GetPtr() + (offset * 2 * sizeof(float));
            result[(int)IntPtrIndex.Normal] = normals.GetPtr() + (offset * sizeof(uint));

            return result;
        }

        public void FlushBuffer(IntPtrIndex idx, int offset, int size)
        {
            switch (idx)
            {
                case IntPtrIndex.Index:
                    if (index_cnt != 0)
                    {
                        indices.FlushBuffer((offset * sizeof(ushort)), size * sizeof(ushort));
                        indiceF.PlaceFence();
                    }
                    break;
                case IntPtrIndex.Normal:
                    normals.FlushBuffer((offset * sizeof(uint)), size * sizeof(uint));
                    normalF.PlaceFence();
                    break;
                case IntPtrIndex.UV:
                    uvs.FlushBuffer((offset * 2 * sizeof(float)), size * sizeof(float) * 2);
                    uvF.PlaceFence();
                    break;
                case IntPtrIndex.Vertex:
                    vertices.FlushBuffer((offset * 3 * sizeof(float)), size * vertex_size);
                    verticeF.PlaceFence();
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
