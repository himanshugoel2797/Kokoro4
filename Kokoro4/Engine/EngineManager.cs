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
using Kokoro.Engine.Graphics;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif

namespace Kokoro.Engine
{
    public static class EngineManager
    {
        public static StateManager StateManager { get; private set; }
        public static MeshGroup CurrentMeshGroup { get; private set; }

        public static Matrix4 View { get { if (VisibleCamera == null) return Matrix4.Identity; return VisibleCamera.View; } }
        public static Matrix4 Projection { get { if (VisibleCamera == null) return Matrix4.Identity; return VisibleCamera.Projection; } }
        public static Camera VisibleCamera { get; private set; }

        public static string Name { get { return GraphicsDevice.Name; } set { GraphicsDevice.Name = value; } }
        public static string EngineName { get { return $"{typeof(EngineManager).Assembly.GetName().Name} {typeof(EngineManager).Assembly.GetName().Version}"; } }

        private static Dictionary<string, Camera> Cameras;

        private static Queue<Action> BackgroundTasks;   //execute these tasks during waits and finally, before swapbuffers
        private static Queue<Action> NextFrameTasks;    //tasks to execute starting next frame
        private static List<Action> DeregisterTasks;    //tasks to deregister

        static EngineManager()
        {
            Name = EngineName;
            Cameras = new Dictionary<string, Camera>();
            BackgroundTasks = new Queue<Action>();
            NextFrameTasks = new Queue<Action>();
            DeregisterTasks = new List<Action>();

            BackgroundTasks.Enqueue(GraphicsDevice.DeleteSomeObjects);

            //Initialize the state machine
            StateManager = new StateManager();
            GraphicsDevice.GameLoop.RegisterIState(new _SceneMan());
        }

        public static void RegisterBackgroundTask(Action a)
        {
            BackgroundTasks.Enqueue(a);
        }

        public static void DeregisterBackgroundTask(Action a)
        {
            DeregisterTasks.Add(a);
        }

        public static void Clear()
        {
            GraphicsDevice.Clear();
        }

        public static void ClearDepthBuffer()
        {
            GraphicsDevice.ClearDepthBuffer();
        }

        public static void SetRenderState(RenderState state)
        {
            GraphicsDevice.CullMode = state.CullMode;
            GraphicsDevice.ClearColor = state.ClearColor;
            GraphicsDevice.DepthTest = state.DepthTest;
            GraphicsDevice.DepthWriteEnabled = state.DepthWrite;
            GraphicsDevice.ColorWriteEnabled = state.ColorWrite;
            GraphicsDevice.ClearDepth = state.ClearDepth;
            GraphicsDevice.AlphaSrc = state.Src;
            GraphicsDevice.AlphaDst = state.Dst;
            GraphicsDevice.Framebuffer = state.Framebuffer;
            GraphicsDevice.SetDepthRange(state.NearPlane, state.FarPlane);

            for (int i = 0; i < state.Viewports.Length; i++)
                GraphicsDevice.SetViewport(i, state.Viewports[i].X, state.Viewports[i].Y, state.Viewports[i].Z, state.Viewports[i].W);

            if (state.ShaderStorageBufferBindings != null)
            {
                ShaderStorageBuffer[] pendingBindings = new ShaderStorageBuffer[state.ShaderStorageBufferBindings.Length];
                Array.Copy(state.ShaderStorageBufferBindings, pendingBindings, pendingBindings.Length);
                int pendingCnt = pendingBindings.Length;
                while (pendingCnt > 0)
                {
                    for (int i = 0; i < pendingBindings.Length; i++)
                    {
                        if (pendingBindings[i] != null && pendingBindings[i].IsReady)
                        {
                            GraphicsDevice.SetShaderStorageBufferBinding(pendingBindings[i], i);
                            pendingBindings[i] = null;
                            pendingCnt--;
                        }
                    }
                }
            }

            if (state.UniformBufferBindings != null)
            {
                UniformBuffer[] pendingBindings = new UniformBuffer[state.UniformBufferBindings.Length];
                Array.Copy(state.UniformBufferBindings, pendingBindings, pendingBindings.Length);
                int pendingCnt = pendingBindings.Length;
                while (pendingCnt > 0)
                {
                    for (int i = 0; i < pendingBindings.Length; i++)
                    {
                        if (pendingBindings[i] != null && pendingBindings[i].IsReady)
                        {
                            GraphicsDevice.SetUniformBufferBinding(pendingBindings[i], i);
                            pendingBindings[i] = null;
                            pendingCnt--;
                        }
                    }
                }
            }

            if (state.ShaderProgram != null)
                GraphicsDevice.ShaderProgram = state.ShaderProgram;
        }

        public static void SetCurrentMeshGroup(MeshGroup grp)
        {
            ExecuteBackgroundTasksUntil(() => grp.IsReady);
            while (!grp.IsReady) ;
            CurrentMeshGroup = grp;
            GraphicsDevice.SetVertexArray(grp.varray);
        }

        #region Execution Management
        public static void Run(double fps, double ups)
        {
            GraphicsDevice.Run(ups, fps);
        }

        public static bool ExecuteBackgroundTasksUntil(Func<bool> a)
        {
            while (!a())
            {
                if (BackgroundTasks.Count == 0)
                    return a();

                ExecuteBackgroundTask();
            }

            return true;
        }

        public static void ExecuteBackgroundTask()
        {
            if (BackgroundTasks.Count == 0)
                return;

            Action a = BackgroundTasks.Dequeue();
            if (DeregisterTasks.Contains(a))
            {
                DeregisterTasks.Remove(a);
                return;
            }
            a();
            NextFrameTasks.Enqueue(a);

        }

        public static void DispatchSyncComputeJob(ShaderProgram task, int x, int y, int z)
        {
            GraphicsDevice.DispatchSyncComputeJob(task, x, y, z);
        }

        public static void Exit()
        {
            GraphicsDevice.CleanupStrong();
            GraphicsDevice.Cleanup?.Invoke();
            GraphicsDevice.Exit();
        }
        #endregion

        #region Scene manager
        private class _SceneMan : IState
        {
            public void Update(double interval)
            {
                StateManager.Update(interval); 
            }

            public void Render(double interval)
            {
                GraphicsDevice.Clear();
                StateManager.Render(interval);
                {
                    for (int i = 0; i < BackgroundTasks.Count; i++)
                    {
                        ExecuteBackgroundTask();
                    }
                    var tmp = NextFrameTasks;
                    NextFrameTasks = BackgroundTasks;
                    BackgroundTasks = tmp;
                } 
                GraphicsDevice.SwapBuffers();
            }

            public void Enter(IState prev)
            {
                StateManager.Enter(prev);
            }

            public void Exit(IState next)
            {
                StateManager.Exit(next);
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
