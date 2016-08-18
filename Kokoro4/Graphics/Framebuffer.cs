﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Messier.Graphics
{
    public class Framebuffer : IDisposable
    {
        public static Framebuffer Default;

        static Framebuffer()
        {
            Default = new Framebuffer(0, 0);
            Default.id = 0;
            Default.bindings[FramebufferAttachment.ColorAttachment0] = null;
        }

        internal int id;
        internal Dictionary<FramebufferAttachment, Texture> bindings;

        public Framebuffer(int width, int height)
        {
            id = GL.GenFramebuffer();
            bindings = new Dictionary<FramebufferAttachment, Texture>();
            GraphicsDevice.Cleanup += Dispose;
        }

        public Texture this[FramebufferAttachment attachment]
        {
            set
            {
                bindings[attachment] = value;
                GPUStateMachine.BindFramebuffer(id);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, value.texTarget, value.id, 0);

                GL.DrawBuffers(bindings.Keys.Count,
                    bindings.Keys.OrderByDescending((a) => (int)a).Reverse().Cast<DrawBuffersEnum>().ToArray());

                if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                {
                    throw new Exception("Incomplete Framebuffer!");
                }

                GPUStateMachine.UnbindFramebuffer();
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
                if(id != 0)GL.DeleteFramebuffer(id);
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
