using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics
{
    public class GBuffer
    {
        private Framebuffer fbuf;
        private Texture color_tex, material_tex, depth_tex;

        public GBuffer(int w, int h)
        {
            int levels = 1;

            FramebufferTextureSource color = new FramebufferTextureSource(w, h, levels)
            {
                PixelType = OpenTK.Graphics.OpenGL.PixelType.UnsignedByte,
                InternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba8
            };

            FramebufferTextureSource materials = new FramebufferTextureSource(w, h, levels)
            {
                PixelType = OpenTK.Graphics.OpenGL.PixelType.Short,
                InternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba16ui
            };

            FramebufferTextureSource normals = new FramebufferTextureSource(w, h, levels)
            {
                PixelType = OpenTK.Graphics.OpenGL.PixelType.Float,
                InternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgb16f
            };

            DepthTextureSource depth = new DepthTextureSource(w, h);
            depth.InternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat.DepthComponent32f;


            color_tex = new Texture();
            color_tex.SetData(color, 0);

            material_tex = new Texture();
            material_tex.SetData(materials, 0);

            depth_tex = new Texture();
            depth_tex.SetData(depth, 0);

            fbuf = new Framebuffer(w, h);

            fbuf[OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0] = color_tex;
            fbuf[OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment1] = material_tex;
            fbuf[OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthAttachment] = depth_tex;
        }

        public static implicit operator Framebuffer(GBuffer buf)
        {
            return buf.fbuf;
        }
    }
}
