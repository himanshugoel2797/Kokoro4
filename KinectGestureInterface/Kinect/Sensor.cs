using Kokoro.Engine.Graphics;
using Kokoro.Math;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface.Kinect
{
    struct SensorSource
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Bpp { get; set; }
    }

    class Sensor
    {
        private KinectSensor kSensor;
        private MultiSourceFrameReader reader;
        private ushort[] DepthFrameData;

        public const int Size = 128;

        public bool IsConnected { get { return kSensor.IsAvailable && kSensor.IsOpen; } }

        public SensorSource Color { get; internal set; }
        public SensorSource Depth { get; internal set; }
        public SensorSource Infrared { get; internal set; }

        public ushort MaxDepth { get; private set; }
        public ushort MinDepth { get; private set; }

        public float[] Points { get; private set; }
        public object PointsLock { get; set; }

        public int PointCount { get; private set; }

        public KinectFrameTextureSource ColorFrame { get; private set; }
        public KinectFrameTextureSource DepthFrame { get; private set; }
        public KinectFrameTextureSource InfraredFrame { get; private set; }

        public Vector2 LeftHandPos { get; private set; }
        public Vector2 RightHandPos { get; private set; }
        public Vector2 LeftHandTip { get; private set; }
        public Vector2 RightHandTip { get; private set; }

        public bool SaveColorFrame { get; set; }
        public bool SaveDepthFrame { get; set; }
        public bool SaveInfraredFrame { get; set; }

        public ShaderStorageBuffer PointBuffer { get; private set; }

        public Bitmap CurrentHand { get; private set; }
        public byte[] CurrentImageData { get; private set; }

        public ImageMatching Matcher { get; private set; }

        public Sensor()
        {
            kSensor = KinectSensor.GetDefault();

            var colorDesc = kSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            var depthDesc = kSensor.DepthFrameSource.FrameDescription;
            var infraredDesc = kSensor.InfraredFrameSource.FrameDescription;

            CurrentHand = new Bitmap(Size, Size);
            CurrentImageData = new byte[Size * Size];
            Matcher = new ImageMatching();

            PointBuffer = new ShaderStorageBuffer(5000 * sizeof(float) * 4, true);

            {
                Color = new SensorSource()
                {
                    Width = colorDesc.Width,
                    Height = colorDesc.Height,
                    Bpp = (int)colorDesc.BytesPerPixel,
                };
                ColorFrame = new KinectFrameTextureSource(Color.Width, Color.Height, Color.Bpp, Kokoro.Engine.Graphics.PixelFormat.Bgra, Kokoro.Engine.Graphics.PixelInternalFormat.Rgba8, Kokoro.Engine.Graphics.PixelType.UnsignedByte);
            }

            {
                Depth = new SensorSource()
                {
                    Width = depthDesc.Width,
                    Height = depthDesc.Height,
                    Bpp = (int)depthDesc.BytesPerPixel,
                };
                DepthFrame = new KinectFrameTextureSource(Depth.Width, Depth.Height, Depth.Bpp, Kokoro.Engine.Graphics.PixelFormat.RedInteger, Kokoro.Engine.Graphics.PixelInternalFormat.R16ui, Kokoro.Engine.Graphics.PixelType.UnsignedShort);
                DepthFrameData = new ushort[Depth.Width * Depth.Height];

                MaxDepth = kSensor.DepthFrameSource.DepthMaxReliableDistance;
                MinDepth = kSensor.DepthFrameSource.DepthMinReliableDistance;
            }

            {
                Infrared = new SensorSource()
                {
                    Width = infraredDesc.Width,
                    Height = infraredDesc.Height,
                    Bpp = (int)infraredDesc.BytesPerPixel,
                };
                InfraredFrame = new KinectFrameTextureSource(Infrared.Width, Infrared.Height, Infrared.Bpp, Kokoro.Engine.Graphics.PixelFormat.RedInteger, Kokoro.Engine.Graphics.PixelInternalFormat.R16ui, Kokoro.Engine.Graphics.PixelType.UnsignedShort);
            }

            Points = new float[5000 * 4];
            PointsLock = new object();
        }

        public void Open()
        {
            kSensor.Open();
            while (!kSensor.IsOpen) ;
            reader = kSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared);
            reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            MultiSourceFrame frame;
            try
            {
                frame = e.FrameReference.AcquireFrame();
            }
            catch (Exception ex)
            {
                return;
            }

            var bodyFrame = frame.BodyFrameReference.AcquireFrame();
            var depthFrame = frame.DepthFrameReference.AcquireFrame();
            var colorFrame = frame.ColorFrameReference.AcquireFrame();
            var infraredFrame = frame.InfraredFrameReference.AcquireFrame();

            if (colorFrame != null && depthFrame != null && infraredFrame != null && bodyFrame != null)
            {
                if (bodyFrame.BodyCount > 0)
                {
                    if (SaveColorFrame) colorFrame.CopyConvertedFrameDataToIntPtr(ColorFrame.Data, (uint)(Color.Width * Color.Height * Color.Bpp), ColorImageFormat.Bgra);
                    if (SaveDepthFrame) depthFrame.CopyFrameDataToIntPtr(DepthFrame.Data, (uint)(Depth.Width * Depth.Height * Depth.Bpp));
                    if (SaveInfraredFrame) infraredFrame.CopyFrameDataToIntPtr(InfraredFrame.Data, (uint)(Infrared.Width * Infrared.Height * Infrared.Bpp));

                    depthFrame.CopyFrameDataToArray(DepthFrameData);

                    Body[] bodies = new Body[bodyFrame.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(bodies);

                    int bodyIdx = -1;
                    for (int i = 0; i < bodies.Length; i++)
                    {
                        if (bodies[i].IsTracked && (bodies[i].HandRightState != HandState.NotTracked | bodies[i].HandLeftState != HandState.NotTracked))
                        {
                            bodyIdx = i;
                        }
                    }
                    if (bodyIdx == -1)
                    {
                        bodyFrame?.Dispose();
                        depthFrame?.Dispose();
                        colorFrame?.Dispose();
                        infraredFrame?.Dispose();
                        return;
                    }

                    var lHand = bodies[bodyIdx].Joints[JointType.WristLeft];
                    var rHand = bodies[bodyIdx].Joints[JointType.WristRight];
                    var lTipHand = bodies[bodyIdx].Joints[JointType.HandTipLeft];
                    var rTipHand = bodies[bodyIdx].Joints[JointType.HandTipRight];

                    CoordinateMapper mapper = kSensor.CoordinateMapper;
                    var lHandImg = mapper.MapCameraPointToDepthSpace(lHand.Position);
                    var rHandImg = mapper.MapCameraPointToDepthSpace(rHand.Position);
                    var lTipHandImg = mapper.MapCameraPointToDepthSpace(lTipHand.Position);
                    var rTipHandImg = mapper.MapCameraPointToDepthSpace(rTipHand.Position);

                    //Determine the image space coordinates of the hand set to filter out everything else
                    LeftHandPos = (LeftHandPos + new Vector2(lHandImg.X, lHandImg.Y)) * 0.5f;
                    RightHandPos = (RightHandPos + new Vector2(rHandImg.X, rHandImg.Y)) * 0.5f;
                    LeftHandTip = (LeftHandTip + new Vector2(lTipHandImg.X, lTipHandImg.Y)) * 0.5f;
                    RightHandTip = (RightHandTip + new Vector2(rTipHandImg.X, rTipHandImg.Y)) * 0.5f;

                    //Extract a spherical region between the average of the two points obtained above within radius 2.5*(Tip - Center), per hand
                    {
                        var handPos = new Vector3(lHand.Position.X, lHand.Position.Y, lHand.Position.Z);
                        var handTipPos = new Vector3(lTipHand.Position.X, lTipHand.Position.Y, lTipHand.Position.Z);

                        var handPosDepth = LeftHandPos;
                        var handTipDepth = LeftHandTip;

                        //Get handCenter
                        var handCenter = (handPos + handTipPos) * 0.5f;
                        var handCenterDepth = mapper.MapCameraPointToDepthSpace(new CameraSpacePoint() { X = handCenter.X, Y = handCenter.Y, Z = handCenter.Z });
                        var depthTable = mapper.GetDepthFrameToCameraSpaceTable();

                        var tan = (handTipPos - handPos);
                        tan.Normalize();

                        //Get orthogonal axis to compute normal vector
                        var cotanPosDepth = new DepthSpacePoint()
                        {
                            X = (int)(handCenterDepth.X - 10),
                            Y = (int)(handCenterDepth.Y)
                        };
                        var cotanCamSpace = mapper.MapDepthPointToCameraSpace(cotanPosDepth, DepthFrameData[(int)(cotanPosDepth.Y * Depth.Width + cotanPosDepth.X)]);
                        var cotan = new Vector3(cotanCamSpace.X - handPos.X, cotanCamSpace.Y - handPos.Y, cotanCamSpace.Z - handPos.Z);
                        cotan.Normalize();

                        var normal = Vector3.Cross(tan, cotan);
                        normal.Normalize();

                        //Determine the radius squared in camera space
                        float distsq = (handTipPos - handCenter).LengthSquared;

                        //Multiply the entries in the depth table with the associated depths and only store the ones that are within the sphere 
                        lock (PointsLock)
                        {
                            CurrentImageData = new byte[Size * Size];

                            int pos = 0;
                            for (int i = 0; i < DepthFrameData.Length; i++)
                            {
                                var sample = new Vector3(depthTable[i].X, depthTable[i].Y, 1) * DepthFrameData[i] * 0.001f;
                                if ((sample - handCenter).LengthSquared <= distsq * 2.75f * 2.75f)
                                {
                                    //Project these points onto the axis of the hand
                                    var projSample = sample;// - Vector3.Dot(sample - handCenter, normal) * normal;

                                    //if (pos >= Points.Length) continue;

                                    //Add the vector to the gpu buffer
                                    //Points[pos++] = projSample.X;
                                    //Points[pos++] = projSample.Y;
                                    //Points[pos++] = projSample.Z;
                                    //Points[pos++] = 0;

                                    projSample.Normalize(); //Rescale all vectors into a normalized coordinate space

                                    int x = (int)((projSample.X * 0.5f + 0.5f) * CurrentHand.Width);
                                    int y = (int)((projSample.Y * 0.5f + 0.5f) * CurrentHand.Height);

                                    CurrentImageData[y * Size + x] = 255;
                                    CurrentHand.SetPixel(x, y, System.Drawing.Color.Black);
                                }
                            }
                            //PointCount = pos;

                            Matcher.BestMatch(CurrentImageData);

                            try
                            {
                                CurrentHand.Save("hand_proj.png");
                            }
                            catch (Exception) { }
                            CurrentHand.Dispose();
                            CurrentHand = new Bitmap(Size, Size);
                        }

                        //Compute the convex hull of these points.
                    }

                }
            }

            bodyFrame?.Dispose();
            depthFrame?.Dispose();
            colorFrame?.Dispose();
            infraredFrame?.Dispose();
        }
    }
}
