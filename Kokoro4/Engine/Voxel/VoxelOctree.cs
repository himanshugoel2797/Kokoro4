using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Voxel
{
    public struct VoxelColor
    {
        public static VoxelColor Zero { get; private set; } = new VoxelColor() { R = 0, G = 0, B = 0, A = 0 };

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }


        public static VoxelColor Average(params VoxelColor[] vals)
        {
            uint R_a = 0, G_a = 0, B_a = 0, A_a = 0;

            for (int i = 0; i < vals.Length; i++)
            {
                R_a += vals[i].R;
                G_a += vals[i].G;
                B_a += vals[i].B;
                A_a += vals[i].A;
            }

            return new VoxelColor()
            {
                R = (byte)(R_a / vals.Length),
                G = (byte)(G_a / vals.Length),
                B = (byte)(B_a / vals.Length),
                A = (byte)(A_a / vals.Length),
            };
        }

        public static bool operator ==(VoxelColor a, VoxelColor b)
        {
            return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        public static bool operator !=(VoxelColor a, VoxelColor b)
        {
            return !(a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A);
        }
    }

    public enum VoxelOctreeChildren
    {
        X = 0,
        Y,
        Z
    }

    public class VoxelOctree
    {
        //Color of node
        public VoxelColor Color { get; set; } = VoxelColor.Zero;

        //Children if needed
        public VoxelOctree[] Children { get; set; }

        public VoxelOctree Parent { get; private set; }

        //Current octree level
        public long Level { get; set; }

        //Current voxel side
        public long Side { get; set; }

        public const int ChildrenCount = 8;

        public VoxelOctree(long lvl, long side)
        {
            Level = lvl;
            Side = side;
        }

        //Optimize tree by checking children if they're all set and all the same color
        public void Optimize()
        {
            if (Children == null)
                return;

            VoxelColor[] cols = new VoxelColor[Children.Length];

            for (int i = 0; i < Children.Length; i++)
            {
                if (Children[i] != null)
                {
                    Children[i].Optimize();
                    cols[i] = Children[i].Color;
                }
                else
                {
                    cols[i] = VoxelColor.Zero;
                }
            }

            //Get rid of any duplicates
            cols = cols.Distinct().ToArray();

            //Update this node's color to the average of it's color
            Color = VoxelColor.Average(cols);

            //If all the colors are the same, collapse the children
            if (cols.Length == 1 & cols[0] == Color)
                Children = null;

        }

        private static int ChildIndex(long X, long Y, long Z, long X_c, long Y_c, long Z_c)
        {
            return Convert.ToInt32(X >= X_c) | Convert.ToInt32(Y >= Y_c) << 1 | Convert.ToInt32(Z >= Z_c) << 2;
        }

        public void Remove(long X, long Y, long Z, long side)
        {
            Add(VoxelColor.Zero, X, Y, Z, side);
        }

        private void Add(VoxelColor color, long X, long Y, long Z, long x_c, long y_c, long z_c, long side)
        {
            //If the side matches the side of this voxel, set the color and fill the entire voxel
            if (side == Side)
            {
                Color = color;
                Children = null;  //drop all the children to mark them as free
                return;
            }

            long x_o = X;
            long y_o = Y;
            long z_o = Z;

            int idx = ChildIndex(X, Y, Z, x_c, y_c, z_c);

            if (Children == null)
                Children = new VoxelOctree[ChildrenCount];

            if (Children[idx] == null)
            {
                Children[idx] = new VoxelOctree(Level + 1, Side / 2)
                {
                    Color = color,
                    Children = null,
                    Parent = this,
                };
            }

            long x_side = ((X >= x_c) ? 1 : -1) * Side / 4;
            long y_side = ((Y >= y_c) ? 1 : -1) * Side / 4;
            long z_side = ((Z >= z_c) ? 1 : -1) * Side / 4;

            Children[idx].Add(color, X, Y, Z, x_c + x_side, y_c + y_side, z_c + z_side, side);
        }

        //Add a voxel, specify a side length and a location
        public void Add(VoxelColor color, long X, long Y, long Z, long side)
        {
            if (Math.MathHelper.IsLog2((ulong)side))
                throw new ArgumentException("side must be a power of 2");

            if (X % side != 0)
                throw new ArgumentException("X must be a multiple of side");

            if (Y % side != 0)
                throw new ArgumentException("Y must be a multiple of side");

            if (Z % side != 0)
                throw new ArgumentException("Z must be a multiple of side");

            Add(color, X, Y, Z, 0, 0, 0, side);
        }
    }
}
