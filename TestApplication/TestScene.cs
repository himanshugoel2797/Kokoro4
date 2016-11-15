using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class TestScene : IState
    {
        bool inited = false;
        MeshGroup grp;
        Mesh mesh, cube, fsq;
        RenderState state, fsq_state;
        RenderQueue queue, fsq_queue;
        TextureHandle handle;
        UniformBuffer transform_params;
        FirstPersonCamera camera;
        GBuffer gbuf;

        struct transformData
        {
            public Matrix4[] matrices;
        };

        public void Update(double interval)
        {
            camera?.Update(interval);
        }

        public void Render(double interval)
        {
            //TODO: Make a portal rendering based game to simplify algorithms
            //Create MeshGroup, create mesh, create render state, create render queue
            //Use meshgroup to store mesh
            //Create renderstate with necessary state
            //Record draw to render queue
            //Perform render

            if (!inited)
            {
                gbuf = new GBuffer(1280, 720);
                grp = new MeshGroup(3 * 11000, 3 * 11000);
                mesh = new Mesh(grp, 25000, 25000, "car_0A.k4_stmesh");
                fsq = Kokoro.Graphics.Prefabs.FullScreenQuadFactory.Create(grp);
                cube = Kokoro.Graphics.Prefabs.SphereFactory.Create(grp);
                camera = new FirstPersonCamera(Vector3.Zero, Vector3.UnitX, "FPV");
                camera.Enabled = true;

                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                transform_params = new UniformBuffer();



                unsafe
                {
                    byte* data = transform_params.Update();

                    var matrices = new Matrix4[] { Matrix4.CreateTranslation(10, 0, 0), Matrix4.Scale(695700 * 1000) * Matrix4.CreateTranslation(-Vector3.UnitX * 1000000 * 1000) };

                    fixed (Matrix4* mats = matrices)
                    {
                        long* s = (long*)mats;
                        long* d = (long*)data;

                        for (int i = 0; i < sizeof(Matrix4) * matrices.Length / sizeof(long); i++)
                        {
                            d[i] = s[i];
                        }
                    }
                    transform_params.UpdateDone();
                }

                state = new RenderState(gbuf, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Default/fragment.glsl")), null, new UniformBuffer[] { transform_params }, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, new Vector4(0, 0.5f, 1.0f, 0.0f), 0, CullFaceMode.Back);
                state.ShaderProgram.SetUniformBufferMapping("transforms", 0);

                queue = new RenderQueue();
                queue.ClearAndBeginRecording();
                queue.ClearFramebufferBeforeSubmit = true;
                queue.RecordDraw(new RenderQueue.DrawData() { Meshes = new Mesh[] { mesh, cube }, State = state });
                queue.EndRecording();

                handle = ((Framebuffer)gbuf)[FramebufferAttachment.ColorAttachment0].GetHandle(TextureSampler.Default);
                handle.SetResidency(TextureResidency.Resident);

                fsq_state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Framebuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Framebuffer/fragment.glsl")), null, null, false, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);

                fsq_queue = new RenderQueue();
                fsq_queue.ClearAndBeginRecording();
                fsq_queue.ClearFramebufferBeforeSubmit = true;
                fsq_queue.RecordDraw(new RenderQueue.DrawData() { Meshes = new Mesh[] { fsq }, State = fsq_state });
                fsq_queue.EndRecording();

                inited = true;
            }

            state.ShaderProgram.Set("View", EngineManager.View);
            state.ShaderProgram.Set("Projection", EngineManager.Projection);
            fsq_state.ShaderProgram.Set("AlbedoMap", handle);

            queue.Submit();
            fsq_queue.Submit();
        }

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {

        }
    }
}
