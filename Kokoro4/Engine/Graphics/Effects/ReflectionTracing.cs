using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Effects
{
    public class ReflectionTracing
    {
        Texture reflectionMap_matID, reflection_uv;
        TextureHandle worldPos, uv_norm, color;
        ImageHandle reflection_matID_Hndl, reflection_uv_Hndl;
        ShaderProgram reflectionProgram;
        int w, h;

        public Texture MaterialIDs { get { return reflectionMap_matID; } }
        public Texture UVs { get { return reflection_uv; } }

        const int bounces = 3;

        public ReflectionTracing(int w, int h, Texture worldPos, Texture uv_norm, Texture color)
        {
            reflectionMap_matID = new Texture();
            reflectionMap_matID.SetData(new FramebufferTextureSource(w, h, 1)
            {
                Format = PixelFormat.RedInteger,
                InternalFormat = PixelInternalFormat.R32ui,
                PixelType = PixelType.UnsignedInt
            }, 0);

            reflection_uv = new Texture();
            reflection_uv.SetData(new FramebufferTextureSource(w, h, 1)
            {
                Format = PixelFormat.Rg,
                InternalFormat = PixelInternalFormat.Rg16f,
                PixelType = PixelType.HalfFloat
            }, 0);

            this.w = w;
            this.h = h;

            TextureSampler sampler = new TextureSampler();
            sampler.SetEnableLinearFilter(true);
            sampler.SetTileMode(false, false);

            this.worldPos = worldPos.GetHandle(sampler);
            this.uv_norm = uv_norm.GetHandle(TextureSampler.Default);
            this.color = color.GetHandle(sampler);
            this.reflection_matID_Hndl = reflectionMap_matID.GetImageHandle(0, 0, PixelInternalFormat.R32ui);
            this.reflection_uv_Hndl = reflection_uv.GetImageHandle(0, 0, PixelInternalFormat.Rg16f);

            reflectionProgram = new ShaderProgram(
                ShaderSource.Load(ShaderType.ComputeShader, "Shaders/Lighting/ReflectionTracing/compute.glsl", $"#define BOUNCE_CNT ({bounces})\n#define WIDTH ({w})\n#define HEIGHT ({h})\n"));


            reflection_matID_Hndl.SetResidency(Residency.Resident, AccessMode.Write);
            reflection_uv_Hndl.SetResidency(Residency.Resident, AccessMode.Write);
            this.worldPos.SetResidency(Residency.Resident);
            this.uv_norm.SetResidency(Residency.Resident);
            this.color.SetResidency(Residency.Resident);

            reflectionProgram.Set("reflectionMap_matID", reflection_matID_Hndl);
            reflectionProgram.Set("reflectionMap_uv", reflection_uv_Hndl);
            reflectionProgram.Set("worldPosMap", this.worldPos);
            reflectionProgram.Set("uvNormMap", this.uv_norm);
            reflectionProgram.Set("colorMap", this.color);
        }

        public void Render(Matrix4[] view, Matrix4[] proj, Vector3[] pos)
        {
            reflectionProgram.Set("VP", view[0] * proj[0]);
            reflectionProgram.Set("viewPos", pos[0]);

            EngineManager.DispatchSyncComputeJob(reflectionProgram, w, h, 1);
        }
    }
}
