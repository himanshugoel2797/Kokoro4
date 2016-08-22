using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics
{
    public enum TextureResidency
    {
        NonResident,
        Resident
    }

    public class TextureHandle
    {
        internal long hndl = 0;

        internal TextureHandle(long hndl)
        {
            this.hndl = hndl;
        }

        public static implicit operator long(TextureHandle handle)
        {
            return handle.hndl;
        }
    }

    public class Texture : IDisposable
    {

        internal int id;
        internal PixelInternalFormat internalformat;
        internal TextureTarget texTarget;
        internal PixelFormat format;

        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int Depth { get; internal set; }
        public int LevelCount { get; internal set; }

        public TextureHandle Handle { get; private set; }

        public static float MaxAnisotropy { get; internal set; }

        static Texture()
        {
            float a = 0;
            GL.GetFloat((GetPName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out a);
            MaxAnisotropy = a;
        }

        public Texture()
        {
            id = 0;
            GraphicsDevice.Cleanup += Dispose;
        }

        public void SetResidency(TextureResidency residency)
        {
            if(Handle == null)
                Handle = new TextureHandle(GL.Arb.GetTextureHandle(id));

            if (residency == TextureResidency.Resident) GL.Arb.MakeTextureHandleResident(Handle);
            else GL.Arb.MakeTextureHandleNonResident(Handle);
        }

        public virtual void SetData(ITextureSource src, int level)
        {
            bool inited = false;
            if (id == 0)
            {
                GL.CreateTextures(src.GetTextureTarget(), 1, out id);
                inited = true;
            }

            switch (src.GetDimensions())
            {
                case 1:
                    if (inited) GL.TextureStorage1D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth());
                    GL.TextureSubImage1D(id, level, 0, src.GetWidth(), src.GetFormat(), src.GetType(), src.GetPixelData(level));
                    break;
                case 2:
                    if (inited) GL.TextureStorage2D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth(), src.GetHeight());
                    GL.TextureSubImage2D(id, level, 0, 0, src.GetWidth(), src.GetHeight(), src.GetFormat(), src.GetType(), src.GetPixelData(level));
                    break;
                case 3:
                    if (inited) GL.TextureStorage3D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth(), src.GetHeight(), src.GetDepth());
                    GL.TextureSubImage3D(id, level, 0, 0, 0, src.GetWidth(), src.GetHeight(), src.GetDepth(), src.GetFormat(), src.GetType(), src.GetPixelData(level));
                    break;
            }

            this.Width = src.GetWidth();
            this.Height = src.GetHeight();
            this.Depth = src.GetDepth();
            this.LevelCount = src.GetLevels();

            this.format = src.GetFormat();
            this.internalformat = src.GetInternalFormat();
            this.texTarget = src.GetTextureTarget();
        }

        public void SetTileMode(bool tileX, bool tileY)
        {
            GL.TextureParameter(id, TextureParameterName.TextureWrapS, tileX ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
            GL.TextureParameter(id, TextureParameterName.TextureWrapT, tileY ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
        }

        public void SetEnableLinearFilter(bool linear)
        {
            GL.TextureParameter(id, TextureParameterName.TextureMagFilter, linear ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            GL.TextureParameter(id, TextureParameterName.TextureMinFilter, linear ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);
        }

        public void SetAnisotropicFilter(float taps)
        {
            GL.TextureParameter(id, (TextureParameterName)All.TextureMaxAnisotropyExt, taps);
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

                if (id != 0) GL.DeleteTexture(id);
                id = 0;

                disposedValue = true;
            }
        }

        ~Texture()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
