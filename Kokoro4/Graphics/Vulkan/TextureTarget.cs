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
        Texture1D = ImageViewType.View1D,
        Texture2D = ImageViewType.View2D,
        Texture3D = ImageViewType.View3D,
        TextureCubeMap = ImageViewType.Cube

    }
}
