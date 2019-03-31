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
        private bool[] write_test;
        private bool isVisible;
        private Matrix4 vP;
        private IntPtr lightData_ptr;
#if DEBUG
        System.Drawing.Bitmap bmp;
#endif

        const int SubPixelRes = 1;
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
            int step = 10;
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
                Parallel.For(0, width * height, a => ptr[a] = 0);
            }
#if DEBUG
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    bmp.SetPixel(x, y, System.Drawing.Color.Black);
#endif
        }

        private void drawLine(int x0, int x1, int y, ushort idx)
        {
            x0 /= SubPixelRes;
            x1 /= SubPixelRes;
            y /= SubPixelRes;

            if (x0 < 0) x0 = 0;
            if (x1 < 0) return;
            if (x0 >= width) return;
            if (x1 >= width) x1 = width - 1;
            if (y < 0) return;
            if (y >= height) return;

            for (int x = x0; x < x1; x++)
            {
                if (!write_test[y * width + x])
                {
                    isVisible = true;
                    write_test[y * width + x] = true;
                    var w_idx = target[y * width + x]++;
                    if (w_idx >= maxLights)
                    {
                        Console.WriteLine("Too many lights per tile! Ignoring excess lights...");
#if DEBUG
                        //throw new Exception("Too many lights per tile!");
#endif
                    }
                    else
                    {
#if DEBUG
                        lock (LightData)
                        {
                            bmp.SetPixel(x, y, System.Drawing.Color.White);
                        }
#endif
                        unsafe
                        {
                            uint* light_idxs = (uint*)lightData_ptr;
                            light_idxs[y * width + x] = (uint)(w_idx + 1);  //Update the total light count for this tile
                            light_idxs[(y * width + x) + ((w_idx + 1) * width * height)] = idx;    //Add the light index
                        }
                    }
                }
            }
        }

        private void writeBuf(int x, int y, ushort idx)
        {
            if (!write_test[y * width + x])
            {
                isVisible = true;
                write_test[y * width + x] = true;
                var w_idx = target[y * width + x]++;
                if (w_idx >= maxLights)
                {
                    Console.WriteLine("Too many lights per tile! Ignoring excess lights...");
#if DEBUG
                    //throw new Exception("Too many lights per tile!");
#endif
                }
                else
                {
#if DEBUG
                    lock (LightData)
                    {
                        bmp.SetPixel(x, y, System.Drawing.Color.White);
                    }
#endif
                    unsafe
                    {
                        uint* light_idxs = (uint*)lightData_ptr;
                        light_idxs[y * width + x] = (uint)(w_idx + 1);  //Update the total light count for this tile
                        light_idxs[(y * width + x) + ((w_idx + 1) * width * height)] = idx;    //Add the light index
                    }
                }
            }
        }

        private void fillBottomFlatTriangle(Vector2 v1, Vector2 v2, Vector2 v3, ushort idx)
        {
            float invslope1 = (v2.X - v1.X) / (v2.Y - v1.Y);
            float invslope2 = (v3.X - v1.X) / (v3.Y - v1.Y);

            float curx1 = v1.X;
            float curx2 = v1.X;

            for (int scanlineY = (int)System.Math.Round(v1.Y); scanlineY <= System.Math.Round(v2.Y); scanlineY++)
            {
                drawLine((int)System.Math.Round(curx1), (int)System.Math.Round(curx2), scanlineY, idx);
                curx1 += invslope1;
                curx2 += invslope2;
            }
        }

        private void fillTopFlatTriangle(Vector2 v1, Vector2 v2, Vector2 v3, ushort idx)
        {
            float invslope1 = (v3.X - v1.X) / (v3.Y - v1.Y);
            float invslope2 = (v3.X - v2.X) / (v3.Y - v2.Y);

            float curx1 = v3.X;
            float curx2 = v3.X;

            for (int scanlineY = (int)System.Math.Round(v3.Y); scanlineY > System.Math.Round(v1.Y); scanlineY--)
            {
                drawLine((int)System.Math.Round(curx1), (int)System.Math.Round(curx2), scanlineY, idx);
                curx1 -= invslope1;
                curx2 -= invslope2;
            }
        }

        public bool Render(Vector4 trans, Vector4[] tris, ushort[] indices, ushort idx)
        {
            if (lightData_ptr == IntPtr.Zero) throw new ArgumentException("Call StartRender first.");

            //transform all the tris
            Vector4[] transformed = new Vector4[tris.Length];
            Vector4[] transformed_ndc = new Vector4[tris.Length];
            for (int i = 0; i < tris.Length; i++)
            {
                var tmp = trans + tris[i];
                tmp.W = 1;
                var res = Vector4.Transform(tmp, vP);
                //convert them into pixel space
                transformed_ndc[i] = res;
                transformed[i] = new Vector4()
                {
                    X = (float)System.Math.Round((res.X / res.W + 1.0f) * width * 0.5f * SubPixelRes),
                    Y = (float)System.Math.Round((res.Y / res.W + 1.0f) * height * 0.5f * SubPixelRes),
                    Z = res.Z,
                    W = res.W,
                };
            }

            isVisible = false;
            write_test = new bool[width * height];
            for (int i = 90; i < 93/*indices.Length*/; i += 3)
            {
                //Assemble a triangle for every 3 indices and determine the bounding box of the triangle
                float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                var v0 = transformed[indices[i]];
                var v1 = transformed[indices[i + 1]];
                var v2 = transformed[indices[i + 2]];

                var v0_clip = transformed_ndc[indices[i]];
                var v1_clip = transformed_ndc[indices[i + 1]];
                var v2_clip = transformed_ndc[indices[i + 2]];

                /*
                var b = new bool[3];
                var v = new Vector3[3];
                v[0] = transformed[indices[i]];
                v[1] = transformed[indices[i + 1]];
                v[2] = transformed[indices[i + 2]];

                for (int j = 0; j < b.Length; j++)
                    if (v[j].X >= 0 && v[j].X <= (width - 1) * SubPixelRes && v[j].Y >= 0 && v[j].Y <= (height - 1) * SubPixelRes && v[j].Z >= 0 && v[j].Z <= 1)
                        b[j] = true;

                if (b.Any(a => a))
                {
                    v = v.OrderBy(a => a.Y).ToArray();
                    if (v[1].Y == v[2].Y)
                    {
                        fillBottomFlatTriangle(v[0].Xy, v[1].Xy, v[2].Xy, idx);
                    }
                    // check for trivial case of top-flat triangle
                    else if (v[0].Y == v[1].Y)
                    {
                        fillTopFlatTriangle(v[0].Xy, v[1].Xy, v[2].Xy, idx);
                    }
                    else
                    {
                        // general case - split the triangle in a topflat and bottom-flat one
                        Vector2 v4 = new Vector2(
                          (int)(v[0].X + ((float)(v[1].Y - v[0].Y) / (float)(v[2].Y - v[0].Y)) * (v[2].X - v[0].X)), v[1].Y);
                        fillBottomFlatTriangle(v[0].Xy, v[1].Xy, v4, idx);
                        fillTopFlatTriangle(v[1].Xy, v4, v[2].Xy, idx);
                    }
                }*/

                float clip = 0;

                bool v0_vis = false;
                if (v0.X >= 0 && v0.X <= (width - 1) * SubPixelRes && v0.Y >= 0 && v0.Y <= (height - 1) * SubPixelRes && v0.Z >= clip && v0.Z <= 1 && v0.W > 0)
                    if (v0_clip.X >= -v0_clip.W && v0_clip.X <= v0_clip.W && v0_clip.Y >= -v0_clip.W && v0_clip.Y <= v0_clip.W && v0_clip.Z >= -v0_clip.W && v0_clip.Z <= v0_clip.W)
                        v0_vis = true;

                bool v1_vis = false;
                if (v1.X >= 0 && v1.X <= (width - 1) * SubPixelRes && v1.Y >= 0 && v1.Y <= (height - 1) * SubPixelRes && v1.Z >= clip && v1.Z <= 1 && v1.W > 0)
                    if (v1_clip.X >= -v1_clip.W && v1_clip.X <= v1_clip.W && v1_clip.Y >= -v1_clip.W && v1_clip.Y <= v1_clip.W && v1_clip.Z >= -v1_clip.W && v1_clip.Z <= v1_clip.W)
                        v1_vis = true;

                bool v2_vis = false;
                if (v2.X >= 0 && v2.X <= (width - 1) * SubPixelRes && v2.Y >= 0 && v2.Y <= (height - 1) * SubPixelRes && v2.Z >= clip && v2.Z <= 1 && v2.W > 0)
                    if (v2_clip.X >= -v2_clip.W && v2_clip.X <= v2_clip.W && v2_clip.Y >= -v2_clip.W && v2_clip.Y <= v2_clip.W && v2_clip.Z >= -v2_clip.W && v2_clip.Z <= v2_clip.W)
                        v2_vis = true;

                if (!v0_vis && !v1_vis && !v2_vis)
                    continue;

                //Calculate the slopes to each point
                //Compute the span on each scanline

                minX = System.Math.Min(v0.X, System.Math.Min(v1.X, v2.X));
                minY = System.Math.Min(v0.Y, System.Math.Min(v1.Y, v2.Y));
                maxX = System.Math.Max(v0.X, System.Math.Max(v1.X, v2.X));
                maxY = System.Math.Max(v0.Y, System.Math.Max(v1.Y, v2.Y));

                if (maxX > (width - 1) * SubPixelRes) maxX = (width - 1) * SubPixelRes;
                if (maxY > (height - 1) * SubPixelRes) maxY = (height - 1) * SubPixelRes;
                if (minX < 0) minX = 0;
                if (minY < 0) minY = 0;

                var vs1 = new Vector3(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
                var vs2 = new Vector3(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);

                //Iterate over all pixels in the bounding box looking for intersections
                //for (float y0 = minY; y0 <= maxY; y0++)
                Parallel.For((int)minY, (int)maxY, (y0) =>
                {
                    for (float x0 = minX; x0 <= maxX; x0++)
                    {
                        int y = (int)System.Math.Round((float)y0 / SubPixelRes);
                        int x = (int)System.Math.Round(x0 / SubPixelRes);
                        //Don't write the same light twice
                        if (!write_test[y * width + x])
                        {
                            var q = new Vector3(x0 - v0.X, y0 - v0.Y, 0);

                            var cross_base = ((vs1.X * vs2.Y) - (vs2.X * vs1.Y));
                            var s = ((q.X * vs2.Y) - (vs2.X * q.Y)) / cross_base;
                            var t = ((vs1.X * q.Y) - (q.X * vs1.Y)) / cross_base;

                            if (s >= 0 && t >= 0 && (s + t <= 1))
                                writeBuf(x, y, idx);
                        }
                    }
                });
            }

            return isVisible;
        }

        public void FinishUpdate()
        {
            lightData_ptr = IntPtr.Zero;
            lightData.UpdateDone();
            OpenTK.Graphics.OpenGL.GL.Flush();
            OpenTK.Graphics.OpenGL.GL.Finish();
#if DEBUG
            bmp.Save("test.png");
#endif
        }
    }
}
