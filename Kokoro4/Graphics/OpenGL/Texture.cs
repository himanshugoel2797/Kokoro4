#if OPENGL
using Cloo;
using Kokoro.Engine.Graphics;
using Kokoro.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public enum TextureResidency
    {
        NonResident,
        Resident
    }

    public class TextureHandle
    {
        internal long hndl = 0;
        internal Texture parent;
        private ComputeImage computeTex;

        internal ComputeImage GetImageForCompute(ComputeMemoryFlags flags, int mipLevel)
        {
            if (computeTex == null || (computeTex.Flags != flags))
            {
                switch (parent.texTarget)
                {
                    case TextureTarget.TextureCubeMapNegativeX:
                    case TextureTarget.TextureCubeMapNegativeY:
                    case TextureTarget.TextureCubeMapNegativeZ:
                    case TextureTarget.TextureCubeMapPositiveX:
                    case TextureTarget.TextureCubeMapPositiveY:
                    case TextureTarget.TextureCubeMapPositiveZ:
                    case TextureTarget.Texture2D:
                        computeTex = ComputeImage2D.CreateFromGLTexture2D(GraphicsDevice._comp_ctxt, flags, (int)parent.texTarget, mipLevel, parent.id);
                        break;
                    case TextureTarget.Texture3D:
                        computeTex = ComputeImage3D.CreateFromGLTexture3D(GraphicsDevice._comp_ctxt, flags, (int)TextureTarget.Texture3D, mipLevel, parent.id);
                        break;
                }
            }
            return computeTex;
        }

        internal TextureHandle(long hndl, Texture parent)
        {
            this.hndl = hndl;
            this.parent = parent;
        }

        public void SetResidency(TextureResidency residency)
        {
            if (residency == TextureResidency.Resident) GL.Arb.MakeTextureHandleResident(hndl);
            else GL.Arb.MakeTextureHandleNonResident(hndl);
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

        public static float MaxAnisotropy { get; internal set; }

        private Dictionary<int, TextureHandle> handles;

        static Texture()
        {
            float a = 0;
            GL.GetFloat((GetPName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out a);
            MaxAnisotropy = a;
        }

        public Texture()
        {
            id = 0;
            handles = new Dictionary<int, TextureHandle>();
            GraphicsDevice.Cleanup += Dispose;
        }

        public TextureHandle GetHandle(TextureSampler sampler)
        {
            if (!handles.ContainsKey(sampler.id))
            {
                if (sampler.id != 0)
                    handles[sampler.id] = new TextureHandle(GL.Arb.GetTextureSamplerHandle(id, sampler.id), this);
                else
                    handles[0] = new TextureHandle(GL.Arb.GetTextureHandle(id), this);
            }

            return handles[sampler.id];
        }

        public virtual void SetData(ITextureSource src, int level)
        {
            bool inited = false;
            if (id == 0)
            {
                GL.CreateTextures((OpenTK.Graphics.OpenGL.TextureTarget)src.GetTextureTarget(), 1, out id);
                inited = true;
            }
            

            switch (src.GetDimensions())
            {
                case 1:
                    if (inited) GL.TextureStorage1D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth());
                    GL.TextureSubImage1D(id, level, 0, src.GetWidth(), (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetType(), src.GetPixelData(level));
                    break;
                case 2:
                    if (inited) GL.TextureStorage2D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth(), src.GetHeight());
                    GL.TextureSubImage2D(id, level, 0, 0, src.GetWidth(), src.GetHeight(), (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetType(), src.GetPixelData(level));
                    break;
                case 3:
                    if (inited) GL.TextureStorage3D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth(), src.GetHeight(), src.GetDepth());
                    GL.TextureSubImage3D(id, level, 0, 0, 0, src.GetWidth(), src.GetHeight(), src.GetDepth(), (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetType(), src.GetPixelData(level));
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
            if (handles.ContainsKey(0)) handles.Remove(0); //Force regeneration of the handle next time it's accessed
            GL.TextureParameter(id, TextureParameterName.TextureWrapS, tileX ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
            GL.TextureParameter(id, TextureParameterName.TextureWrapT, tileY ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
        }

        public void SetEnableLinearFilter(bool linear)
        {
            if (handles.ContainsKey(0)) handles.Remove(0); //Force regeneration of the handle next time it's accessed
            GL.TextureParameter(id, TextureParameterName.TextureMagFilter, linear ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            GL.TextureParameter(id, TextureParameterName.TextureMinFilter, linear ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);
        }

        public void SetAnisotropicFilter(float taps)
        {
            if (handles.ContainsKey(0)) handles.Remove(0); //Force regeneration of the handle next time it's accessed
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
#endif