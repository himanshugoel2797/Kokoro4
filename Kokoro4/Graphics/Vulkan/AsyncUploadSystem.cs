using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Kokoro.Graphics.Vulkan
{
    public static class AsyncUploadSystem
    {
        private struct UploadRequest
        {

        }

        private static ConcurrentQueue<UploadRequest> requests;
        private static bool ExitSystem = false;

        private static void Uploader()
        {
            int idleCnt = 0;
            while (true)
            {
                if(requests.Count == 0)
                    idleCnt++;
                else
                {
                    idleCnt = 0;

                }

                if (idleCnt >= 5000) Thread.Yield();
            }
        }

        public static void StartSystem(int transfer_queue)
        {
            requests = new ConcurrentQueue<UploadRequest>();
            Thread t = new Thread(Uploader);
            t.Start();
        }
    }
}
