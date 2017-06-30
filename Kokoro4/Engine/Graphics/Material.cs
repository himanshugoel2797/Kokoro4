using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public abstract class Material : EngineRenderable
    {
        public bool Transparent { get; set; }
        public ShaderProgram Shader { get; private set; }
        public bool DepthWrite { get; private set; }
        public BlendFactor Src { get; private set; }
        public BlendFactor Dst { get; private set; }
        public CullFaceMode CullMode { get; private set; }

        public abstract void Apply();
        public abstract ShaderStorageBuffer[] GetSSBOs();
        public abstract UniformBuffer[] GetUBOs();
    }
}
