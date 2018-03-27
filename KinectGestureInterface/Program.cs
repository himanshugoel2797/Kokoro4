using Kokoro.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectGestureInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            EngineManager.Name = EngineManager.EngineName;

            EngineManager.StateManager.AddState(nameof(KinectHandContour), new KinectHandContour());
            EngineManager.StateManager.AddState(nameof(KinectHandConvexHull), new KinectHandConvexHull());
            EngineManager.StateManager.AddState(nameof(KinectHand3D), new KinectHand3D());
            EngineManager.StateManager.SetActiveState(nameof(KinectHandConvexHull));
            EngineManager.Run(60, 60);
            EngineManager.Exit();
        }
    }
}
