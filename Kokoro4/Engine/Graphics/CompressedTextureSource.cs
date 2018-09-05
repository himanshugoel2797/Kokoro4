using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace Kokoro.Engine.Graphics
{
    public class CompressedTextureSource : RawTextureSource, IDisposable
    {
        private enum DXGI_FORMAT
        {
            DXGI_FORMAT_UNKNOWN,
            DXGI_FORMAT_R32G32B32A32_TYPELESS,
            DXGI_FORMAT_R32G32B32A32_FLOAT,
            DXGI_FORMAT_R32G32B32A32_UINT,
            DXGI_FORMAT_R32G32B32A32_SINT,
            DXGI_FORMAT_R32G32B32_TYPELESS,
            DXGI_FORMAT_R32G32B32_FLOAT,
            DXGI_FORMAT_R32G32B32_UINT,
            DXGI_FORMAT_R32G32B32_SINT,
            DXGI_FORMAT_R16G16B16A16_TYPELESS,
            DXGI_FORMAT_R16G16B16A16_FLOAT,
            DXGI_FORMAT_R16G16B16A16_UNORM,
            DXGI_FORMAT_R16G16B16A16_UINT,
            DXGI_FORMAT_R16G16B16A16_SNORM,
            DXGI_FORMAT_R16G16B16A16_SINT,
            DXGI_FORMAT_R32G32_TYPELESS,
            DXGI_FORMAT_R32G32_FLOAT,
            DXGI_FORMAT_R32G32_UINT,
            DXGI_FORMAT_R32G32_SINT,
            DXGI_FORMAT_R32G8X24_TYPELESS,
            DXGI_FORMAT_D32_FLOAT_S8X24_UINT,
            DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS,
            DXGI_FORMAT_X32_TYPELESS_G8X24_UINT,
            DXGI_FORMAT_R10G10B10A2_TYPELESS,
            DXGI_FORMAT_R10G10B10A2_UNORM,
            DXGI_FORMAT_R10G10B10A2_UINT,
            DXGI_FORMAT_R11G11B10_FLOAT,
            DXGI_FORMAT_R8G8B8A8_TYPELESS,
            DXGI_FORMAT_R8G8B8A8_UNORM,
            DXGI_FORMAT_R8G8B8A8_UNORM_SRGB,
            DXGI_FORMAT_R8G8B8A8_UINT,
            DXGI_FORMAT_R8G8B8A8_SNORM,
            DXGI_FORMAT_R8G8B8A8_SINT,
            DXGI_FORMAT_R16G16_TYPELESS,
            DXGI_FORMAT_R16G16_FLOAT,
            DXGI_FORMAT_R16G16_UNORM,
            DXGI_FORMAT_R16G16_UINT,
            DXGI_FORMAT_R16G16_SNORM,
            DXGI_FORMAT_R16G16_SINT,
            DXGI_FORMAT_R32_TYPELESS,
            DXGI_FORMAT_D32_FLOAT,
            DXGI_FORMAT_R32_FLOAT,
            DXGI_FORMAT_R32_UINT,
            DXGI_FORMAT_R32_SINT,
            DXGI_FORMAT_R24G8_TYPELESS,
            DXGI_FORMAT_D24_UNORM_S8_UINT,
            DXGI_FORMAT_R24_UNORM_X8_TYPELESS,
            DXGI_FORMAT_X24_TYPELESS_G8_UINT,
            DXGI_FORMAT_R8G8_TYPELESS,
            DXGI_FORMAT_R8G8_UNORM,
            DXGI_FORMAT_R8G8_UINT,
            DXGI_FORMAT_R8G8_SNORM,
            DXGI_FORMAT_R8G8_SINT,
            DXGI_FORMAT_R16_TYPELESS,
            DXGI_FORMAT_R16_FLOAT,
            DXGI_FORMAT_D16_UNORM,
            DXGI_FORMAT_R16_UNORM,
            DXGI_FORMAT_R16_UINT,
            DXGI_FORMAT_R16_SNORM,
            DXGI_FORMAT_R16_SINT,
            DXGI_FORMAT_R8_TYPELESS,
            DXGI_FORMAT_R8_UNORM,
            DXGI_FORMAT_R8_UINT,
            DXGI_FORMAT_R8_SNORM,
            DXGI_FORMAT_R8_SINT,
            DXGI_FORMAT_A8_UNORM,
            DXGI_FORMAT_R1_UNORM,
            DXGI_FORMAT_R9G9B9E5_SHAREDEXP,
            DXGI_FORMAT_R8G8_B8G8_UNORM,
            DXGI_FORMAT_G8R8_G8B8_UNORM,
            DXGI_FORMAT_BC1_TYPELESS,
            DXGI_FORMAT_BC1_UNORM,
            DXGI_FORMAT_BC1_UNORM_SRGB,
            DXGI_FORMAT_BC2_TYPELESS,
            DXGI_FORMAT_BC2_UNORM,
            DXGI_FORMAT_BC2_UNORM_SRGB,
            DXGI_FORMAT_BC3_TYPELESS,
            DXGI_FORMAT_BC3_UNORM,
            DXGI_FORMAT_BC3_UNORM_SRGB,
            DXGI_FORMAT_BC4_TYPELESS,
            DXGI_FORMAT_BC4_UNORM,
            DXGI_FORMAT_BC4_SNORM,
            DXGI_FORMAT_BC5_TYPELESS,
            DXGI_FORMAT_BC5_UNORM,
            DXGI_FORMAT_BC5_SNORM,
            DXGI_FORMAT_B5G6R5_UNORM,
            DXGI_FORMAT_B5G5R5A1_UNORM,
            DXGI_FORMAT_B8G8R8A8_UNORM,
            DXGI_FORMAT_B8G8R8X8_UNORM,
            DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM,
            DXGI_FORMAT_B8G8R8A8_TYPELESS,
            DXGI_FORMAT_B8G8R8A8_UNORM_SRGB,
            DXGI_FORMAT_B8G8R8X8_TYPELESS,
            DXGI_FORMAT_B8G8R8X8_UNORM_SRGB,
            DXGI_FORMAT_BC6H_TYPELESS,
            DXGI_FORMAT_BC6H_UF16,
            DXGI_FORMAT_BC6H_SF16,
            DXGI_FORMAT_BC7_TYPELESS,
            DXGI_FORMAT_BC7_UNORM,
            DXGI_FORMAT_BC7_UNORM_SRGB,
            DXGI_FORMAT_AYUV,
            DXGI_FORMAT_Y410,
            DXGI_FORMAT_Y416,
            DXGI_FORMAT_NV12,
            DXGI_FORMAT_P010,
            DXGI_FORMAT_P016,
            DXGI_FORMAT_420_OPAQUE,
            DXGI_FORMAT_YUY2,
            DXGI_FORMAT_Y210,
            DXGI_FORMAT_Y216,
            DXGI_FORMAT_NV11,
            DXGI_FORMAT_AI44,
            DXGI_FORMAT_IA44,
            DXGI_FORMAT_P8,
            DXGI_FORMAT_A8P8,
            DXGI_FORMAT_B4G4R4A4_UNORM,
            DXGI_FORMAT_P208,
            DXGI_FORMAT_V208,
            DXGI_FORMAT_V408,
            DXGI_FORMAT_FORCE_UINT
        };
        private enum D3D10_RESOURCE_DIMENSION
        {
            D3D10_RESOURCE_DIMENSION_UNKNOWN,
            D3D10_RESOURCE_DIMENSION_BUFFER,
            D3D10_RESOURCE_DIMENSION_TEXTURE1D,
            D3D10_RESOURCE_DIMENSION_TEXTURE2D,
            D3D10_RESOURCE_DIMENSION_TEXTURE3D
        };

        public static CompressedTextureSource Create(string file)
        {
            PixelInternalFormat iFmt;
            int dimCnt = 2;
            int depth = 1;
            int width = 0;
            int height = 0;
            int linSz = 0;
            int mipCnt = 0;
            int bufSz = 0;
            byte[] imgData = null;

            using (FileStream f = File.OpenRead(file))
            using (BinaryReader binR = new BinaryReader(f))
            {
                var magic = binR.ReadChars(4);
                if (!magic.SequenceEqual(new char[] { 'D', 'D', 'S', ' ' }))
                    throw new ArgumentException();

                binR.ReadInt64();   //Skip 8 bytes
                height = binR.ReadInt32();
                width = binR.ReadInt32();
                linSz = binR.ReadInt32();
                binR.ReadInt64();
                mipCnt = binR.ReadInt32() + 1;
                binR.ReadBytes(80 - 32);
                string fourCC = new string(binR.ReadChars(4));
                binR.ReadBytes(124 - 84);

                switch (fourCC)
                {
                    case "ATI1":    //BC4
                        iFmt = PixelInternalFormat.CompressedRedRgtc1;
                        break;
                    case "ATI2":    //BC5
                        iFmt = PixelInternalFormat.CompressedRgRgtc2;
                        break;
                    case "DX10":    //BC7
                        {
                            var dxgi_fmt = (DXGI_FORMAT)binR.ReadUInt32();
                            var dxgi_dims = (D3D10_RESOURCE_DIMENSION)binR.ReadUInt32();
                            uint misc = binR.ReadUInt32();
                            uint arrSz = binR.ReadUInt32();
                            uint misc2 = binR.ReadUInt32();

                            depth = (int)arrSz;
                            switch (dxgi_dims)
                            {
                                case D3D10_RESOURCE_DIMENSION.D3D10_RESOURCE_DIMENSION_TEXTURE1D:
                                    dimCnt = 1;
                                    break;
                                case D3D10_RESOURCE_DIMENSION.D3D10_RESOURCE_DIMENSION_TEXTURE2D:
                                    dimCnt = 2;
                                    break;
                                case D3D10_RESOURCE_DIMENSION.D3D10_RESOURCE_DIMENSION_TEXTURE3D:
                                    dimCnt = 3;
                                    break;
                            }

                            switch (dxgi_fmt)
                            {
                                case DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM:
                                    iFmt = PixelInternalFormat.CompressedRgbaBptcUnorm;
                                    break;
                                case DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM:
                                    iFmt = PixelInternalFormat.CompressedRgRgtc2;
                                    break;
                                case DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM:
                                    iFmt = PixelInternalFormat.CompressedRedRgtc1;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }

                bufSz = mipCnt > 1 ? linSz * 2 : linSz;
                imgData = binR.ReadBytes(bufSz);
            }

            IntPtr data = Marshal.AllocHGlobal(bufSz);
            Marshal.Copy(imgData, 0, data, imgData.Length);

            return new CompressedTextureSource(dimCnt, width, height, depth, mipCnt, PixelFormat.Rgba, iFmt, TextureTarget.Texture2D, PixelType.UnsignedByte, data);
        }

        private IntPtr pixelData;
        private CompressedTextureSource(int dim, int width, int height, int depth, int levels, PixelFormat pFormat, PixelInternalFormat iFormat, TextureTarget target, PixelType pType, IntPtr data) : base(dim, width, height, depth, levels, pFormat, iFormat, target, pType)
        {
            pixelData = data;
        }

        public override IntPtr GetPixelData(int level)
        {
            int off = 0;
            for(int i = 0; i < level; i++)
            {
                int blockSize = (GetInternalFormat() == PixelInternalFormat.CompressedRedRgtc1) ? 8 : 16;
                off += ((GetWidth() >> i + 3) / 4) * ((GetHeight() >> i + 3) / 4) * blockSize;
            }

            return pixelData + off;
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

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                Marshal.FreeHGlobal(pixelData);
                pixelData = IntPtr.Zero;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~CompressedTextureSource()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
