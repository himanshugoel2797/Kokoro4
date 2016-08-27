using Kokoro.Graphics.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace Kokoro.Engine.Graphics
{
    public enum FramebufferAttachment
    {
        ColorAttachment0,
        ColorAttachment1,
        ColorAttachment2,
        ColorAttachment3,
        DepthAttachment
    }

    public struct FramebufferAttachmentData
    {
        public Texture Texture { get; set; }
        public FramebufferAttachment Attachment { get; set; }
    }

    public class Framebuffer : IDisposable
    {
        private Dictionary<FramebufferAttachment, Texture> Attachments;
        private global::Vulkan.Framebuffer fbuf;

        public int Width { get; set; }
        public int Height { get; set; }

        public Framebuffer(int width, int height, params FramebufferAttachmentData[] RenderTargets)
        {
            Width = width;
            Height = height;
            Attachments = new Dictionary<FramebufferAttachment, Texture>();
            for(int i = 0; i < RenderTargets.Length; i++)
            {
                Attachments[RenderTargets[i].Attachment] = RenderTargets[i].Texture;
            }

            FramebufferCreateInfo fbuf_create_info = new FramebufferCreateInfo()
            {
                AttachmentCount = (uint)Attachments.Count,

            };

            fbuf = GraphicsDevice.CreateFramebuffer(fbuf_create_info);
        }

        public Texture this[FramebufferAttachment a]
        {
            get
            {
                return Attachments[a];
            }
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

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Framebuffer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
