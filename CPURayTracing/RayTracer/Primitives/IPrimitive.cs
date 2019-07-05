using CPURayTracing.RayTracer.Materials;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPURayTracing.RayTracer.Primitives
{
    public interface IPrimitive
    {
        int ID { get; set; }
        IMaterial Material { get; set; }
        bool Intersects(Ray r, out float dist, out Vector3 normal, out Vector2 uv);
    }
}
