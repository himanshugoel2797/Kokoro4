using CPURayTracing.RayTracer.Primitives;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPURayTracing.RayTracer
{
    public class Scene
    {
        private Camera Camera;
        private List<IPrimitive> Primitives;

        public Vector4 ClearColor { get; set; }
        public float Epsilon = 1e-3f;

        public Scene(int w, int h, float fov, Vector3 pos, Vector3 dir, Vector3 up, float dist_max)
        {
            Primitives = new List<IPrimitive>();
            Camera = new Camera(w, h, fov, pos, dir, up, dist_max);
        }

        public void AddPrimitive(IPrimitive prim)
        {
            Primitives.Add(prim);
        }

        public void Render(Bitmap bmp, int bounces)
        {
            Camera.Render(this, bmp, bounces);
        }

        public bool Trace(Ray r, out float dist, out Vector3 normal, out Vector2 uv, out IPrimitive prim)
        {
            var closestDist = float.MaxValue;
            var closestNorm = Vector3.One;
            var closestUV = Vector2.One;
            var closestIdx = -1;
            for (int i = 0; i < Primitives.Count; i++)
            {
                var intersects = Primitives[i].Intersects(r, out float curDist, out var curNorm, out var curUV);
                if (intersects && curDist < closestDist && curDist > 0)
                {
                    closestIdx = i;
                    closestNorm = curNorm;
                    closestUV = curUV;
                    closestDist = curDist;
                }
            }
            if (closestIdx == -1)
            {
                dist = -1;
                normal = Vector3.One;
                prim = null;
                uv = Vector2.One;
                return false;
            }
            else
            {
                //Console.WriteLine($"{closestIdx},{closestDist}");
                dist = closestDist;
                normal = closestNorm;
                uv = closestUV;
                prim = Primitives[closestIdx];
                return true;
            }
        }
    }
}
