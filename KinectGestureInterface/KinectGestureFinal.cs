using KinectGestureInterface.Kinect;
using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface
{
    class KinectGestureFinal : IState
    {
        private bool inited = false;
        private Keyboard keybd;

        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh fsq;
        private RenderState state;
        private RenderQueue queue;
        
        private SensorManager sensorManager;
        private VRWorldManager VRWorldManager;
        
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

                //Setup rendering infrastructure
                camera = new FirstPersonCamera(new Keyboard(), Vector3.UnitX, Vector3.UnitY, "FPV")
                {
                    Enabled = true
                };
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                sensorManager = new SensorManager();
                sensorManager.Init();

                VRWorldManager = new VRWorldManager(keybd);
                VRWorldManager.Enter(null);

                //Setup the full screen quad
                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 20, 20);
                fsq = FullScreenQuadFactory.Create(grp);
                
                state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/FrameBuffer/fragment.glsl")), null, new UniformBuffer[] { sensorManager.UBO }, false, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);
                
                queue = new RenderQueue(10, false);
                queue.ClearAndBeginRecording();
                queue.ClearFramebufferBeforeSubmit = true;
                queue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = fsq } },
                    State = state
                });
                queue.EndRecording();

                inited = true;
            }

            queue.Submit();
            sensorManager.Render();
            VRWorldManager.Render(sensorManager.Prediction);
        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
