using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.OpenGL
{
    public class VertexArray : IDisposable
    {
        internal int id;

        public VertexArray()
        {
            GL.CreateVertexArrays(1, out id);
            GraphicsDevice.Cleanup += Dispose;
        }

        public void SetElementBufferObject(GPUBuffer buffer)
        {
            GL.VertexArrayElementBuffer(id, buffer.id);
        }

        public void SetBufferObject(int index, GPUBuffer buffer, int elementCount, VertexAttribPointerType type)
        {
            GL.EnableVertexArrayAttrib(id, index);

            GL.VertexArrayAttribFormat(id, index, elementCount, (VertexAttribType)type, false, 0);
            GL.VertexArrayVertexBuffer(id, index, buffer.id, IntPtr.Zero, elementCount * 4);
            GL.VertexArrayAttribBinding(id, index, index);
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

                GL.DeleteVertexArray(id);
                id = 0;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        ~VertexArray()
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
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
