using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base
{
    class MainMenu : IState
    {
        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {
            throw new NotImplementedException();
        }

        public void Render(double interval)
        {
            //TODO Load
            //TODO New
            //TODO Settings
            //TODO Exit
        }

        public void Update(double interval)
        {

        }
    }
}
