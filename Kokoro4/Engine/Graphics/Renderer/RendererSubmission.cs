using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Renderer
{
    public class RendererSubmission
    {
        public Material Material { get; private set; }
        public Mesh Mesh { get; private set; }
        public int InstanceCount { get; private set; }
        public int BaseInstance { get; private set; }

        public RendererSubmission(Material mat, bool transient)
        {

        }

        public void Add(Mesh mesh, int instCount, int baseInst)
        {

        }

        public void Clear()
        {

        }
    }
}
