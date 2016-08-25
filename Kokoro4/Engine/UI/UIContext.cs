using Kokoro.Engine.Graphics;
using Kokoro.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public class UIContext : IDisposable
    {
        Texture ui_col;
        Framebuffer ui_fbuf;

        public List<UIContainer> Containers { get; set; }
        private UICompositor Compositor;

        public UIContext(int w, int h)
        {
            ui_col = new Texture();

            FramebufferTextureSource ui_col_tex = new FramebufferTextureSource(w, h, 1)
            {
                InternalFormat = PixelInternalFormat.Rgba,
                PixelType = PixelType.UnsignedByte
            };

            ui_col.SetData(ui_col_tex, 0);
            ui_fbuf = new Framebuffer(w, h);
            ui_fbuf[OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0] = ui_col;

            Compositor = new UICompositor();

            Containers = new List<UIContainer>();
        }

#warning Move this into a post processing pass
        public void Draw()
        {
            bool depth_state = EngineManager.DepthTestEnabled;
            bool depth_write = EngineManager.DepthWriteEnabled;
            EngineManager.DepthWriteEnabled = false;
            EngineManager.DepthTestEnabled = false;
            EngineManager.Framebuffer = ui_fbuf;

            EngineManager.Clear();

            for(int i = 0; i < Containers.Count; i++)
            {
                Containers[i].Draw();
            }
            
            //Now blend the UI on top

            Compositor.Apply(ui_col);

            //GraphicsDevice.DepthTestEnabled = depth_state;
            //GraphicsDevice.DepthWriteEnabled = depth_write;
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
                    ui_fbuf.Dispose();
                    ui_col.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UIContext() {
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
