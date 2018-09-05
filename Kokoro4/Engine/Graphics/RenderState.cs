using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#else

#endif

namespace Kokoro.Engine.Graphics
{
    public class RenderState
    {
        public Framebuffer Framebuffer { get; private set; }
        public ShaderProgram ShaderProgram { get; private set; }
        public ShaderStorageBuffer[] ShaderStorageBufferBindings { get; private set; }
        public UniformBuffer[] UniformBufferBindings { get; private set; }

        public bool DepthWrite { get; private set; }
        public bool ColorWrite { get; private set; }
        public DepthFunc DepthTest { get; private set; }
        public BlendFactor Src { get; private set; }
        public BlendFactor Dst { get; private set; }
        public Vector4 ClearColor { get; private set; }
        public float ClearDepth { get; private set; }
        public float FarPlane { get; private set; }
        public float NearPlane { get; private set; }
        public CullFaceMode CullMode { get; private set; }
        public Vector4[] Viewports { get; private set; }

        public RenderState(Framebuffer fbuf,
                           ShaderProgram prog,
                           ShaderStorageBuffer[] ssboBindings,
                           UniformBuffer[] uboBindings,
                           bool dWrite,
                           bool colorWrite,
                           DepthFunc dTest,
                           float far,
                           float near,
                           BlendFactor src,
                           BlendFactor dst,
                           Vector4 ClearColor,
                           float ClearDepth,
                           CullFaceMode cullMode,
                           Vector4[] viewports = null)
        {
            Framebuffer = fbuf;
            ShaderProgram = prog;
            DepthWrite = dWrite;
            ColorWrite = colorWrite;
            DepthTest = dTest;
            FarPlane = far;
            NearPlane = near;
            Src = src;
            Dst = dst;
            this.ClearColor = ClearColor;
            this.ClearDepth = ClearDepth;
            CullMode = cullMode;
            ShaderStorageBufferBindings = ssboBindings;
            UniformBufferBindings = uboBindings;

            if (viewports == null)
                Viewports = new Vector4[] { new Vector4(0, 0, Framebuffer.Width, Framebuffer.Height) };
            else
                Viewports = viewports;
        }
    }
}
