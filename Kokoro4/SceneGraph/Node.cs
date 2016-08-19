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
        public Matrix4 Transform { get; protected set; }
        public Matrix4 NetTransform { get; private set; }

        public Node Parent { get; set; }
        public List<Node> Children { get; set; }

        public string Name { get; set; }

        public Node(Node parent, string name)
        {
            Parent = parent;
            Children = new List<Node>();
            Transform = Matrix4.Identity;
            NetTransform = Transform;
            Name = name;
        }

        public void UpdateTree()
        {
            //Update the current transform
            if (Parent != null)
                NetTransform = Parent.NetTransform * Transform;

            Children.AsParallel().ForAll(a => a?.UpdateTree());
        }
    }
}
