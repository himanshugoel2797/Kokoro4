using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Voxel
{
    public class Voxelizer
    {
        public static Mesh Voxelize(MeshGroup grp, VoxelOctree octree, double x, double y, double z, uint adj_mask0)
        {
            Mesh cube = CubeFactory.Create(grp);

            #region Voxelization routine
            Action<VoxelOctree, double, double, double, uint> build_mesh = null;
            build_mesh = (a, x_c, y_c, z_c, adj_mask) =>
            {
                if (a.Children == null)
                {
                    if (a.Color.A == 0) return;

                    float obj_side = (a.Data.WorldSide >> (a.Level + 2));

                    //Make this actually generate the geometry based on the adjacency mask

                    //meshList.Add(cube);
                    //transforms.Add(Matrix4.Scale(a.Data.WorldSide >> a.Level) * Matrix4.CreateTranslation((float)(x_c - obj_side), (float)(y_c - obj_side), (float)(z_c - obj_side)));
                    return;
                }

                for (int i = 0; i < a.Children.Length; i++)
                {
                    VoxelOctree child = a.Children[i];


                    if (child == null)
                    {
                        long x_side = ((i & 1) == 1 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> (a.Level + 2));
                        long y_side = ((i & 2) == 2 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> (a.Level + 2));
                        long z_side = ((i & 4) == 4 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> (a.Level + 2));

                        //These voxels are not visible
                        //meshList.Add(cube);
                        //transforms.Add(Matrix4.Scale(a.Data.WorldSide >> (a.Level + 1)) * Matrix4.CreateTranslation((float)(x_c + x_side), (float)(y_c + y_side), (float)(z_c + z_side)));
                    }
                    else
                    {
                        double x_side = ((i & 1) == 1 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> a.Level);
                        double y_side = ((i & 2) == 2 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> a.Level);
                        double z_side = ((i & 4) == 4 ? 1 : -1) * System.Math.Max(1, a.Data.WorldSide >> a.Level);

                        x_side /= 4;
                        y_side /= 4;
                        z_side /= 4;

                        //Depending on the location, we will depend on the adjacency data passed down
                        //Actually, adjacency data will not work, we do not have enough high resolution data

                        //TODO calculate adjacency mask for the child and pass it down in the last parameter
                        build_mesh(child, x_c + x_side, y_c + y_side, z_c + z_side, 0);
                    }
                }
            };
            #endregion

            build_mesh(octree, x, y, z, adj_mask0);
            return null;
        }
    }
}
