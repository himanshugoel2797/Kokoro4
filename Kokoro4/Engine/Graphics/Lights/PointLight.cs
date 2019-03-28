using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Math;

namespace Kokoro.Engine.Graphics.Lights
{
    public class PointLight : ILight
    {
        public int TypeIndex => 0;
        public Vector3 Position { get; set; }
        public Vector3 Color { get; set; }
        public float Radius { get; set; }
        public float Intensity { get; set; }
        public float MaxEffectiveRadius { get { return (float)(Radius * (System.Math.Sqrt(Intensity / Threshold) - 1)); } }

        public const float Threshold = 0.001f;
    }
}
