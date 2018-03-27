using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface.Kinect
{
    public class ImageMatching
    {
        public byte[][] CompareTargets { get; private set; }

        private string[] Gestures = new string[]
        {
            "open_hand.png",
            "two_fingers.png"
        };

        public ImageMatching()
        {
            CompareTargets = new byte[Gestures.Length][];

            for (int i = 0; i < Gestures.Length; i++)
                CompareTargets[i] = LoadImage(Gestures[i]);
        }

        private static byte[] LoadImage(string img)
        {
            Bitmap bmp = new Bitmap(img);
            byte[] pixels = new byte[bmp.Width * bmp.Height];

            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    pixels[y * bmp.Width + x] = bmp.GetPixel(x, y).A;

            return pixels;
        }

        private float GetMatch(byte[] src, int idx)
        {
            float diff = 0;

            var src2 = CompareTargets[idx];

            for (int i = 0; i < src.Length; i++)
            {
                float d = (src[i] - src2[i]) / 255.0f;
                diff += d * d;
            }

            return diff / src.Length;
        }

        public int BestMatch(byte[] src)
        {
            float curMin = float.MaxValue;
            int minIdx = -1;

            for (int i = 0; i < Gestures.Length; i++)
            {
                float res = GetMatch(src, i);
                if (res < curMin)
                {
                    curMin = res;
                    minIdx = i;
                }
            }

            if (curMin > 0.05f)
                return -1;

            Console.WriteLine($"CurMatch:{minIdx}  Confidence:{curMin}");
            return minIdx;
        }
    }
}
