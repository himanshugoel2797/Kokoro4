using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor.Texture
{
    class DerivativeMap
    {
        Bitmap outBmp;
        public void Compute(Bitmap input)
        {
            //Compute X derivatives
            //Compute Y derivatives

            outBmp = new Bitmap(input.Width / 2, input.Height / 2);
            for(int y = 0; y < outBmp.Height; y++)
            {
                for(int x = 0; x < outBmp.Width; x++)
                {
                    int diff = (int)input.GetPixel(x * 2, y * 2).R - (int)input.GetPixel(x * 2 + 1, y * 2 + 1).R;
                    diff = Math.Min(byte.MaxValue, Math.Max(diff / 2 + 128, 0));
                    outBmp.SetPixel(x, y, Color.FromArgb(diff, diff, diff, diff));
                }
            }
        }

        public void Save(string file)
        {
            outBmp.Save(file);
            outBmp.Dispose();
        }
    }
}
