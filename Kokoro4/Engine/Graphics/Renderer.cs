using Kokoro.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    static class Renderer
    {
        public static int SSAOSampleCount { get; set; } = 0;
        public static int PBRSampleCount { get; set; } = 0;

        public static void Render(List<Mesh> meshes, RenderState state)
        {
            //Apply the render state
            EngineManager.SetRenderState(state);

            //Submit the render calls
            for(int i = 0; i < meshes.Count; i++)
            {
                GraphicsDevice.Draw(PrimitiveType.Triangles, meshes[i].StartOffset, meshes[i].IndexCount, true);
            }

        }

        public static void Render(ShaderStorageBuffer draws, uint offset, int count, RenderState state)
        {
            EngineManager.SetRenderState(state);
            GraphicsDevice.SetMultiDrawParameterBuffer(draws);
            GraphicsDevice.MultiDrawIndirect(PrimitiveType.Triangles, offset, count, true);
        }

        public static void Render(ShaderStorageBuffer draws, uint drawOffset, ShaderStorageBuffer count, uint countOffset, RenderState state)
        {
            EngineManager.SetRenderState(state);
            GraphicsDevice.SetMultiDrawParameterBuffer(draws);
            GraphicsDevice.SetParameterBuffer(count);

            GraphicsDevice.MultiDrawIndirectCount(PrimitiveType.Triangles, drawOffset, countOffset, true);
        }

    }
}
