using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.OpenGL
{
    public class Fence : IDisposable
    {
        IntPtr id = IntPtr.Zero;
        bool raised = false;

        public void PlaceFence()
        {
            if (id != IntPtr.Zero) GL.DeleteSync(id);
            id = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
            raised = false;
        }

        public bool Raised(long timeout)
        {
            if (raised)
            {
                return true;
            }

            if (timeout == 0)
            {
                WaitSyncStatus s = WaitSyncStatus.WaitFailed;
                while (s != WaitSyncStatus.ConditionSatisfied && s != WaitSyncStatus.AlreadySignaled)
                {
                    s = GL.ClientWaitSync(id, ClientWaitSyncFlags.SyncFlushCommandsBit, 10);
                }
                GL.DeleteSync(id);
                raised = true;
                return true;
            }
            else
            {
                WaitSyncStatus s = GL.ClientWaitSync(id, ClientWaitSyncFlags.SyncFlushCommandsBit, timeout);

                if (s == WaitSyncStatus.ConditionSatisfied || s == WaitSyncStatus.AlreadySignaled)
                {
                    GL.DeleteSync(id);
                    raised = true;
                    return true;
                }
                else return false;
            }
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

                if(!raised)GL.DeleteSync(id);
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        ~Fence()
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
