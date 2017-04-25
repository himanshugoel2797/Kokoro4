using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Renderer
{
    class DeferredOptimized
    {
        //TODO add a flag to meshes to allow them to request the depth prepass testing.
        //setup a background task to start clustering lights into a clustered frustum -> 16 * 16 * 16 region with logarithmic depth
        //Draw the entire scene to just depth buffer, first static stuff, then dynamic
        //Set depth test to equal, light the scene into an hdr buffer, render out a normal and specular map as well
        //calculate screen space reflections using the previous frame's hdr buffer
        //render the ui
        //blend the results together

        private Framebuffer gbuffer;
        private int tile_x_cnt, tile_y_cnt, w, h;
        const int MaxLightCount = 10;

        public ShaderProgram DeferredOptimizedShader { get; set; }
        private Texture depth, uv, mat;

        public DeferredOptimized(int tile_x_cnt, int tile_y_cnt, int w, int h)
        {
            this.tile_x_cnt = tile_x_cnt;
            this.tile_y_cnt = tile_y_cnt;
            this.w = w;
            this.h = h;

            depth = new Texture();
            depth.SetData(new DepthTextureSource(w, h)
            {
                InternalFormat = PixelInternalFormat.DepthComponent32f
            }, 0);

            uv = new Texture();
            uv.SetData(new FramebufferTextureSource(w, h, 1)
            {
                InternalFormat = PixelInternalFormat.Rg16f,
                PixelType = PixelType.HalfFloat
            }, 0);

            mat = new Texture();
            mat.SetData(new FramebufferTextureSource(w, h, 1)
            {
                InternalFormat = PixelInternalFormat.Rg32ui,
                PixelType = PixelType.UnsignedInt
            }, 0);

            gbuffer = new Framebuffer(w, h);
            gbuffer[FramebufferAttachment.DepthAttachment] = depth;
            gbuffer[FramebufferAttachment.ColorAttachment0] = uv;
            gbuffer[FramebufferAttachment.ColorAttachment1] = mat;

            DeferredOptimizedShader = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/DeferredOpt/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/DeferredOpt/fragment.glsl"));
        }

        private void BucketTask(Light[] lights, out Light[][][] buckets)
        {
            int tile_w = w / tile_x_cnt;
            int tile_h = h / tile_y_cnt;

            buckets = new Light[tile_x_cnt][][];

            for (int x = 0; x < w; x += tile_w)
            {
                buckets[x] = new Light[tile_y_cnt][];
                for (int y = 0; y < h; y += tile_h)
                {
                    int i = 0;
                    buckets[x][y] = new Light[MaxLightCount];

                    //TODO actually implement this, dummy implementation for testing
                    if(lights.Length > i)
                        buckets[x][y][i] = lights[i];

                }
            }
        }

        public void Submit(Light[] lights, RenderQueue queue)
        {
            //TODO ensure the framebuffer is bound
            //Dispatch a background thread to place light influence into 2d bins
            Light[][][] buckets = null;
            Thread bucket_thd = new Thread(() => BucketTask(lights, out buckets));
            bucket_thd.Start();

            //Render objects, saving the uv and material ID
            queue.Submit();

            //Wait for the binning task
            bucket_thd.Join();

            //Export the depth buffer to an opencl texture


            //Perform lighting computation in an opengl compute shader, reading the light bins


            //Perform shadowing computation in an opencl compute shader, ray tracing shadows from each bin to intersected surfaces

        }
    }
}
