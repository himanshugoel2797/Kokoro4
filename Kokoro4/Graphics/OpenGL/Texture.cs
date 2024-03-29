﻿#if OPENGL
using Cloo;
using Kokoro.Engine.Graphics;
using Kokoro.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public enum Residency
    {
        NonResident,
        Resident
    }

    public enum AccessMode
    {
        Read = All.ReadOnly,
        Write = All.WriteOnly,
        ReadWrite = All.ReadWrite,
    }

    public class ImageHandle
    {
        internal long hndl = 0;
        internal Texture parent;

        internal ImageHandle(long hndl, Texture parent)
        {
            this.hndl = hndl;
            this.parent = parent;
        }

        public void SetResidency(Residency residency, AccessMode m)
        {
            if (residency == Residency.Resident) GL.Arb.MakeImageHandleResident(hndl, (ArbBindlessTexture)m);
            else GL.Arb.MakeImageHandleNonResident(hndl);
        }

        public static implicit operator long(ImageHandle handle)
        {
            return handle.hndl;
        }
    }

    public class TextureHandle
    {
        internal long hndl = 0;
        internal Texture parent;
        private ComputeImage computeTex;
        private bool isResident = false;

        public TextureSampler Sampler { get; private set; }

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

        internal TextureHandle(long hndl, Texture parent, TextureSampler sampler)
        {
            this.hndl = hndl;
            this.parent = parent;
            this.Sampler = sampler;
        }

        public void SetResidency(Residency residency)
        {
            if (residency == Residency.Resident && !isResident)
            {
                GL.Arb.MakeTextureHandleResident(hndl);
                isResident = true;
            }
            else if (residency == Residency.NonResident && isResident)
            {
                GL.Arb.MakeTextureHandleNonResident(hndl);
                isResident = false;
            }
        }

        public static implicit operator long(TextureHandle handle)
        {
            return handle.hndl;
        }
    }

    public class Texture : IDisposable
    {
        public static Texture Default { get; private set; }

        static Texture()
        {
            GL.GetFloat((GetPName)OpenTK.Graphics.OpenGL.ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out float a);
            MaxAnisotropy = a;

            Default = new Texture();
            var src = new System.Drawing.Bitmap(256, 256);
            var g = System.Drawing.Graphics.FromImage(src);
            g.FillRectangle(System.Drawing.Brushes.BlueViolet, 0, 0, 256, 256);
            g.DrawRectangle(System.Drawing.Pens.Black, 0, 0, 256, 256);

            for (int x = 0; x < 256; x += 16)
                g.DrawLine(System.Drawing.Pens.Green, x, 0, x, 256);

            for (int x = 0; x < 256; x += 16)
                g.DrawLine(System.Drawing.Pens.Red, 0, x, 256, x);

            BitmapTextureSource s = new BitmapTextureSource(src, 1);
            Default.SetData(s, 0);

        }



        internal int id;
        internal PixelInternalFormat internalformat;
        internal TextureTarget texTarget;
        internal PixelFormat format;

        private int _writeLevel;
        private int _baseReadLevel;
        private int _maxReadLevel;

        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int Depth { get; internal set; }
        public int LevelCount { get; internal set; }

        public bool GenerateMipmaps { get; set; }

        public int WriteLevel { get; set; }

        //TODO: these can't be changed once gethandle has been called
        public int BaseReadLevel
        {
            get { return _baseReadLevel; }
            set { if (_baseReadLevel != value) { _baseReadLevel = value; GL.TextureParameter(id, TextureParameterName.TextureBaseLevel, (float)_baseReadLevel); } }
        }
        public int MaxReadLevel
        {
            get { return _maxReadLevel; }
            set { if (_maxReadLevel != value) { _maxReadLevel = value; GL.TextureParameter(id, TextureParameterName.TextureMaxLevel, (float)_maxReadLevel); } }
        }

        public static float MaxAnisotropy { get; internal set; }

        private Dictionary<int, TextureHandle> handles;


        public Texture()
        {
            id = 0;
            handles = new Dictionary<int, TextureHandle>();
            GraphicsDevice.Cleanup.Add(Dispose);
        }

        public TextureHandle GetHandle(TextureSampler sampler)
        {
            if (!handles.ContainsKey(sampler.id))
            {
                if (sampler.id != 0)
                    handles[sampler.id] = new TextureHandle(GL.Arb.GetTextureSamplerHandle(id, sampler.id), this, sampler);
                else
                    handles[0] = new TextureHandle(GL.Arb.GetTextureHandle(id), this, sampler);
            }

            return handles[sampler.id];
        }

        public ImageHandle GetImageHandle(int level, int layer, Kokoro.Engine.Graphics.PixelInternalFormat iFormat)
        {
            long hndl = 0;
            if (layer < 0)
                hndl = GL.Arb.GetImageHandle(id, level, true, 0, (ArbBindlessTexture)iFormat);
            else
                hndl = GL.Arb.GetImageHandle(id, level, false, layer, (ArbBindlessTexture)iFormat);
            return new ImageHandle(hndl, this);
        }

        public virtual void SetData(ITextureSource src, int level)
        {
            bool inited = false;
            if (id == 0)
            {
                GL.CreateTextures((OpenTK.Graphics.OpenGL.TextureTarget)src.GetTextureTarget(), 1, out id);
                inited = true;

                this.Width = src.GetWidth();
                this.Height = src.GetHeight();
                this.Depth = src.GetDepth();
                this.LevelCount = src.GetLevels();

                this.format = src.GetFormat();
                this.internalformat = src.GetInternalFormat();
                this.texTarget = src.GetTextureTarget();
            }

            if (src.GetTextureTarget() != TextureTarget.TextureBuffer)
            {
                IntPtr ptr = src.GetPixelData(level);
                switch (src.GetDimensions())
                {
                    case 1:
                        if (inited) GL.TextureStorage1D(id, LevelCount, (SizedInternalFormat)internalformat, Width);
                        if (ptr != IntPtr.Zero)
                        {
                            switch (internalformat)
                            {
                                case PixelInternalFormat.CompressedRedRgtc1:    //BC4
                                case PixelInternalFormat.CompressedRgRgtc2:     //BC5
                                case PixelInternalFormat.CompressedRgbaBptcUnorm:    //BC7
                                    {
                                        int blockSize = (internalformat == PixelInternalFormat.CompressedRedRgtc1) ? 8 : 16;
                                        int size = ((src.GetWidth() >> level + 3) / 4) * blockSize;
                                        GL.CompressedTextureSubImage1D(id, level, src.GetBaseWidth() >> level, src.GetWidth() >> level, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), size, ptr);
                                    }
                                    break;
                                default:
                                    GL.TextureSubImage1D(id, level, src.GetBaseWidth() >> level, src.GetWidth() >> level, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetPixelType(), ptr);
                                    break;
                            }
                        }
                        break;
                    case 2:
                        if (inited) GL.TextureStorage2D(id, LevelCount, (SizedInternalFormat)internalformat, Width, Height);
                        if (ptr != IntPtr.Zero)
                        {
                            switch (internalformat)
                            {
                                case PixelInternalFormat.CompressedRedRgtc1:    //BC4
                                case PixelInternalFormat.CompressedRgRgtc2:     //BC5
                                case PixelInternalFormat.CompressedRgbaBptcUnorm:    //BC7
                                    {
                                        int blockSize = (internalformat == PixelInternalFormat.CompressedRedRgtc1) ? 8 : 16;
                                        int size = ((src.GetWidth() >> level + 3) / 4) * ((src.GetHeight() >> level + 3) / 4) * blockSize;
                                        GL.CompressedTextureSubImage2D(id, level, src.GetBaseWidth() >> level, src.GetBaseHeight() >> level, src.GetWidth() >> level, src.GetHeight() >> level, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), size, ptr);
                                    }
                                    break;
                                default:
                                    GL.TextureSubImage2D(id, level, src.GetBaseWidth() >> level, src.GetBaseHeight() >> level, src.GetWidth() >> level, src.GetHeight() >> level, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetPixelType(), ptr);
                                    break;
                            }
                        }
                        break;
                    case 3:
                        if (inited) GL.TextureStorage3D(id, LevelCount, (SizedInternalFormat)internalformat, Width, Height, Depth);
                        if (ptr != IntPtr.Zero)
                        {

                            switch (internalformat)
                            {
                                case PixelInternalFormat.CompressedRedRgtc1:    //BC4
                                case PixelInternalFormat.CompressedRgRgtc2:     //BC5
                                case PixelInternalFormat.CompressedRgbaBptcUnorm:    //BC7
                                    {
                                        int blockSize = (internalformat == PixelInternalFormat.CompressedRedRgtc1) ? 8 : 16;
                                        int size = ((src.GetWidth() >> level + 3) / 4) * ((src.GetHeight() >> level + 3) / 4) * src.GetDepth() >> level * blockSize;
                                        GL.CompressedTextureSubImage3D(id, level, src.GetBaseWidth() >> level, src.GetBaseHeight() >> level, src.GetBaseDepth() >> level, src.GetWidth() >> level, src.GetHeight() >> level, src.GetDepth() >> level, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), size, ptr);
                                    }
                                    break;
                                default:
                                    GL.TextureSubImage3D(id, level, src.GetBaseWidth() >> level, src.GetBaseHeight() >> level, src.GetBaseDepth() >> level, src.GetWidth() >> level, src.GetHeight() >> level, src.GetDepth() >> level, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetPixelType(), ptr);
                                    break;
                            }
                        }
                        break;
                }
            }
            else
            {
                GL.TextureBufferRange(id, (SizedInternalFormat)src.GetInternalFormat(), (int)src.GetPixelData(level), (IntPtr)src.GetBaseWidth(), src.GetWidth());
            }


            if (GenerateMipmaps)
            {
                GL.GenerateTextureMipmap(id);
            }
        }

        public void SetTileMode(bool tileX, bool tileY)
        {
            if (texTarget == TextureTarget.TextureBuffer) return;
            if (handles.ContainsKey(0)) handles.Remove(0); //Force regeneration of the handle next time it's accessed
            GL.TextureParameter(id, TextureParameterName.TextureWrapS, tileX ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
            GL.TextureParameter(id, TextureParameterName.TextureWrapT, tileY ? (int)TextureWrapMode.Repeat : (int)TextureWrapMode.ClampToEdge);
        }

        public void SetEnableLinearFilter(bool linear)
        {
            if (texTarget == TextureTarget.TextureBuffer) return;
            if (handles.ContainsKey(0)) handles.Remove(0); //Force regeneration of the handle next time it's accessed
            GL.TextureParameter(id, TextureParameterName.TextureMagFilter, linear ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            GL.TextureParameter(id, TextureParameterName.TextureMinFilter, linear ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);
        }

        public void SetAnisotropicFilter(float taps)
        {
            if (texTarget == TextureTarget.TextureBuffer) return;
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

                if (id != 0) GraphicsDevice.QueueForDeletion(id, GLObjectType.Texture);
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