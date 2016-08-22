using Kokoro.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    class UICompositor
    {
        ShaderProgram program;
        EngineObject fsq;

        public UICompositor()
        {
            program = new ShaderProgram(ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Graphics/Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Graphics/Shaders/FrameBuffer/fragment.glsl"));
            fsq = Graphics.Prefabs.FullScreenQuadFactory.Create();
        }

        public void Apply(Texture tex)
        {
            tex.SetResidency(TextureResidency.Resident);
            program.Set("AlbedoMap", tex.Handle);

            fsq.Bind();
            GraphicsDevice.SetShaderProgram(program);
            GraphicsDevice.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 6, true);

            tex.SetResidency(TextureResidency.NonResident);
        }
    }
}
