#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public enum DepthFunc
    {
        None = OpenTK.Graphics.OpenGL.DepthFunction.Never,
        LEqual = OpenTK.Graphics.OpenGL.DepthFunction.Lequal,
        GEqual = OpenTK.Graphics.OpenGL.DepthFunction.Gequal,
        Less = OpenTK.Graphics.OpenGL.DepthFunction.Less,
        Greater = OpenTK.Graphics.OpenGL.DepthFunction.Greater,
        Equal = OpenTK.Graphics.OpenGL.DepthFunction.Equal,
        Always = OpenTK.Graphics.OpenGL.DepthFunction.Always,
    }
}
#endif