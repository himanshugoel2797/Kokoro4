using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics
{
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

        public static float MaxAnisotropy { get; internal set; }

        static Texture()
        {
            float a = 0;
            GL.GetFloat((GetPName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out a);
            MaxAnisotropy = a;
        }

        public Texture()
        {
            id = GL.GenTexture();
            GraphicsDevice.Cleanup += Dispose;
        }

        public void SetData(ITextureSource src)
        {
            GPUStateMachine.BindTexture(0, src.GetTextureTarget(), id);
            switch (src.GetDimensions())
            {
                case 1:
                    GL.TexImage1D(src.GetTextureTarget(), src.GetLevels(), src.GetInternalFormat(), src.GetWidth(), 0, src.GetFormat(), src.GetType(), src.GetPixelData());
                    break;
                case 2:
                    GL.TexImage2D(src.GetTextureTarget(), src.GetLevels(), src.GetInternalFormat(), src.GetWidth(), src.GetHeight(), 0, src.GetFormat(), src.GetType(), src.GetPixelData());
                    break;
                case 3:
                    GL.TexImage3D(src.GetTextureTarget(), src.GetLevels(), src.GetInternalFormat(), src.GetWidth(), src.GetHeight(), src.GetDepth(), 0, src.GetFormat(), src.GetType(), src.GetPixelData());
                    break;
            }

            GL.GenerateMipmap((GenerateMipmapTarget)src.GetTextureTarget());
            GPUStateMachine.UnbindTexture(0, src.GetTextureTarget());

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
            GPUStateMachine.BindTexture(0, texTarget, id);
            GL.TexParameter(texTarget, TextureParameterName.TextureWrapS, tileX ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(texTarget, TextureParameterName.TextureWrapT, tileY ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
            GPUStateMachine.UnbindTexture(0, texTarget);
        }

        public void SetEnableLinearFilter(bool linear)
        {
            GPUStateMachine.BindTexture(0, texTarget, id);
            GL.TexParameter(texTarget, TextureParameterName.TextureMagFilter, linear ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            GL.TexParameter(texTarget, TextureParameterName.TextureMinFilter, linear ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);
            GPUStateMachine.UnbindTexture(0, texTarget);
        }

        public void SetAnisotropicFilter(float taps)
        {
            GPUStateMachine.BindTexture(0, texTarget, id);
            GL.TexParameter(texTarget, (TextureParameterName)All.TextureMaxAnisotropyExt, taps);
            GPUStateMachine.UnbindTexture(0, texTarget);
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
