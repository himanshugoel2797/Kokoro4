#if OPENGL
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{

    #region License Notification
    /*
    
The Open Toolkit library license

Copyright (c) 2006 - 2014 Stefanos Apostolopoulos <stapostol@gmail.com> for the Open Toolkit library.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    */
    #endregion

    /// <summary>
    /// Used in GL.Arb.CompressedTexSubImage1D, GL.Arb.CompressedTexSubImage2D and 80 other functions
    /// </summary>
    public enum PixelFormat : int
    {
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT = 0x1403
        /// </summary>
        UnsignedShort = ((int)0x1403),
        /// <summary>
        /// Original was GL_UNSIGNED_INT = 0x1405
        /// </summary>
        UnsignedInt = ((int)0x1405),
        /// <summary>
        /// Original was GL_COLOR_INDEX = 0x1900
        /// </summary>
        ColorIndex = ((int)0x1900),
        /// <summary>
        /// Original was GL_STENCIL_INDEX = 0x1901
        /// </summary>
        StencilIndex = ((int)0x1901),
        /// <summary>
        /// Original was GL_DEPTH_COMPONENT = 0x1902
        /// </summary>
        DepthComponent = ((int)0x1902),
        /// <summary>
        /// Original was GL_RED = 0x1903
        /// </summary>
        Red = ((int)0x1903),
        /// <summary>
        /// Original was GL_RED_EXT = 0x1903
        /// </summary>
        RedExt = ((int)0x1903),
        /// <summary>
        /// Original was GL_GREEN = 0x1904
        /// </summary>
        Green = ((int)0x1904),
        /// <summary>
        /// Original was GL_BLUE = 0x1905
        /// </summary>
        Blue = ((int)0x1905),
        /// <summary>
        /// Original was GL_ALPHA = 0x1906
        /// </summary>
        Alpha = ((int)0x1906),
        /// <summary>
        /// Original was GL_RGB = 0x1907
        /// </summary>
        Rgb = ((int)0x1907),
        /// <summary>
        /// Original was GL_RGBA = 0x1908
        /// </summary>
        Rgba = ((int)0x1908),
        /// <summary>
        /// Original was GL_LUMINANCE = 0x1909
        /// </summary>
        Luminance = ((int)0x1909),
        /// <summary>
        /// Original was GL_LUMINANCE_ALPHA = 0x190A
        /// </summary>
        LuminanceAlpha = ((int)0x190A),
        /// <summary>
        /// Original was GL_ABGR_EXT = 0x8000
        /// </summary>
        AbgrExt = ((int)0x8000),
        /// <summary>
        /// Original was GL_CMYK_EXT = 0x800C
        /// </summary>
        CmykExt = ((int)0x800C),
        /// <summary>
        /// Original was GL_CMYKA_EXT = 0x800D
        /// </summary>
        CmykaExt = ((int)0x800D),
        /// <summary>
        /// Original was GL_BGR = 0x80E0
        /// </summary>
        Bgr = ((int)0x80E0),
        /// <summary>
        /// Original was GL_BGRA = 0x80E1
        /// </summary>
        Bgra = ((int)0x80E1),
        /// <summary>
        /// Original was GL_YCRCB_422_SGIX = 0x81BB
        /// </summary>
        Ycrcb422Sgix = ((int)0x81BB),
        /// <summary>
        /// Original was GL_YCRCB_444_SGIX = 0x81BC
        /// </summary>
        Ycrcb444Sgix = ((int)0x81BC),
        /// <summary>
        /// Original was GL_RG = 0x8227
        /// </summary>
        Rg = ((int)0x8227),
        /// <summary>
        /// Original was GL_RG_INTEGER = 0x8228
        /// </summary>
        RgInteger = ((int)0x8228),
        /// <summary>
        /// Original was GL_R5_G6_B5_ICC_SGIX = 0x8466
        /// </summary>
        R5G6B5IccSgix = ((int)0x8466),
        /// <summary>
        /// Original was GL_R5_G6_B5_A8_ICC_SGIX = 0x8467
        /// </summary>
        R5G6B5A8IccSgix = ((int)0x8467),
        /// <summary>
        /// Original was GL_ALPHA16_ICC_SGIX = 0x8468
        /// </summary>
        Alpha16IccSgix = ((int)0x8468),
        /// <summary>
        /// Original was GL_LUMINANCE16_ICC_SGIX = 0x8469
        /// </summary>
        Luminance16IccSgix = ((int)0x8469),
        /// <summary>
        /// Original was GL_LUMINANCE16_ALPHA8_ICC_SGIX = 0x846B
        /// </summary>
        Luminance16Alpha8IccSgix = ((int)0x846B),
        /// <summary>
        /// Original was GL_DEPTH_STENCIL = 0x84F9
        /// </summary>
        DepthStencil = ((int)0x84F9),
        /// <summary>
        /// Original was GL_RED_INTEGER = 0x8D94
        /// </summary>
        RedInteger = ((int)0x8D94),
        /// <summary>
        /// Original was GL_GREEN_INTEGER = 0x8D95
        /// </summary>
        GreenInteger = ((int)0x8D95),
        /// <summary>
        /// Original was GL_BLUE_INTEGER = 0x8D96
        /// </summary>
        BlueInteger = ((int)0x8D96),
        /// <summary>
        /// Original was GL_ALPHA_INTEGER = 0x8D97
        /// </summary>
        AlphaInteger = ((int)0x8D97),
        /// <summary>
        /// Original was GL_RGB_INTEGER = 0x8D98
        /// </summary>
        RgbInteger = ((int)0x8D98),
        /// <summary>
        /// Original was GL_RGBA_INTEGER = 0x8D99
        /// </summary>
        RgbaInteger = ((int)0x8D99),
        /// <summary>
        /// Original was GL_BGR_INTEGER = 0x8D9A
        /// </summary>
        BgrInteger = ((int)0x8D9A),
        /// <summary>
        /// Original was GL_BGRA_INTEGER = 0x8D9B
        /// </summary>
        BgraInteger = ((int)0x8D9B),
    }

}
#endif