#if VULKAN
using Kokoro.Engine.Graphics;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Vulkan
{
    public class IntShaderProgram : IDisposable
    {
        public IntShaderProgram(params ShaderSource[] shaders)
        {
            //Put all the shaders into a PipelineShaderStageCreateInfo structure
        }

        public void Set(string name, int val)
        {

        }

        public void Set(string name, float val)
        {

        }

        public void Set(string name, Vector2 val)
        {

        }

        public void Set(string name, Vector2I val)
        {

        }

        public void Set(string name, Vector3 val)
        {

        }

        public void Set(string name, Vector4 val)
        {

        }

        public void Set(string name, Matrix4 val)
        {

        }

        public void Set(string name, TextureView view)
        {

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
        // ~IntShaderProgram() {
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
#endif