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
        public Math.BoundingBox Bounds { get; set; }

        internal VertexArray mesh;
        internal GPUBuffer verts, indices, uvs, norms;

        public int IndexCount { get; private set; }

        private bool lock_changes = false;

        public Mesh()
        {
            mesh = new VertexArray();
            verts = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer);
            indices = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ElementArrayBuffer);
            uvs = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer);
            norms = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer);
            mesh.SetElementBufferObject(indices);
        }

        public Mesh(Mesh src, bool lockChanges)
        {
            mesh = src.mesh;
            verts = src.verts;
            indices = src.indices;
            uvs = src.uvs;
            norms = src.norms;
            IndexCount = src.IndexCount;
            
            lock_changes = lockChanges;
        }

        public void SetVertices(int offset, float[] vertices, bool Dynamic)
        {
            if (lock_changes) return;
            verts.BufferData(offset, vertices, Dynamic ? OpenTK.Graphics.OpenGL.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(0, verts, 3, OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float);
        }

        public void SetIndices(int offset, uint[] i, bool Dynamic)
        {
            if (lock_changes) return;
            IndexCount = i.Length;
            indices.BufferData(offset, i, Dynamic ? OpenTK.Graphics.OpenGL.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL.BufferUsageHint.StaticDraw);
        }

        public void SetUVs(int offset, float[] uv, bool Dynamic)
        {
            if (lock_changes) return;
            uvs.BufferData(offset, uv, Dynamic ? OpenTK.Graphics.OpenGL.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(1, uvs, 2, OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float);
        }

        public void SetNormals(int offset, float[] n, bool Dynamic)
        {
            if (lock_changes) return;
            norms.BufferData(offset, n, Dynamic ? OpenTK.Graphics.OpenGL.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(2, norms, 3, OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float);
        }

        public void Bind()
        {
            GraphicsDevice.SetVertexArray(mesh);
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
