using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class ShaderGroup
    {
        public const string LightingShader = "Lighting";
        public const string ShadowShader = "Shadow";

        private Dictionary<string, ShaderProgram> Programs;

        public ShaderGroup()
        {
            Programs = new Dictionary<string, ShaderProgram>();
        }

        public void SetShader(string shaderName, ShaderProgram program)
        {
            Programs[shaderName] = program;
        }
    }
}
