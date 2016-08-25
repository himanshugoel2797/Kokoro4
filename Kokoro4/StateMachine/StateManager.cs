using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.StateMachine
{
    public class StateManager : IScene
    {
        public Dictionary<string, IScene> Scenes { get; private set; }
        public string CurrentSceneName { get; private set; }
        public IScene CurrentScene { get; private set; }

        public StateManager()
        {
            Scenes = new Dictionary<string, IScene>();
        }

        public void SetActiveScene(string name)
        {
            if (Scenes.ContainsKey(name))
            {
                CurrentSceneName = name;
                CurrentScene = Scenes[name];
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }
        }

        public void Register(StateGroup grp)
        {
            grp.RegisterIScene(this);
        }

        public void AddScene(string name, IScene scene)
        {
            Scenes.Add(name, scene);
        }

        public void RemoveScene(string name)
        {
            Scenes.Remove(name);
        }

        public void Update(double interval)
        {
            CurrentScene?.Update(interval);
        }

        public void Render(double interval)
        {
            CurrentScene?.Render(interval);
        }
    }
}
