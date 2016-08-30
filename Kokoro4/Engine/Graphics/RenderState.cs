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
        public BlendFunction AlphaBlend { get; private set; }
        public Vector4 ClearColor { get; private set; }
        public float ClearDepth { get; private set; }
    }
}
