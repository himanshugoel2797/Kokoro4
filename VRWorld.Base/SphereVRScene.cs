using Kokoro.Engine;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Graphics.Lights;
using Kokoro.Engine.Graphics.Materials;
using Kokoro.Engine.Graphics.Renderer;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.StateMachine;
using Kokoro.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRWorld.Base
{
    class SphereVRScene : IState
    {
        struct VRComponents
        {
            public Matrix4 Projection;
            public Mesh[] model;
            public Texture[] model_tex;
            public SimpleStaticMeshRenderer staticMeshRenderer;
        }

        VRClient vr;
        VRComponents[] vr_data;
        TexturelessDeferred deferred;

        MeshGroup meshGroup;
        Mesh sphere;

        public SphereVRScene()
        {

        }

        public void Enter(IState prev)
        {
            {
                vr = VRClient.Create(ExperienceKind.Standing);
                vr_data = new VRComponents[2];

                for (int i = 0; i < 2; i++)
                    vr_data[i] = new VRComponents()
                    {
                        Projection = vr.GetEyeProjection(VRHand.Get(i), 0.01f)
                    };

                deferred = new TexturelessDeferred(vr.Width, vr.Height, new Framebuffer[] { vr.LeftFramebuffer, vr.RightFramebuffer }, new Matrix4[] { vr_data[VRHand.Left.Value].Projection, vr_data[VRHand.Right.Value].Projection });
            }
            {
                meshGroup = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 30000, 30000);
                sphere = SphereFactory.Create(meshGroup);

                int mat_idx = deferred.RegisterMaterial(new PBRMetalnessMaterial("Sphere_default_mat")
                {
                    Albedo = Texture.Default,
                    AlbedoSampler = TextureSampler.Default,
                    MetalRoughnessDerivative = Texture.Default,
                    MetalRoughnessDerivativeSampler = TextureSampler.Default
                });

                for (int i = 0; i < 2; i++)
                {
                    vr_data[i].staticMeshRenderer = new SimpleStaticMeshRenderer(512, false, deferred.Resources[i].Programs[TexturelessDeferred.ProgramIndex.StaticMesh], deferred.Resources[i].GBuffer, true, Vector4.Zero);
                    vr_data[i].staticMeshRenderer.AddDraw(sphere, 81, (short)mat_idx);
                }

                for (int y = 0; y < 9; y++)
                    for (int x = 0; x < 9; x++)
                    {
                        var mat = Matrix4.CreateTranslation((x - 5) * 3, 0, (y - 5) * 3);
                        vr_data[0].staticMeshRenderer.Update(y * 9 + x, mat);
                        vr_data[1].staticMeshRenderer.Update(y * 9 + x, mat);
                    }
            }
            {
                //Setup input
                vr.InitializeControllers(@"manifests\actions.json", new VRActionSet[]
                {
                    new VRActionSet("/actions/vrworld",
                        new VRAction("pickup", ActionHandleDirection.Input, ActionKind.Digital),
                        new VRAction("activate", ActionHandleDirection.Input, ActionKind.Digital),
                        new VRAction("analog_right", ActionHandleDirection.Input, ActionKind.Analog),
                        new VRAction("analog_left", ActionHandleDirection.Input, ActionKind.Analog),
                        new VRAction("hand_right", ActionHandleDirection.Input, ActionKind.Pose),
                        new VRAction("hand_left", ActionHandleDirection.Input, ActionKind.Pose),
                        new VRAction("menu_right", ActionHandleDirection.Input, ActionKind.Digital),
                        new VRAction("menu_left", ActionHandleDirection.Input, ActionKind.Digital),
                        new VRAction("haptic_right", ActionHandleDirection.Output, ActionKind.Haptic),
                        new VRAction("haptic_left", ActionHandleDirection.Output, ActionKind.Haptic))
                });
            }

            var pl = new PointLight();
            pl.Position = new Vector3(0, 0, 0);
            pl.Radius = 80;
            pl.Intensity = 1;
            pl.Color = new Vector3(1, 1, 1);
            deferred.RegisterLight(pl);
        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {
            var pose = vr.GetPose();

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Matrix4 leftEyeView, rightEyeView;
            leftEyeView = vr.GetEyeView(VRHand.Left);
            rightEyeView = vr.GetEyeView(VRHand.Right);
            deferred.Update(new Matrix4[] { leftEyeView * pose.PoseMatrix, rightEyeView * pose.PoseMatrix });

            var left_poseData = vr.GetPoseData("/actions/vrworld/in/hand_left");
            vr.GetControllerMesh(left_poseData.ActiveOrigin, meshGroup, out var ctrl, out var ctrl_tex);
            if (vr_data[VRHand.Left.Value].model == null)
            {
                vr_data[VRHand.Left.Value].model = ctrl;
                vr_data[VRHand.Left.Value].model_tex = ctrl_tex;

                int mat_idx = deferred.RegisterMaterial(new PBRMetalnessMaterial("ctrl_default_mat")
                {
                    Albedo = ctrl_tex[0],
                    AlbedoSampler = TextureSampler.Default,
                    MetalRoughnessDerivative = ctrl_tex[0],
                    MetalRoughnessDerivativeSampler = TextureSampler.Default
                });

                for (int j = 0; j < 2; j++)
                    for (int i = 0; i < ctrl.Length; i++)
                        vr_data[j].staticMeshRenderer.AddDraw(ctrl[i], 1, (short)mat_idx);
            }

            var mats = vr.GetComponentTransforms(left_poseData.ActiveOrigin);
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mats[i] * left_poseData.PoseMatrix;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < mats.Length; j++)
                    vr_data[i].staticMeshRenderer.Update(j + 81, mats[j]);
                vr_data[i].staticMeshRenderer.Submit();
            }

            vr.LeftFramebuffer.Blit(deferred.Resources[0].AccumulatorBuffer, true, false, true);
            vr.RightFramebuffer.Blit(deferred.Resources[1].AccumulatorBuffer, true, false, true);

            Framebuffer.Default.Blit(vr.RightFramebuffer, true, false, true);
            for (int i = 0; i < 2; i++)
                vr.Submit(VRHand.Get(i));

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedTicks / (float)System.Diagnostics.Stopwatch.Frequency * 1000);
        }

        public void Update(double interval)
        {
            vr.Update();
        }
    }
}
