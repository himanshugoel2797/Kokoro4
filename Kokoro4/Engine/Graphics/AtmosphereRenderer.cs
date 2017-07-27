using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kokoro.Engine.Graphics.RenderQueue;

namespace Kokoro.Engine.Graphics
{
    public class AtmosphereRenderer
    {
        private Vector3 rayleigh;
        private float mie;
        private float rayleighScale, mieScale, gnd, atmos;

        private Texture transmitance_cache;
        private Texture single_scattering_cache;
        private Texture mie_single_scattering_cache;

        private const int Transmitance_W = 256;
        private const int Transmitance_H = 128;
        private ShaderProgram Transmitance_Precalc, SingleScatter_Precalc;
        private ImageHandle TransmitanceHandle, SingleScatterHandle, mie_SingleScatterHandle;

        private RenderState AtmosphereRenderState;
        private RenderQueue AtmosphereRender;
        private Mesh AtmoSphere; //:^)
        private ShaderProgram AtmosphereShader;

        public float Rg { get { return gnd; } }
        public float Rt { get { return atmos; } }
        public TextureHandle SingleScatterSamplerHandle { get; set; }
        public TextureHandle MieSingleScatterSamplerHandle { get; set; }
        public TextureHandle TransmitanceSamplerHandle { get; set; }
        public Vector3 SunDir { get; set; }

        public AtmosphereRenderer(Vector3 rayleigh, float rayleighScaleHeight, float mie, float mieScaleHeight, float gnd, float atmos, MeshGroup grp, Framebuffer fbuf)
        {
            this.rayleigh = rayleigh;
            this.mie = mie;
            this.rayleighScale = rayleighScaleHeight;
            this.mieScale = mieScaleHeight;
            this.gnd = gnd;
            this.atmos = atmos;

            int sideX = 128;
            int sideY = 128;
            int sideZ = 128;

            TextureSampler sampler = new TextureSampler();
            sampler.SetEnableLinearFilter(true); 
            sampler.SetTileMode(false, false, false);

            //populate Transmitance in a compute shader 
            transmitance_cache = new Texture();
            RawTextureSource trans_cacheSrc = new RawTextureSource(2, Transmitance_W, Transmitance_H, 0, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture2D, PixelType.Float);
            transmitance_cache.SetData(trans_cacheSrc, 0);
            transmitance_cache.SetEnableLinearFilter(true);
            transmitance_cache.SetTileMode(false, false);
            TransmitanceHandle = transmitance_cache.GetImageHandle(0, 0, PixelInternalFormat.Rgba16f);
            TransmitanceHandle.SetResidency(Residency.Resident, AccessMode.ReadWrite);

            TransmitanceSamplerHandle = transmitance_cache.GetHandle(sampler);
            TransmitanceSamplerHandle.SetResidency(Residency.Resident);

            single_scattering_cache = new Texture();
            RawTextureSource single_scatterCacheSrc = new RawTextureSource(3, sideX, sideY, sideZ, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture3D, PixelType.Float);
            single_scattering_cache.SetData(single_scatterCacheSrc, 0); 
            single_scattering_cache.SetEnableLinearFilter(true);
            single_scattering_cache.SetTileMode(false, false);
            SingleScatterHandle = single_scattering_cache.GetImageHandle(0, -1, PixelInternalFormat.Rgba16f);
            SingleScatterHandle.SetResidency(Residency.Resident, AccessMode.Write);

            SingleScatterSamplerHandle = single_scattering_cache.GetHandle(sampler);
            SingleScatterSamplerHandle.SetResidency(Residency.Resident);

            mie_single_scattering_cache = new Texture();
            RawTextureSource mie_single_scatterCacheSrc = new RawTextureSource(3, sideX, sideY, sideZ, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba16f, TextureTarget.Texture3D, PixelType.Float);
            mie_single_scattering_cache.SetData(mie_single_scatterCacheSrc, 0);
            mie_single_scattering_cache.SetEnableLinearFilter(true);
            mie_single_scattering_cache.SetTileMode(false, false);
            mie_SingleScatterHandle = mie_single_scattering_cache.GetImageHandle(0, -1, PixelInternalFormat.Rgba16f);
            mie_SingleScatterHandle.SetResidency(Residency.Resident, AccessMode.Write);

            MieSingleScatterSamplerHandle = mie_single_scattering_cache.GetHandle(sampler);
            MieSingleScatterSamplerHandle.SetResidency(Residency.Resident);

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
            SingleScatter_Precalc.Set("MieScatterCache", MieSingleScatterSamplerHandle);
            SingleScatter_Precalc.Set("TransCache", TransmitanceSamplerHandle);
            SingleScatter_Precalc.Set("Rayleigh", rayleigh);
            SingleScatter_Precalc.Set("Mie", mie);
            SingleScatter_Precalc.Set("RayleighScaleHeight", rayleighScale);
            SingleScatter_Precalc.Set("MieScaleHeight", mieScale);
            SingleScatter_Precalc.Set("Rt", atmos);
            SingleScatter_Precalc.Set("Rg", gnd);
            SingleScatter_Precalc.Set("Count", sideZ);
            SingleScatter_Precalc.Set("YLen", sideY);

            int YWorkSize = 4;

            for (int i = 0; i < sideZ; i++)
            {
                SingleScatter_Precalc.Set("Layer", i);

                for (int j = 0; j < sideY; j += YWorkSize)
                {
                    SingleScatter_Precalc.Set("YOff", j);
                    EngineManager.DispatchSyncComputeJob(SingleScatter_Precalc, sideX, YWorkSize, 1);
                }
            }
            #endregion


            AtmosphereShader = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Atmosphere/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Atmosphere/fragment.glsl"));
            AtmosphereShader.Set("TransCache", TransmitanceSamplerHandle);
            AtmosphereShader.Set("ScatterCache", SingleScatterSamplerHandle);
            AtmosphereShader.Set("MieScatterCache", MieSingleScatterSamplerHandle);

            AtmosphereRenderState = new RenderState(fbuf, AtmosphereShader, null, null, false, true, DepthFunc.Greater, 1, -1, BlendFactor.SrcAlpha, BlendFactor.OneMinusSrcAlpha, Vector4.Zero, 0, CullFaceMode.Front);
            AtmosphereRender = new RenderQueue(1, false);

            AtmoSphere = SphereFactory.Create(grp, 180);
            AtmosphereRender.BeginRecording();
            AtmosphereRender.ClearFramebufferBeforeSubmit = !true;
            AtmosphereRender.RecordDraw(new DrawData()
            {
                Meshes = new MeshData[] { new MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = AtmoSphere } },
                State = AtmosphereRenderState
            });
            AtmosphereRender.EndRecording();
        }

        public void Draw(Matrix4 view, Matrix4 proj, Vector3 position, Vector3 sunDir)
        {
            this.SunDir = sunDir;

            AtmosphereShader.Set("View", view);
            AtmosphereShader.Set("Projection", proj);
            AtmosphereShader.Set("Rt", atmos);
            AtmosphereShader.Set("Rg", gnd);
            AtmosphereShader.Set("EyePosition", position);
            AtmosphereShader.Set("SunDir", sunDir);
            AtmosphereRender.Submit();
        }
    }
}
