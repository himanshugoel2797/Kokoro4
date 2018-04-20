using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelTests
{
    class HighResVoxelOctreeTest : IState
    {
        SparseVoxelOctree octree;

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {
            if (octree == null)
            {
                //Generate/Load an octree of a limited region of the view
                octree = new SparseVoxelOctree(1L << 33);

                for (float rho = 0; rho <= 2 * Math.PI; rho += 0.001f)
                    for (float theta = 0; theta <= 2 * Math.PI; theta += 0.001f)
                        octree.Add(1000L * 100 * 10, (long)(6000L * 1000 * 100 * 10 * Math.Sin(theta) * Math.Cos(rho)), (long)(70000L * 1000 * 100 * 10 * Math.Sin(theta) * Math.Sin(rho)), (long)(70000L * 1000 * 100 * 10 * Math.Cos(theta)));

                //octree.Add(1, -1, -1, -1);
            }
        }

        public void Update(double interval)
        {

        }
    }
}
