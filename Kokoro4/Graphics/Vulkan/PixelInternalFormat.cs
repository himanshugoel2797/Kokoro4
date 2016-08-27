using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public enum PixelInternalFormat
    {
        Rgba8 = global::Vulkan.Format.R8g8b8a8Uint,
        Rgba = Rgba8,
        Rgba16ui = global::Vulkan.Format.R16g16b16a16Uint,
        Rgb16f = global::Vulkan.Format.R16g16b16Sfloat,
        DepthComponent32f = global::Vulkan.Format.D32Sfloat
    }
}
