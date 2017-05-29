using Kokoro.Engine;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Prefabs
{
    public class QuadFactory
    {
        public static Mesh Create(MeshGroup grp, ushort xSize, ushort ySize, Vector3 normal, Vector3 idxs)
        {
            float[] vertices = new float[(xSize + 1) * (ySize + 1) * 3];
            uint[] norms = new uint[(xSize + 1) * (ySize + 1)];
            float[] uvs = new float[(xSize + 1) * (ySize + 1) * 2];
            for (int i = 0, y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++, i++)
                {
                    vertices[i * 3 + (int)idxs.X] = x;
                    vertices[i * 3 + (int)idxs.Y] = 0;
                    vertices[i * 3 + (int)idxs.Z] = y;

                    norms[i] = Mesh.CompressNormal(normal.X, normal.Y, normal.Z);

                    uvs[i * 2 + 0] = (float)x / (float)xSize;
                    uvs[i * 2 + 1] = (float)y / (float)ySize;
                }
            }

            ushort[] triangles = new ushort[xSize * ySize * 6];
            for (ushort ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
            {
                for (int x = 0; x < xSize; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = (ushort)(vi + 1);
                    triangles[ti + 4] = triangles[ti + 1] = (ushort)(vi + xSize + 1);
                    triangles[ti + 5] = (ushort)(vi + xSize + 2);
                }
            }

            return new Mesh(grp, vertices, uvs, norms, triangles);
        }
    }
}
