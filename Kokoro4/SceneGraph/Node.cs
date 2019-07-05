using Kokoro.Engine;
using Kokoro.Math;
using Kokoro.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.SceneGraph
{
    public class Node
    {
        private object _transform_lock, dirty_lock, net_transform_lock;

        private bool _dirty;
        private bool dirty
        {
            get { return _dirty; }
            set
            {
                lock (dirty_lock)
                {
                    _dirty = value;
                }

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
            set
            {
                lock (_transform_lock)
                {
                    dirty = (_transform != value);
                    _transform = value;
                }
            }
        }
        public Matrix4 NetTransform { get; private set; }

        public Node Parent { get; set; }
        public List<Node> Children { get; set; }

        public ulong LayerMask { get; set; }

        public bool Visible { get; set; }

        public Mesh Mesh { get; set; }

        public string Name { get; set; }

        public Node(Node parent, string name, ulong layers)
        {
            _transform_lock = new object();
            dirty_lock = new object();
            net_transform_lock = new object();

            Parent = parent;
            if (parent != null) parent.Children.Add(this);
            Children = new List<Node>();
            Transform = Matrix4.Identity;
            NetTransform = Transform;
            Name = name;
            LayerMask = layers;
            dirty = true;
        }

        public void UpdateTree()
        {
            //Update the current transform
            if (Parent != null && dirty)
            {
                lock (net_transform_lock)
                    NetTransform = Parent.NetTransform * Transform;

                dirty = false;
            }

            Children.AsParallel().ForAll(a => a?.UpdateTree());
        }
    }
}
