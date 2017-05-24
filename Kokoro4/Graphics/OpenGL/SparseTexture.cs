using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace Kokoro.Engine.Graphics
{
    public class SparseTexture : Texture
    {
        public int TileX { get; internal set; }
        public int TileY { get; internal set; }
        public int TileZ { get; internal set; }

        public SparseTexture() : base()
        {

        }

        public override void SetData(ITextureSource src, int level)
        {
            bool inited = false;
            if (id == 0)
            {
                GL.CreateTextures((OpenTK.Graphics.OpenGL.TextureTarget)src.GetTextureTarget(), 1, out id);
                inited = true;

                int isSparse = 1;
                GL.TexParameterI((OpenTK.Graphics.OpenGL.TextureTarget)src.GetTextureTarget(), (TextureParameterName)All.TextureSparseArb, ref isSparse);
            }
            
            switch (src.GetDimensions())
            {
                case 1:
                    if (inited) GL.TextureStorage1D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth());
                    break;
                case 2:
                    if (inited) GL.TextureStorage2D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth(), src.GetHeight());
                    break;
                case 3:
                    if (inited) GL.TextureStorage3D(id, src.GetLevels(), (SizedInternalFormat)src.GetInternalFormat(), src.GetWidth(), src.GetHeight(), src.GetDepth());
                    break;
            }

            int tX, tY, tZ;
            GL.GetInternalformat((ImageTarget)src.GetTextureTarget(), (SizedInternalFormat)src.GetInternalFormat(), (InternalFormatParameter)All.VirtualPageSizeXArb, 1, out tX);
            GL.GetInternalformat((ImageTarget)src.GetTextureTarget(), (SizedInternalFormat)src.GetInternalFormat(), (InternalFormatParameter)All.VirtualPageSizeYArb, 1, out tY);
            GL.GetInternalformat((ImageTarget)src.GetTextureTarget(), (SizedInternalFormat)src.GetInternalFormat(), (InternalFormatParameter)All.VirtualPageSizeZArb, 1, out tZ);

            TileX = tX;
            TileY = tY;
            TileZ = tZ;

            this.texTarget = src.GetTextureTarget();
            this.internalformat = src.GetInternalFormat();
            this.format = src.GetFormat();
        }

        public void Commit(ITextureSource src, int x, int y, int z, int level)
        {
            if (src.GetTextureTarget() != texTarget)
                throw new ArgumentException("TextureTargets must match.");

            if (src.GetInternalFormat() != internalformat)
                throw new ArgumentException("InternalFormats must match.");

            if (src.GetFormat() != format)
                throw new ArgumentException("Formats must match.");

            GL.Arb.TexPageCommitment((ArbSparseTexture)texTarget, level, x, y, z, src.GetWidth(), src.GetHeight(), src.GetDepth(), true);

            switch (src.GetDimensions())
            {
                case 1:
                    GL.TextureSubImage1D(id, level, 0, src.GetWidth(), (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetType(), src.GetPixelData(level));
                    break;
                case 2:
                    GL.TextureSubImage2D(id, level, 0, 0, src.GetWidth(), src.GetHeight(), (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetType(), src.GetPixelData(level));
                    break;
                case 3:
                    GL.TextureSubImage3D(id, level, 0, 0, 0, src.GetWidth(), src.GetHeight(), src.GetDepth(), (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetType(), src.GetPixelData(level));
                    break;
            }
        }

        public void Discard(int x, int y, int z, int w, int h, int d, int level)
        {
            GL.Arb.TexPageCommitment((ArbSparseTexture)texTarget, level, x, y, z, w, h, d, false);
        }
    }
}
