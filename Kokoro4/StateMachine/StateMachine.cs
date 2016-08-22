using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.StateMachine
{
    public class StateMachine
    {
        public Dictionary<string, IScene> Scenes { get; private set; }
        public string CurrentSceneName { get; private set; }
        public IScene CurrentScene { get; private set; }

        public StateMachine()
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

        public void AddScene(string name, IScene scene)
        {
            Scenes.Add(name, scene);
        }

        public void RemoveScene(string name)
        {
            Scenes.Remove(name);
        }

        private void Update(double interval)
        {
            CurrentScene?.Update(interval);
        }

        private void Render(double interval)
        {
            CurrentScene?.Render(interval);
        }
    }
}
