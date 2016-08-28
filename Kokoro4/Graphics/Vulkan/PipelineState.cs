#if VULKAN
using Kokoro.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public struct PipelineState
    {
        public DepthTestMode DepthTestMode { get; set; }
        public bool DepthWrite { get; set; }

        public bool BlendingEnabled { get; set; }
        public BlendFunc BlendingSrc { get; set; }
        public BlendFunc BlendingDst { get; set; }

        public FillMode FillMode { get; set; }

        public ShaderProgram Program { get; set; }
    }
}
#endif