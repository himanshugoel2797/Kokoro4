using Kokoro.Graphics;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public class UIRenderer
    {
        static ShaderProgram program;
        static EngineObject fsq;

        public UIRenderer()
        {
            if(program == null)program = new ShaderProgram(ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Graphics/Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Graphics/Shaders/FilledRectangle/fragment.glsl"));
            if(fsq == null)fsq = Graphics.Prefabs.FullScreenQuadFactory.Create();
        }

        public void Apply(Vector4 col, Vector2 Position, Vector2 Size)
        {
            program.Set("AlbedoColor", col);
            program.Set("Position", Position);
            program.Set("Size", Size);

            bool blendstate = GraphicsDevice.AlphaEnabled;
            GraphicsDevice.AlphaEnabled = true;
            fsq.Bind();
            GraphicsDevice.SetShaderProgram(program);
            GraphicsDevice.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 6, true);
            GraphicsDevice.AlphaEnabled = blendstate;
        }
    }
}
