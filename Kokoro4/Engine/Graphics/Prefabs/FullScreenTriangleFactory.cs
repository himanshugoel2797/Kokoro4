using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Prefabs
{
    public class FullScreenTriangleFactory
    {
        public static Mesh Create(MeshGroup group)
        {
            return new Mesh(
                group,
                new float[]{
                -1, -1, 0.5f,
                3, -1, 0.5f,
                -1, 3, 0.5f
            },
            new float[] {
                0,0,
                2,0,
                0,2
            },
            new uint[] { 0, 0, 0 },
            new ushort[] { 0, 1, 2 });
        }
    }
}
