using Kokoro.Engine.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.OpenGL
{
    public class ShaderStorageBuffer
    {
        #region Bind point allocation
        private static int freebindPoint = 0;
        private static int maxBindPoints = 0;

        private static int getFreeBindPoint()
        {
            if (freebindPoint >= maxBindPoints)
                throw new Exception("Too many SSBOs!");
            return (freebindPoint++ % maxBindPoints);
        }
        #endregion

        static ShaderStorageBuffer()
        {
            maxBindPoints = GL.GetInteger((GetPName)All.MaxShaderStorageBufferBindings);
        }


        internal GPUBuffer buf;
        internal int bindPoint = 0;
        internal Fence readyFence;

        bool dirty = false;

        public ShaderStorageBuffer(GPUBuffer buf)
        {
            this.buf = buf;
            bindPoint = getFreeBindPoint();
            readyFence = new Fence();
            readyFence.PlaceFence();
        }

        public ShaderStorageBuffer(int size) : this(new GPUBuffer(BufferTarget.ShaderStorageBuffer, size, false))
        {

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

        public static explicit operator GPUBuffer(ShaderStorageBuffer b)
        {
            return b.buf;
        }

    }
}
