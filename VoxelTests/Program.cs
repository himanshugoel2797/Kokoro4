using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelTests
{
    class Program
    {
        static void Main(string[] args)
        {
            EngineManager.Name = EngineManager.EngineName;
            
            EngineManager.StateManager.AddState(nameof(HighResVoxelOctreeTest), new HighResVoxelOctreeTest());
            
            EngineManager.StateManager.SetActiveState(nameof(HighResVoxelOctreeTest));

            //Work on ice/snow rendering//

            EngineManager.Run(60, 60);
            EngineManager.Exit();
        }
    }
}
