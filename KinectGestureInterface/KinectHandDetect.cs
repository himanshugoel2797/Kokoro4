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
    class KinectHandDetect : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh mesh;
        private RenderState state;
        private RenderQueue queue;
        private UniformBuffer ubo;
        private Texture tex;
        private Queue<IDisposable> delete_texs;
        private TextureStreamer.TextureStream stream;
        private TextureHandle handle;

        private Texture volumeData;

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
                    SaveInfraredFrame = true
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
                mesh = FullScreenQuadFactory.Create(grp);

                volumeData = new Texture();
                var volSrc = VolumeDataTextureSource.Load("../../../CThead/CThead.", 256, 256, 113);
                volumeData.SetData(volSrc, 0);

                tStreamer = new TextureStreamer(10);
                ubo = new UniformBuffer(true);

                stream = tStreamer.UploadTexture(Sensor.DepthFrame);
                tex = stream.TargetTexture;
                handle = tex.GetHandle(TextureSampler.Default);
                delete_texs = new Queue<IDisposable>();

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

                        ubo.UpdateDone();
                    }
                }

                state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/FrameBuffer/fragment.glsl")), null, new UniformBuffer[] { ubo }, false, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);

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

                    ubo.UpdateDone();
                }

                //Start streaming the next frame
                if (stream.IsDone)
                {
                    tex.GetHandle(stream.TargetSampler).SetResidency(Residency.NonResident);
                    delete_texs.Enqueue(stream.TargetSampler);
                    delete_texs.Enqueue(tex);  //Make sure to delete previous frames
                    stream.Free();  //TODO these can only be deleted after ensuring they're out of the system (4 frames after submission)

                    stream = tStreamer.UploadTexture(Sensor.InfraredFrame);
                    tex = stream.TargetTexture;
                }

                if(delete_texs.Count > 16)
                {
                    delete_texs.Dequeue().Dispose();
                    delete_texs.Dequeue().Dispose();
                }
            }



        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
