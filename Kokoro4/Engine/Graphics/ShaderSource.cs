﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif

namespace Kokoro.Engine.Graphics
{
    public class ShaderSource : IDisposable
    {
        #region Static Methods
        public static ShaderSource Load(ShaderType sType, string file)
        {
            return new ShaderSource(sType, File.ReadAllText(file));
        }

        public static ShaderSource Load(ShaderType sType, string file, string libraryName)
        {
            return new ShaderSource(sType, File.ReadAllText(file), libraryName);
        }
        #endregion

        internal IntShaderSource shader_src;

        public ShaderSource(ShaderType sType, string src, string libraryName = null)
        {
            shader_src = new IntShaderSource(sType, src, libraryName);
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
                    shader_src.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ShaderSource() {
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
