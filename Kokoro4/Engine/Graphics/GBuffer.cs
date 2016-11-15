using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif

namespace Kokoro.Engine.Graphics
{
    public class GBuffer
    {
        private Framebuffer fbuf;
        private Texture color_tex, normal_tex, material_tex, depth_tex;

        public GBuffer(int w, int h)
        {
            int levels = 1;

            FramebufferTextureSource color = new FramebufferTextureSource(w, h, levels)
            {
                PixelType = PixelType.UnsignedByte,
                InternalFormat = PixelInternalFormat.Rgba8
            };

            FramebufferTextureSource materials = new FramebufferTextureSource(w, h, levels)
            {
                PixelType = PixelType.Float,
                InternalFormat = PixelInternalFormat.R11fG11fB10f
            };

            FramebufferTextureSource normals = new FramebufferTextureSource(w, h, levels)
            {
                PixelType = PixelType.Float,
                InternalFormat = PixelInternalFormat.R11fG11fB10f
            };

            DepthTextureSource depth = new DepthTextureSource(w, h);
            depth.InternalFormat = PixelInternalFormat.DepthComponent32f;


            color_tex = new Texture();
            color_tex.SetData(color, 0);

            normal_tex = new Texture();
            normal_tex.SetData(normals, 0);

            material_tex = new Texture();
            material_tex.SetData(materials, 0);

            depth_tex = new Texture();
            depth_tex.SetData(depth, 0);

            fbuf = new Framebuffer(w, h);

            fbuf[FramebufferAttachment.DepthAttachment] = depth_tex;
            fbuf[FramebufferAttachment.ColorAttachment0] = color_tex;
            fbuf[FramebufferAttachment.ColorAttachment1] = material_tex;
            fbuf[FramebufferAttachment.ColorAttachment2] = normal_tex;
        }

        public static implicit operator Framebuffer(GBuffer buf)
        {
            return buf.fbuf;
        }
    }
}
