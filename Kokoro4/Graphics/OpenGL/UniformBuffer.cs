using Kokoro.Engine.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class UniformBuffer
    {
        #region Bind point allocation
        private static int freebindPoint = 0;
        private static int maxBindPoints = 0;

        private static int getFreeBindPoint()
        {
            if (freebindPoint >= maxBindPoints)
                throw new Exception("Too many UBOs!");
            return (freebindPoint++ % maxBindPoints);
        }
        #endregion


        private const int UniformBufferSize = 16 * 1024;
        public const int UniformBufferMaxSize = (UniformBufferSize / rungs);

        static UniformBuffer()
        {
            maxBindPoints = GL.GetInteger(GetPName.MaxUniformBufferBindings);
        }

        const int rungs = 4;
        internal GPUBuffer buf;
        internal int bindPoint = 0;
        internal int curRung = 0;
        internal Fence[] readyFence;

        public bool IsReady
        {
            get
            {
                return readyFence[curRung].Raised(1);
            }
        }

        public UniformBuffer()
        {
            buf = new GPUBuffer(BufferTarget.UniformBuffer, UniformBufferSize, false);
            bindPoint = getFreeBindPoint();

            readyFence = new Fence[rungs];
            for (int i = 0; i < rungs; i++)
            {
                readyFence[i] = new Fence();
                readyFence[i].PlaceFence();
            }
        }

        public unsafe byte* Update()
        {
            curRung = (curRung + 1) % rungs;
            while (!readyFence[curRung].Raised(0)) ;
            return (byte*)buf.GetPtr() + curRung * UniformBufferMaxSize; ;
        }

        public void UpdateDone()
        {
            buf.FlushBuffer(curRung * UniformBufferMaxSize, UniformBufferMaxSize);
            readyFence[curRung].PlaceFence();
        }



    }
}
