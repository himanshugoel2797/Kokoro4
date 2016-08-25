using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kokoro.StateMachine;
using Kokoro.SceneGraph;
using Kokoro.Math;
using Kokoro.Graphics;
using Kokoro.Engine.Cameras;

namespace Kokoro.Engine
{
    public static class EngineManager
    {
        public static StateManager StateManager { get; private set; }

        public static Matrix4 View { get; set; }
        public static Matrix4 Projection { get; set; }
        public static Camera VisibleCamera { get; private set; }

        public static string Name { get { return GraphicsDevice.Name; } set { GraphicsDevice.Name = value; } }
        public static string EngineName { get { return $"{typeof(EngineManager).Assembly.GetName().Name} {typeof(EngineManager).Assembly.GetName().Version}"; } }

        private static Dictionary<string, Camera> Cameras;

        static EngineManager()
        {
            Name = EngineName;
            Cameras = new Dictionary<string, Camera>();

            StateManager = new StateManager();
            GraphicsDevice.GameLoop.RegisterIScene(new _SceneMan());
        }

        public static void Run(double fps, double ups)
        {
            GraphicsDevice.Run(ups, fps);
        }

        #region Scene manager

        private class _SceneMan : IScene
        {
            public void Update(double interval)
            {
                StateManager.Update(interval);
            }

            public void Render(double interval)
            {
                GraphicsDevice.Clear();
                StateManager.Render(interval);
                GraphicsDevice.SwapBuffers();
            }
        }



        #endregion

        #region Camera management
        public static void AddCamera(Camera a)
        {
            Cameras.Add(a.Name, a);
        }

        public static void SetVisibleCamera(string name)
        {
            VisibleCamera = Cameras[name];
        }

        public static void RemoveCamera(string name)
        {
            Cameras.Remove(name);
        }
        #endregion

    }
}
