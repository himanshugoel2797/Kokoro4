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
    /// Used in GL.Arb.ReadnPixels, GL.ClearTexImage and 61 other functions
    /// </summary>
    public enum PixelType : int
    {
        /// <summary>
        /// Original was GL_BYTE = 0x1400
        /// </summary>
        Byte = ((int)0x1400),
        /// <summary>
        /// Original was GL_UNSIGNED_BYTE = 0x1401
        /// </summary>
        UnsignedByte = ((int)0x1401),
        /// <summary>
        /// Original was GL_SHORT = 0x1402
        /// </summary>
        Short = ((int)0x1402),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT = 0x1403
        /// </summary>
        UnsignedShort = ((int)0x1403),
        /// <summary>
        /// Original was GL_INT = 0x1404
        /// </summary>
        Int = ((int)0x1404),
        /// <summary>
        /// Original was GL_UNSIGNED_INT = 0x1405
        /// </summary>
        UnsignedInt = ((int)0x1405),
        /// <summary>
        /// Original was GL_FLOAT = 0x1406
        /// </summary>
        Float = ((int)0x1406),
        /// <summary>
        /// Original was GL_HALF_FLOAT = 0x140B
        /// </summary>
        HalfFloat = ((int)0x140B),
        /// <summary>
        /// Original was GL_BITMAP = 0x1A00
        /// </summary>
        Bitmap = ((int)0x1A00),
        /// <summary>
        /// Original was GL_UNSIGNED_BYTE_3_3_2 = 0x8032
        /// </summary>
        UnsignedByte332 = ((int)0x8032),
        /// <summary>
        /// Original was GL_UNSIGNED_BYTE_3_3_2_EXT = 0x8032
        /// </summary>
        UnsignedByte332Ext = ((int)0x8032),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033
        /// </summary>
        UnsignedShort4444 = ((int)0x8033),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT_4_4_4_4_EXT = 0x8033
        /// </summary>
        UnsignedShort4444Ext = ((int)0x8033),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034
        /// </summary>
        UnsignedShort5551 = ((int)0x8034),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT_5_5_5_1_EXT = 0x8034
        /// </summary>
        UnsignedShort5551Ext = ((int)0x8034),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_8_8_8_8 = 0x8035
        /// </summary>
        UnsignedInt8888 = ((int)0x8035),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_8_8_8_8_EXT = 0x8035
        /// </summary>
        UnsignedInt8888Ext = ((int)0x8035),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_10_10_10_2 = 0x8036
        /// </summary>
        UnsignedInt1010102 = ((int)0x8036),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_10_10_10_2_EXT = 0x8036
        /// </summary>
        UnsignedInt1010102Ext = ((int)0x8036),
        /// <summary>
        /// Original was GL_UNSIGNED_BYTE_2_3_3_REVERSED = 0x8362
        /// </summary>
        UnsignedByte233Reversed = ((int)0x8362),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT_5_6_5 = 0x8363
        /// </summary>
        UnsignedShort565 = ((int)0x8363),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT_5_6_5_REVERSED = 0x8364
        /// </summary>
        UnsignedShort565Reversed = ((int)0x8364),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT_4_4_4_4_REVERSED = 0x8365
        /// </summary>
        UnsignedShort4444Reversed = ((int)0x8365),
        /// <summary>
        /// Original was GL_UNSIGNED_SHORT_1_5_5_5_REVERSED = 0x8366
        /// </summary>
        UnsignedShort1555Reversed = ((int)0x8366),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_8_8_8_8_REVERSED = 0x8367
        /// </summary>
        UnsignedInt8888Reversed = ((int)0x8367),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_2_10_10_10_REVERSED = 0x8368
        /// </summary>
        UnsignedInt2101010Reversed = ((int)0x8368),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_24_8 = 0x84FA
        /// </summary>
        UnsignedInt248 = ((int)0x84FA),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_10F_11F_11F_REV = 0x8C3B
        /// </summary>
        UnsignedInt10F11F11FRev = ((int)0x8C3B),
        /// <summary>
        /// Original was GL_UNSIGNED_INT_5_9_9_9_REV = 0x8C3E
        /// </summary>
        UnsignedInt5999Rev = ((int)0x8C3E),
        /// <summary>
        /// Original was GL_FLOAT_32_UNSIGNED_INT_24_8_REV = 0x8DAD
        /// </summary>
        Float32UnsignedInt248Rev = ((int)0x8DAD),
    }

}
#endif