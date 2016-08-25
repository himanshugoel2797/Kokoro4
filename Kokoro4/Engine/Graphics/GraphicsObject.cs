using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class GraphicsObject
    {
        public Material Material { get; private set; }

        public GraphicsObject(Material mat)
        {
            Material = mat;
        }
    }
}
