using Kokoro.Engine;
using Kokoro.Graphics;
using Kokoro.Engine.Cameras;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kokoro.Engine.Graphics;

namespace TestApplication
{
    static class Program
    { 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            EngineManager.Name = EngineManager.EngineName;

            EngineManager.StateManager.AddState(nameof(TestScene), new TestScene());
            EngineManager.StateManager.AddState(nameof(TextureStreamingTest), new TextureStreamingTest());
            EngineManager.StateManager.AddState(nameof(QuadTreeTerrainTest), new QuadTreeTerrainTest());
            EngineManager.StateManager.AddState(nameof(CubeTerrainTest), new CubeTerrainTest());
            EngineManager.StateManager.AddState(nameof(PlanetTerrainRendererTest), new PlanetTerrainRendererTest());
            EngineManager.StateManager.AddState(nameof(FenceTest), new FenceTest());
            EngineManager.StateManager.AddState(nameof(HeightfieldGITest), new HeightfieldGITest());

            //EngineManager.StateManager.SetActiveState(nameof(HeightfieldGITest));
            EngineManager.StateManager.SetActiveState(nameof(PlanetTerrainRendererTest));
            //EngineManager.StateManager.SetActiveState(nameof(TextureStreamingTest));
            //EngineManager.StateManager.SetActiveState(nameof(FenceTest));

            EngineManager.Run(60, 60);
            EngineManager.Exit();
        }
    }
}
