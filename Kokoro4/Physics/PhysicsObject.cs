using Kokoro.Engine.Graphics;
using Kokoro.Engine.Graphics.Lights;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Physics
{
    public class PhysicsObject
    {
        public PhysicsData PhysicsData;
        public EngineRenderable RenderableData;
        public BoundingBox Bounds;

        public bool Active { get; set; }
        public bool Visible { get; set; }

        public string Name { get; private set; }
        //TODO: Figure out how to represent joints in the hierarchy

        public PhysicsObject(string name, PhysicsType pType, float mass, BoundingBox bounds)
        {
            Name = name;

            PhysicsData = new PhysicsData();
            PhysicsData.PhysicsType = pType;
            PhysicsData.Mass = mass;

            Bounds = bounds;
        }

        public void ApplyImpulse()
        {

        }
    }
}
