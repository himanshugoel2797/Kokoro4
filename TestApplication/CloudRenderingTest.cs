using Kokoro.Engine.Graphics;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class CloudRenderingTest : IState
    {
        CloudRenderer renderer;

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {
            if (renderer == null)
                renderer = new CloudRenderer(2.1f * 1.1f, 6420, null, null);

            renderer.Render();
        }

        public void Update(double interval)
        {

        }
    }
}
