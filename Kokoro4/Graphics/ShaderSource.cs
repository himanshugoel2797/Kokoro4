using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.IO;

namespace Messier.Graphics
{
    public class ShaderSource : IDisposable
    {
        #region Static Methods
        public static ShaderSource Load(ShaderType sType, string file)
        {
            return new ShaderSource(sType, File.ReadAllText(file));
        }
        #endregion

        internal int id;
        internal ShaderType sType;

        public ShaderSource(ShaderType sType, string src)
        {
            id = GL.CreateShader(sType);
            GL.ShaderSource(id, src);
            GL.CompileShader(id);

            this.sType = sType;

            int result = 0;
            GL.GetShader(id, ShaderParameter.CompileStatus, out result);
            if (result == 0)
            {
                //Fetch the error log
                string errorLog = "";
                GL.GetShaderInfoLog(id, out errorLog);

                GL.DeleteShader(id);

                Console.WriteLine(errorLog);
                throw new Exception("Shader Compilation Exception : " + errorLog);
            }
            GraphicsDevice.Cleanup += Dispose;
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

                if(id != 0)GL.DeleteShader(id);
                id = 0;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        ~ShaderSource() {
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
