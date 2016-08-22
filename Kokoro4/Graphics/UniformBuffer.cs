using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics
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


        internal GPUBuffer buf;
        internal int bindPoint = 0;
        internal Fence readyFence;

        public UniformBuffer()
        {
            buf = new GPUBuffer(BufferTarget.UniformBuffer, UniformBufferSize, false);
            bindPoint = getFreeBindPoint();
            readyFence = new Fence();
            readyFence.PlaceFence();
        }

        public unsafe byte* Update()
        {
            while (!readyFence.Raised(0)) ;
            return (byte*)buf.GetPtr();
        }

        public void UpdateDone()
        {
            readyFence.PlaceFence();
        }

    }
}
