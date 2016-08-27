#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Kokoro.Engine.Graphics
{
    public class Framebuffer : IDisposable
    {
        public static Framebuffer Default;

        static Framebuffer()
        {
            Default = new Framebuffer(0, EngineManager.WindowSize.Width, EngineManager.WindowSize.Height);
        }

        internal static void RecreateDefaultFramebuffer()
        {
            Default = new Framebuffer(0, EngineManager.WindowSize.Width, EngineManager.WindowSize.Height);
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
            EngineManager.Cleanup += Dispose;
        }

        public Texture this[FramebufferAttachment attachment]
        {
            set
            {
                bindings[attachment] = value;

                GL.NamedFramebufferTexture(id, attachment, value.id, 0);

                GL.NamedFramebufferDrawBuffers(id, bindings.Keys.Count,
                    bindings.Keys.OrderByDescending((a) => (int)a).Reverse().Cast<DrawBuffersEnum>().ToArray());

                if(GL.CheckNamedFramebufferStatus(id, FramebufferTarget.Framebuffer) != (All)FramebufferErrorCode.FramebufferComplete)
                {
                    throw new Exception("Incomplete Framebuffer!");
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
#endif