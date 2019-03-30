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
#if DEBUG
        System.Drawing.Bitmap bmp;
#endif

        public ShaderStorageBuffer LightData { get => lightData; }

        public CPULightRaster(int w, int h, byte max_lights)
        {
            width = w;
            height = h;
            maxLights = max_lights;
            lightData = new ShaderStorageBuffer(w * h * (max_lights + 1) * sizeof(uint), true);
#if DEBUG
            bmp = new System.Drawing.Bitmap(width, height);
#endif
        }

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

        public void GetSphere(float radius, out Vector4[] vertices_o, out ushort[] indices_o)
        {
            int step = 7;
            float angleStep = 360f / (float)step;
            double toRad = MathHelper.Pi / 180;

            List<Vector4> verts = new List<Vector4>();
            List<ushort> indices = new List<ushort>();

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
                        verts.Add(new Vector4(vert1, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert3, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert4, 1));
                        indices.Add(n++);
                    }
                    else if (aY + angleStep >= 180)
                    {
                        verts.Add(new Vector4(vert3, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert1, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert2, 1));
                        indices.Add(n++);
                    }
                    else
                    {

                        verts.Add(new Vector4(vert1, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert2, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert4, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert2, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert3, 1));
                        indices.Add(n++);

                        verts.Add(new Vector4(vert4, 1));
                        indices.Add(n++);
                    }
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
                uint* ptr = (uint*)lightData_ptr;
                for (int i = 0; i < width * height; i++)
                {
                    ptr[i] = 0;
                }
            }
#if DEBUG_BMP
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    bmp.SetPixel(x, y, System.Drawing.Color.Black);
#endif
        }

        public bool Render(Vector4 trans, Vector4[] tris, ushort[] indices, ushort idx)
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
                    X = (int)((res.X / res.W + 1.0f) * width * 0.5f),
                    Y = (int)((res.Y / res.W + 1.0f) * height * 0.5f),
                };
            }

            bool isVisible = false;
            bool[] write_test = new bool[width * height];
            for (int i = 0; i < indices.Length; i += 3)
            {
                //Assemble a triangle for every 3 indices and determine the bounding box of the triangle
                int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
                Vector2 v0 = transformed[indices[i]];
                Vector2 v1 = transformed[indices[i + 1]];
                Vector2 v2 = transformed[indices[i + 2]];

                //Calculate the slopes to each point
                //Compute the span on each scanline

                minX = (int)System.Math.Max(0, System.Math.Min(v0.X, System.Math.Min(v1.X, v2.X)));
                minY = (int)System.Math.Max(0, System.Math.Min(v0.Y, System.Math.Min(v1.Y, v2.Y)));
                maxX = (int)System.Math.Min(width - 1, System.Math.Max(v0.X, System.Math.Max(v1.X, v2.X)));
                maxY = (int)System.Math.Min(height - 1, System.Math.Max(v0.Y, System.Math.Max(v1.Y, v2.Y)));

                if (maxX < 0 | maxY < 0) continue;

                var vs1 = new Vector2(v1.X - v0.X, v1.Y - v0.Y);
                var vs2 = new Vector2(v2.X - v0.X, v2.Y - v0.Y);

                //Iterate over all pixels in the bounding box looking for intersections
                for (int y = minY; y <= maxY; y++)
                    for (int x = minX; x <= maxX; x++)
                    {
                        //Don't write the same light twice
                        if (write_test[y * width + x])
                            continue;

                        var q = new Vector2(x - v0.X, y - v0.Y);

                        var cross_base = ((vs1.X * vs2.Y) - (vs2.X * vs1.Y));
                        var s = ((q.X * vs2.Y) - (vs2.X * q.Y)) / cross_base;
                        var t = ((vs1.X * q.Y) - (q.X * vs1.Y)) / cross_base;

                        if (s >= 0 && t >= 0 && (s + t <= 1))
                        {
                            write_test[y * width + x] = true;
                            isVisible = true;

#if DEBUG_BMP
                            bmp.SetPixel(x, y, System.Drawing.Color.White);
#endif

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
                                uint* light_idxs = (uint*)lightData_ptr;
                                light_idxs[(y * width + x)] = (uint)(w_idx + 1);  //Update the total light count for this tile
                                light_idxs[(y * width + x) * (w_idx + 2)] = idx;    //Add the light index
                            }
                        }
                    }
            }

            return isVisible;
        }

        public void FinishUpdate()
        {
            lightData.UpdateDone();
            lightData_ptr = IntPtr.Zero;
#if DEBUG_BMP
            bmp.Save("test.png");
#endif
        }
    }
}
