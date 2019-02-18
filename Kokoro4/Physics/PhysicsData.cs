using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Physics
{
    public struct PhysicsData
    {
        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }
        public Vector3 AngularVelocity { get; set; }
        public Vector3 Velocity { get; set; }

        public float Mass { get; set; }
        public PhysicsType PhysicsType { get; set; }
    }
}
