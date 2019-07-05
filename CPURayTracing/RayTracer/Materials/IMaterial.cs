using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPURayTracing.RayTracer.Materials
{
    public interface IMaterial
    {
        void Compute(Scene parent, Vector3 point, Ray incomingDir, Vector3 normal, Vector2 uv, int max_bounces, out Vector4 color);
    }
}
