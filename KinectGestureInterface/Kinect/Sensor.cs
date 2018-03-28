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

        public const int Size = 256;

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
                        float weight = 0.55f;
                        var handCenter = (handPos * weight + handTipPos * (1 - weight));
                        var handCenterDepth = mapper.MapCameraPointToDepthSpace(new CameraSpacePoint() { X = handCenter.X, Y = handCenter.Y, Z = handCenter.Z });
                        var depthTable = mapper.GetDepthFrameToCameraSpaceTable();

                        var tan = (LeftHandTip - LeftHandPos);
                        tan.Normalize();

                        //Get a vector orthogonal to tan to form the hand coordinate space
                        var cotan = new Vector2(0);
                        {
                            var cotan_y = (float)Math.Sqrt(1.0f / (1.0f + (tan.Y / tan.X) * (tan.Y / tan.X)));
                            var cotan_x = -tan.Y / tan.X * cotan_y;
                            cotan = new Vector2(cotan_x, cotan_y);
                            cotan.Normalize();
                        }

                        var handCenterDepth_l = new Vector2(handCenterDepth.X, handCenterDepth.Y);

                        Matrix3 conversion_matrix = new Matrix3(cotan.X, cotan.Y, 0, tan.X, tan.Y, 0, 0, 0, 1);
                        var angle = Math.Acos(Vector2.Dot(tan, -Vector2.UnitY));

                        //Determine the radius squared in camera space
                        float distsq = (handTipPos - handCenter).LengthSquared;

                        Dictionary<int, Vector3> CameraCoordinates = new Dictionary<int, Vector3>();
                        //Multiply the entries in the depth table with the associated depths and only store the ones that are within the sphere 
                        lock (PointsLock)
                        {
                            CurrentImageData = new byte[Size * Size];

                            int pos = 0;
                            for (int i = 0; i < DepthFrameData.Length; i++)
                            {
                                var sample = new Vector3(depthTable[i].X, depthTable[i].Y, 1) * DepthFrameData[i] * 0.001f;
                                if ((sample - handCenter).LengthSquared <= distsq * 2.25f * 2.25f)
                                {
                                    //Project these points onto the axis of the hand
                                    var projP = mapper.MapCameraPointToDepthSpace(new CameraSpacePoint() { X = sample.X, Y = sample.Y, Z = sample.Z });

                                    var proj = new Vector3(projP.X - handCenterDepth_l.X, projP.Y - handCenterDepth_l.Y, 1);   //Get the point relative to the hand
                                    Matrix3.Transform(ref conversion_matrix, ref proj);                                     //Rotate the point around the center

                                    int x = (int)((proj.X + 150) / 300 * CurrentHand.Width);
                                    int y = (int)((proj.Y + 150) / 300 * CurrentHand.Height);

                                    if (tan.X < 0)
                                        x = CurrentHand.Width - x;

                                    x = Math.Min(Size - 1, x);
                                    y = Math.Min(Size - 1, y);
                                    x = Math.Max(0, x);
                                    y = Math.Max(0, y);

                                    CameraCoordinates[y * Size + x] = sample;
                                    CurrentImageData[y * Size + x] = 255;

                                }
                            }

                            byte[] res = Dilate.Apply(CurrentImageData, Size, 255);
                            for (int i = 0; i < 2; i++) res = Erode.Apply(res, Size, 255);
                            res = SinglePixel.Apply(res, Size, 255);

                            bool nonzero = !false;
                            CurrentImageData = res;
                            /*
                            for (int i = 0; i < res.Length; i++)
                            {
                                CurrentImageData[i] = (byte)Math.Max(0, CurrentImageData[i] - res[i]);
                                if (!nonzero) nonzero = CurrentImageData[i] != 0;
                            }*/
                            if (nonzero)
                            {

                                Dictionary<int, float> distsqs = new Dictionary<int, float>();

                                //Determine the radius of the center blob approximately
                                for (int x = 0; x < Size; x++)
                                {
                                    if (CurrentImageData[(Size / 2) * Size + x] == 0)
                                    {
                                        var dist_sq = new Vector2(x - Size / 2, 0).LengthSquared;  //Center is at center of image
                                        distsqs[Size / 2 * Size + x] = dist_sq;
                                    }
                                }

                                var circle_tip = distsqs.OrderBy(a => a.Value).ThenByDescending(a => a.Key).First().Value + 9;

                                //Clear out the region within this circle, as well as everything with Y less than 30% (to eliminate the arm)
                                Dictionary<int, Dictionary<int, Vector3>> Blobs = new Dictionary<int, Dictionary<int, Vector3>>();
                                Dictionary<int, Vector2> BlobCenters = new Dictionary<int, Vector2>();
                                Dictionary<int, Vector3> BlobDepthCenters = new Dictionary<int, Vector3>();
                                Dictionary<int, Vector3> BlobFarthestPoints = new Dictionary<int, Vector3>();
                                Dictionary<int, float> DistanceMap = new Dictionary<int, float>();
                                Dictionary<int, float> EdgeDistanceMap = new Dictionary<int, float>();
                                float avgEdgeDistSq = 0;

                                for (int y = 1; y < Size - 1; y++)
                                    for (int x = 1; x < Size - 1; x++)
                                    {
                                        if (CurrentImageData[y * Size + x] == 255)
                                        {
                                            CurrentHand.SetPixel(x, y, System.Drawing.Color.Red);
                                            DistanceMap[y * Size + x] = new Vector2(x - Size / 2, y - Size / 2).LengthSquared;
                                        }
                                        else if (CurrentImageData[y * Size + x] == 0)
                                        {
                                            bool top = CurrentImageData[(y - 1) * Size + x] == 255;
                                            bool btm = CurrentImageData[(y + 1) * Size + x] == 255;
                                            bool lft = CurrentImageData[y * Size + (x - 1)] == 255;
                                            bool rgt = CurrentImageData[y * Size + (x + 1)] == 255;
                                            bool btm_lft = CurrentImageData[(y - 1) * Size + (x - 1)] == 255;
                                            bool top_rgt = CurrentImageData[(y + 1) * Size + (x + 1)] == 255;
                                            bool top_lft = CurrentImageData[(y + 1) * Size + (x - 1)] == 255;
                                            bool btm_rgt = CurrentImageData[(y - 1) * Size + (x + 1)] == 255;

                                            if (top | btm | lft | rgt | btm_lft | btm_rgt | top_lft | top_rgt)
                                            {
                                                EdgeDistanceMap[y * Size + x] = new Vector2(x - Size / 2, y - Size / 2).LengthSquared;
                                                avgEdgeDistSq += EdgeDistanceMap[y * Size + x];
                                                //CurrentHand.SetPixel(x, y, System.Drawing.Color.Green);
                                            }
                                        }
                                    }
                                avgEdgeDistSq /= EdgeDistanceMap.Count;

                                //avgEdgeDistSq = 0;
                                avgEdgeDistSq = EdgeDistanceMap.OrderBy(a => a.Value).First().Value;
                                EdgeDistanceMap = EdgeDistanceMap.OrderByDescending(a => a.Value).Take(EdgeDistanceMap.Count - 100).ToDictionary(a => a.Key, a => a.Value);
                                foreach (int key in EdgeDistanceMap.Keys)
                                {
                                    //CurrentHand.SetPixel(key % Size, key / Size, System.Drawing.Color.Black);
                                    //avgEdgeDistSq += EdgeDistanceMap[key];
                                }

                                for (int y = 1; y < Size - 1; y++)
                                    for (int x = 1; x < Size - 1; x++)
                                    {
                                        if (new Vector2(x - Size / 2, y - Size / 2).LengthSquared <= avgEdgeDistSq)
                                        {
                                            if (CurrentImageData[y * Size + x] == 255)
                                            {
                                                //CurrentHand.SetPixel(x, y, System.Drawing.Color.Chocolate);
                                                //CurrentHand.SetPixel(x, y, System.Drawing.Color.Transparent);
                                                CurrentImageData[y * Size + x] = 0;
                                            }
                                        }
                                    }

                                var tips = DistanceMap.OrderByDescending(a => a.Key / Size).ThenBy(a => a.Value).Take(5).ToArray();
                                for (int i = 0; i < tips.Length; i++)
                                {
                                    CurrentHand.SetPixel(tips[i].Key % Size, tips[i].Key / Size, System.Drawing.Color.Blue);
                                }
                                /*
                                */

                                //Extract blobs using connected components
                                ///*
                                byte lbl = 0;
                                for (int y = 0; y < Size; y++)
                                    for (int x = 0; x < Size; x++)
                                    {
                                        float y_percent = (float)y / Size;
                                        if (y_percent < 0.35f)
                                        {
                                            CurrentImageData[y * Size + x] = 0;
                                            CurrentHand.SetPixel(x, y, System.Drawing.Color.Transparent);
                                            continue;
                                        }

                                        /*
                                        var dist_sq = new Vector2(x - Size / 2, y - Size / 2).LengthSquared;
                                        if (dist_sq <= circle_tip)
                                        {
                                            CurrentImageData[y * Size + x] = 0;
                                            CurrentHand.SetPixel(x, y, System.Drawing.Color.Transparent);
                                        }
                                        else*/
                                        if (CurrentImageData[y * Size + x] == 255)
                                        {
                                            //determine center and dominant axis for blobs, also 3d position for center point
                                            if (x > 0 && CurrentImageData[y * Size + x - 1] != 0)
                                                CurrentImageData[y * Size + x] = CurrentImageData[y * Size + x - 1];
                                            else if (y > 0 && CurrentImageData[(y - 1) * Size + x] != 0)
                                                CurrentImageData[y * Size + x] = CurrentImageData[(y - 1) * Size + x];
                                            else
                                            {
                                                lbl++;
                                                CurrentImageData[y * Size + x] = lbl;
                                                Blobs[lbl] = new Dictionary<int, Vector3>();
                                                BlobDepthCenters[lbl] = new Vector3(0);
                                                BlobCenters[lbl] = new Vector2(0);
                                            }

                                            Blobs[lbl][y * Size + x] = new Vector3(x, y, 0); //CameraCoordinates[y * Size + x];
                                            BlobCenters[lbl] += new Vector2(x, y);
                                            BlobDepthCenters[lbl] += Blobs[lbl][y * Size + x];
                                        }
                                    }

                                Blobs = Blobs.Where(a => a.Value.Count > 10).OrderByDescending(a => a.Value.Count).Take(5).ToDictionary(a => a.Key, a => a.Value);

                                int col = 0;
                                System.Drawing.Color[] cols = new System.Drawing.Color[]
                                {
                                System.Drawing.Color.Fuchsia,
                                System.Drawing.Color.Gold,
                                System.Drawing.Color.Gray,
                                System.Drawing.Color.Lavender,
                                System.Drawing.Color.LightCyan,
                                };

                                foreach (int key in Blobs.Keys)
                                {
                                    BlobCenters[key] /= Blobs[key].Count;
                                    BlobDepthCenters[key] /= Blobs[key].Count;

                                    //Get the farthest point from the blob center in 3d
                                    BlobFarthestPoints[key] = Blobs[key].OrderByDescending(a => (a.Value - handCenter).LengthSquared).ThenByDescending(a => a.Key / Size).First().Value;

                                    //CurrentHand.SetPixel((int)BlobCenters[key].X, (int)BlobCenters[key].Y, cols[col++]);
                                }
                                //*/


                                //learn gestures by switching to a recording mode, trying to determine tolerances for a gesture



                                //Extract blobs from the above
                                //Consider increasing the number of pixels included per frame until 4 blobs for y, and 1 for x are obtained, and decreasing if more than 5 pixels are in the smallest blob


                                try
                                {
                                    CurrentHand.Save("hand_proj.png");
                                }
                                catch (Exception) { }
                                CurrentHand.Dispose();
                                CurrentHand = new Bitmap(Size, Size);
                            }
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
