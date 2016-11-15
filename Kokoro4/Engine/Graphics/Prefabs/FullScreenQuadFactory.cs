using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Prefabs
{
    public class FullScreenQuadFactory
    {
        public static Mesh Create(MeshGroup group)
        {
            return new Mesh(
                group,
                new float[]{
                -1, 1, 0.5f,
                1, 1, 0.5f,
                1, -1,0.5f,
                -1, -1,0.5f
            }, 
            new float[] {
                0,1,
                1,1,
                1,0,
                0,0
            },
            new uint[]{ 0, 0, 0, 0 }, 
            new ushort[] { 3, 2, 0, 0, 2, 1 });
        }
    }
}
