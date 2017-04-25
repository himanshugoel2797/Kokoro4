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
        public BoundingBox Bounds { get; set; }
        public IPhysicsData DetailedData { get; set; }

        public double Mass { get; set; }
        public double Charge { get; set; }

        public Vector3d Force { get; set; }
        public Vector3d Origin { get; set; }
        public Vector3d Velocity { get; set; }
        public Quaternion Orientation { get; set; }
        public Vector3d Position { get; set; }
        public bool Dynamic { get; set; }
        public ulong CollisionLayers { get; set; }

        private Vector3d LastAcceleration;

        internal void UpdateForce(Vector3d force, double timestep)
        {
            Vector3d accel = force / Mass;
            Position += Velocity * timestep + (0.5d * LastAcceleration * timestep * timestep);
            Velocity += (LastAcceleration + accel) / 2.0d * timestep;
            LastAcceleration = accel;
        }
    }
}
