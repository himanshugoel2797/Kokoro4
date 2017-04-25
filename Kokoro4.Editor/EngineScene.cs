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
