using Kokoro.Graphics.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulkan;

namespace Kokoro.Engine.Graphics
{
    public enum TextureResidency
    {
        NonResident,
        Resident
    }

    public class Texture : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        internal ImageCreateInfo imgInfo;
        internal Image img;

        public Texture()
        {
            imgInfo = new ImageCreateInfo();
        }

        public TextureView CreateTextureView(int x, int y, int z, int width, int height, int depth)
        {
            return null;
        }

        public void SetData(ITextureSource tex, int level)
        {
            ImageType img_t = 0;
            switch (tex.GetLevels())
            {
                case 1:
                    img_t = ImageType.Image1D;
                    break;
                case 2:
                    img_t = ImageType.Image2D;
                    break;
                case 3:
                    img_t = ImageType.Image3D;
                    break;
            }

            ImageCreateInfo transImgInfo = new ImageCreateInfo();
            transImgInfo.ImageType = img_t;
            transImgInfo.Extent = new Extent3D() { Width = (uint)tex.GetWidth(), Height = (uint)tex.GetHeight(), Depth = (uint)tex.GetDepth() };
            transImgInfo.MipLevels = (uint)tex.GetLevels();
            transImgInfo.ArrayLayers = (uint)tex.GetLayersCount();
            transImgInfo.Format = (Format)tex.GetFormat();
            transImgInfo.Tiling = (ImageTiling)tex.GetTilingMode();
            transImgInfo.InitialLayout = ImageLayout.Preinitialized;
            transImgInfo.Usage = ImageUsageFlags.TransferSrc;
            transImgInfo.Samples = SampleCountFlags.Count1;
            transImgInfo.SharingMode = SharingMode.Concurrent;
            transImgInfo.QueueFamilyIndices = GraphicsDevice.GetQueueFamilyIndices();
            transImgInfo.QueueFamilyIndexCount = (uint)transImgInfo.QueueFamilyIndices.Length;

            Image trans_img = GraphicsDevice.CreateImage(transImgInfo);


            imgInfo.ImageType = img_t;
            imgInfo.Extent = new Extent3D() { Width = (uint)tex.GetWidth(), Height = (uint)tex.GetHeight(), Depth = (uint)tex.GetDepth() };
            imgInfo.MipLevels = (uint)tex.GetLevels();
            imgInfo.ArrayLayers = (uint)tex.GetLayersCount();
            imgInfo.Format = (Format)tex.GetInternalFormat();
            imgInfo.Tiling = (ImageTiling)tex.GetTilingMode();
            imgInfo.InitialLayout = ImageLayout.Preinitialized;
            imgInfo.Usage = ImageUsageFlags.TransferDst;
            imgInfo.Samples = SampleCountFlags.Count1;
            imgInfo.SharingMode = SharingMode.Concurrent;
            imgInfo.QueueFamilyIndices = GraphicsDevice.GetQueueFamilyIndices();
            imgInfo.QueueFamilyIndexCount = (uint)imgInfo.QueueFamilyIndices.Length;

            img = GraphicsDevice.CreateImage(imgInfo);


        }

        public void SetResidency(TextureResidency res)
        {

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

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Texture() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
