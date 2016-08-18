using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace Kokoro.Math
{
    public class ColorTools
    {
        public static Vector3 hsv2rgb(Vector3 c)
        {
            Vector4 K = new Vector4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
            Vector3 p = new Vector3(Abs(MathHelper.Fract(c.X + K.X) * 6 - K.W), Abs(MathHelper.Fract(c.X + K.Y) * 6 - K.W), Abs(MathHelper.Fract(c.X + K.Z) * 6 - K.W));

            Vector3 tmp = p - Vector3.One * K.X;
            tmp = new Vector3(Max(0, Min(1, tmp.X)), Max(0, Min(1, tmp.Y)), Max(0, Min(1, tmp.Z)));

            return c.Z * new Vector3((1 - c.Y) * K.X + c.Y * tmp.X, (1 - c.Y) * K.Y + c.Y * tmp.Y, (1 - c.Y) * K.Z + c.Y * tmp.Z);
        }

        public static Vector3 temperatureToColor(float temp)
        {
            temp = temp * 0.01f;

            float red = temp - 60;
            red = 329.698727446f * (float)Pow(red, -0.1332047592f);
            red = Max(0, Min(1, red / 255f));

            float green = temp - 60;
            green = 288.1221695283f * (float)Pow(green, -0.0755148492f);
            green = Max(0, Min(1, green / 255f));

            float blue = 1;

            return hsv2rgb(new Vector3(red, green, blue));
        }
    }
}
