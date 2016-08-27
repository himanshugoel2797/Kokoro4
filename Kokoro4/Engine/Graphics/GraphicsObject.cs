using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class GraphicsObject : EngineRenderable
    {
        public Material Material { get; private set; }
        public ShaderGroup Shaders { get; private set; }
        public Mesh Mesh { get; private set; }

        public GraphicsObject(Mesh mesh, Material mat, ShaderGroup grp)
        {
            Material = mat;
            Mesh = mesh;
            Shaders = grp;
        }
    }
}
