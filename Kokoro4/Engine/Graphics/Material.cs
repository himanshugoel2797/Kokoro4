using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class Material : EngineRenderable
    {
        public bool Transparent { get; set; }
        public ShaderGroup Shaders { get; set; }
        public Dictionary<string, TextureHandle> Textures { get; set; }
    }
}
