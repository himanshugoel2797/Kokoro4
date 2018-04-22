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
    class KinectHand3D : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh mesh;
        private RenderState state;
        private RenderQueue queue;
        private UniformBuffer ubo;
        private Texture tex;
        private TextureStreamer.TextureStream stream;
        private TextureHandle handle;
        private ShaderStorageBuffer point_cloud;

        private TextureStreamer tStreamer;

        private Sensor Sensor;

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
                //Setup the kinect
                Sensor = new Sensor
                {
                    SaveDepthFrame = true
                };
                Sensor.Open();

                //Setup rendering infrastructure
                camera = new FirstPersonCamera(new Keyboard(), Vector3.UnitX, Vector3.UnitY, "FPV")
                {
                    Enabled = true
                };
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                //Setup the full screen quad
                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 10000, 10000);
                mesh = SphereFactory.Create(grp);

                point_cloud = new ShaderStorageBuffer(5000 * 4 * sizeof(float), true);
                tStreamer = new TextureStreamer(10);
                ubo = new UniformBuffer(true);

                stream = tStreamer.UploadTexture(Sensor.DepthFrame);
                tex = stream.TargetTexture;
                handle = tex.GetHandle(TextureSampler.Default);

                //Upload initial data
                unsafe
                {
                    for (int i = 0; i < 4; i++)
                    {
                        long* l = (long*)ubo.Update();
                        float* f = (float*)l;
                        l[0] = handle;

                        f[4] = Sensor.LeftHandPos.X;
                        f[5] = Sensor.LeftHandPos.Y;
                        f[6] = Sensor.LeftHandTip.X;
                        f[7] = Sensor.LeftHandTip.Y;

                        f[8] = Sensor.RightHandPos.X;
                        f[9] = Sensor.RightHandPos.Y;
                        f[10] = Sensor.RightHandTip.X;
                        f[11] = Sensor.RightHandTip.Y;

                        f[12] = Sensor.Depth.Width;
                        f[13] = Sensor.Depth.Height;

                        f[16] = 0;

                        ubo.UpdateDone();
                    }
                }

                state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/PointCloud/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/FrameBuffer/fragment.glsl")), new ShaderStorageBuffer[] { point_cloud }, new UniformBuffer[] { ubo }, false, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);

                queue = new RenderQueue(10, false);

                queue.ClearAndBeginRecording();
                queue.ClearFramebufferBeforeSubmit = true;
                queue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = mesh } },
                    State = state
                });
                queue.EndRecording();

                inited = true;
            }

            state.ShaderProgram.Set("View", EngineManager.View);
            state.ShaderProgram.Set("Projection", EngineManager.Projection);
            queue.Submit();

            if (stream != null)
            {
                //Load in the next texture
                handle?.SetResidency(Residency.NonResident);
                TextureSampler sampler = stream.TargetSampler;
                handle = tex.GetHandle(sampler);
                handle.SetResidency(Residency.Resident);

                //Upload current hand data
                unsafe
                {
                    long* l = (long*)ubo.Update();
                    float* f = (float*)l;
                    l[0] = handle;

                    f[4] = Sensor.LeftHandPos.X;
                    f[5] = Sensor.LeftHandPos.Y;
                    f[6] = Sensor.LeftHandTip.X;
                    f[7] = Sensor.LeftHandTip.Y;

                    f[8] = Sensor.RightHandPos.X;
                    f[9] = Sensor.RightHandPos.Y;
                    f[10] = Sensor.RightHandTip.X;
                    f[11] = Sensor.RightHandTip.Y;

                    f[12] = Sensor.Infrared.Width;
                    f[13] = Sensor.Infrared.Height;

                    f[16] = Sensor.PointCount;

                    ubo.UpdateDone();

                    f = (float*)point_cloud.Update();
                    lock (Sensor.PointsLock)
                    {
                        for (int i = 0; i < Sensor.PointCount; i++)
                            f[i] = Sensor.Points[i];
                    }
                    point_cloud.UpdateDone();
                }

                //Start streaming the next frame
                if (stream.IsDone)
                {
                    tex.GetHandle(stream.TargetSampler).SetResidency(Residency.NonResident);
                    stream.Free();
                    tex.Dispose();  //Make sure to delete previous frames

                    stream = tStreamer.UploadTexture(Sensor.DepthFrame);
                    tex = stream.TargetTexture;
                }
            }



        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
