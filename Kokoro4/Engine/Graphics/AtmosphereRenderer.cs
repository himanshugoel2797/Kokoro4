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

        private const int Transmitance_W = 256;
        private const int Transmitance_H = 256;
        private ShaderProgram Transmitance_Precalc;
        private ImageHandle TransmitanceHandle;

        public AtmosphereRenderer(Vector3 rayleigh, float rayleighScaleHeight, float mie, float mieScaleHeight, float gnd, float atmos)
        {
            this.rayleigh = rayleigh;
            this.mie = mie;
            this.rayleighScale = rayleighScaleHeight;
            this.mieScale = mieScaleHeight;
            this.gnd = gnd;
            this.atmos = atmos;

            int side = 512;

            //populate Transmitance in a compute shader
            transmitance_cache = new Texture();
            RawTextureSource cacheSrc = new RawTextureSource(2, side, side, side, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture2D, PixelType.Float);
            transmitance_cache.SetData(cacheSrc, 0);
            transmitance_cache.SetEnableLinearFilter(true);
            transmitance_cache.SetTileMode(false, false);


            ShaderSource trans_compute = ShaderSource.Load(ShaderType.ComputeShader, "Graphics/OpenGL/Shaders/Atmosphere/compute.glsl");
            Transmitance_Precalc = new ShaderProgram(trans_compute);

            TransmitanceHandle = transmitance_cache.GetImageHandle(0, 0, PixelInternalFormat.Rgba16f);
            TransmitanceHandle.SetResidency(Residency.Resident, AccessMode.Write);

            Transmitance_Precalc.Set("TransCache", TransmitanceHandle);
            Transmitance_Precalc.Set("Rayleigh", rayleigh);
            Transmitance_Precalc.Set("Mie", mie);
            Transmitance_Precalc.Set("RayleighScaleHeight", rayleighScale);
            Transmitance_Precalc.Set("MieScaleHeight", mieScale);
            Transmitance_Precalc.Set("Rt", atmos);
            Transmitance_Precalc.Set("Rg", gnd);


            EngineManager.DispatchSyncComputeJob(Transmitance_Precalc, side, side, 1); 
            Kokoro.Graphics.OpenGL.GraphicsDevice.SaveTexture(transmitance_cache, "trans.png");
        }

        public void Render()
        {

        }
    }
}
