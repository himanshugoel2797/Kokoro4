using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.SceneGraph
{
    public class Node
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public Matrix4 Transform { get; set; }

        public Node Parent { get; set; }
        public List<Node> Children { get; set; }

        public string Name { get; set; }

        public Node(Node parent, string name)
        {
            Parent = parent;
            Children = new List<Node>();
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
            Transform = Matrix4.Identity;
            Name = name;
        }
    }
}
