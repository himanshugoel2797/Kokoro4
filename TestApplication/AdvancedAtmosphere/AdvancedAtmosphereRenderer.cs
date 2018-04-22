using Kokoro.Engine;
using Kokoro.Engine.Graphics;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication.AdvancedAtmosphere
{
    public class AdvancedAtmosphereRenderer
    {
        private const int TransW = 256;
        private const int TransH = 64;

        private const int ScatterW = 32;
        private const int ScatterH = 128;
        private const int ScatterD = 32;
        private const int ScatterT = 8;

        private Texture Transmittance;
        private Texture[] ScatteringBuffers;

        private ImageHandle TransmittanceHandle;
        private ImageHandle[] ScatteringBufferHandles;

        public AdvancedAtmosphereRenderer(Vector3 rayleigh, float rayleighScaleHeight, float mie, float mieScaleHeight, float gnd, float atmos, MeshGroup grp, params Framebuffer[] fbuf)
        {
            //initialize transmittance texture and 2 multiscattering 4d volumes

            TextureSampler sampler = new TextureSampler();
            sampler.SetEnableLinearFilter(true);
            sampler.SetTileMode(false, false, false);

            #region Transmittance Initialization
            Transmittance = new Texture();

            RawTextureSource trans_src = new RawTextureSource(2, TransW, TransH, 0, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture2D, PixelType.Float);
            Transmittance.SetData(trans_src, 0);
            Transmittance.SetEnableLinearFilter(true);
            Transmittance.SetTileMode(false, false);
            TransmittanceHandle = Transmittance.GetImageHandle(0, 0, PixelInternalFormat.Rgba16f);
            TransmittanceHandle.SetResidency(Residency.Resident, AccessMode.ReadWrite);

            var TransmitanceSamplerHandle = Transmittance.GetHandle(sampler);
            TransmitanceSamplerHandle.SetResidency(Residency.Resident);
            #endregion

            #region Multiscattering Volumes
            ScatteringBuffers = new Texture[2];
            ScatteringBufferHandles = new ImageHandle[2];

            for (int i = 0; i < ScatteringBuffers.Length; i++)
            {
                ScatteringBuffers[i] = new Texture();

                RawTextureSource scatter_src = new RawTextureSource(3, ScatterW, ScatterH, ScatterD * ScatterT, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture3D, PixelType.Float);
                ScatteringBuffers[i].SetData(scatter_src, 0);
                ScatteringBuffers[i].SetEnableLinearFilter(true);
                ScatteringBuffers[i].SetTileMode(false, false);

                ScatteringBufferHandles[i] = ScatteringBuffers[i].GetImageHandle(0, 0, PixelInternalFormat.Rgba16f);
                ScatteringBufferHandles[i].SetResidency(Residency.Resident, AccessMode.ReadWrite);
            }
            #endregion

            #region Calculate Transmittance
            ShaderSource trans_compute = ShaderSource.Load(ShaderType.ComputeShader, "AdvancedAtmosphere/Shaders/transmittance.glsl");
            var Transmitance_Precalc = new ShaderProgram(trans_compute);

            Transmitance_Precalc.Set("TransCache", TransmittanceHandle);
            Transmitance_Precalc.Set("Rayleigh", rayleigh);
            Transmitance_Precalc.Set("Mie", mie);
            Transmitance_Precalc.Set("RayleighScaleHeight", rayleighScaleHeight);
            Transmitance_Precalc.Set("MieScaleHeight", mieScaleHeight);
            Transmitance_Precalc.Set("Rt", atmos);
            Transmitance_Precalc.Set("Rg", gnd);
            
            EngineManager.DispatchSyncComputeJob(Transmitance_Precalc, TransW, TransH, 1);
            Kokoro.Graphics.OpenGL.GraphicsDevice.SaveTexture(Transmittance, "trans.png");
            #endregion

            #region Calculate Single Scattering
            ShaderSource single_scatter_compute = ShaderSource.Load(ShaderType.ComputeShader, "Shaders/Atmosphere/single_scatter.glsl");
            var SingleScatter_Precalc = new ShaderProgram(single_scatter_compute);

            SingleScatter_Precalc.Set("ScatterCache", ScatteringBufferHandles[0]);
            SingleScatter_Precalc.Set("TransCache", TransmitanceSamplerHandle);
            SingleScatter_Precalc.Set("Rayleigh", rayleigh);
            SingleScatter_Precalc.Set("Mie", mie);
            SingleScatter_Precalc.Set("RayleighScaleHeight", rayleighScaleHeight);
            SingleScatter_Precalc.Set("MieScaleHeight", mieScaleHeight);
            SingleScatter_Precalc.Set("Rt", atmos);
            SingleScatter_Precalc.Set("Rg", gnd);

            SingleScatter_Precalc.Set("SunAngleCount", ScatterT);
            SingleScatter_Precalc.Set("Count", ScatterD);
            SingleScatter_Precalc.Set("YLen", ScatterH);

            int YWorkSize = 4;

            for (int i0 = 0; i0 < ScatterT; i0++)
            {
                SingleScatter_Precalc.Set("SunAngleOff", i0);
                for (int i = 0; i < ScatterD; i++)
                {
                    SingleScatter_Precalc.Set("Layer", i);

                    for (int j = 0; j < ScatterH; j += YWorkSize)
                    {
                        SingleScatter_Precalc.Set("YOff", j);
                        EngineManager.DispatchSyncComputeJob(SingleScatter_Precalc, ScatterW, YWorkSize, 1);
                    }
                }
            }
            #endregion
            //compute several scattering passes, ping-pong between the two buffers

            //free the second scattering buffer
        }

        public void Draw(Matrix4 view, Matrix4 proj, Vector3 position, Vector3 sunDir, int idx = 0)
        {
            /*
            this.SunDir = sunDir;

            AtmosphereShader.Set("View", view);
            AtmosphereShader.Set("Projection", proj);
            AtmosphereShader.Set("Rt", atmos);
            AtmosphereShader.Set("Rg", gnd);
            AtmosphereShader.Set("EyePosition", position);
            AtmosphereShader.Set("SunDir", sunDir);
            AtmosphereRender[idx].Submit();*/
        }
    }
}
