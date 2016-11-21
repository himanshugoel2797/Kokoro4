using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kokoro.Engine.Voxel;

namespace VoxelTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            VoxelOctree octree = new VoxelOctree(0, 1 << 16);
            octree.Add(new VoxelColor() { R = 255, G = 0, B = 255, A = 255 }, 0, 0, 0, 1);
        }
    }
}
