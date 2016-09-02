using Kokoro.Engine.Graphics;
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

        public void SetBufferObject(int index, GPUBuffer buffer, int elementCount, VertexAttribPointerType type, bool normalize)
        {
            int eSize = sizeof(uint);
            switch (type)
            {
                case VertexAttribPointerType.UnsignedByte:
                case VertexAttribPointerType.Byte:
                    eSize = sizeof(byte);
                    break;
                case VertexAttribPointerType.Double:
                    eSize = sizeof(double);
                    break;
                case VertexAttribPointerType.Float:
                    eSize = sizeof(float);
                    break;
                case VertexAttribPointerType.HalfFloat:
                    eSize = sizeof(float) / 2;
                    break;
                case VertexAttribPointerType.UnsignedInt:
                case VertexAttribPointerType.UnsignedInt2101010Rev:
                case VertexAttribPointerType.Int2101010Rev:
                case VertexAttribPointerType.Int:
                    eSize = sizeof(int);
                    break;
                case VertexAttribPointerType.UnsignedShort:
                case VertexAttribPointerType.Short:
                    eSize = sizeof(short);
                    break;
            }

            GL.EnableVertexArrayAttrib(id, index);
            GL.VertexArrayAttribFormat(id, index, elementCount, (VertexAttribType)type, normalize, 0);
            GL.VertexArrayVertexBuffer(id, index, buffer.id, IntPtr.Zero, elementCount * eSize);
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
