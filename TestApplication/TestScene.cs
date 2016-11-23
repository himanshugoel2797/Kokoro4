using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Voxel;
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
        Mesh fsq;
        Mesh[] meshes;
        RenderState state, fsq_state;
        RenderQueue queue, fsq_queue;
        TextureHandle handle;
        ShaderStorageBuffer transform_params;
        FirstPersonCamera camera;
        GBuffer gbuf;
        VoxelOctree octree;

        public float km(float val)
        {
            return val * 1000f;
        }

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
            int side = 1 << 8;

            int lim = 120;

            if (!inited)
            {
                gbuf = new GBuffer(1920, 1080);
                grp = new MeshGroup(3 * 11000, 3 * 11000);
                fsq = Kokoro.Graphics.Prefabs.FullScreenQuadFactory.Create(grp);
                camera = new FirstPersonCamera(Vector3.UnitX, Vector3.UnitY, "FPV");
                camera.Enabled = true;
                
                //mesh = new Mesh(grp, 25000, 25000, "car_0A.k4_stmesh");

                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                List<Mesh> meshList = new List<Mesh>();
                List<Matrix4> transforms = new List<Matrix4>();

                octree = new VoxelOctree(0, 1 << 10);

                for (int k = 0; k <= side; k++)
                    for (int j = 0; j <= side; j++)
                        for (int i = 0; i <= side; i++)
                        {
                            if ((i - side / 2 >= lim | j - side / 2 >= lim | k - side / 2 >= lim) || ( i - side / 2 < -lim | j - side / 2 < -lim | k - side / 2 < -lim))
                                octree.Add(new VoxelColor() { R = 255, G = 0, B = 255, A = 255 }, i - side / 2, j - side / 2, k - side / 2, 1);
                        }

                octree.Optimize();
                meshes = meshList.ToArray();


                unsafe
                {
                    transform_params = new ShaderStorageBuffer(transforms.Count * sizeof(Matrix4));
                    byte* data = transform_params.Update();

                    var matrices = transforms.ToArray();

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

                state = new RenderState(gbuf, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Default/fragment.glsl")), new ShaderStorageBuffer[] { transform_params }, new UniformBuffer[] { }, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, new Vector4(0, 0.5f, 1.0f, 0.0f), 0, CullFaceMode.Back);
                state.ShaderProgram.SetShaderStorageBufferMapping("transforms", 0);

                queue = new RenderQueue(meshes.Length);
                queue.ClearAndBeginRecording();
                queue.ClearFramebufferBeforeSubmit = true;
                queue.RecordDraw(new RenderQueue.DrawData() { Meshes = meshes, State = state });
                queue.EndRecording();

                handle = ((Framebuffer)gbuf)[FramebufferAttachment.ColorAttachment0].GetHandle(TextureSampler.Default);
                handle.SetResidency(TextureResidency.Resident);

                fsq_state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Framebuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Framebuffer/fragment.glsl")), null, null, false, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);

                fsq_queue = new RenderQueue(1);
                fsq_queue.ClearAndBeginRecording();
                fsq_queue.ClearFramebufferBeforeSubmit = true;
                fsq_queue.RecordDraw(new RenderQueue.DrawData() { Meshes = new Mesh[] { fsq }, State = fsq_state });
                fsq_queue.EndRecording();

                inited = true;
            }


            state.ShaderProgram.Set("View", EngineManager.View);
            state.ShaderProgram.Set("Projection", EngineManager.Projection);
            fsq_state.ShaderProgram.Set("AlbedoMap", handle);

            //Kokoro.Graphics.OpenGL.GraphicsDevice.Wireframe = true;

            queue.Submit();

            Kokoro.Graphics.OpenGL.GraphicsDevice.Wireframe = false;
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
