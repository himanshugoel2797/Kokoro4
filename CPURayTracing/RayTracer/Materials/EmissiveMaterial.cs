using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Math;

namespace CPURayTracing.RayTracer.Materials
{
    public class EmissiveMaterial : IMaterial
    {
        public Vector4 EmissiveColor;
        public float EmissiveStrength;

        public void Compute(Scene parent, Vector3 point, Ray incomingDir, Vector3 normal, Vector2 uv, int max_bounces, out Vector4 color)
        {
            color = EmissiveColor;
            return;
        }
    }
}
