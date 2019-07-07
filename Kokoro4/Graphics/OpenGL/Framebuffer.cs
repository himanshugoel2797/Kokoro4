#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using Kokoro.Graphics.OpenGL;

namespace Kokoro.Engine.Graphics
{
    public class Framebuffer : IDisposable
    {
        public static Framebuffer Default { get; private set; }

        static Framebuffer()
        {
            Default = new Framebuffer(0, GraphicsDevice.WindowSize.Width, GraphicsDevice.WindowSize.Height);
        }

        internal static void RecreateDefaultFramebuffer()
        {
            Default.Width = GraphicsDevice.WindowSize.Width;
            Default.Height = GraphicsDevice.WindowSize.Height;
        }

        private Framebuffer(int id, int w, int h)
        {
            Width = w;
            Height = h;
            this.id = id;
        }

        internal int id;
        internal Dictionary<FramebufferAttachment, Texture> bindings;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Framebuffer(int width, int height)
        {
            Width = width;
            Height = height;

            GL.CreateFramebuffers(1, out id);
            bindings = new Dictionary<FramebufferAttachment, Texture>();
            GraphicsDevice.Cleanup.Add(Dispose);
        }

        public void Blit(Framebuffer src, bool blitColor, bool blitDepth, bool linearFilter)
        {
            GL.BlitNamedFramebuffer(src.id, this.id, 0, 0, src.Width, src.Height, 0, 0, Width, Height, (blitColor ? ClearBufferMask.ColorBufferBit : 0) | (blitDepth ? ClearBufferMask.DepthBufferBit : 0), linearFilter ? BlitFramebufferFilter.Linear : BlitFramebufferFilter.Nearest);
        }

        public Texture this[FramebufferAttachment attachment]
        {
            set
            {
                if (value == null)
                {
                    bindings.Remove(attachment);
                    GL.NamedFramebufferTexture(id, (OpenTK.Graphics.OpenGL.FramebufferAttachment)attachment, 0, 0);
                }
                else
                {
                    bindings[attachment] = value;
                    GL.NamedFramebufferTexture(id, (OpenTK.Graphics.OpenGL.FramebufferAttachment)attachment, value.id, value.WriteLevel);
                }

                if (attachment != FramebufferAttachment.DepthAttachment)
                {
                    GL.NamedFramebufferDrawBuffers(id, bindings.Keys.Count,
                        bindings.Keys.Except(new FramebufferAttachment[] { FramebufferAttachment.DepthAttachment })
                        .OrderBy((a) => (int)a).Cast<DrawBuffersEnum>().ToArray());
                } 

                if (GL.CheckNamedFramebufferStatus(id, FramebufferTarget.Framebuffer) != (All)FramebufferErrorCode.FramebufferComplete)
                {
                    throw new Exception(GL.CheckNamedFramebufferStatus(id, FramebufferTarget.Framebuffer).ToString());
                }
            }
            get
            {
                if (bindings.ContainsKey(attachment)) return bindings[attachment];
                else return null;
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
                if (id != 0) GraphicsDevice.QueueForDeletion(id, GLObjectType.Framebuffer);
                id = 0;

                disposedValue = true;
            }
        }

        ~Framebuffer()
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
#endif