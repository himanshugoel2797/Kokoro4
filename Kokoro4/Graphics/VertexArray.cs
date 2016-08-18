using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics
{
    public class VertexArray : IDisposable
    {
        internal int id;

        public VertexArray()
        {
            id = GL.GenVertexArray();
            GraphicsDevice.Cleanup += Dispose;
        }

        public void SetBufferObject(int index, GPUBuffer buffer, int elementCount, VertexAttribPointerType type)
        {
            GPUStateMachine.BindVertexArray(id);

            GL.EnableVertexAttribArray(index);
            GPUStateMachine.BindBuffer(buffer.target, buffer.id);
            GL.VertexAttribPointer(index, elementCount, type, false, 0, 0);
            GPUStateMachine.UnbindBuffer(buffer.target);

            GPUStateMachine.UnbindVertexArray();
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
