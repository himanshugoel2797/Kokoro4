using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public enum PixelFormat
    {
        Rgba8 = global::Vulkan.Format.R8g8b8a8Uint,
        Bgra = global::Vulkan.Format.B8g8r8a8Uint,
        DepthComponent = global::Vulkan.Format.D32Sfloat
    }
}
