using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace Kokoro.Engine.Graphics
{
    public enum TextureTarget
    {
        Texture1D = ImageType.Image1D,
        Texture2D = ImageType.Image2D,
        Texture3D = ImageType.Image3D
    }
}
