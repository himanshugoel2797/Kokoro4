using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Voxel
{
    public class ChunkManager
    {
        Dictionary<Vector3, Chunk<byte>> chunks;

        public ChunkManager()
        {
            chunks = new Dictionary<Vector3, Chunk<byte>>();
        }
    }
}
