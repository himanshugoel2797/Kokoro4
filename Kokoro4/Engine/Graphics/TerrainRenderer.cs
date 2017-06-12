using Kokoro.Engine.Cameras;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.Math.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class TerrainRenderer
    {
        protected struct TerrainData
        {

        }

        QuadTree<TerrainData> Data;
        Mesh quad;
        float side;
        public RenderQueue Queue { get; private set; }
        public RenderState State { get; private set; }
        ShaderStorageBuffer WorldBuffer, TextureBuffer;
        const int quadSide = 25;
        protected int maxLevels = 40;
        protected int len = 2048;

        public int XIndex { get; private set; }
        public int YIndex { get; private set; }
        public int ZIndex { get; private set; }
        public float YOff { get; private set; }
        public Vector3 Normal { get; private set; }

        protected TerrainRenderer(float side, MeshGroup grp, Framebuffer fbuf, int xindex, int zindex, float yOff, ShaderSource vshader, ShaderSource fshader)
        {
            XIndex = xindex;
            ZIndex = zindex;
            YOff = yOff;
            for (YIndex = 0; YIndex < 3; YIndex++)
                if (YIndex != XIndex && YIndex != ZIndex)
                    break;

            float[] norm_f = new float[3];
            norm_f[XIndex] = 0;
            norm_f[YIndex] = System.Math.Sign(yOff);
            norm_f[ZIndex] = 0;
            Normal = new Vector3(norm_f);

            this.side = side;
            quad = QuadFactory.Create(grp, quadSide, quadSide, Normal, new Vector3(XIndex, YIndex, ZIndex));
            Data = new QuadTree<TerrainData>(new Math.Vector2(side * -0.5f, side * -0.5f), new Math.Vector2(side * 0.5f, side * 0.5f), 0);

            WorldBuffer = new ShaderStorageBuffer(len * 4 * sizeof(float), true);
            TextureBuffer = new ShaderStorageBuffer(len * 4 * sizeof(float), true);

            TextureHandle h = Texture.Default.GetHandle(TextureSampler.Default);
            h.SetResidency(Residency.Resident);

            unsafe
            {
                for (int i = 0; i < 3; i++)
                {
                    long* l = (long*)TextureBuffer.Update();
                    for (int j = 0; j < len; j++)
                    {
                        l[j * 2] = h;
                    }
                    TextureBuffer.UpdateDone();
                }
            }

            State = new RenderState(fbuf, new ShaderProgram(vshader, fshader), new ShaderStorageBuffer[] { WorldBuffer, TextureBuffer }, null, true, DepthFunc.LEqual, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.One, 1, (YOff < 0 && YIndex != 1) || (YOff >= 0 && YIndex == 1) ? CullFaceMode.Back : CullFaceMode.Front);
            State.ShaderProgram.SetShaderStorageBufferMapping("transforms", 0);
            State.ShaderProgram.SetShaderStorageBufferMapping("heightmaps", 1);

            Queue = new RenderQueue(len, true);
            Queue.ClearAndBeginRecording();
            Queue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = len, Mesh = quad } },
                State = State
            });
            Queue.EndRecording();

        }

        public TerrainRenderer(float side, MeshGroup grp, Framebuffer fbuf, int xindex, int zindex, float yOff, params string[] libraries) : this(side, grp, fbuf, xindex, zindex, yOff, ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/TerrainRenderer/vertex.glsl", libraries), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/TerrainRenderer/fragment.glsl", libraries))
        {

        }

        //Update SSBO to reduce size and add offset data for texturing corrections. scale, location, texture handle
        //Stop rerecording commands, umrendered stuff just positioned out of the way

        protected virtual DistanceState GetMaxLevel(ref QuadTree<TerrainData> d, int level, Vector3 pos, Vector3 dir)
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
            Vector3 tr = new Vector3(tr_f);
            Vector3 bl = new Vector3(bl_f);
            Vector3 br = new Vector3(br_f);
            Vector3 c = (tl + tr + bl + br) * 0.25f;

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

        private List<Vector3> positions = new List<Vector3>();
        private List<float> scales = new List<float>();
        private void Traverse(QuadTree<TerrainData> tData, int level, Vector3 pos, Vector3 dir, DistanceState parentState)
        {
            if (tData == null)
                return;

            DistanceState maxLevel = GetMaxLevel(ref tData, level, pos, dir);

            if (maxLevel == DistanceState.Load)
                maxLevel = DistanceState.Stop;

            if ((maxLevel == DistanceState.Stop) | (parentState == DistanceState.Visible && maxLevel == DistanceState.Invisible) | (maxLevel == DistanceState.Visible && tData.IsLeaf))
            {
                float[] pos_f = new float[3];
                pos_f[XIndex] = tData.Min.X;
                pos_f[YIndex] = YOff;
                pos_f[ZIndex] = tData.Min.Y;

                positions.Add(new Vector3(pos_f));
                scales.Add(side / (1 << level) * 1.0f / quadSide);
                return;
            }

            if (maxLevel == DistanceState.Invisible)
                return;

            for (int i = 0; i < 4; i++)
            {
                if (tData[i] != null)
                {
                    //Check if this is visible and within range
                    //If so, proceed to traverse down it
                    Vector2 tDataCenter = (tData[i].Max - tData[i].Min) * 0.5f + tData[i].Min;
                    float[] c_f = new float[3];
                    c_f[XIndex] = tDataCenter.X;
                    c_f[YIndex] = YOff;
                    c_f[ZIndex] = tDataCenter.Y;
                    Vector3 c = new Vector3(c_f);

                    {
                        Traverse(tData[i], level + 1, pos, dir, maxLevel);
                    }
                }
            }
        }

        public void Update(Vector3 pos, Vector3 dir)
        {
            positions.Clear();
            scales.Clear();
            Traverse(Data, 0, pos, dir, DistanceState.Visible);

            if (positions.Count > len)
                throw new Exception();

            unsafe
            {
                float* f = (float*)WorldBuffer.Update();
                for (int i = 0; i < positions.Count; i++)
                {
                    float[] p = (float[])(positions[i]);// - pos); 
                    float s = scales[i];
                    for (int j = 0; j < p.Length; j++)
                    {
                        f[i * 4 + j] = p[j];
                    }
                    f[i * 4 + 3] = s;
                }
                WorldBuffer.UpdateDone();

                //long* l = (long*)TextureBuffer.Update();
                //for (int i = 0; i < positions.Count; i++)
                //l[i * 2] = testTex;

                //TextureBuffer.UpdateDone();
            }

            Queue.UpdateDrawParams(quad.Parent, State, new RenderQueue.MeshData()
            {
                Mesh = quad,
                BaseInstance = 0,
                InstanceCount = positions.Count
            });
        }

        public void Draw(Matrix4 view, Matrix4 proj)
        {
            State.ShaderProgram.Set("View", view);
            State.ShaderProgram.Set("Projection", proj);
            Queue.Submit();
        }
    }
}
