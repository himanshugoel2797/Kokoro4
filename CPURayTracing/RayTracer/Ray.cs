using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPURayTracing.RayTracer
{
    public struct Ray
    {
        public const float MaxDistance = 1e10f;

        public Vector3 Origin;
        public Vector3 Direction;

        public float MaxLength;

        public Ray(Vector3 o, Vector3 d)
        {
            Origin = o;
            Direction = d;
            MaxLength = MaxDistance;
        }
    }
}
