using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kokoro.Graphics.Prefabs;

#if DEBUG
using Kokoro.Graphics.OpenGL;
#endif

namespace Kokoro.Engine.Graphics.Renderer
{
    public class ForwardPlus
    {
        //setup a background task to start clustering lights into a clustered frustum -> 16 * 16 * 16 region with logarithmic depth
        //Draw the entire scene to just depth buffer, first static stuff, then dynamic
        //Set depth test to equal, light the scene into an hdr buffer, render out a normal map as well
        //calculate screen space reflections using the previous frame's hdr buffer
        //render the ui
        //blend the results together

        private Framebuffer gbuffer, destBuffer;
        private Mesh mesh;
        private Texture depth, albedo;
        private RenderState s;
        private RenderQueue q;
        private int tile_x_cnt, tile_y_cnt, w, h;
        const int MaxLightCount = 10;

        public const string Library = "Deferred";
        public Framebuffer TargetFramebuffer { get { return gbuffer; } }
        public Texture Depth { get { return depth; } }
        public Texture Albedo { get { return albedo; } }
        public DepthFunc DepthFunction { get { return DepthFunc.Greater; } }
        public float NearClip { get { return -1; } }
        public float FarClip { get { return 1; } }
        public float ClearDepth { get { return 0; } }

        public int Width { get { return w * tile_x_cnt; } }
        public int Height { get { return h * tile_y_cnt; } }

        public const int OutputColorAttachment = 0;
        //TODO: make all resources accesses use functions to retrieve things, thus allowing control of resource access through libraries.

        static ForwardPlus()
        {
            //var lib = ShaderLibrary.Create(Library);
            //lib.AddSourceFile("Graphics/Shaders/Deferred/library.glsl");
        }

        public ForwardPlus(int tile_x_cnt, int tile_y_cnt, int w, int h, MeshGroup grp, Framebuffer destFb = null)
        {
            this.tile_x_cnt = tile_x_cnt;
            this.tile_y_cnt = tile_y_cnt;
            this.w = w;
            this.h = h;
            this.destBuffer = (destFb == null) ? Framebuffer.Default : destFb;
            this.mesh = FullScreenTriangleFactory.Create(grp);

            depth = new Texture();
            depth.SetData(new DepthTextureSource(w, h)
            {
                InternalFormat = PixelInternalFormat.DepthComponent32f
            }, 0);

            //Color accumulation buffer
            albedo = new Texture();
            albedo.SetData(new FramebufferTextureSource(w, h, 1)
            {
                InternalFormat = PixelInternalFormat.Rgba16f,
                PixelType = PixelType.HalfFloat
            }, 0);

            //Deploy async task to cull lights
            //Fill the depth buffer
            //Render everything with their own shaders
            //Place fragments into a cubemap, computing an updated irradiance map every few frames
            //Dispatch compute shader to compute lighting on specifically the pixels that require it, with the lights that specifically need it.

            //TODO: need to make adjustments to how draws are submitted in order to support the Z-prepass

            gbuffer = new Framebuffer(w, h);
            gbuffer[FramebufferAttachment.DepthAttachment] = depth; 
            gbuffer[FramebufferAttachment.ColorAttachment0 + OutputColorAttachment] = albedo;

            
            s = new RenderState(destBuffer, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/FrameBufferTriangle/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/FrameBufferTriangle/fragment.glsl")), null, null, false, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.Back);

            q = new RenderQueue(10, false);

            q.ClearAndBeginRecording();
            q.ClearFramebufferBeforeSubmit = false;
            q.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = mesh } },
                State = s
            });
            q.EndRecording();
        }

        public void SubmitDraw()
        {
#if DEBUG
            bool wframe = GraphicsDevice.Wireframe;
            GraphicsDevice.Wireframe = false;
#endif

            TextureHandle hndl = albedo.GetHandle(TextureSampler.Default);
            hndl.SetResidency(Residency.Resident);
            s.ShaderProgram.Set("AlbedoMap", hndl);

            q.Submit();
            hndl.SetResidency(Residency.NonResident);
            //GL.BlitNamedFramebuffer(gbuffer.id, destBuffer.id, 0, 0, w, h, 0, 0, w, h, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

#if DEBUG
            GraphicsDevice.Wireframe = wframe;
#endif
        }

    }
}
