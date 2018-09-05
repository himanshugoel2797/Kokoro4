using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.Demos
{
    class Program
    {
        static void Main(string[] args)
        {
            EngineManager.Name = EngineManager.EngineName;

            EngineManager.StateManager.AddState(nameof(PBR.LambertTest), new PBR.LambertTest());

            EngineManager.StateManager.SetActiveState(nameof(PBR.LambertTest));
            
            EngineManager.Run(60, 60);
            EngineManager.Exit();
        }
    }
}
