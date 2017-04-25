using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Physics
{
    public class ElectromagnetismImplementer : IForceImplementer
    {
        public double k = 8.987551787e9;

        public void ApplyForce(PhysicsObject a, PhysicsObject b, double timestep)
        {
            double force = k * a.Charge * b.Charge / (a.Position - b.Position).LengthSquared;

            Vector3d forceOnA = force * Vector3d.Normalize((b.Position - a.Position));
            Vector3d forceOnB = -forceOnA;

            a.Force += forceOnA;
            b.Force += forceOnB;
        }
    }
}
