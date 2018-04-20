using Kokoro.Engine.Graphics;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Kokoro.VR
{
    public class VRRenderer
    {
        private CVRSystem vr;
        private CVRCompositor cr;
        private const int HMD_Idx = 0;

        public Framebuffer LeftFramebuffer { get; private set; }
        public Framebuffer RightFramebuffer { get; private set; }

        public Texture LeftColorTexture { get; private set; }
        public Texture RightColorTexture { get; private set; }

        private VRRenderer(CVRSystem vr)
        {
            this.vr = vr;
            cr = OpenVR.Compositor;

            uint width = 0, height = 0;
            vr.GetRecommendedRenderTargetSize(ref width, ref height);

            LeftFramebuffer = new Framebuffer((int)width, (int)height);
            {
                FramebufferTextureSource color = new FramebufferTextureSource((int)width, (int)height, 1)
                {
                    InternalFormat = PixelInternalFormat.Rgba8,
                    PixelType = PixelType.UnsignedByte
                };
                Texture colorTex = new Texture();
                colorTex.SetData(color, 0);
                LeftColorTexture = colorTex;

                DepthTextureSource depth = new DepthTextureSource((int)width, (int)height)
                {
                    InternalFormat = PixelInternalFormat.DepthComponent16
                };
                Texture depthTex = new Texture();
                depthTex.SetData(depth, 0);

                LeftFramebuffer[FramebufferAttachment.DepthAttachment] = depthTex;
                LeftFramebuffer[FramebufferAttachment.ColorAttachment0] = colorTex;
            }

            RightFramebuffer = new Framebuffer((int)width, (int)height);
            {
                FramebufferTextureSource color = new FramebufferTextureSource((int)width, (int)height, 1)
                {
                    InternalFormat = PixelInternalFormat.Rgba8,
                    PixelType = PixelType.UnsignedByte
                };
                Texture colorTex = new Texture();
                colorTex.SetData(color, 0);
                RightColorTexture = colorTex;

                DepthTextureSource depth = new DepthTextureSource((int)width, (int)height)
                {
                    InternalFormat = PixelInternalFormat.DepthComponent16
                };
                Texture depthTex = new Texture();
                depthTex.SetData(depth, 0);

                RightFramebuffer[FramebufferAttachment.DepthAttachment] = depthTex;
                RightFramebuffer[FramebufferAttachment.ColorAttachment0] = colorTex;
            }
        }

        public Matrix4 GetEye(bool isLeft, float nearZ, float farZ)
        {
            var mat = vr.GetProjectionMatrix(isLeft ? EVREye.Eye_Left : EVREye.Eye_Right, nearZ, farZ);

            return new Matrix4(mat.m0, mat.m1, mat.m2, mat.m3,
                               mat.m4, mat.m5, mat.m6, mat.m7,
                               mat.m8, mat.m9, mat.m10, mat.m11,
                               mat.m12, mat.m13, mat.m14, mat.m15);
        }

        public void Submit()
        {
            Texture_t tex = new Texture_t()
            {
                handle = LeftColorTexture.Id
            };
            //cr.Submit(EVREye.Eye_Left, )
        }

        public static VRRenderer Create()
        {
            EVRInitError err = EVRInitError.None;
            var sys = OpenVR.Init(ref err);

            if (err != EVRInitError.None)
                throw new Exception("Failed to initialize OpenVR.");


            return new VRRenderer(sys);
        }
    }
}
