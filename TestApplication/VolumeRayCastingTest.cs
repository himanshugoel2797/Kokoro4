using KinectGestureInterface;
using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Graphics.Renderer;
using Kokoro.Engine.Input;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class VolumeRayCastingTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh cube, fsq;
        private RenderState clearState;
        private RenderQueue clearQueue;

        private Texture volumeData;

        private ForwardPlus forwardPlus;

        private VoxelRayCaster caster;

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
                //Setup rendering infrastructure
                camera = new FirstPersonCamera(new Keyboard(), Vector3.UnitX, Vector3.UnitY, "FPV")
                {
                    Enabled = true
                };
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                //Load in the volumetric data
                volumeData = new Texture();
                var volSrc = VolumeDataTextureSource.Load("../../../CThead/CThead.", 256, 256, 113);
                volumeData.SetData(volSrc, 0);

                //Setup the mesh data
                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 1000, 1000);
                
                //Setup forward plus renderer
                forwardPlus = new ForwardPlus(1, 1, 1280, 720, grp);
                caster = new VoxelRayCaster(forwardPlus.Width, forwardPlus.Height, volumeData, grp, forwardPlus.TargetFramebuffer);

                #region Clear
                clearState = new RenderState(forwardPlus.TargetFramebuffer, null, null, null, true, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.None);
                clearQueue = new RenderQueue(10, false);
                clearQueue.ClearAndBeginRecording();
                clearQueue.ClearFramebufferBeforeSubmit = true;
                clearQueue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 0, Mesh = null } },
                    State = clearState
                });
                clearQueue.EndRecording();
                #endregion
                
                inited = true;
            }

            clearQueue.Submit();    //Clear the display

            caster.Draw(Matrix4.Identity, EngineManager.View, EngineManager.Projection, 0);
            forwardPlus.SubmitDraw();   //Update the screen
        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
