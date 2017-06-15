using Kokoro.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class TextureSampler : IDisposable
    {
        public static TextureSampler Default { get; private set; } = new TextureSampler(0);

        internal int id;
        private int _maxReadLevel, _baseReadLevel;
        private bool locked = false;

        public int MinLOD
        {
            get { return _baseReadLevel; }
            set
            {
                if (locked && _baseReadLevel != value) throw new Exception("Sampler state has been locked due to use with GetHandle.");
                if (_baseReadLevel != value) { _baseReadLevel = value; GL.SamplerParameter(id, SamplerParameterName.TextureMinLod, (float)_baseReadLevel); }
            }
        }

        public int MaxLOD
        {
            get { return _maxReadLevel; }
            set
            {

                if (locked && _maxReadLevel != value) throw new Exception("Sampler state has been locked due to use with GetHandle.");
                if (_maxReadLevel != value) { _maxReadLevel = value; GL.SamplerParameter(id, SamplerParameterName.TextureMaxLod, (float)_maxReadLevel); }
            }
        }

        public TextureSampler()
        {
            GL.CreateSamplers(1, out id);

            GraphicsDevice.Cleanup.Add(Dispose);
        }

        internal TextureSampler(int id)
        {
            this.id = id;
        }

        internal long GetHandle(int tex)
        {
            locked = true;
            return GL.Arb.GetTextureSamplerHandle(tex, id);
        }

        public void SetTileMode(bool tileX, bool tileY)
        {
            GL.SamplerParameter(id, SamplerParameterName.TextureWrapS, tileX ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge); 
            GL.SamplerParameter(id, SamplerParameterName.TextureWrapT, tileY ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
        }

        public void SetEnableLinearFilter(bool linear)
        {
            GL.SamplerParameter(id, SamplerParameterName.TextureMagFilter, linear ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            GL.SamplerParameter(id, SamplerParameterName.TextureMinFilter, linear ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);
        }

        public void SetAnisotropicFilter(float taps)
        {
            GL.SamplerParameter(id, (SamplerParameterName)All.TextureMaxAnisotropyExt, taps);
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

                if (id != 0) GraphicsDevice.QueueForDeletion(id, GLObjectType.Sampler);
                id = 0;
                
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~TextureSampler()
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
