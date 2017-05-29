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
        struct TerrainData
        {

        }

        QuadTree<TerrainData> Data;
        Mesh quad;
        float side;
        RenderQueue queue;
        RenderState state;
        ShaderStorageBuffer WorldBuffer, TextureBuffer;
        TextureHandle testTex;
        const int quadSide = 25;
        int maxLevels = 40;
        int len = 2048;

        public TerrainRenderer(float side, MeshGroup grp, Framebuffer fbuf, TextureHandle tex)
        {

            testTex = tex;

            this.side = side;
            quad = QuadFactory.Create(grp, quadSide, quadSide, new Vector3(0, 1, 2));
            Data = new QuadTree<TerrainData>(new Math.Vector2(side * -0.5f, side * -0.5f), new Math.Vector2(side * 0.5f, side * 0.5f), 0);

            WorldBuffer = new ShaderStorageBuffer(len * 4 * sizeof(float), true);
            TextureBuffer = new ShaderStorageBuffer(len * 4 * sizeof(float), true);

            unsafe
            {
                for (int i = 0; i < 3; i++)
                {
                    long* l = (long*)TextureBuffer.Update();
                    for (int j = 0; j < len; j++)
                    {
                        l[j * 2] = tex;
                    }
                    TextureBuffer.UpdateDone();
                }
            }

            state = new RenderState(fbuf, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/TerrainRenderer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/TerrainRenderer/fragment.glsl")), new ShaderStorageBuffer[] { WorldBuffer, TextureBuffer }, null, true, DepthFunc.LEqual, -1, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 1, CullFaceMode.Back);
            state.ShaderProgram.SetShaderStorageBufferMapping("transforms", 0);
            state.ShaderProgram.SetShaderStorageBufferMapping("heightmaps", 1);

            queue = new RenderQueue(len, true);
            queue.ClearFramebufferBeforeSubmit = true;
            
            queue.ClearAndBeginRecording();
            queue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = len, Mesh = quad } },
                State = state
            });
            queue.EndRecording();

        }

        //Update SSBO to reduce size and add offset data for texturing corrections. scale, location, texture handle
        //Stop rerecording commands, umrendered stuff just positioned out of the way

        private DistanceState GetMaxLevel(ref QuadTree<TerrainData> d, int level, Vector3 pos, Vector3 dir)
        {
            Vector3 tl = new Vector3(d.Min.X, 0, d.Max.Y);
            Vector3 tr = new Vector3(d.Max.X, 0, d.Max.Y);
            Vector3 bl = new Vector3(d.Min.X, 0, d.Min.Y);
            Vector3 br = new Vector3(d.Max.X, 0, d.Min.Y);
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
                maxLevels++;
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
                positions.Add(new Vector3(tData.Min.X, 0, tData.Min.Y));
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
                    Vector3 c = new Vector3(tDataCenter.X, 0, tDataCenter.Y);

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
                    float[] p = (float[])(positions[i] - pos);
                    float s = scales[i];
                    for (int j = 0; j < p.Length; j++)
                    {
                        f[i * 4 + j] = p[j];
                    }
                    f[i * 4 + 3] = s;
                }
                WorldBuffer.UpdateDone();

                long* l = (long*)TextureBuffer.Update();
                for (int i = 0; i < positions.Count; i++)
                    l[i * 2] = testTex;

                TextureBuffer.UpdateDone();
            }

            queue.UpdateDrawParams(quad.Parent, state, new RenderQueue.MeshData()
            {
                Mesh = quad,
                BaseInstance = 0,
                InstanceCount = positions.Count
            });
        }

        public void Draw(Matrix4 view, Matrix4 proj)
        {
            state.ShaderProgram.Set("View", view);
            state.ShaderProgram.Set("Projection", proj);
            queue.Submit();
        }
    }
}
