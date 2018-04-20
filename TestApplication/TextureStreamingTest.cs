using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class TextureStreamingTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh mesh;
        private RenderState state;
        private RenderQueue queue;
        private UniformBuffer ubo;
        private Texture tex;
        private TextureStreamer.TextureStream stream;
        private TextureHandle handle;

        private TextureStreamer tStreamer;

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {
        }

        public void Render(double interval)
        {
            if (!inited)
            {
                camera = new FirstPersonCamera(new Keyboard(), Vector3.UnitX, Vector3.UnitY, "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                tStreamer = new TextureStreamer(10);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 10000, 10000);
                mesh = FullScreenQuadFactory.Create(grp);

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("test.png", 10);
                stream = tStreamer.UploadTexture(bitmapSrc);
                tex = stream.TargetTexture;
                ubo = new UniformBuffer(false);

                handle = tex.GetHandle(TextureSampler.Default);

                Console.WriteLine(OpenTK.Graphics.OpenGL.GL.GetInteger((OpenTK.Graphics.OpenGL.GetPName)OpenTK.Graphics.OpenGL.All.ShaderStorageBufferOffsetAlignment));

                //Fill anything that will contain handles multiple times in order to have valid data everywhere.
                unsafe
                {
                    for (int i = 0; i < 4; i++)
                    {
                        long* l = (long*)ubo.Update();
                        l[0] = handle;
                        ubo.UpdateDone();
                    }
                }

                state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Default/fragment.glsl")), null, new UniformBuffer[] { ubo }, false, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);

                queue = new RenderQueue(10, false);
                 
                queue.ClearAndBeginRecording();
                queue.ClearFramebufferBeforeSubmit = true;
                queue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = mesh } },
                    State = state
                });
                queue.EndRecording();

                inited = true;
            }


            if (stream != null)
            {
                handle?.SetResidency(Residency.NonResident);
                TextureSampler sampler = stream.TargetSampler;
                handle = tex.GetHandle(sampler);
                handle.SetResidency(Residency.Resident);

                GC.Collect();

                unsafe
                {
                    long* l = (long*)ubo.Update();
                    l[0] = handle;
                    ubo.UpdateDone();
                }

                if (stream.IsDone)
                {
                    stream.Free();
                    stream = null;
                }
            }


            queue.Submit();

        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
