using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Physics
{
    public interface IForceImplementer
    {
        void ApplyForce(PhysicsObject a, PhysicsObject b, double timestep);
    }
}
