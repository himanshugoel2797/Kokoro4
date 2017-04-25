using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Editor
{
    class Program
    {
        static void Main(string[] args)
        {
            EngineManager.Name = EngineManager.EngineName;

            EngineManager.StateManager.AddState(nameof(EngineScene), new EngineScene());
            EngineManager.StateManager.SetActiveState(nameof(EngineScene));

            EngineManager.Run(60, 60);
            EngineManager.Exit();
        }
    }
}
