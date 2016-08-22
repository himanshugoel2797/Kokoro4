using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Kokoro.Graphics
{
    public class CubeMapTextureSource : ITextureSource
    {
        public static Texture LoadCubemap(string px, string py, string pz, string nx, string ny, string nz, int mipmapLevs)
        {
            Texture t = new Texture();

            BitmapTextureSource b0 = new BitmapTextureSource(px, mipmapLevs), 
                                b1 = new BitmapTextureSource(py, mipmapLevs), 
                                b2 = new BitmapTextureSource(pz, mipmapLevs), 
                                b3 = new BitmapTextureSource(nx, mipmapLevs), 
                                b4 = new BitmapTextureSource(ny, mipmapLevs), 
                                b5 = new BitmapTextureSource(nz, mipmapLevs);

            CubeMapTextureSource c0 = new CubeMapTextureSource(CubeMapFace.PositiveX, b0), 
                                 c1 = new CubeMapTextureSource(CubeMapFace.PositiveY, b1), 
                                 c2 = new CubeMapTextureSource(CubeMapFace.PositiveZ, b2), 
                                 c3 = new CubeMapTextureSource(CubeMapFace.NegativeX, b3), 
                                 c4 = new CubeMapTextureSource(CubeMapFace.NegativeY, b4), 
                                 c5 = new CubeMapTextureSource(CubeMapFace.NegativeZ, b5);
            
            t.SetData(c0, 0);
            t.SetData(c1, 0);
            t.SetData(c2, 0);
            t.SetData(c3, 0);
            t.SetData(c4, 0);
            t.SetData(c5, 0);

            return t;
        }

        public enum CubeMapFace{
            PositiveX, PositiveY, PositiveZ,
            NegativeX, NegativeY, NegativeZ
        };

        private ITextureSource texSrc;
        private CubeMapFace curFace;

        public CubeMapTextureSource(CubeMapFace face, ITextureSource tex)
        {
            curFace = face;
            texSrc = tex;
        }

        public int GetDepth()
        {
            return 0;
        }

        public int GetDimensions()
        {
            return 2;
        }

        public PixelFormat GetFormat()
        {
            return texSrc.GetFormat();
        }

        public int GetHeight()
        {
            return texSrc.GetHeight();
        }

        public PixelInternalFormat GetInternalFormat()
        {
            return texSrc.GetInternalFormat();
        }

        public int GetLevels()
        {
            return texSrc.GetLevels();
        }

        public IntPtr GetPixelData(int level)
        {
            return texSrc.GetPixelData(level);
        }


        private int targetCallNum = 0;
        public TextureTarget GetTextureTarget()
        {
            if (targetCallNum == 5) targetCallNum = 0;
            if (targetCallNum++ != 1) return TextureTarget.TextureCubeMap;
            switch(curFace)
            {
                case CubeMapFace.NegativeX:
                    return TextureTarget.TextureCubeMapNegativeX;
                case CubeMapFace.NegativeY:
                    return TextureTarget.TextureCubeMapNegativeY;
                case CubeMapFace.NegativeZ:
                    return TextureTarget.TextureCubeMapNegativeZ;
                case CubeMapFace.PositiveX:
                    return TextureTarget.TextureCubeMapPositiveX;
                case CubeMapFace.PositiveY:
                    return TextureTarget.TextureCubeMapPositiveY;
                case CubeMapFace.PositiveZ:
                    return TextureTarget.TextureCubeMapPositiveZ;
            }
            return TextureTarget.TextureCubeMap;
        }

        public int GetWidth()
        {
            return texSrc.GetWidth();
        }

        PixelType ITextureSource.GetType()
        {
            return texSrc.GetType();
        }
    }
}
