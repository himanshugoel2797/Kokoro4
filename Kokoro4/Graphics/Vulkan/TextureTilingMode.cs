using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Vulkan
{
    public enum TextureTilingMode
    {
        Linear = global::Vulkan.ImageTiling.Linear,
        Optimal = global::Vulkan.ImageTiling.Optimal
    }
}
