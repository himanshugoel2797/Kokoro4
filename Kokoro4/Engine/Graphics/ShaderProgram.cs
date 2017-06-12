using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Math;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif

namespace Kokoro.Engine.Graphics
{
    public class ShaderProgram : IDisposable
    {
        internal IntShaderProgram prog;

        public ShaderProgram(params ShaderSource[] src)
        {
            prog = new IntShaderProgram(src);
        }

        public void Set(string name, TextureHandle handle)
        {
            prog.Set(name, handle);
        }

        public void Set(string name, ImageHandle handle)
        {
            prog.Set(name, handle);
        }

        public void Set(string name, UniformBuffer ubo)
        {
            prog.Set(name, ubo);
        }

        public int GetUniformBlockLocation(string name)
        {
            return prog.GetUniformBlockLocation(name);
        }

        public int GetShaderStorageBufferLocation(string name)
        {
            return prog.GetShaderStorageBufferLocation(name);
        }

        public void SetShaderStorageBufferMapping(string name, int binding)
        {
            prog.SetShaderStorageBufferMapping(name, binding);
        }

        public void SetUniformBufferMapping(string name, int binding)
        {
            prog.SetUniformBufferMapping(name, binding);
        }

        public void Set(string name, Vector3 vec)
        {
            prog.Set(name, vec);
        }

        public void Set(string name, Vector4 vec)
        {
            prog.Set(name, vec);
        }

        public void Set(string name, Vector2 vec)
        {
            prog.Set(name, vec);
        }

        public void Set(string name, Matrix4 vec)
        {
            prog.Set(name, vec);
        }

        public void Set(string name, float val)
        {
            prog.Set(name, val);
        }

        public void Set(string name, int index)
        {
            prog.Set(name, index);
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
                    prog.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ShaderProgram() {
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
