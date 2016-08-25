#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    /// <summary>
    /// Used in GL.CreateShader, GL.CreateShaderProgram and 9 other functions
    /// </summary>
    public enum ShaderType : int
    {
        /// <summary>
        /// Original was GL_FRAGMENT_SHADER = 0x8B30
        /// </summary>
        FragmentShader = ((int)0x8B30),
        /// <summary>
        /// Original was GL_VERTEX_SHADER = 0x8B31
        /// </summary>
        VertexShader = ((int)0x8B31),
        /// <summary>
        /// Original was GL_GEOMETRY_SHADER = 0x8DD9
        /// </summary>
        GeometryShader = ((int)0x8DD9),
        /// <summary>
        /// Original was GL_GEOMETRY_SHADER_EXT = 0x8DD9
        /// </summary>
        GeometryShaderExt = ((int)0x8DD9),
        /// <summary>
        /// Original was GL_TESS_EVALUATION_SHADER = 0x8E87
        /// </summary>
        TessEvaluationShader = ((int)0x8E87),
        /// <summary>
        /// Original was GL_TESS_CONTROL_SHADER = 0x8E88
        /// </summary>
        TessControlShader = ((int)0x8E88),
        /// <summary>
        /// Original was GL_COMPUTE_SHADER = 0x91B9
        /// </summary>
        ComputeShader = ((int)0x91B9),
    }

}
#endif