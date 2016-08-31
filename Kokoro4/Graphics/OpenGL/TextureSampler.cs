using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class TextureSampler
    {
        public static TextureSampler Default { get; private set; } = new TextureSampler(0);

        internal int id;

        public TextureSampler()
        {
            GL.CreateSamplers(1, out id);
        }

        internal TextureSampler(int id)
        {
            this.id = id;
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
    }
}
