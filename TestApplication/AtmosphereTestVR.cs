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
using Kokoro.VR;

namespace TestApplication
{
    class AtmosphereTestVR : IState
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

        private VRRenderer vrProvider;
        private Keyboard keybd;

        AtmosphereRenderer renderer;
        float angle = 0;

        private Matrix4 leftEyeProj;
        private Matrix4 rightEyeProj;
        private Matrix4 centerPose;

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
                keybd.KeyMap["ResetHeadset"] = Key.C;

                camera = new FirstPersonCamera(keybd, new Vector3(0, 6360, 0), Vector3.UnitY, "FPV");
                camera.Enabled = true;

                GraphicsDevice.WindowSize = new System.Drawing.Size(1776, 1776);
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 300000, 300000);

                fplus = new ForwardPlus(1, 1, 1280, 720, grp);
                var fbuf = fplus.TargetFramebuffer;

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("heightmap.png", 1);
                tex = new Texture();
                tex.SetData(bitmapSrc, 0);

                handle = tex.GetHandle(TextureSampler.Default);
                handle.SetResidency(Residency.Resident);

                //GraphicsDevice.Wireframe = true;

                float side = 500;
                float off = side * 0.5f;

                vrProvider = VRRenderer.Create();

                var RenderState0 = new RenderState(vrProvider.LeftFramebuffer, null, null, null, true, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                var RenderState1 = new RenderState(vrProvider.RightFramebuffer, null, null, null, true, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                var RenderState2 = new RenderState(fbuf, null, null, null, true, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                clearQueue = new RenderQueue(3, false);
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
                    State = RenderState2
                });
                clearQueue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = 0,
                            Mesh = null
                        }
                    },
                    State = RenderState0
                });
                clearQueue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = 0,
                            Mesh = null
                        }
                    },
                    State = RenderState1
                });
                clearQueue.EndRecording();

                renderer = new AtmosphereRenderer(new Kokoro.Math.Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f), 8, 20e-3f, 1.2f, 6360, 6420, grp, fbuf, vrProvider.LeftFramebuffer, vrProvider.RightFramebuffer);
                planetRenderer = new PlanetRenderer(grp, new Framebuffer[] { fbuf, vrProvider.LeftFramebuffer, vrProvider.RightFramebuffer }, 6360, renderer);


                leftEyeProj = vrProvider.GetEyeProjection(true, 0.01f);
                rightEyeProj = vrProvider.GetEyeProjection(false, 0.01f);

                centerPose = Matrix4.Identity;

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

                planetRenderer.Update(camPos, camera.Direction);
            }

            clearQueue.Submit();
            
            var pose = vrProvider.GetPose();

            if (keybd.IsKeyReleased("ResetHeadset"))
            {
                centerPose = Matrix4.Invert(pose);
            }
            pose = pose * centerPose;

            var leftEyeView = vrProvider.GetEyeView(true);
            var rightEyeView = vrProvider.GetEyeView(false);
            
            leftEyeView = camera.View * leftEyeView;
            rightEyeView = camera.View * rightEyeView;

            renderer.Draw(rightEyeView * pose, leftEyeProj, camPos, new Vector3((float)System.Math.Sin(angle), (float)System.Math.Cos(angle), 0), 0);
            renderer.Draw(leftEyeView * pose, leftEyeProj, camPos, new Vector3((float)System.Math.Sin(angle), (float)System.Math.Cos(angle), 0), 1);
            renderer.Draw(rightEyeView * pose, rightEyeProj, camPos, new Vector3((float)System.Math.Sin(angle), (float)System.Math.Cos(angle), 0), 2);

            planetRenderer.Draw(leftEyeView * pose, leftEyeProj, 0);
            planetRenderer.Draw(leftEyeView * pose, leftEyeProj, 1);
            planetRenderer.Draw(rightEyeView * pose, rightEyeProj, 2);
            vrProvider.Submit(true);
            vrProvider.Submit(false);
            fplus.SubmitDraw();
        }

        public void Update(double interval)
        {
            camera?.Update(interval);
            angle = 0.0105f;

        }
    }
}
