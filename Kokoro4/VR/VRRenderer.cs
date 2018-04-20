using Kokoro.Engine.Graphics;
using Kokoro.Graphics.OpenGL;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Kokoro.VR
{
    public class VRRenderer : IDisposable
    {
        private CVRSystem vr;
        private CVRCompositor cr;
        private const int HMD_Idx = 0;

        public Framebuffer LeftFramebuffer { get; private set; }
        public Framebuffer RightFramebuffer { get; private set; }

        public Texture LeftColorTexture { get; private set; }
        public Texture RightColorTexture { get; private set; }

        private Texture_t leftEye;
        private Texture_t rightEye;

        private VRTextureBounds_t defaultBounds;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private VRRenderer(CVRSystem vr)
        {
            this.vr = vr;
            cr = OpenVR.Compositor;

            uint width = 0, height = 0;
            vr.GetRecommendedRenderTargetSize(ref width, ref height);

            Width = (int)width;
            Height = (int)height;

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
                    InternalFormat = PixelInternalFormat.DepthComponent32
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
                    InternalFormat = PixelInternalFormat.DepthComponent32
                };
                Texture depthTex = new Texture();
                depthTex.SetData(depth, 0);

                RightFramebuffer[FramebufferAttachment.DepthAttachment] = depthTex;
                RightFramebuffer[FramebufferAttachment.ColorAttachment0] = colorTex;
            }

            leftEye = new Texture_t()
            {
                handle = (IntPtr)LeftColorTexture.id,
                eColorSpace = EColorSpace.Auto,
                eType = ETextureType.OpenGL
            };

            rightEye = new Texture_t()
            {
                handle = (IntPtr)RightColorTexture.id,
                eColorSpace = EColorSpace.Auto,
                eType = ETextureType.OpenGL
            };

            defaultBounds = new VRTextureBounds_t()
            {
                uMin = 0,
                vMin = 0,
                uMax = 1,
                vMax = 1,
            };

            GraphicsDevice.Cleanup.Add(Dispose);
        }

        public Matrix4 GetEyeProjection(bool isLeft, float nearZ, float farZ)
        {
            var mat = vr.GetProjectionMatrix(isLeft ? EVREye.Eye_Left : EVREye.Eye_Right, nearZ, farZ);

            return Matrix4.Transpose(new Matrix4(mat.m0, mat.m1, mat.m2, mat.m3,
                               mat.m4, mat.m5, mat.m6, mat.m7,
                               mat.m8, mat.m9, mat.m10, mat.m11,
                               mat.m12, mat.m13, mat.m14, mat.m15));
        }

        public Matrix4 GetEyeView(bool isLeft)
        {
            var mat = vr.GetEyeToHeadTransform(isLeft ? EVREye.Eye_Left : EVREye.Eye_Right);

            return Matrix4.Invert(new Matrix4(mat.m0, mat.m4, mat.m8, 0,
                               mat.m1, mat.m5, mat.m9, 0,
                               mat.m2, mat.m6, mat.m10, 0,
                               mat.m3, mat.m7, mat.m11, 1));
        }

        public void Clear()
        {
            cr.ClearLastSubmittedFrame();
        }

        public void Submit(bool isLeft)
        {
            EVRCompositorError err = EVRCompositorError.None;
            if (isLeft)
                err = cr.Submit(EVREye.Eye_Left, ref leftEye, ref defaultBounds, EVRSubmitFlags.Submit_Default);
            else
                err = cr.Submit(EVREye.Eye_Right, ref rightEye, ref defaultBounds, EVRSubmitFlags.Submit_Default);

            //if (err != EVRCompositorError.None)
            //    throw new Exception();
        }

        public Matrix4 GetPose()
        {
            var tPose = new TrackedDevicePose_t[1];
            var gPose = new TrackedDevicePose_t[0];


            var err = cr.WaitGetPoses(tPose, gPose);
            if (err != EVRCompositorError.None)
                throw new Exception();

            var mat = tPose[0].mDeviceToAbsoluteTracking;
            /*return new Matrix4(mat.m0, mat.m1, mat.m2, 0,
                   mat.m3, mat.m4, mat.m5, 0,
                   mat.m6, mat.m7, mat.m8, 0,
                   mat.m9, mat.m10, mat.m11, 1);*/

            return Matrix4.Invert(new Matrix4(mat.m0, mat.m4, mat.m8, 0,
                               mat.m1, mat.m5, mat.m9, 0,
                               mat.m2, mat.m6, mat.m10, 0,
                               mat.m3, mat.m7, mat.m11, 1));
        }

        public static VRRenderer Create()
        {
            EVRInitError err = EVRInitError.None;
            var sys = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Scene);

            if (err != EVRInitError.None)
                throw new Exception("Failed to initialize OpenVR.");


            return new VRRenderer(sys);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                OpenVR.Shutdown();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~VRRenderer()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
