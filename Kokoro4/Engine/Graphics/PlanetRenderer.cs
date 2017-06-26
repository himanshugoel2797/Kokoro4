using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Math;
using Kokoro.Math.Data;
using Kokoro.Graphics.OpenGL.ShaderLibraries;

namespace Kokoro.Engine.Graphics
{
    public class PlanetRenderer
    {
        class PlanetTerrainSide : TerrainRenderer
        {
            private float radius;

            public PlanetTerrainSide(float side, MeshGroup grp, Framebuffer fbuf, int xindex, int zindex, float yOff, float radius, TextureCache cache, params string[] libs) : base(side, grp, fbuf, xindex, zindex, yOff, ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/PlanetRenderer/vertex.glsl", libs), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/PlanetRenderer/fragment.glsl", libs), ShaderSource.Load(ShaderType.ComputeShader, "Graphics/OpenGL/Shaders/TerrainSource/compute.glsl", Noise.Name), cache)
            {
                this.radius = radius;
            }

            protected override DistanceState GetMaxLevel(ref QuadTree<TerrainData> d, int level, Vector3 pos, Vector3 dir)
            {
                float[] tl_f = new float[3];
                tl_f[XIndex] = d.Min.X;
                tl_f[ZIndex] = d.Max.Y;
                tl_f[YIndex] = YOff;

                float[] tr_f = new float[3];
                tr_f[XIndex] = d.Max.X;
                tr_f[ZIndex] = d.Max.Y;
                tr_f[YIndex] = YOff;

                float[] bl_f = new float[3];
                bl_f[XIndex] = d.Min.X;
                bl_f[ZIndex] = d.Min.Y;
                bl_f[YIndex] = YOff;

                float[] br_f = new float[3];
                br_f[XIndex] = d.Max.X;
                br_f[ZIndex] = d.Min.Y;
                br_f[YIndex] = YOff;

                Vector3 tl = new Vector3(tl_f);
                tl.Normalize();
                tl *= radius;

                Vector3 tr = new Vector3(tr_f);
                tr.Normalize();
                tr *= radius;


                Vector3 bl = new Vector3(bl_f);
                bl.Normalize();
                bl *= radius;

                Vector3 br = new Vector3(br_f);
                br.Normalize();
                br *= radius;

                Vector3 c = (tl + tr + bl + br) * 0.25f;
                c.Normalize();
                c *= radius;

                float side = System.Math.Abs((d.Max.X - d.Min.X));
                float dist_side = side * 2;

                float dist_tl = (pos - tl).LengthSquared;
                float dist_tr = (pos - tr).LengthSquared;
                float dist_bl = (pos - bl).LengthSquared;
                float dist_br = (pos - br).LengthSquared;

                float dist_c = (pos - c).LengthSquared;

                float min_dist0 = System.Math.Min(dist_tl, dist_tr);
                float min_dist1 = System.Math.Min(dist_bl, dist_br);
                float min_dist = System.Math.Min(min_dist0, min_dist1);
                min_dist = System.Math.Min(min_dist, dist_c);

                //Inside tile
                if (dist_tl <= side && dist_tr <= side && dist_bl <= side && dist_br <= side)
                    return (d.IsLeaf ? DistanceState.Stop : DistanceState.Visible);

                //subdivide when very close to the current level relative to the side
                if (min_dist <= dist_side * dist_side && d.IsLeaf && d.Level < maxLevels)
                {
                    d.Split();
                }

                if (min_dist <= dist_side * dist_side)
                {
                    if (min_dist <= dist_side * dist_side / 4)
                        return DistanceState.Visible;
                    else
                        return DistanceState.Stop;
                }

                return DistanceState.Invisible;
            }
        }

        private PlanetTerrainSide[] sides;
        private TextureCache cache;
        private AtmosphereRenderer atmosphere;

        public PlanetRenderer(MeshGroup grp, Framebuffer fbuf, float radius, AtmosphereRenderer atmos, params string[] libraries)
        {
            float side = radius * 2;
            float off = radius;

            atmosphere = atmos;

            cache = new TextureCache(1024 * 2, 64, 64, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba8, PixelType.Byte);

            sides = new PlanetTerrainSide[]
            {
                new PlanetTerrainSide(side, grp, fbuf, 0, 2, off, radius, cache, libraries),
                new PlanetTerrainSide(side, grp, fbuf, 0, 2, -off, radius, cache, libraries),
                new PlanetTerrainSide(side, grp, fbuf, 0, 1, off, radius, cache, libraries),
                new PlanetTerrainSide(side, grp, fbuf, 0, 1, -off, radius, cache, libraries),
                new PlanetTerrainSide(side, grp, fbuf, 1, 2, off, radius, cache, libraries),
                new PlanetTerrainSide(side, grp, fbuf, 1, 2, -off, radius, cache, libraries),
            };

            foreach (PlanetTerrainSide r in sides)
            {
                r.State.ShaderProgram.Set("Radius", radius);
                r.State.ShaderProgram.Set("Rt", atmosphere.Rt);
                r.State.ShaderProgram.Set("Rg", atmosphere.Rg);
                r.State.ShaderProgram.Set("TransCache", atmosphere.TransmitanceSamplerHandle);
                r.State.ShaderProgram.Set("ScatterCache", atmosphere.SingleScatterSamplerHandle);
                r.State.ShaderProgram.Set("MieScatterCache", atmosphere.MieSingleScatterSamplerHandle);
            }
        }
         
        public void Update(Vector3 pos, Vector3 dir)
        {
            foreach (PlanetTerrainSide r in sides)
            {
                r.State.ShaderProgram.Set("EyePosition", pos);
                r.State.ShaderProgram.Set("SunDir", atmosphere.SunDir);
                r.Update(pos, dir);
            }
        }


        public void Draw(Matrix4 view, Matrix4 proj)
        {
            foreach (PlanetTerrainSide r in sides)
                r.Draw(view, proj);
        }
    }
}
