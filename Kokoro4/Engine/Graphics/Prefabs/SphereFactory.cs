using Kokoro.Engine;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Prefabs
{
    public class SphereFactory
    {
        private static void Step(float aY, float aX, double toRad, float radius, out Vector3 vert, out Vector2 uv, out Vector3 norm)
        {
            float x = (float)(radius * System.Math.Cos(aX * toRad) * System.Math.Sin(aY * toRad));
            float y = (float)(radius * System.Math.Sin(aX * toRad) * System.Math.Sin(aY * toRad));
            float z = (float)(radius * System.Math.Cos(aY * toRad));

            float uvX = aX / 360;
            float uvY = (2 * aY) / 360;

            vert = new Vector3(x, y, z);
            uv = new Vector2(uvX, uvY);

            norm = new Vector3(x, y, z);
            norm.Normalize();
        }

        public static Mesh Create(MeshGroup group, float step = 36)
        {
            List<float> verts = new List<float>();
            List<float> uvs = new List<float>();
            List<uint> normals = new List<uint>();
            List<ushort> indices = new List<ushort>();

            float radius = 1;

            float angleStep = 360f / (float)step;
            double toRad = MathHelper.Pi / 180;

            ushort n = 0;
            for (float aY = 0; aY < 180; aY += angleStep)
            {
                for (float aX = 0; aX < 360; aX += angleStep)
                {
                    Step(aY, aX, toRad, radius, out var vert1, out var uv1, out var norm1);
                    Step(aY, aX + angleStep, toRad, radius, out var vert2, out var uv2, out var norm2);
                    Step(aY + angleStep, aX + angleStep, toRad, radius, out var vert3, out var uv3, out var norm3);
                    Step(aY + angleStep, aX, toRad, radius, out var vert4, out var uv4, out var norm4);

                    if (aY == 0)
                    {
                        verts.Add(vert4.X);
                        verts.Add(vert4.Y);
                        verts.Add(vert4.Z);
                        uvs.Add(uv4.X);
                        uvs.Add(uv4.Y);
                        normals.Add(Mesh.CompressNormal(norm4.X, norm4.Y, norm4.Z));
                        indices.Add(n++);

                        verts.Add(vert3.X);
                        verts.Add(vert3.Y);
                        verts.Add(vert3.Z);
                        uvs.Add(uv3.X);
                        uvs.Add(uv3.Y);
                        normals.Add(Mesh.CompressNormal(norm3.X, norm3.Y, norm3.Z));
                        indices.Add(n++);

                        verts.Add(vert1.X);
                        verts.Add(vert1.Y);
                        verts.Add(vert1.Z);
                        uvs.Add(uv1.X);
                        uvs.Add(uv1.Y);
                        normals.Add(Mesh.CompressNormal(norm1.X, norm1.Y, norm1.Z));
                        indices.Add(n++);
                    }
                    else if (aY + angleStep >= 180)
                    {
                        verts.Add(vert2.X);
                        verts.Add(vert2.Y);
                        verts.Add(vert2.Z);
                        uvs.Add(uv2.X);
                        uvs.Add(uv2.Y);
                        normals.Add(Mesh.CompressNormal(norm2.X, norm2.Y, norm2.Z));
                        indices.Add(n++);

                        verts.Add(vert1.X);
                        verts.Add(vert1.Y);
                        verts.Add(vert1.Z);
                        uvs.Add(uv1.X);
                        uvs.Add(uv1.Y);
                        normals.Add(Mesh.CompressNormal(norm1.X, norm1.Y, norm1.Z));
                        indices.Add(n++);

                        verts.Add(vert3.X);
                        verts.Add(vert3.Y);
                        verts.Add(vert3.Z);
                        uvs.Add(uv3.X);
                        uvs.Add(uv3.Y);
                        normals.Add(Mesh.CompressNormal(norm3.X, norm3.Y, norm3.Z));
                        indices.Add(n++);
                    }
                    else
                    {

                        verts.Add(vert4.X);
                        verts.Add(vert4.Y);
                        verts.Add(vert4.Z);
                        uvs.Add(uv4.X);
                        uvs.Add(uv4.Y);
                        normals.Add(Mesh.CompressNormal(norm4.X, norm4.Y, norm4.Z));
                        indices.Add(n++);

                        verts.Add(vert2.X);
                        verts.Add(vert2.Y);
                        verts.Add(vert2.Z);
                        uvs.Add(uv2.X);
                        uvs.Add(uv2.Y);
                        normals.Add(Mesh.CompressNormal(norm2.X, norm2.Y, norm2.Z));
                        indices.Add(n++);

                        verts.Add(vert1.X);
                        verts.Add(vert1.Y);
                        verts.Add(vert1.Z);
                        uvs.Add(uv1.X);
                        uvs.Add(uv1.Y);
                        normals.Add(Mesh.CompressNormal(norm1.X, norm1.Y, norm1.Z));
                        indices.Add(n++);

                        verts.Add(vert4.X);
                        verts.Add(vert4.Y);
                        verts.Add(vert4.Z);
                        uvs.Add(uv4.X);
                        uvs.Add(uv4.Y);
                        normals.Add(Mesh.CompressNormal(norm4.X, norm4.Y, norm4.Z));
                        indices.Add(n++);

                        verts.Add(vert3.X);
                        verts.Add(vert3.Y);
                        verts.Add(vert3.Z);
                        uvs.Add(uv3.X);
                        uvs.Add(uv3.Y);
                        normals.Add(Mesh.CompressNormal(norm3.X, norm3.Y, norm3.Z));
                        indices.Add(n++);

                        verts.Add(vert2.X);
                        verts.Add(vert2.Y);
                        verts.Add(vert2.Z);
                        uvs.Add(uv2.X);
                        uvs.Add(uv2.Y);
                        normals.Add(Mesh.CompressNormal(norm2.X, norm2.Y, norm2.Z));
                        indices.Add(n++);
                    }
                }
            }

            return new Mesh(group, verts.ToArray(), uvs.ToArray(), normals.ToArray(), indices.ToArray());
        }
    }
}
