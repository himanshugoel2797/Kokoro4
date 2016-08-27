#if VULKAN
using Kokoro.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace Kokoro.Graphics.Vulkan
{
    public class IntShaderSource
    {
        internal ShaderModule module;
        internal ShaderType sType;

        public IntShaderSource(ShaderType type, string src)
        {
            ShaderModuleCreateInfo info = new ShaderModuleCreateInfo()
            {
                
            };

            module = GraphicsDevice.CreateShaderModule(info);
        }
    }
}
#endif