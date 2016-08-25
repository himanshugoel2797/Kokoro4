using Kokoro.Engine.Graphics;
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
        static ShaderProgram fill_prog, tex_prog;
        static Mesh fsq;

        public UIRenderer()
        {
            if(fill_prog == null)fill_prog = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/Shaders/FilledRectangle/fragment.glsl"));
            if (tex_prog == null) tex_prog = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/Shaders/TextureRectangle/fragment.glsl"));
            if (fsq == null)fsq = Kokoro.Graphics.Prefabs.FullScreenQuadFactory.Create();
        }

        public void Apply(Vector4 col, Vector2 Position, Vector2 Size)
        {
            fill_prog.Set("AlbedoColor", col);
            fill_prog.Set("Position", Position);
            fill_prog.Set("Size", Size);

            bool blendstate = EngineManager.AlphaEnabled;
            EngineManager.AlphaEnabled = true;
            fsq.Bind();
            EngineManager.SetShaderProgram(fill_prog);
            GraphicsDevice.Draw(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, 6, true);
            EngineManager.AlphaEnabled = blendstate;
        }
        
        public void Apply(Texture tex, Vector2 Position, Vector2 Size)
        {
            tex.SetResidency(TextureResidency.Resident);
            tex_prog.Set("AlbedoMap", tex.Handle);
            tex_prog.Set("Position", Position);
            tex_prog.Set("Size", Size);

            bool blendstate = GraphicsDevice.AlphaEnabled;
            EngineManager.AlphaEnabled = true;
            fsq.Bind();
            GraphicsDevice.SetShaderProgram(tex_prog);
            GraphicsDevice.Draw(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, 6, true);
            EngineManager.AlphaEnabled = blendstate;

            tex.SetResidency(TextureResidency.NonResident);
        }
    }
}
