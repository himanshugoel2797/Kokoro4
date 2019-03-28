using Kokoro.Engine.Graphics;
using Kokoro.Graphics.OpenGL;
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
        private static int alignmentRequirement = 0;
        static ShaderStorageBuffer()
        {
            alignmentRequirement = GL.GetInteger((GetPName)All.ShaderStorageBufferOffsetAlignment);
        }

        const int rungs = 3;

        internal int curRung = 0;
        internal GPUBuffer buf;
        internal Fence[] readyFence;
        internal int size;

        bool dirty = false;
        bool stream = false;

        public int Size
        {
            get { return size; }
        }

        public ShaderStorageBuffer(GPUBuffer buf, bool stream)
        {
            size = buf.size / (stream ? rungs : 1);
            this.buf = buf;
            this.stream = stream;

            readyFence = new Fence[rungs];

            for (int i = 0; i < rungs; i++)
            {
                readyFence[i] = new Fence();
                readyFence[i].PlaceFence();
            }
        }

        private static int AlignSize(int size)
        {
            if (size % alignmentRequirement == 0) return size;
            return size + (alignmentRequirement - (size % alignmentRequirement));
        }

        public ShaderStorageBuffer(int size, bool stream) : this(new GPUBuffer(BufferTarget.ShaderStorageBuffer, AlignSize(size) * (stream ? rungs : 1), false), stream)
        {

        }

        internal int GetReadyOffset()
        {
            if (!stream)
                return 0;

            int idx = curRung;
            for(int i = 0; i < rungs; i++)
            {
                if (readyFence[idx].Raised(1))
                    return idx * size;

                if (idx == 0)
                    idx = rungs - 1;
                else
                    idx--;
            }

            return curRung - 1;
        }

        public unsafe byte* Update()
        {
            if (stream) curRung = (curRung + 1) % rungs;
            while (!readyFence[curRung].Raised(0)) ;// System.Threading.Thread.Sleep(1);
            return (byte*)buf.GetPtr() + curRung * size;
        }

        public void UpdateDone()
        {
            buf.FlushBuffer(curRung * size, size);
            readyFence[curRung].PlaceFence();
        }

        public bool IsReady
        {
            get
            {
                return readyFence[curRung].Raised(1);
            }
        }

        public static explicit operator GPUBuffer(ShaderStorageBuffer b)
        {
            return b.buf;
        }

    }
}
