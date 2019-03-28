using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Engine;

namespace VRWorld.Base
{
    class Program
    {
        static void Main(string[] args)
        {
            EngineManager.Name = EngineManager.EngineName;

            EngineManager.StateManager.AddState(nameof(SphereVRScene), new SphereVRScene());
            EngineManager.StateManager.SetActiveState(nameof(SphereVRScene));
            EngineManager.Run(90, 90);
            EngineManager.Exit();
        }
    }
}
