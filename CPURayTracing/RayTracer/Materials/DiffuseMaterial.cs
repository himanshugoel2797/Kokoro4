using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Math;

namespace CPURayTracing.RayTracer.Materials
{
    public class DiffuseMaterial : IMaterial
    {
        public Vector4 Color;
        //Random rng = new Random(0);

        public void Compute(Scene parent, Vector3 point, Ray incomingRay, Vector3 normal, Vector2 uv, int max_bounces, out Vector4 color)
        {
            //The next bounce is at an equal angle to the normal
            Vector3 reflected_dir = incomingRay.Direction - 2 * Vector3.Dot(normal, incomingRay.Direction) * normal;
            Ray reflected = new Ray(point, reflected_dir);

            var diff_color = Vector4.Zero;
            if (max_bounces > 0)
            {
                int cnt = 2048;
                object locker = new object();
                var diff_parts = new Vector4[cnt];
                //var norm_polar = Vector3.ToSpherical(normal)
                Parallel.For(0, cnt, (i) =>//for (int i = 0; i < 256; i++)
                {
                    var rng = new Random(i);

                    var dir = Vector3.FromSpherical(new Vector3(1, (float)Math.Acos(rng.NextDouble() * 2 - 1), (float)(rng.NextDouble() * 2 * Math.PI)));
                    //dir.Normalize();
                    dir += normal;
                    dir.Normalize();
                    Ray sample_ray = new Ray(point, dir);
                    if (parent.Trace(sample_ray, out var l_dist, out var l_norm, out var l_uv, out var l_prim))
                    {
                        l_prim.Material.Compute(parent, point + dir * (l_dist - parent.Epsilon), sample_ray, l_norm, l_uv, max_bounces - 1, out var l_col);
                        diff_parts[i] = Math.Max(0, Vector3.Dot(normal, dir)) * l_col;
                        //diff_color += l_col;//new Vector4(Math.Max(0, Vector3.Dot(normal, dir)));
                    }
                }
                );
                for (int i = 0; i < cnt; i++) diff_color += diff_parts[i] * diff_parts[i].W;
                diff_color /= cnt;

                //Compute the incoming light
                var diff_coeff = Math.Max(0, Vector3.Dot(normal, reflected_dir));
                if (parent.Trace(reflected, out var closestDist, out var closestNorm, out var closestUv, out var closestPrim))
                {
                    closestPrim.Material.Compute(parent, point + reflected_dir * (closestDist - parent.Epsilon), reflected, closestNorm, closestUv, max_bounces - 1, out var p_color);
                    color = diff_coeff * diff_color;
                }
                else
                {
                    color = diff_coeff * diff_color;
                }
                //color = diff_color;
            }
            else
            {
                color = Vector4.Zero;
                return;
            }
        }
    }
}
