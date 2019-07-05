using CPURayTracing.RayTracer.Materials;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPURayTracing.RayTracer.Primitives
{
    public class Sphere : IPrimitive
    {
        public Vector3 Center;
        public float Radius;

        public Sphere(Vector3 center, float rad, int id)
        {
            Center = center;
            Radius = rad;
            ID = id;
        }

        public IMaterial Material { get; set; }
        public int ID { get; set; }

        public bool Intersects(Ray r, out float dist, out Vector3 normal, out Vector2 uv)
        {
            Vector3 oc = r.Origin - Center;
            float a = Vector3.Dot(r.Direction, r.Direction);
            float b = 2.0f * Vector3.Dot(oc, r.Direction);
            float c = Vector3.Dot(oc, oc) - Radius * Radius;
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                dist = -1;
                normal = Vector3.Zero;
                uv = Vector2.Zero;
                return false;
            }
            else
            {
                dist = (float)(-b - Math.Sqrt(discriminant));
                if (dist < 0)
                    dist = (float)(-b + Math.Sqrt(discriminant));
                dist /= (2.0f * a);
                normal = ((r.Origin + r.Direction * dist) - Center);
                normal.Normalize();
                uv = normal.Xy;
                return true;
            }
        }
    }
}
