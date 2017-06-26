using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Renderer
{
    public class Deferred
    {
        //TODO add a flag to meshes to allow them to request the depth prepass testing.
        //setup a background task to start clustering lights into a clustered frustum -> 16 * 16 * 16 region with logarithmic depth
        //Draw the entire scene to just depth buffer, first static stuff, then dynamic
        //Set depth test to equal, light the scene into an hdr buffer, render out a normal map as well
        //calculate screen space reflections using the previous frame's hdr buffer
        //render the ui
        //blend the results together

        private Framebuffer gbuffer;
        private Texture depth, uv, mat;
        private int tile_x_cnt, tile_y_cnt, w, h;
        const int MaxLightCount = 10;

        public const string Library = "Deferred";
        public Framebuffer DeferredFramebuffer { get { return gbuffer; } }
        public Texture Depth { get { return depth; } }
        public Texture UV { get { return uv; } }
        public Texture Material { get { return mat; } }

        //TODO: make all resources accesses use functions to retrieve things, thus allowing control of resource access through libraries.

        public Deferred(int tile_x_cnt, int tile_y_cnt, int w, int h)
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
                InternalFormat = PixelInternalFormat.Rgba16f,
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

            var lib = ShaderLibrary.Create(Library);
            lib.AddSourceFile("Graphics/Shaders/Deferred/library.glsl");
        }
    }
}
