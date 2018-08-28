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

namespace TestApplication
{
    class AtmosphereTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private PlanetRenderer planetRenderer;
        private Texture tex;
        private TextureHandle handle;
        private RenderQueue clearQueue;
        private ForwardPlus fplus;

        private Vector3 camPos;
        private bool updateCamPos = true;

        private Keyboard keybd;

        AtmosphereRenderer renderer;
        float angle = 0;
        private bool updateAngle = false;

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
                keybd.KeyMap["ToggleDayNight"] = Key.D;
                keybd.KeyMap["ToggleWireframe"] = Key.W;

                camera = new FirstPersonCamera(keybd, new Vector3(0, 6360.005f, 0), Vector3.UnitY, "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 300000, 300000);

                GraphicsDevice.WindowSize = new System.Drawing.Size(1920, 1080);
                fplus = new ForwardPlus(1, 1, 1920, 1080, grp);
                var fbuf = fplus.TargetFramebuffer;

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("heightmap.png", 1);
                tex = new Texture();
                tex.SetData(bitmapSrc, 0);

                handle = tex.GetHandle(TextureSampler.Default);
                handle.SetResidency(Residency.Resident);

                //GraphicsDevice.Wireframe = true;

                float side = 500;
                float off = side * 0.5f;

                var RenderState = new RenderState(fbuf, null, null, null, true, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.None);
                clearQueue = new RenderQueue(1, false);
                clearQueue.BeginRecording();
                clearQueue.ClearFramebufferBeforeSubmit = true;
                clearQueue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = 0,
                            Mesh = null
                        }
                    },
                    State = RenderState
                });
                clearQueue.EndRecording();

                renderer = new AtmosphereRenderer(new Kokoro.Math.Vector3(19.918e-3f, 13.57e-3f, 5.75e-3f), 10.8f, 20e-3f, 1.2f, 3390, 3520, grp, fbuf);
                planetRenderer = new PlanetRenderer(grp, new Framebuffer[] { fbuf }, 3390, renderer);

                //renderer = new AtmosphereRenderer(new Kokoro.Math.Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f), 7.994f, 20e-3f, 1.2f, 6372.797f, 6472.797f, grp, fbuf);
                //planetRenderer = new PlanetRenderer(grp, new Framebuffer[] { fbuf }, 6360, renderer);

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

            if (keybd.IsKeyReleased("ToggleDayNight"))
            {
                updateAngle = !updateAngle;
            }

            if (updateCamPos && camPos != camera.Position)
            {
                camPos = camera.Position;

                planetRenderer.Update(camPos, camera.Direction);
            }

            clearQueue.Submit();
            //if(Vector3.Dot(camera.Direction, r.Normal) <= 0)

            //TODO: start designing system to procedurally generate materials given terrain properties

            renderer.Draw(camera.View, camera.Projection, camPos, new Vector3((float)System.Math.Sin(angle), (float)System.Math.Cos(angle), 0));
            planetRenderer.Draw(camera.View, camera.Projection);
            fplus.SubmitDraw();
        }

        public void Update(double interval)
        {
            camera?.Update(interval);


            if (updateAngle)
                angle += 0.00025f;

        }
    }
}
