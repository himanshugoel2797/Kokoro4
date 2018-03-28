using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface.Kinect
{
    public class Dilate
    {
        public static byte[] Apply(byte[] CurrentImageData, int Size, byte val)
        {
            /*
            byte[] ret = new byte[CurrentImageData.Length];
            for (int y = 1; y < Size - 1; y++)
                for (int x = 1; x < Size - 1; x++)
                {
                    if (CurrentImageData[y * Size + x] != 0)
                    {
                        ret[y * Size + x] = CurrentImageData[y * Size + x];
                        continue;
                    }

                    bool top = CurrentImageData[(y - 1) * Size + x] != 0;
                    bool btm = CurrentImageData[(y + 1) * Size + x] != 0;
                    bool lft = CurrentImageData[y * Size + (x - 1)] != 0;
                    bool rgt = CurrentImageData[y * Size + (x + 1)] != 0;
                    bool btm_lft = CurrentImageData[(y - 1) * Size + (x - 1)] != 0;
                    bool top_rgt = CurrentImageData[(y + 1) * Size + (x + 1)] != 0;
                    bool top_lft = CurrentImageData[(y + 1) * Size + (x - 1)] != 0;
                    bool btm_rgt = CurrentImageData[(y - 1) * Size + (x + 1)] != 0;

                    var neighbors = new bool[] { top, btm, lft, rgt, btm_lft, btm_rgt, top_lft, top_rgt };

                    if (neighbors.Where(a => a).Count() >= 3)
                        ret[y * Size + x] = val;
                }
            return ret;
            */
            byte[] ret = new byte[CurrentImageData.Length];
            for (int y = 1; y < Size - 1; y++)
                for (int x = 1; x < Size - 1; x++)
                {
                    if (CurrentImageData[y * Size + x] != 0)
                    {

                        ret[(y - 1) * Size + x] = val;
                        ret[(y + 1) * Size + x] = val;
                        ret[y * Size + (x - 1)] = val;
                        ret[y * Size + (x + 1)] = val;
                        ret[(y - 1) * Size + (x - 1)] = val;
                        ret[(y + 1) * Size + (x + 1)] = val;
                        ret[(y + 1) * Size + (x - 1)] = val;
                        ret[(y - 1) * Size + (x + 1)] = val;
                    }
                }
            return ret;
        }
    }
}
