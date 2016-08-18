using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics
{
    public class Fence
    {
        IntPtr id = IntPtr.Zero;

        public void PlaceFence()
        {
            if (id != IntPtr.Zero) GL.DeleteSync(id);
            id = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
        }

        public bool Raised(long timeout)
        {
            if (timeout == 0)
            {
                WaitSyncStatus s = WaitSyncStatus.WaitFailed;
                while (s != WaitSyncStatus.ConditionSatisfied && s != WaitSyncStatus.AlreadySignaled)
                {
                    s = GL.ClientWaitSync(id, ClientWaitSyncFlags.SyncFlushCommandsBit, 10);
                }
                return true;
            }
            else
            {
                WaitSyncStatus s = GL.ClientWaitSync(id, ClientWaitSyncFlags.SyncFlushCommandsBit, timeout);

                if (s == WaitSyncStatus.ConditionSatisfied || s == WaitSyncStatus.AlreadySignaled) return true;
                else return false;
            }
        }
    }
}
