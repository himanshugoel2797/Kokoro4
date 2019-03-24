using Kokoro.Engine;
using Kokoro.StateMachine;
using Messier.Base.Data;
using Messier.Base.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base
{
    class LoadingScreen : IState
    {
        Queue<string> loadables;
        string currentConfig;
        int configCnt, configIdx;

        public void Enter(IState prev)
        {
            var configs = Directory.EnumerateFiles("Modules", "*.json", SearchOption.AllDirectories).Where(a => a.Contains("/Configs/"));
            loadables = new Queue<string>(configs);
            configCnt = configs.Count();
            configIdx = 0;
        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {
            //TODO show the currently loaded config
            Console.WriteLine(currentConfig);
        }

        public void Update(double interval)
        {
            if(loadables.Count > 0)
            {
                var cfgPath = loadables.Dequeue();
                configIdx++;

                try
                {
                    var obj = Configuration.LoadFile(cfgPath);
                    if(obj != null)
                    {
                        ObjectManager.Default.AddObject(obj);
                        currentConfig = $"{currentConfig} ({configIdx}/{configCnt})";
                    }
                }
                catch (ConfigurationLoadException e)
                {
                    Logger.Log($"Loading Configuration: {cfgPath}");
                    Logger.Error(e.Message);
                }
            }
            else
            {
                //Switch to the main menu scene
                EngineManager.StateManager.SetActiveState(nameof(MainMenu));
            }
        }
    }
}
