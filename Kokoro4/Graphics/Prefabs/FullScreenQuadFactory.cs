using Kokoro.Engine.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Prefabs
{
    public class FullScreenQuadFactory
    {
        static EngineObject eObj;

        static FullScreenQuadFactory()
        {
            Init();
        }

        private static void Init()
        {
            eObj = new EngineObject();

            eObj.SetIndices(0, new uint[] { 3, 2, 0, 0, 2, 1 }, false);
            eObj.SetUVs(0, new float[] {
                0,1,
                1,1,
                1,0,
                0,0
            }, false);

            eObj.SetVertices(0, new float[]{
                -1, 1, 0.5f,
                1, 1, 0.5f,
                1, -1,0.5f,
                -1, -1,0.5f
            }, false);
        }
        
        public static EngineObject Create()
        {
            return new EngineObject(eObj, true);    //Lock the buffers from changes
        }
    }
}
