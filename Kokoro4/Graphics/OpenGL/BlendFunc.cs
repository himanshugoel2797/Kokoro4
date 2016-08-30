using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Kokoro.Engine.Graphics
{
    public enum BlendFactor
    {
        One = BlendingFactorSrc.One,
        Zero = BlendingFactorSrc.Zero,
        SrcAlpha = BlendingFactorSrc.SrcAlpha,
        OneMinusSrcAlpha = BlendingFactorSrc.OneMinusSrcAlpha,
        DstAlpha = BlendingFactorSrc.DstAlpha,
        OneMinusDstAlpha = BlendingFactorSrc.OneMinusDstAlpha,
    }

    public enum BlendFunction
    {
        None = AlphaFunction.Never,
        LEqual = AlphaFunction.Lequal,
        GEqual = AlphaFunction.Gequal,
        Less = AlphaFunction.Less,
        Greater = AlphaFunction.Greater,
        Equal = AlphaFunction.Equal,
        Always = AlphaFunction.Always,
    }
}
