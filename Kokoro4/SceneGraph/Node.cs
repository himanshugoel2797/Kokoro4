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
        private bool _dirty;
        private bool dirty
        {
            get { return _dirty; }
            set
            {
                _dirty = value;

                if (_dirty)
                {
                    Children.AsParallel().ForAll(a => a.dirty = true);
                }
            }
        }
        private Matrix4 _transform;
        public Matrix4 Transform
        {
            get
            {
                return _transform;
            }
            protected set
            {
                dirty = (_transform != value);
                _transform = value;
            }
        }
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
            dirty = true;
        }

        public void UpdateTree()
        {
            //Update the current transform
            if (Parent != null && dirty)
            {
                NetTransform = Parent.NetTransform * Transform;
                dirty = false;
            }

            Children.AsParallel().ForAll(a => a?.UpdateTree());
        }
    }
}
