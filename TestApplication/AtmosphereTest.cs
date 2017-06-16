using Kokoro.Engine.Graphics;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class AtmosphereTest : IState
    {
        AtmosphereRenderer renderer;

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {
            
        }

        public void Render(double interval)
        {
            if (renderer == null)
                renderer = new AtmosphereRenderer(new Kokoro.Math.Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f), 8, 20, 1.2f, 6360, 6420); 

            renderer.Render();
        }

        public void Update(double interval)
        {
            
        }
    }
}
