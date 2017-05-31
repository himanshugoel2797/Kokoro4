using Kokoro.Engine.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class BufferTexture
    {
        internal int id;
        internal GPUBuffer buffer;
        internal Fence fence;

        public BufferTexture(int size, SizedInternalFormat iFormat)
        {
            id = GL.GenTexture();
            buffer = new GPUBuffer(BufferTarget.TextureBuffer, size, false);

            GL.TextureBuffer(id, iFormat, buffer.id);
        }
    }
}
