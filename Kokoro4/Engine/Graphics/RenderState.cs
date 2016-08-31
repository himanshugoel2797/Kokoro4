using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class RenderState
    {
        public Framebuffer Framebuffer { get; private set; }
        public ShaderProgram ShaderProgram { get; private set; }
        public bool DepthWrite { get; private set; }
        public DepthFunc DepthTest { get; private set; }
        public BlendFactor Src { get; private set; }
        public BlendFactor Dst { get; private set; }
        public Vector4 ClearColor { get; private set; }
        public float ClearDepth { get; private set; }
        public float FarPlane { get; private set; }
        public float NearPlane { get; private set; }
        public CullFaceMode CullMode { get; private set; }

        public RenderState(Framebuffer fbuf, ShaderProgram prog, bool dWrite, DepthFunc dTest, float far, float near, BlendFactor src, BlendFactor dst, Vector4 ClearColor, float ClearDepth, CullFaceMode cullMode)
        {
            Framebuffer = fbuf;
            ShaderProgram = prog;
            DepthWrite = dWrite;
            DepthTest = dTest;
            FarPlane = far;
            NearPlane = near;
            Src = src;
            Dst = dst;
            this.ClearColor = ClearColor;
            this.ClearDepth = ClearDepth;
            CullMode = cullMode;
        }
    }
}
