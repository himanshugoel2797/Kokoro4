using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
using Kokoro.Graphics.OpenGL;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Engine.Graphics.Renderer;
using Kokoro.Graphics.Prefabs;

namespace TestApplication
{
    class ForwardPlusTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh mesh;
        private Texture tex;
        private TextureHandle handle;
        private RenderQueue clearQueue;
        private ForwardPlus fplus;
        private RenderState RenderState;
        private ShaderStorageBuffer transforms;
        private UniformBuffer mats;

        private Vector3 camPos;
        private bool updateCamPos = true;

        private Keyboard keybd;

        float angle = 0;

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
                keybd.KeyMap["ToggleCamera"] = Key.Z;
                keybd.KeyMap["ToggleWireframe"] = Key.W;

                camera = new FirstPersonCamera(keybd, new Vector3(0, 10, 0), Vector3.UnitY, "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 300000, 300000);
                mesh = CubeFactory.Create(grp);

                fplus = new ForwardPlus(1, 1, 1280, 720, grp);
                var fbuf = fplus.TargetFramebuffer;

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("heightmap.png", 1);
                tex = new Texture();
                tex.SetData(bitmapSrc, 0);

                handle = Texture.Default.GetHandle(TextureSampler.Default);
                handle.SetResidency(Residency.Resident);

                transforms = new ShaderStorageBuffer(1000, false);
                mats = new UniformBuffer(false);

                unsafe
                {
                    byte* data = transforms.Update();

                    var matrices = new Matrix4[] { Matrix4.Scale(10) };

                    fixed (Matrix4* mats = matrices)
                    {
                        long* s = (long*)mats;
                        long* d = (long*)data;

                        for (int i = 0; i < sizeof(Matrix4) * matrices.Length / sizeof(long); i++)
                        {
                            d[i] = s[i];
                        }
                    }
                    transforms.UpdateDone();

                    byte* mat_data = mats.Update();
                    long* m = (long*)mat_data;
                    m[0] = handle;
                    mats.UpdateDone();
                }

                RenderState = new RenderState(fbuf, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Default/fragment.glsl")), new ShaderStorageBuffer[] { transforms }, new UniformBuffer[] { mats }, true, true, DepthFunc.LEqual, 1, -1, BlendFactor.One, BlendFactor.Zero, Vector4.One, 1, CullFaceMode.None);
                RenderState.ShaderProgram.SetShaderStorageBufferMapping("transforms", 0);
                RenderState.ShaderProgram.SetUniformBufferMapping("Material_t", 0);

                clearQueue = new RenderQueue(10, false);
                clearQueue.ClearFramebufferBeforeSubmit = true;
                clearQueue.BeginRecording();
                clearQueue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = 1,
                            Mesh = mesh
                        }
                    },
                    State = RenderState
                });
                clearQueue.EndRecording();

                inited = true;
            }

            if (keybd.IsKeyReleased("ToggleCamera"))
            {
                updateCamPos = !updateCamPos;
            }

            if (keybd.IsKeyReleased("ToggleWireframe"))
            {
                GraphicsDevice.Wireframe = !GraphicsDevice.Wireframe;
            }

            if (updateCamPos && camPos != camera.Position)
            {
                camPos = camera.Position;
                //  planetRenderer.Update(camPos, camera.Direction);
            }

            RenderState.ShaderProgram.Set("View", EngineManager.View);
            RenderState.ShaderProgram.Set("Projection", EngineManager.Projection);

            clearQueue.Submit();
            //if(Vector3.Dot(camera.Direction, r.Normal) <= 0)

            //TODO: try disabling renderers to see if it's only one of two passes interfering
            //TODO: start designing system to procedurally generate materials given terrain properties

            fplus.SubmitDraw();
        }

        public void Update(double interval)
        {
            camera?.Update(interval);
            angle += 0.0005f;

        }
    }
}
