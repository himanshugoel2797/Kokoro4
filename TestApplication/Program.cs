using Kokoro.Engine;
using Kokoro.Engine.UI;
using Kokoro.Graphics;
using Kokoro.Engine.Cameras;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            EngineManager.Name = EngineManager.EngineName;
            EngineManager.Run(60, 60);
        }
    }
}
