﻿using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics
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
            GPUStateMachine.BindTexture(0, TextureTarget.TextureBuffer, id);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, internalFormat, storage.id);
            GPUStateMachine.UnbindTexture(0, TextureTarget.TextureBuffer);
        }
    }
}
