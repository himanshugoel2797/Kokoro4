using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics
{
    public class BufferTexture
    {
        internal int id;

        public BufferTexture()
        {
            id = GL.GenTexture();
        }

        public void SetStorage(GPUBuffer storage, SizedInternalFormat internalFormat)
        {
            GL.TextureBuffer(id, internalFormat, storage.id);
        }
    }
}
