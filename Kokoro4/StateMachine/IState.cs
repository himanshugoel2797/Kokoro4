using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.StateMachine
{
    public interface IState
    {
        void Enter(IState prev);
        void Exit(IState next);

        void Update(double interval);
        void Render(double interval);
    }
}
