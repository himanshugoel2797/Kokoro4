using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
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
        RenderQueue.MeshData[] meshes;
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
            int side = 1 << 2;

            int lim = 100;

            if (!inited)
            {
                gbuf = new GBuffer(1920, 1080);
                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 3 * 11000, 3 * 11000);
                fsq = Kokoro.Graphics.Prefabs.FullScreenQuadFactory.Create(grp);
                camera = new FirstPersonCamera(new Keyboard(), Vector3.UnitX, Vector3.UnitY, "FPV");
                camera.Enabled = true;

                //mesh = new Mesh(grp, 25000, 25000, "car_0A.k4_stmesh");

                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                List<RenderQueue.MeshData> meshList = new List<RenderQueue.MeshData>();
                List<Matrix4> transforms = new List<Matrix4>();

                octree = new VoxelOctree(0, 1 << 10);

                for (int k = 0; k <= side; k++)
                {
                    for (int j = 0; j <= side; j++)
                        for (int i = 0; i <= side; i++)
                        {
                            if ((i - side / 2 >= lim | j - side / 2 >= lim | k - side / 2 >= lim) || (i - side / 2 < -lim | j - side / 2 < -lim | k - side / 2 < -lim))
                            {
                                octree.Add(new VoxelColor() { R = 255, G = 0, B = 255, A = 255 }, i - side / 2, j - side / 2, k - side / 2, 1);
                            }
                        }
                    octree.Optimize();
                }

                //Free up the memory from the octree before proceeding
                GC.Collect(GC.GetGeneration(octree), GCCollectionMode.Forced, true);

                Mesh cube = Kokoro.Graphics.Prefabs.CubeFactory.Create(grp);
                int inst_cnt = 0;

                int maxLevel = 6;

                #region Voxelization routine
                Action<VoxelOctree, Vector3, uint> build_mesh = null;
                build_mesh = (a, vec, adj_mask) =>
                {
                    if (a.Children == null | a.Level == maxLevel)
                    {
                        if (a.Color.A == 0) return;

                        float obj_side = (a.Data.WorldSide >> (a.Level + 2));
                        Vector3 obj_s = new Vector3(obj_side);

                        //Make this actually generate the geometry based on the adjacency mask
                        inst_cnt++;
                        transforms.Add(Matrix4.Scale(a.Data.WorldSide >> a.Level) * Matrix4.CreateTranslation(vec - obj_s));
                        return;
                    }

                    for (int i = 0; i < a.Children.Length; i++)
                    {
                        VoxelOctree child = a.Children[i];


                        if (child == null)
                        {
                            long x_side = ((i & 1) == 1 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> (a.Level + 2));
                            long y_side = ((i & 2) == 2 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> (a.Level + 2));
                            long z_side = ((i & 4) == 4 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> (a.Level + 2));

                            //These voxels are not visible
                            //meshList.Add(cube);
                            //transforms.Add(Matrix4.Scale(a.Data.WorldSide >> (a.Level + 1)) * Matrix4.CreateTranslation((float)(x_c + x_side), (float)(y_c + y_side), (float)(z_c + z_side)));
                        }
                        else
                        {
                            float x_side = ((i & 1) == 1 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> a.Level);
                            float y_side = ((i & 2) == 2 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> a.Level);
                            float z_side = ((i & 4) == 4 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> a.Level);

                            x_side /= 4;
                            y_side /= 4;
                            z_side /= 4;

                            //Depending on the location, we will depend on the adjacency data passed down
                            //Actually, adjacency data will not work, we do not have enough high resolution data

                            Vector3 s = new Vector3(x_side, y_side, z_side);

                            //TODO calculate adjacency mask for the child and pass it down in the last parameter
                            build_mesh(child, vec + s, 0);
                        }
                    }
                };
                #endregion

                build_mesh(octree, Vector3.Zero, 0);

                meshList.Add(new RenderQueue.MeshData() { Mesh = cube, InstanceCount = inst_cnt, BaseInstance = 0 });
                meshes = meshList.ToArray();


                unsafe
                {
                    transform_params = new ShaderStorageBuffer(transforms.Count * sizeof(Matrix4), false);
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

                queue = new RenderQueue(meshes.Length, false);
                queue.ClearAndBeginRecording();
                queue.ClearFramebufferBeforeSubmit = true;
                queue.RecordDraw(new RenderQueue.DrawData() { Meshes = meshes, State = state });
                queue.EndRecording();

                handle = ((Framebuffer)gbuf)[FramebufferAttachment.ColorAttachment0].GetHandle(TextureSampler.Default);
                handle.SetResidency(TextureResidency.Resident);

                fsq_state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Framebuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Framebuffer/fragment.glsl")), null, null, false, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);

                fsq_queue = new RenderQueue(1, false);
                fsq_queue.ClearAndBeginRecording();
                fsq_queue.ClearFramebufferBeforeSubmit = true;
                fsq_queue.RecordDraw(new RenderQueue.DrawData() { Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { Mesh = fsq, BaseInstance = 0, InstanceCount = 1 } }, State = fsq_state });
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
