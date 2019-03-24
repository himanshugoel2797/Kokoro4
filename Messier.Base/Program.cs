using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base
{
    class Program
    {
        static void Main(string[] args)
        {
            //Planet rendering is controlled by modules that provide quadtree levels, described in json
            //  Collisions handled by sampling the heightmap
            //  Advanced ground scatter system
            //      Support high detail LoD scatters like grass
            //      Scatters support animations
            //Parts are described via json, animated via mesh interpolation
            //  Physics colliders are desicribed by combining primitive objects like cubes, spheres, cones, triangular pyramids, rectangular pyramids and cylinders
            //  Colliders can be set up for specific keyframes
            //  The craft is modeled as a graph
            //Planet system relies on a source barycenter, but stars are renderables
            //Every body is a renderable
            //  Planet renderer is a custom implemented renderable
            //  Stars are just sphere renderables
            //  Crafts are renderables
            //      Parts are sub-renderables - only called when craft are nearby
            //Physics engine uses doubles for everything
            //Messier.Base defines renderable base class, module loading and then config initialization
            //  Load all modules first
            //      Start loading initialization configurations, following specified dependency order
            //Messier.Base also provides file/texture management - reduce memory consumption

            EngineManager.Name = EngineManager.EngineName;

            Modules.ModuleLoader.Setup();
            Modules.ModuleLoader.LoadAll();

            EngineManager.StateManager.AddState(nameof(LoadingScreen), new LoadingScreen());
            EngineManager.StateManager.AddState(nameof(MainMenu), new MainMenu());

            EngineManager.StateManager.SetActiveState(nameof(LoadingScreen));

            EngineManager.Run(60, 60);
            EngineManager.Exit();
        }
    }
}
