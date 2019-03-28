using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Renderer
{
    public class CPULightRaster
    {
        private int width, height;
        private byte maxLights;
        private ShaderStorageBuffer lightData;
        private byte[] target;
        private Matrix4 vP;
        private IntPtr lightData_ptr;

        public CPULightRaster(int w, int h, byte max_lights)
        {
            width = w;
            height = h;
            maxLights = max_lights;
            lightData = new ShaderStorageBuffer(w * h * (max_lights + 1) * sizeof(ushort), true);
        }

        public void GetSphere(float radius, out Vector4[] vertices_o, out ushort[] indices_o)
        {
            int step = 10;
            float angleStep = 360f / (float)step;
            double toRad = MathHelper.Pi / 180;

            float maxX = 0;
            float maxY = 0;
            float maxZ = 0;

            float minX = 0;
            float minY = 0;
            float minZ = 0;

            List<Vector4> verts = new List<Vector4>();
            List<ushort> indices = new List<ushort>();

            uint n = 0;
            for (float aY = 0; aY < 180; aY += angleStep)
            {
                for (float aX = 0; aX < 360; aX += angleStep)
                {
                    float x = (float)(radius * System.Math.Cos(aX * toRad) * System.Math.Sin(aY * toRad));
                    float y = (float)(radius * System.Math.Sin(aX * toRad) * System.Math.Sin(aY * toRad));
                    float z = (float)(radius * System.Math.Cos(aY * toRad));

                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                    if (z > maxZ) maxZ = z;

                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (z < minZ) minZ = z;

                    verts.Add(new Vector4(x, y, z, 1));
                    indices.Add((ushort)n);
                    n++;


                    x = (float)(radius * System.Math.Cos(aX * toRad) * System.Math.Sin((aY + angleStep) * toRad));
                    y = (float)(radius * System.Math.Sin(aX * toRad) * System.Math.Sin((aY + angleStep) * toRad));
                    z = (float)(radius * System.Math.Cos((aY + angleStep) * toRad));

                    verts.Add(new Vector4(x, y, z, 1));
                    indices.Add((ushort)n);
                    n++;


                    x = (float)(radius * System.Math.Cos((aX + angleStep) * toRad) * System.Math.Sin(aY * toRad));
                    y = (float)(radius * System.Math.Sin((aX + angleStep) * toRad) * System.Math.Sin(aY * toRad));
                    z = (float)(radius * System.Math.Cos(aY * toRad));

                    verts.Add(new Vector4(x, y, z, 1));
                    indices.Add((ushort)n);
                    n++;

                    x = (float)(radius * System.Math.Cos((aX + angleStep) * toRad) * System.Math.Sin((aY + angleStep) * toRad));
                    y = (float)(radius * System.Math.Sin((aX + angleStep) * toRad) * System.Math.Sin((aY + angleStep) * toRad));
                    z = (float)(radius * System.Math.Cos((aY + angleStep) * toRad));

                    verts.Add(new Vector4(x, y, z, 1));
                    indices.Add((ushort)n);
                    indices.Add((ushort)(n - 1));
                    indices.Add((ushort)(n - 2));
                    n++;

                }
            }
            vertices_o = verts.ToArray();
            indices_o = indices.ToArray();
        }

        public void StartRender(Matrix4 vp)
        {
            //Allocate render target
            target = new byte[width * height];
            vP = vp;
            unsafe
            {
                lightData_ptr = (IntPtr)lightData.Update();
            }
        }

        public void Render(Vector4 trans, Vector4[] tris, ushort[] indices, ushort idx)
        {
            if (lightData_ptr == IntPtr.Zero) throw new ArgumentException("Call StartRender first.");

            //transform all the tris
            Vector2[] transformed = new Vector2[tris.Length];
            for (int i = 0; i < tris.Length; i++)
            {
                var tmp = trans + tris[i];
                tmp.W = 1;
                var res = Vector4.Transform(tmp, vP);
                //convert them into pixel space
                transformed[i] = new Vector2()
                {
                    X = (int)((res.X / res.W * 0.5f + 0.5f) * width),
                    Y = (int)((res.Y / res.W * 0.5f + 0.5f) * height),
                };
            }

            bool[] write_test = new bool[width * height];
            for (int i = 0; i < indices.Length; i += 3)
            {
                //Assemble a triangle for every 3 indices and determine the bounding box of the triangle
                int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
                Vector2 v0 = transformed[indices[i]];
                Vector2 v1 = transformed[indices[i + 1]];
                Vector2 v2 = transformed[indices[i + 2]];

                minX = (int)System.Math.Min(v0.X, System.Math.Min(v1.X, v2.X));
                minY = (int)System.Math.Min(v0.Y, System.Math.Min(v1.Y, v2.Y));
                maxX = (int)System.Math.Max(v0.X, System.Math.Max(v1.X, v2.X));
                maxY = (int)System.Math.Max(v0.Y, System.Math.Max(v1.Y, v2.Y));

                //Iterate over all pixels in the bounding box looking for intersections
                for (int y = minY; y <= maxY; y++)
                    for (int x = minX; x <= maxX; x++)
                    {
                        //Don't write the same light twice
                        if (write_test[y * width + x])
                            continue;

                        var s = v0.Y * v2.X - v0.X * v2.Y + (v2.Y - v0.Y) * x + (v0.X - v2.X) * y;
                        var t = v0.X * v1.Y - v0.Y * v1.X + (v0.Y - v1.Y) * x + (v1.X - v0.X) * y;

                        if ((s < 0) != (t < 0))
                            continue;

                        var A = -v1.Y * v2.X + v0.Y * (v2.X - v1.X) + v0.X * (v1.Y - v2.Y) + v1.X * v2.Y;

                        if (A < 0 ?
                                (s <= 0 && s + t >= A) :
                                (s >= 0 && s + t <= A))
                        {
                            write_test[y * width + x] = true;

                            var w_idx = target[y * width + x]++;
                            if (w_idx == maxLights)
                            {
                                Console.WriteLine("Too many lights per tile! Ignoring excess lights...");
#if DEBUG
                                throw new Exception("Too many lights per tile!");
#endif
                            }
                            unsafe
                            {
                                ushort* light_idxs = (ushort*)lightData_ptr;
                                light_idxs[(y * width + x)] = (ushort)(w_idx + 1);  //Update the total light count for this tile
                                light_idxs[(y * width + x) * (w_idx + 1)] = idx;    //Add the light index
                            }
                        }
                    }
            }
        }

        public void FinishUpdate()
        {
            lightData.UpdateDone();
            lightData_ptr = IntPtr.Zero;
        }
    }
}
