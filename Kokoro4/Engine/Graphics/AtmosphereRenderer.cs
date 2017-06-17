using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class AtmosphereRenderer
    {
        private Vector3 rayleigh;
        private float mie;
        private float rayleighScale, mieScale, gnd, atmos;

        private Texture transmitance_cache;
        private Texture single_scattering_cache;

        private const int Transmitance_W = 256;
        private const int Transmitance_H = 128;
        private ShaderProgram Transmitance_Precalc, SingleScatter_Precalc;
        private ImageHandle TransmitanceHandle, SingleScatterHandle;

        public AtmosphereRenderer(Vector3 rayleigh, float rayleighScaleHeight, float mie, float mieScaleHeight, float gnd, float atmos)
        {
            this.rayleigh = rayleigh;
            this.mie = mie;
            this.rayleighScale = rayleighScaleHeight;
            this.mieScale = mieScaleHeight;
            this.gnd = gnd;
            this.atmos = atmos;

            int side = 64;

            //populate Transmitance in a compute shader 
            transmitance_cache = new Texture();
            RawTextureSource trans_cacheSrc = new RawTextureSource(2, Transmitance_W, Transmitance_H, 0, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture2D, PixelType.Float);
            transmitance_cache.SetData(trans_cacheSrc, 0);
            transmitance_cache.SetEnableLinearFilter(true);
            transmitance_cache.SetTileMode(false, false); 
            TransmitanceHandle = transmitance_cache.GetImageHandle(0, 0, PixelInternalFormat.Rgba16f);
            TransmitanceHandle.SetResidency(Residency.Resident, AccessMode.ReadWrite);

            single_scattering_cache = new Texture();
            RawTextureSource single_scatterCacheSrc = new RawTextureSource(3, side, side, side, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture3D, PixelType.Float);
            single_scattering_cache.SetData(single_scatterCacheSrc, 0);
            single_scattering_cache.SetEnableLinearFilter(true);
            single_scattering_cache.SetTileMode(false, false);
            SingleScatterHandle = single_scattering_cache.GetImageHandle(0, -1, PixelInternalFormat.Rgba16f);
            SingleScatterHandle.SetResidency(Residency.Resident, AccessMode.Write);

            #region Calculate Transmittance
            ShaderSource trans_compute = ShaderSource.Load(ShaderType.ComputeShader, "Graphics/OpenGL/Shaders/Atmosphere/transmittance.glsl");
            Transmitance_Precalc = new ShaderProgram(trans_compute); 


            Transmitance_Precalc.Set("TransCache", TransmitanceHandle);
            Transmitance_Precalc.Set("Rayleigh", rayleigh); 
            Transmitance_Precalc.Set("Mie", mie);
            Transmitance_Precalc.Set("RayleighScaleHeight", rayleighScale);
            Transmitance_Precalc.Set("MieScaleHeight", mieScale);
            Transmitance_Precalc.Set("Rt", atmos);
            Transmitance_Precalc.Set("Rg", gnd); 
             

            EngineManager.DispatchSyncComputeJob(Transmitance_Precalc, Transmitance_W, Transmitance_H, 1);
            Kokoro.Graphics.OpenGL.GraphicsDevice.SaveTexture(transmitance_cache, "trans.png");
            #endregion

            #region Calculate Single Scattering
            ShaderSource single_scatter_compute = ShaderSource.Load(ShaderType.ComputeShader, "Graphics/OpenGL/Shaders/Atmosphere/single_scatter.glsl");
            SingleScatter_Precalc = new ShaderProgram(single_scatter_compute);

            SingleScatter_Precalc.Set("ScatterCache", SingleScatterHandle); 
            SingleScatter_Precalc.Set("TransCache", TransmitanceHandle);
            SingleScatter_Precalc.Set("Rayleigh", rayleigh);
            SingleScatter_Precalc.Set("Mie", mie);
            SingleScatter_Precalc.Set("RayleighScaleHeight", rayleighScale);
            SingleScatter_Precalc.Set("MieScaleHeight", mieScale);
            SingleScatter_Precalc.Set("Rt", atmos);
            SingleScatter_Precalc.Set("Rg", gnd);

            EngineManager.DispatchSyncComputeJob(SingleScatter_Precalc, side, side, side);
            #endregion
        }

        public void Render()
        {

        }
    }
}
