using Kokoro.Engine;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Editor
{
    class EngineScene : IState
    {
        GameObjectCollection objects;

        public EngineScene()
        {
            objects = new GameObjectCollection();
        }

        public void Enter(IState prev)
        {
            //Initialize the ui and viewport layers

            //Design is to have engine provide asset management, VS integration for code and editing at game runtime using a networked communications system
            //Would require engine restart on code changes

            //Projects are saved as json files. Each project contains a root json file with all the scenes. Each scene has its own json file describing scripts and resources connected to it, as well as event handlers.
            //At build, all scripts are compiled and setup internally. References can be registered at the project level.

            //In addition to scenes, renderpasses can be defined for implementing effects. Renderpasses can then be added for each scene.
            //Similarly, shaders can be defined to assign to objects and renderpasses.
            
            //Renderpasses carry their own shaders
            //Setup two windows, one for object selection, other for object properties

            //First work on tool for shader representation and render passes with hot reload.
            //Then tool for setting up scenes and physics objects.
            //Generate code, which can then be tied together using code.

            //Alternatively, engine internally manages asset loads, scripting code compiled into dll, state serialized, previous assembly unloaded, new assembly loaded, state deserialized.
            //Engine requires application restart for library changes, scene changes are scripts so reloaded?
        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {

        }

        public void Update(double interval)
        {
            for(int i = 0; i < objects.Count; i++)
            {
                var obj = objects.ElementAt(i).Value;
                if (obj.Enabled)
                    obj.Update(interval);
            }
        }
    }
}
