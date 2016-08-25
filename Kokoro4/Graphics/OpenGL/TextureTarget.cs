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
    /// Source: OpenTK
    /// Used in GL.Arb.CompressedTexImage1D, GL.Arb.CompressedTexImage2D and 124 other functions
    /// </summary>
    public enum TextureTarget : int
    {
        /// <summary>
        /// Original was GL_TEXTURE_1D = 0x0DE0
        /// </summary>
        Texture1D = ((int)0x0DE0),
        /// <summary>
        /// Original was GL_TEXTURE_2D = 0x0DE1
        /// </summary>
        Texture2D = ((int)0x0DE1),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_1D = 0x8063
        /// </summary>
        ProxyTexture1D = ((int)0x8063),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_1D_EXT = 0x8063
        /// </summary>
        ProxyTexture1DExt = ((int)0x8063),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_2D = 0x8064
        /// </summary>
        ProxyTexture2D = ((int)0x8064),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_2D_EXT = 0x8064
        /// </summary>
        ProxyTexture2DExt = ((int)0x8064),
        /// <summary>
        /// Original was GL_TEXTURE_3D = 0x806F
        /// </summary>
        Texture3D = ((int)0x806F),
        /// <summary>
        /// Original was GL_TEXTURE_3D_EXT = 0x806F
        /// </summary>
        Texture3DExt = ((int)0x806F),
        /// <summary>
        /// Original was GL_TEXTURE_3D_OES = 0x806F
        /// </summary>
        Texture3DOes = ((int)0x806F),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_3D = 0x8070
        /// </summary>
        ProxyTexture3D = ((int)0x8070),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_3D_EXT = 0x8070
        /// </summary>
        ProxyTexture3DExt = ((int)0x8070),
        /// <summary>
        /// Original was GL_DETAIL_TEXTURE_2D_SGIS = 0x8095
        /// </summary>
        DetailTexture2DSgis = ((int)0x8095),
        /// <summary>
        /// Original was GL_TEXTURE_4D_SGIS = 0x8134
        /// </summary>
        Texture4DSgis = ((int)0x8134),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_4D_SGIS = 0x8135
        /// </summary>
        ProxyTexture4DSgis = ((int)0x8135),
        /// <summary>
        /// Original was GL_TEXTURE_MIN_LOD = 0x813A
        /// </summary>
        TextureMinLod = ((int)0x813A),
        /// <summary>
        /// Original was GL_TEXTURE_MIN_LOD_SGIS = 0x813A
        /// </summary>
        TextureMinLodSgis = ((int)0x813A),
        /// <summary>
        /// Original was GL_TEXTURE_MAX_LOD = 0x813B
        /// </summary>
        TextureMaxLod = ((int)0x813B),
        /// <summary>
        /// Original was GL_TEXTURE_MAX_LOD_SGIS = 0x813B
        /// </summary>
        TextureMaxLodSgis = ((int)0x813B),
        /// <summary>
        /// Original was GL_TEXTURE_BASE_LEVEL = 0x813C
        /// </summary>
        TextureBaseLevel = ((int)0x813C),
        /// <summary>
        /// Original was GL_TEXTURE_BASE_LEVEL_SGIS = 0x813C
        /// </summary>
        TextureBaseLevelSgis = ((int)0x813C),
        /// <summary>
        /// Original was GL_TEXTURE_MAX_LEVEL = 0x813D
        /// </summary>
        TextureMaxLevel = ((int)0x813D),
        /// <summary>
        /// Original was GL_TEXTURE_MAX_LEVEL_SGIS = 0x813D
        /// </summary>
        TextureMaxLevelSgis = ((int)0x813D),
        /// <summary>
        /// Original was GL_TEXTURE_RECTANGLE = 0x84F5
        /// </summary>
        TextureRectangle = ((int)0x84F5),
        /// <summary>
        /// Original was GL_TEXTURE_RECTANGLE_ARB = 0x84F5
        /// </summary>
        TextureRectangleArb = ((int)0x84F5),
        /// <summary>
        /// Original was GL_TEXTURE_RECTANGLE_NV = 0x84F5
        /// </summary>
        TextureRectangleNv = ((int)0x84F5),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_RECTANGLE = 0x84F7
        /// </summary>
        ProxyTextureRectangle = ((int)0x84F7),
        /// <summary>
        /// Original was GL_TEXTURE_CUBE_MAP = 0x8513
        /// </summary>
        TextureCubeMap = ((int)0x8513),
        /// <summary>
        /// Original was GL_TEXTURE_BINDING_CUBE_MAP = 0x8514
        /// </summary>
        TextureBindingCubeMap = ((int)0x8514),
        /// <summary>
        /// Original was GL_TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515
        /// </summary>
        TextureCubeMapPositiveX = ((int)0x8515),
        /// <summary>
        /// Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516
        /// </summary>
        TextureCubeMapNegativeX = ((int)0x8516),
        /// <summary>
        /// Original was GL_TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517
        /// </summary>
        TextureCubeMapPositiveY = ((int)0x8517),
        /// <summary>
        /// Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518
        /// </summary>
        TextureCubeMapNegativeY = ((int)0x8518),
        /// <summary>
        /// Original was GL_TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519
        /// </summary>
        TextureCubeMapPositiveZ = ((int)0x8519),
        /// <summary>
        /// Original was GL_TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851A
        /// </summary>
        TextureCubeMapNegativeZ = ((int)0x851A),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_CUBE_MAP = 0x851B
        /// </summary>
        ProxyTextureCubeMap = ((int)0x851B),
        /// <summary>
        /// Original was GL_TEXTURE_1D_ARRAY = 0x8C18
        /// </summary>
        Texture1DArray = ((int)0x8C18),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_1D_ARRAY = 0x8C19
        /// </summary>
        ProxyTexture1DArray = ((int)0x8C19),
        /// <summary>
        /// Original was GL_TEXTURE_2D_ARRAY = 0x8C1A
        /// </summary>
        Texture2DArray = ((int)0x8C1A),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_2D_ARRAY = 0x8C1B
        /// </summary>
        ProxyTexture2DArray = ((int)0x8C1B),
        /// <summary>
        /// Original was GL_TEXTURE_BUFFER = 0x8C2A
        /// </summary>
        TextureBuffer = ((int)0x8C2A),
        /// <summary>
        /// Original was GL_TEXTURE_CUBE_MAP_ARRAY = 0x9009
        /// </summary>
        TextureCubeMapArray = ((int)0x9009),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_CUBE_MAP_ARRAY = 0x900B
        /// </summary>
        ProxyTextureCubeMapArray = ((int)0x900B),
        /// <summary>
        /// Original was GL_TEXTURE_2D_MULTISAMPLE = 0x9100
        /// </summary>
        Texture2DMultisample = ((int)0x9100),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_2D_MULTISAMPLE = 0x9101
        /// </summary>
        ProxyTexture2DMultisample = ((int)0x9101),
        /// <summary>
        /// Original was GL_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9102
        /// </summary>
        Texture2DMultisampleArray = ((int)0x9102),
        /// <summary>
        /// Original was GL_PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9103
        /// </summary>
        ProxyTexture2DMultisampleArray = ((int)0x9103),
    }

}
#endif