﻿using Kokoro.Engine;
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

            EngineManager.StateManager.AddScene(nameof(TestScene), new TestScene());
            EngineManager.StateManager.SetActiveScene(nameof(TestScene));

            EngineManager.Run(60, 60);
            EngineManager.Exit();
        }
    }
}
