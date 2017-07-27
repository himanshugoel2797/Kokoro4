using Kokoro.Engine;
using Kokoro.Engine.Graphics;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.Math;

namespace TestApplication
{
    public class HeightfieldGITest : IState
    {
        private bool inited = false;
        private RenderState stateF;
        private RenderQueue queueF;
        private UniformBuffer v_dash;
        private Texture[] v_sh;
        private Texture[] u_sh;

        int idx = 0;

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {
            //Max mipmap the heightmap
            //Determine visibility angles from direction for each point and cache them in a second texture
            //Perform this calculation for n directions
            //

            int net = 360;
            int step = 10;
            int res = 512;

            if (!inited)
            {

                Kokoro.Graphics.OpenGL.GraphicsDevice.WindowSize = new System.Drawing.Size(1024, 1024);
                Texture srcMap = new Texture();
                srcMap.GenerateMipmaps = true;
                BitmapTextureSource srcMapSrc = new BitmapTextureSource("heightmap.png", 10);
                srcMap.SetData(srcMapSrc, 0);
                srcMap.SetTileMode(false, false);
                TextureHandle srcMapH = srcMap.GetHandle(TextureSampler.Default);
                srcMapH.SetResidency(Residency.Resident);

                Texture srcColorMap = new Texture();
                BitmapTextureSource srcColorMapSrc = new BitmapTextureSource("colormap.png", 1);
                srcColorMap.SetData(srcColorMapSrc, 0);
                TextureHandle srcColorMapH = srcColorMap.GetHandle(TextureSampler.Default);
                srcColorMapH.SetResidency(Residency.Resident);

                Framebuffer[] fbufs = new Framebuffer[net / step];
                v_sh = new Texture[net / step];  
                u_sh = new Texture[net / step];

                MeshGroup grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 100, 100);
                Mesh fsq = FullScreenQuadFactory.Create(grp);


                //Precompute 256 values of v_dash
                v_dash = new UniformBuffer(false);
                unsafe
                {
                    Vector4 P_i(double angle)
                    {
                        return new Vector4((float)((Sin(angle) + 1) / Sqrt(2)),
                                            (float)(-3 * Cos(angle) * Cos(angle) / (2 * Sqrt(6))),
                                            (float)(-5 * Sin(angle) * Cos(angle) * Cos(angle) / (2 * Sqrt(10))),
                                            (float)((7 * Cos(angle) * Cos(angle) * (-4 * 5 * Cos(angle) * Cos(angle))) / (8 * Sqrt(14)))
                                            );
                    }

                    float* f = (float*)v_dash.Update();
                    for (int i = 0; i < 256; i++)
                    {
                        double idx = i * (PI * 0.5f) / 255.0f;
                        Vector4 v_dash_res = P_i(idx);

                        f[0] = v_dash_res.X;
                        f[1] = v_dash_res.Y;
                        f[2] = v_dash_res.Z;
                        f[3] = v_dash_res.W;

                        f += 4;
                    }
                    v_dash.UpdateDone();
                }


                for (int i = 0, j = 0; i < net; i += step, j++)
                {
                    fbufs[j] = new Framebuffer(res, res);
                    v_sh[j] = new Texture();
                    FramebufferTextureSource src = new FramebufferTextureSource(res, res, 1)
                    {
                        InternalFormat = PixelInternalFormat.Rgba16f,
                        PixelType = PixelType.Float
                    };
                    v_sh[j].SetData(src, 0);
                    fbufs[j][FramebufferAttachment.ColorAttachment0] = v_sh[j];


                    u_sh[j] = new Texture();
                    FramebufferTextureSource src_u = new FramebufferTextureSource(res, res, 1)
                    {
                        InternalFormat = PixelInternalFormat.Rgba16f,
                        PixelType = PixelType.Float
                    };
                    u_sh[j].SetData(src_u, 0);
                    fbufs[j][FramebufferAttachment.ColorAttachment1] = u_sh[j];

                    RenderState state = new RenderState(fbufs[j], new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Framebuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/HeightMapGI_A/fragment.glsl")), null, new UniformBuffer[] { v_dash }, false, true, DepthFunc.None, 0, 1, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha, Vector4.Zero, 1, CullFaceMode.Back);
                    RenderQueue queue = new RenderQueue(10, false);
                    queue.ClearAndBeginRecording();
                    queue.RecordDraw(new RenderQueue.DrawData()
                    {
                        Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = fsq }, },
                        State = state
                    }); 
                    queue.EndRecording();

                    float angle = i * (float)PI / 180.0f;
                    Vector2 dir = new Vector2((float)Cos(angle), (float)Sin(angle));
       
                    state.ShaderProgram.Set("Direction", dir);
                    state.ShaderProgram.Set("SrcRadianceMultiplier", 0);
                    state.ShaderProgram.Set("SrcMap", srcMapH);
                    state.ShaderProgram.Set("SrcColor", srcColorMapH);

                    state.ShaderProgram.SetUniformBufferMapping("V_dash", 0);

                    queue.Submit(); 
                }

                //Store direct illumination samples into a lower res image using UV coordinates
                //Dynamic objects have their shadowing behaviors precomputed, if light hits surflet A, the opposite radiance is propogated out the other end, allowing for use of 'negative radiance' for shadowing
                //Build spatial partition of sample points using clustering to optimize
                //temporal solution, run a pass every frame    

                stateF = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Framebuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Framebuffer/fragment.glsl")), null, null, false, true, DepthFunc.None, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 1, CullFaceMode.Back);
                queueF = new RenderQueue(10, false);
                queueF.ClearAndBeginRecording();
                queueF.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = fsq }, },
                    State = stateF
                });
                queueF.EndRecording();


                inited = true;
            }

            //idx = 52;  
            //idx = 0; 
            TextureHandle h = u_sh[(idx++ / 30) % (net / step)].GetHandle(TextureSampler.Default);
            h.SetResidency(Residency.Resident);
            stateF.ShaderProgram.Set("AlbedoMap", h);
            queueF.Submit();

        }

        public void Update(double interval)
        {

        }
    }
}
