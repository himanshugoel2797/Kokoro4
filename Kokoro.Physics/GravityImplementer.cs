using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Physics
{
    public class GravityImplementer : IForceImplementer
    {
        public double G = 6.6740831e-11;

        public void ApplyForce(PhysicsObject a, PhysicsObject b, double timestep)
        {
            double force = -G * a.Mass * b.Mass / (a.Position - b.Position).LengthSquared;

            Vector3d forceOnA = force * Vector3d.Normalize((b.Position - a.Position));
            Vector3d forceOnB = -forceOnA;

            a.Force += forceOnA;
            b.Force += forceOnB;
        }
    }
}
