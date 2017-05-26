using Cloo;
using Kokoro.Graphics.OpenGL;
using System;

namespace Kokoro.Engine.Graphics
{
    public class AsyncComputeSource : IDisposable
    {
        internal ComputeProgram prog;

        void build_handler(Cloo.Bindings.CLProgramHandle h, System.IntPtr dat)
        {
            if (prog.GetBuildStatus(GraphicsDevice._comp_queue.Device) != ComputeProgramBuildStatus.Success)
            {
                throw new Exception(prog.GetBuildLog(GraphicsDevice._comp_queue.Device));
            }
        }

        public AsyncComputeSource(params string[] src)
        {
            prog = new ComputeProgram(GraphicsDevice._comp_ctxt, src);
            prog.Build(new ComputeDevice[] { GraphicsDevice._comp_queue.Device }, "", build_handler, IntPtr.Zero);

            GraphicsDevice.Cleanup.Add(Dispose);
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
        // ~AsyncComputeSource() {
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