using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public enum CullFaceMode
    {
        None,
        Front = OpenTK.Graphics.OpenGL.CullFaceMode.Front,
        Back = OpenTK.Graphics.OpenGL.CullFaceMode.Back,
    }
}
