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
    public class ShaderStorageBuffer
    {
        internal GPUBuffer buf;
        internal Fence readyFence;

        bool dirty = false;

        public ShaderStorageBuffer(GPUBuffer buf)
        {
            this.buf = buf;
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

        public bool IsReady()
        {
            return readyFence.Raised(1);
        }

        public static explicit operator GPUBuffer(ShaderStorageBuffer b)
        {
            return b.buf;
        }

    }
}
