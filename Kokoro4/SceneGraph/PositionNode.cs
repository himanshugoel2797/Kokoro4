using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.SceneGraph
{
    public class PositionNode : Node
    {
        private Vector3 pos;
        public Vector3 Position
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
                Transform = Matrix4.CreateTranslation(pos);
            }
        }

        public PositionNode(Node parent, string name, Vector3 pos) : base(parent, name)
        {
            Position = pos;
        }
    }
}
