using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.StateMachine
{
    public class StateManager : IState
    {
        public Dictionary<string, IState> States { get; private set; }
        public string CurrentStateName { get; private set; }
        public IState CurrentState { get; private set; }

        public StateManager()
        {
            States = new Dictionary<string, IState>();
        }

        public void SetActiveState(string name)
        {
            if (States.ContainsKey(name))
            {
                CurrentStateName = name;

                var prevState = CurrentState;
                var nextState = States[name];

                Exit(nextState);        //Call exit on the previous State
                CurrentState = nextState;   //Change the current State
                Enter(prevState);       //Call enter on the new State
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        public void Register(StateGroup grp)
        {
            grp.RegisterIState(this);
        }

        public void AddState(string name, IState State)
        {
            States.Add(name, State);
        }

        public void RemoveState(string name)
        {
            States.Remove(name);
        }

        public void Update(double interval)
        {
            CurrentState?.Update(interval);
        }

        public void Render(double interval)
        {
            CurrentState?.Render(interval);
        }

        public void Enter(IState prev)
        {
            CurrentState?.Enter(prev);
        }

        public void Exit(IState next)
        {
            CurrentState?.Exit(next);
        }
    }
}
