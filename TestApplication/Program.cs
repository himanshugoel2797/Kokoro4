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
using TestApplication.AdvancedAtmosphere;

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
            EngineManager.StateManager.AddState(nameof(AtmosphereTest), new AtmosphereTest());
            EngineManager.StateManager.AddState(nameof(AtmosphereTestVR), new AtmosphereTestVR());
            EngineManager.StateManager.AddState(nameof(CloudRenderingTest), new CloudRenderingTest());
            EngineManager.StateManager.AddState(nameof(HeightfieldGITest), new HeightfieldGITest());
            EngineManager.StateManager.AddState(nameof(ForwardPlusTest), new ForwardPlusTest());
            EngineManager.StateManager.AddState(nameof(SubsurfaceMaterialTest), new SubsurfaceMaterialTest());
            EngineManager.StateManager.AddState(nameof(VolumeRayCastingTest), new VolumeRayCastingTest());
            EngineManager.StateManager.AddState(nameof(AdvancedAtmosphereTest), new AdvancedAtmosphereTest());

            //EngineManager.StateManager.SetActiveState(nameof(HeightfieldGITest));
            //EngineManager.StateManager.SetActiveState(nameof(QuadTreeTerrainTest));
            //EngineManager.StateManager.SetActiveState(nameof(PlanetTerrainRendererTest));
            //EngineManager.StateManager.SetActiveState(nameof(TextureStreamingTest)); 
            //EngineManager.StateManager.SetActiveState(nameof(FenceTest));
            EngineManager.StateManager.SetActiveState(nameof(AtmosphereTest));
            //EngineManager.StateManager.SetActiveState(nameof(AtmosphereTestVR));
            //EngineManager.StateManager.SetActiveState(nameof(ForwardPlusTest));
            //EngineManager.StateManager.SetActiveState(nameof(CloudRenderingTest));
            //EngineManager.StateManager.SetActiveState(nameof(SubsurfaceMaterialTest));
            //EngineManager.StateManager.SetActiveState(nameof(TestScene));
            //EngineManager.StateManager.SetActiveState(nameof(VolumeRayCastingTest));
            //EngineManager.StateManager.SetActiveState(nameof(AdvancedAtmosphereTest));

            //Work on ice/snow rendering//

            EngineManager.Run(60, 60);
            EngineManager.Exit();
        }
    }
}
