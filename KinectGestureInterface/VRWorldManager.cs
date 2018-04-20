using Kokoro.Engine;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
using Kokoro.Math;
using Kokoro.StateMachine;
using Kokoro.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface
{
    class VRWorldManager : IState
    {
        MeshGroup grp;

        VRRenderer VRRenderer;
        RenderQueue clearQueue;
        Matrix4 leftEyeProj, rightEyeProj, centerPose;
        Keyboard keybd;

        VoxelRayCaster voxelRayCaster;
        Matrix4 volumePose;
        Texture volumeData;

        public VRWorldManager(Keyboard k)
        {
            keybd = k;
        }

        public void Enter(IState prev)
        {
            grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 10000, 10000);

            //Setup VR
            VRRenderer = VRRenderer.Create();
            leftEyeProj = VRRenderer.GetEyeProjection(true, 0.01f, 1000f);
            rightEyeProj = VRRenderer.GetEyeProjection(false, 0.01f, 1000f);
            centerPose = Matrix4.Identity;
            volumePose = Matrix4.Identity;

            var RenderState0 = new RenderState(VRRenderer.LeftFramebuffer, null, null, null, true, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
            var RenderState1 = new RenderState(VRRenderer.RightFramebuffer, null, null, null, true, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
            clearQueue = new RenderQueue(2, false);
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

            //Load in the volumetric data
            volumeData = new Texture();
            var volSrc = VolumeDataTextureSource.Load("../../../CThead/CThead.", 256, 256, 113);
            volumeData.SetData(volSrc, 0);

            voxelRayCaster = new VoxelRayCaster(VRRenderer.Width, VRRenderer.Height, volumeData, grp, VRRenderer.LeftFramebuffer, VRRenderer.RightFramebuffer);
        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {

            Console.WriteLine($"Match: {interval}");
            var pose = VRRenderer.GetPose();

            if (keybd.IsKeyReleased("ResetHeadset"))
            {
                centerPose = Matrix4.Invert(pose);
            }
            pose = pose * centerPose;

            var leftEyeView = VRRenderer.GetEyeView(true);
            var rightEyeView = VRRenderer.GetEyeView(false);

            leftEyeView = EngineManager.View * leftEyeView;
            rightEyeView = EngineManager.View * rightEyeView;

            clearQueue.Submit();

            voxelRayCaster.Draw(volumePose, leftEyeView * pose, leftEyeProj, 0);
            voxelRayCaster.Draw(volumePose, rightEyeView * pose, rightEyeProj, 1);

            VRRenderer.Submit(true);
            VRRenderer.Submit(false);
        }

        public void Update(double interval)
        {

        }
    }
}
