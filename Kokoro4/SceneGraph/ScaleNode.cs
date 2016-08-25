using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.SceneGraph
{
    public class ScaleNode : Node
    {
        private Vector3 scale;
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                Transform = Matrix4.Scale(scale);
            }
        }

        public ScaleNode(Node parent, string name, ulong layerMask, Vector3 scale) : base(parent, name, layerMask)
        {
            Scale = scale;
        }
    }
}
