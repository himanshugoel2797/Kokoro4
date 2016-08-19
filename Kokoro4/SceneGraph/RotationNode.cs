using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.SceneGraph
{
    public class RotationNode : Node
    {
        private Quaternion rot;
        public Quaternion Rotation
        {
            get
            {
                return rot;
            }
            set
            {
                rot = value;
                Transform = Matrix4.CreateFromQuaternion(rot);
            }
        }

        public RotationNode(Node parent, string name, Quaternion rot) : base(parent, name)
        {
            Rotation = rot;
        }
    }
}
