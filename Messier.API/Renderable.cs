using Kokoro.Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base
{
    public abstract class Renderable : IDisposable
    {
        public string Name { get; private set; }
        public bool Disposed { get => disposedValue; private set => disposedValue = value; }
        public bool Initialized { get => _initialized; private set => _initialized = value; }

        public Renderable(string name)
        {
            Name = name;
        }

        #region Initialization
        [NonSerialized]
        private bool _initialized;
        public virtual void Initialize()
        {
            if (!_initialized)
                _initialized = true;
        }
        #endregion

        public abstract void Update(double time);
        public abstract void Render(double time, Framebuffer framebuffer);

        #region IDisposable Support
        [NonSerialized]
        private bool disposedValue = false;

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
        // ~Renderable() {
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
