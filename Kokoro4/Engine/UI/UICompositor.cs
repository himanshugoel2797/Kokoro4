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

        public UICompositor()
        {
            program = new ShaderProgram(ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Graphics/Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Graphics/Shaders/FrameBuffer/fragment.glsl"));
        }
    }
}
