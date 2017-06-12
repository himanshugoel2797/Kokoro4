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

        static UniformBuffer()
        {
            maxBindPoints = GL.GetInteger(GetPName.MaxUniformBufferBindings);
        }

        const int rungs = 4;
        internal GPUBuffer buf;
        internal int bindPoint = 0;
        internal int curRung = 0;
        internal Fence[] readyFence;
        internal bool dynamic;

        public bool IsReady
        {
            get
            {
                return readyFence[curRung].Raised(1);
            }
        }

        public int Size
        {
            get
            {
                return dynamic ? UniformBufferSize / rungs : UniformBufferSize;
            }
        }

        public UniformBuffer(bool dynamic)
        {
            this.dynamic = dynamic;
            buf = new GPUBuffer(BufferTarget.UniformBuffer, UniformBufferSize, false);
            bindPoint = getFreeBindPoint();

            readyFence = new Fence[dynamic ? rungs : 1];
            for (int i = 0; i < readyFence.Length; i++)
            {
                readyFence[i] = new Fence();
                readyFence[i].PlaceFence();
            }
        }

        internal int GetReadyOffset()
        {
            int idx = curRung;
            for (int i = 0; i < readyFence.Length; i++)
            {
                if (readyFence[idx].Raised(1))
                    return idx * Size;

                if (idx == 0)
                    idx = readyFence.Length - 1;
                else
                    idx--;
            }

            return curRung - 1;
        }

        public unsafe byte* Update()
        {
            if (dynamic) curRung = (curRung + 1) % rungs;
            while (!readyFence[curRung].Raised(0)) ;
            return (byte*)buf.GetPtr() + curRung * Size; ;
        }

        public void UpdateDone()
        {
            buf.FlushBuffer(curRung * Size, Size);
            readyFence[curRung].PlaceFence();
        }



    }
}
