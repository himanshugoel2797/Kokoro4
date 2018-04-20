using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class CloudRenderer
    {
        private float mie;
        private float atmos;

        private Texture transmitance_cache;
        private Texture single_scattering_cache;

        private const int Transmitance_W = 256;
        private const int Transmitance_H = 128;
        private ShaderProgram Transmitance_Precalc, SingleScatter_Precalc;
        private ImageHandle TransmitanceHandle, SingleScatterHandle;

        private RenderState AtmosphereRenderState;
        private RenderQueue AtmosphereRender;
        private Mesh AtmoSphere; //:^)
        private ShaderProgram AtmosphereShader;

        public CloudRenderer(float mie, float atmos, MeshGroup grp, Framebuffer fbuf)
        {
            this.mie = mie;
            this.atmos = atmos;

            int side = 64;
            int sideX = 64;
            int sideY = 64;
            int sideZ = 64;

            //populate Transmitance in a compute shader 
            transmitance_cache = new Texture();
            RawTextureSource trans_cacheSrc = new RawTextureSource(3, side, side, side, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture3D, PixelType.Float);
            transmitance_cache.SetData(trans_cacheSrc, 0);
            transmitance_cache.SetEnableLinearFilter(true);
            transmitance_cache.SetTileMode(false, false);
            TransmitanceHandle = transmitance_cache.GetImageHandle(0, -1, PixelInternalFormat.Rgba16f);
            TransmitanceHandle.SetResidency(Residency.Resident, AccessMode.ReadWrite);
 
            var TransmitanceSamplerHandle = transmitance_cache.GetHandle(TextureSampler.Default);
            TransmitanceSamplerHandle.SetResidency(Residency.Resident);
            
            single_scattering_cache = new Texture();
            RawTextureSource single_scatterCacheSrc = new RawTextureSource(3, sideX, sideY, sideZ, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture3D, PixelType.Float);
            single_scattering_cache.SetData(single_scatterCacheSrc, 0); 
            single_scattering_cache.SetEnableLinearFilter(true);
            single_scattering_cache.SetTileMode(false, false); 
            SingleScatterHandle = single_scattering_cache.GetImageHandle(0, -1, PixelInternalFormat.Rgba16f);
            SingleScatterHandle.SetResidency(Residency.Resident, AccessMode.Write);

            #region Calculate Transmittance
            ShaderSource trans_compute = ShaderSource.Load(ShaderType.ComputeShader, "Shaders/Cloud/transmittance.glsl");
            Transmitance_Precalc = new ShaderProgram(trans_compute);

            Transmitance_Precalc.Set("TransCache", TransmitanceHandle);
            Transmitance_Precalc.Set("Mie", mie); 
            Transmitance_Precalc.Set("Rt", atmos);
              

            EngineManager.DispatchSyncComputeJob(Transmitance_Precalc, side, side, side);
            #endregion

            #region Calculate Single Scattering
            ShaderSource single_scatter_compute = ShaderSource.Load(ShaderType.ComputeShader, "Shaders/Cloud/single_scatter.glsl");
            SingleScatter_Precalc = new ShaderProgram(single_scatter_compute);

            SingleScatter_Precalc.Set("ScatterCache", SingleScatterHandle);
            SingleScatter_Precalc.Set("TransCache", TransmitanceSamplerHandle);
            SingleScatter_Precalc.Set("Mie", mie);
            SingleScatter_Precalc.Set("Rt", atmos);
            
            EngineManager.DispatchSyncComputeJob(SingleScatter_Precalc, sideX, sideY, sideZ);
            #endregion

            //AtmosphereShader = new ShaderProgram(ShaderSource.Load( "Shaders/Atmosphere/vertex.glsl"))
            //AtmosphereRenderState = new RenderState(fbuf, AtmosphereShader, null, null, false, DepthFunc.LEqual, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.Front);
            //AtmosphereRender = new RenderQueue(1, false);
        }

        public void Render()
        {

        }
    }
}
