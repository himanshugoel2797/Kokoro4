﻿using Kokoro.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.SceneGraph
{
    public class EngineObject : IDisposable
    {
        public Math.BoundingBox Bounds { get; set; }

        internal VertexArray mesh;
        internal GPUBuffer verts, indices, uvs, norms;
        internal List<Texture> textures;

        public int IndexCount { get; private set; }

        private bool lock_changes = false;

        public EngineObject()
        {
            mesh = new VertexArray();
            verts = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            indices = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer);
            uvs = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            norms = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            textures = new List<Texture>();
        }

        public EngineObject(EngineObject src, bool lockChanges)
        {
            mesh = src.mesh;
            verts = src.verts;
            indices = src.indices;
            uvs = src.uvs;
            norms = src.norms;
            IndexCount = src.IndexCount;

            textures = new List<Texture>();
            lock_changes = lockChanges;
        }

        public void SetVertices(int offset, float[] vertices, bool Dynamic)
        {
            if (lock_changes) return;
            verts.BufferData(offset, vertices, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(0, verts, 3, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetIndices(int offset, uint[] i, bool Dynamic)
        {
            if (lock_changes) return;
            IndexCount = i.Length;
            indices.BufferData(offset, i, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }

        public void SetUVs(int offset, float[] uv, bool Dynamic)
        {
            if (lock_changes) return;
            uvs.BufferData(offset, uv, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(1, uvs, 2, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetNormals(int offset, float[] n, bool Dynamic)
        {
            if (lock_changes) return;
            norms.BufferData(offset, n, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(2, norms, 3, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetTexture(int slot, Texture tex)
        {
            while (textures.Count <= slot)
            {
                textures.Add(null);
            }
            textures[slot] = tex;
        }

        public void Bind()
        {
            for (int i = 0; i < textures.Count; i++)
                GraphicsDevice.SetTexture(i, textures[i]);

            GraphicsDevice.SetIndexBuffer(indices);
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
