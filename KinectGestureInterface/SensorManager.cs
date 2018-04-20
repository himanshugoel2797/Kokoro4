using KinectGestureInterface.Kinect;
using Kokoro.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface
{
    class SensorManager
    {
        Sensor Sensor;
        private TextureStreamer tStreamer;
        private UniformBuffer ubo;
        private Texture tex;
        private Queue<IDisposable> delete_texs;
        private TextureStreamer.TextureStream stream;
        private TextureHandle handle;

        public UniformBuffer UBO { get { return ubo; } }
        public int Prediction { get { return Sensor.CurrentGuess(); } }

        public SensorManager() { }

        public void Init()
        {
            //Setup the kinect
            Sensor = new Sensor
            {
                SaveInfraredFrame = true
            };
            Sensor.Open();

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
        }

        public void Render()
        {
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

                if (delete_texs.Count > 16)
                {
                    delete_texs.Dequeue().Dispose();
                    delete_texs.Dequeue().Dispose();
                }
            }
        }
    }
}
