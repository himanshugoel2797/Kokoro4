using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Prefabs
{
    public class CubeFactory
    {
        public static Mesh Create(MeshGroup group)
        {

            float width = 0.5f;
            float height = 0.5f;
            float depth = 0.5f;

            uint[] indices = new uint[] {
                0, 1, 2, 3, 0, 4,
                5, 0, 6, 3, 6, 0,
                0, 2, 4, 5, 1, 0,
                2, 1, 5, 7, 6, 3,
                6, 7, 5, 7, 3, 4,
                7, 4, 2, 7, 2, 5
            };

            float[] uvs = new float[] {
                0,1,
                1,1,
                1,0,
                0,0,
                0,1,
                1,1,
                1,0,
                0,0,
            };

            float[] vers = new float[]{
                -width, -height, -depth,    //0
                -width, -height, depth,     //1
                -width, height, depth,      //2
                width, height, -depth,      //3
                -width, height, -depth,     //4
                width, -height, depth,      //5
                width, -height, -depth,     //6
                width, height, depth        //7
            };


            List<uint> norms = new List<uint>();
            List<ushort> inds = new List<ushort>();

            for (int i = 0; i < vers.Length/3; i++)
            {
                norms.Add(Mesh.CompressNormal(vers[i * 3], vers[i * 3 + 1], vers[i * 3 + 2]));
            }

            for(int i = 0; i < indices.Length; i++)
            {
                inds.Add((ushort)indices[i]);
            }

            return new Mesh(group, vers.ToArray(), uvs.ToArray(), norms.ToArray(), inds.ToArray());
        }
    }
}
