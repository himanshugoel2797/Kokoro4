using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
using Kokoro.Graphics.OpenGL;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.Demos.PBR
{
    public class LambertTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh sphere;
        private RenderQueue clearQueue;
        private RenderState renderState;

        private ShaderStorageBuffer WorldTransforms;
        private UniformBuffer Textures;

        private Keyboard keybd;

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
                keybd = new Keyboard();
                keybd.KeyMap["ToggleWireframe"] = Key.W;

                camera = new FirstPersonCamera(keybd, new Vector3(43.74f, 15.3f, 9.07f), new Vector3(-0.951f, -0.309f, 4.1572e-08f), "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 30000, 30000);
                sphere = SphereFactory.Create(grp);

                List<Matrix4> SphereTransforms = new List<Matrix4>();

                for(int y = 0; y < 8; y++)
                    for(int x = 0; x < 8; x++)
                        SphereTransforms.Add(Matrix4.CreateTranslation(x * 3, 0, y * 3));

                //TODO: Scene graph based on joint relationships
                //TODO: Update scene graph based on animations
                //TODO: Physics engine uses the above scene graph
                //TODO: The physics engine then builds a render graph from its updated scene graph
                //TODO: Use the scenegraph to implement an octree construction step for frustum culling
                //TODO: Dump the render graph

                unsafe
                {
                    WorldTransforms = new ShaderStorageBuffer(sizeof(Matrix4) * SphereTransforms.Count, false);

                    var b_ptr = WorldTransforms.Update();
                    var matrices = SphereTransforms.ToArray();
                    fixed (Matrix4* mats = matrices)
                    {
                        long* s = (long*)mats;
                        long* d = (long*)b_ptr;

                        for (int i = 0; i < sizeof(Matrix4) * matrices.Length / sizeof(long); i++)
                        {
                            d[i] = s[i];
                        }
                    }
                    WorldTransforms.UpdateDone();

                    Textures = new UniformBuffer(false);

                    b_ptr = Textures.Update();
                    long* l_ptr = (long*)b_ptr;
                    l_ptr[0] = Texture.Default.GetHandle(TextureSampler.Default);
                    Textures.UpdateDone();

                    Texture.Default.GetHandle(TextureSampler.Default).SetResidency(Residency.Resident);
                }

                renderState = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Lambert/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Lambert/fragment.glsl")), new ShaderStorageBuffer[] { WorldTransforms }, new UniformBuffer[] { Textures }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.None);
                clearQueue = new RenderQueue(1, false);
                clearQueue.BeginRecording();
                clearQueue.ClearFramebufferBeforeSubmit = true;
                clearQueue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = SphereTransforms.Count,
                            Mesh = sphere
                        }
                    },
                    State = renderState
                });
                clearQueue.EndRecording();

                inited = true;
            }

            if (keybd.IsKeyReleased("ToggleWireframe"))
            {
                GraphicsDevice.Wireframe = !GraphicsDevice.Wireframe;
            }

            renderState.ShaderProgram.Set("View", EngineManager.View);
            renderState.ShaderProgram.Set("Projection", EngineManager.Projection);

            clearQueue.Submit();
        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
