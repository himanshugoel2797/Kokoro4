using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public static class Renderer
    {
        public static int SSAOSampleCount { get; set; } = 0;
        public static int PBRSampleCount { get; set; } = 0;

        public static void Render(List<Mesh> meshes, RenderState state)
        {
            //Apply the render state
            EngineManager.SetRenderState(state);

            //Submit the render calls
        }

    }
}
