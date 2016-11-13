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
        Mesh mesh, cube;
        RenderState state;
        RenderQueue queue;
        UniformBuffer transform_params; 
        FirstPersonCamera camera;

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

            if(!inited)
            {
                grp = new MeshGroup(3 * 11000, 3 * 11000);
                mesh = new Mesh(grp, 25000, 25000, "car_0A.k4_stmesh");
                cube = Kokoro.Graphics.Prefabs.CubeFactory.Create(grp);
                camera = new FirstPersonCamera(Vector3.Zero, Vector3.UnitX, "FPV");
                camera.Enabled = true;

                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                transform_params = new UniformBuffer();

          

                unsafe
                {
                    byte* data = transform_params.Update();
                    var matrices = new Matrix4[] { Matrix4.CreateTranslation(0, 100, 100), Matrix4.Identity };

                    fixed(Matrix4* mats = matrices)
                    {
                        long* s = (long*)mats;
                        long* d = (long*)data;

                        for (int i = 0; i < sizeof(Matrix4) * matrices.Length / sizeof(long); i++)
                        {
                            d[i] = s[i];
                        }
                    }
                }

                state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Default/fragment.glsl")), null, new UniformBuffer[] { transform_params }, true, DepthFunc.Greater, 0, 1, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha, Vector4.One, 0, CullFaceMode.Back);
                EngineManager.SetRenderState(state);
                
                state.ShaderProgram.SetUniformBufferMapping("transforms", 0);


                queue = new RenderQueue();
                queue.ClearAndBeginRecording();
                queue.RecordDraw(new RenderQueue.DrawData() { Meshes = new Mesh[] { mesh, cube }, State = state });
                queue.EndRecording();

                inited = true;
            }
            
            state.ShaderProgram.Set("View", EngineManager.View);
            state.ShaderProgram.Set("Projection", EngineManager.Projection);
            queue.Submit();
        }

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {

        }
    }
}
