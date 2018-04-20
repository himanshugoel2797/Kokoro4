using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VoxelTests
{
    [StructLayout(LayoutKind.Explicit)]
    struct SparseVoxelOctreeNode
    {
        [FieldOffset(0)] private SparseVoxelOctreeNode[] Children;
        [FieldOffset(8)] private ushort ColorTable;
        [FieldOffset(10)] private byte ColorIndex;
        [FieldOffset(11)] private byte PresentChildren;

        public static SparseVoxelOctreeNode Create()
        {
            return new SparseVoxelOctreeNode()
            {
                Children = null,
                ColorIndex = 0,
                ColorTable = 0,
                PresentChildren = 0,
            };
        }

        private void Subdivide(int idx)
        {
            //Check if the specified node has been allocated, if not, allocate it
            if ((PresentChildren & (1 << idx)) == 0)
            {
                var n_nodes = new SparseVoxelOctreeNode[(Children?.Length).GetValueOrDefault() + 1];

                int n_idx = 0;
                for (int i = 0; i < idx; i++)
                    if ((PresentChildren & (1 << i)) == 1)
                        n_idx++;

                n_nodes[n_idx] = SparseVoxelOctreeNode.Create();

                if (Children != null)
                {
                    //We need to copy over previous nodes
                    for (int i = 0; i < n_idx; i++)
                    {
                        n_nodes[i] = Children[i];
                    }

                    for (int i = n_idx; i < Children.Length; i++)
                    {
                        n_nodes[i + 1] = Children[i];
                    }
                }
                Children = n_nodes;
                PresentChildren |= (byte)(1 << idx);
            }
        }

        public void Add(long cur_side, long side, long x, long y, long z, long x_c, long y_c, long z_c, ushort table, byte colorIndex)
        {
            if (cur_side <= side && (cur_side << 1) >= side)
            {
                ColorTable = table;
                ColorIndex = colorIndex;

                //A filled node does not have children
                Children = null;
                PresentChildren = 0;
            }
            else
            {
                if (ColorTable != 0)
                    throw new Exception("Cannot add children to a filled node.");

                int lr = (x - x_c) >= 0 ? 1 : 0;
                int tb = (y - y_c) >= 0 ? 1 : 0;
                int fb = (z - z_c) >= 0 ? 1 : 0;

                int idx = lr | (tb << 1) | (fb << 2);

                Subdivide(idx);
                int n_idx = 0;
                for (int i = 0; i < idx; i++)
                    if ((PresentChildren & (1 << i)) == 1)
                        n_idx++;

                cur_side = cur_side >> 1;

                x_c += (2 * lr - 1) * cur_side;
                y_c += (2 * tb - 1) * cur_side;
                z_c += (2 * fb - 1) * cur_side;

                Children[n_idx].Add(cur_side, side, x, y, z, x_c, y_c, z_c, table, colorIndex);
            }
        }
    }

    public class SparseVoxelOctree
    {
        public long WorldSize { get; private set; }
        private SparseVoxelOctreeNode Tree;

        public SparseVoxelOctree(long worldSide)
        {
            WorldSize = worldSide;
            Tree = SparseVoxelOctreeNode.Create();
        }

        public void Add(long side, long x, long y, long z)
        {
            Tree.Add(WorldSize, side, x, y, z, 0, 0, 0, 1, 0);
        }
    }
}
