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
        public Action<IState> Enter { get; set; }
        public Action<IState> Exit { get; set; }

        public void RegisterIState(IState scene)
        {
            Update += scene.Update;
            Render += scene.Render;
            Enter += scene.Enter;
            Exit += scene.Exit;
        }
    }
}
