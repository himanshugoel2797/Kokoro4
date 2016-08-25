using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public abstract class EngineRenderable : EngineComponent
    {
        //Note: The lower the value, the higher the priority
        public int RenderPriority { get; set; }
    }
}
