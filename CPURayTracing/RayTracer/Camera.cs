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
    class Camera
    {
        public Matrix4 View;
        public Matrix4 Projection;

        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 Up;

        public int Width;
        public int Height;

        public Camera(int w, int h, float fov, Vector3 pos, Vector3 dir, Vector3 up, float dist_max)
        {
            View = Matrix4.LookAt(pos, pos + dir, up);
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), w / (float)h, 0.001f);

            Width = w;
            Height = h;
            Position = pos;
            Direction = dir;
            Up = up;
        }

        public void Render(Scene parent, Bitmap dst, int bounces)
        {
            float x_stride = 2.0f / Width;
            float y_stride = 2.0f / Height;

            var VP = View * Projection;
            var iVP = Matrix4.Invert(VP);

            var img = new byte[Width * Height * 4];

            for(int x0 = 0; x0 < Width; x0++)
            //Parallel.For(0, Width, (x0) =>
            {
                float x = -1 + x_stride * x0;
                for (int y0 = 0; y0 < Height; y0++)
                {
                    float y = 1 - y_stride * y0;

                    //compute the pixel coordinates
                    var px = x0;
                    var py = y0;

                    //compute the world coordinates
                    Vector4 ray_e_clip = new Vector4(x, y, 1, 1);
                    Vector4 ray_e_world_ndc = Vector4.Transform(ray_e_clip, iVP);

                    Vector4 ray_o_clip = new Vector4(x, y, 0.01f, 1);
                    Vector4 ray_o_world_ndc = Vector4.Transform(ray_o_clip, iVP);

                    Vector3 ray_o_world = Position;
                    Vector3 ray_e_world = ray_e_world_ndc.Xyz / ray_e_world_ndc.W;

                    Vector3 ray_dir_world = (ray_e_world - ray_o_world);
                    ray_dir_world.Normalize();

                    Ray camRay = new Ray(ray_o_world, ray_dir_world);
                    if (parent.Trace(camRay, out var closestDist, out var closestNorm, out var closestUv, out var closestPrim))
                    {
                        closestPrim.Material.Compute(parent, ray_o_world + ray_dir_world * (closestDist - parent.Epsilon), camRay, closestNorm, closestUv, bounces - 1, out var color);
                        if (color.X > 1) color.X = 1;
                        if (color.Y > 1) color.Y = 1;
                        if (color.Z > 1) color.Z = 1;
                        if (color.W > 1) color.W = 1;

                        if (color.X < 0) color.X = 0;
                        if (color.Y < 0) color.Y = 0;
                        if (color.Z < 0) color.Z = 0;
                        if (color.W < 0) color.W = 0;

                        img[(Width * Height * 0) + py * Width + px] = (byte)(color.X * 255.0f);
                        img[(Width * Height * 1) + py * Width + px] = (byte)(color.Y * 255.0f);
                        img[(Width * Height * 2) + py * Width + px] = (byte)(color.Z * 255.0f);
                        img[(Width * Height * 3) + py * Width + px] = (byte)(color.W * 255.0f);
                    }
                    else
                    {
                        img[(Width * Height * 0) + py * Width + px] = (byte)(parent.ClearColor.X * 255.0f);
                        img[(Width * Height * 1) + py * Width + px] = (byte)(parent.ClearColor.Y * 255.0f);
                        img[(Width * Height * 2) + py * Width + px] = (byte)(parent.ClearColor.Z * 255.0f);
                        img[(Width * Height * 3) + py * Width + px] = (byte)(parent.ClearColor.W * 255.0f);
                    }
                }
            }
            //);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    dst.SetPixel(x, y, Color.FromArgb(img[y * Width + x], img[Width * Height + y * Width + x], img[Width * Height * 2 + y * Width + x]));
        }
    }
}
