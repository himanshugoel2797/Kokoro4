using Kokoro.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.OpenGL.ShaderLibraries
{
    public class Noise
    {
        public static ShaderLibrary Library { get; private set; }
        public static string Name { get; private set; } = nameof(Noise);

        //TODO write a compute shader that outputs the value of this noise to the provided texture sampled along a sphere defined by the points passed to the shader.

        static Noise()
        {
            Library = ShaderLibrary.Create(nameof(Noise));
            Library.AddSourceFile("ShaderLibraries/Noise/snoise3d.glsl");
            Library.AddSourceFile("ShaderLibraries/Noise/valnoise3d.glsl");
        }
    }
}
