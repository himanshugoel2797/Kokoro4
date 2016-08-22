using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.StateMachine
{
    public class StateGroup
    {
        public Action<double> Update { get; set; }
        public Action<double> Render { get; set; }

        public void RegisterIScene(IScene scene)
        {
            Update += scene.Update;
            Render += scene.Render;
        }
    }
}
