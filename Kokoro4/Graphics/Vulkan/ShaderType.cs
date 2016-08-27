#if VULKAN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace Kokoro.Engine.Graphics
{
    public enum ShaderType
    {
        FragmentShader = ShaderStageFlags.Fragment,
        VertexShader = ShaderStageFlags.Vertex,
        GeometryShader = ShaderStageFlags.Geometry,
        TessEvaluationShader = ShaderStageFlags.TessellationEvaluation,
        TessControlShader = ShaderStageFlags.TessellationControl,
        ComputeShader = ShaderStageFlags.Compute,
    }
}
#endif