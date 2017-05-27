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
        ShaderStorageBuffer WorldBuffer;
        const int quadSide = 50;
        const int maxLevels = 4;

        private void GenerateTest(QuadTree<TerrainData> d, int lv)
        {
            if (lv == 0)
                return;

            d.Split();
            for (int i = 0; i < 4; i++)
            {
                GenerateTest(d[i], lv - 1);
            }
        }

        public TerrainRenderer(float side, MeshGroup grp, RenderState state, ShaderStorageBuffer ssbo)
        {
            int len = 256;

            quad = QuadFactory.Create(grp, quadSide, quadSide);
            Data = new QuadTree<TerrainData>(new Math.Vector2(side * -0.5f, side * -0.5f), new Math.Vector2(side * 0.5f, side * 0.5f));
            this.side = side;
            this.state = state;

            WorldBuffer = ssbo;
            queue = new RenderQueue(len);
            queue.ClearFramebufferBeforeSubmit = true;

            //Create a base data set
            GenerateTest(Data, maxLevels);
        }

        private int GetMaxLevel(QuadTree<TerrainData> d, Vector3 pos, Vector3 dir)
        {
            Vector3 tl = new Vector3(d.Min.X, 0, d.Max.Y);
            Vector3 tr = new Vector3(d.Max.X, 0, d.Max.Y);
            Vector3 bl = new Vector3(d.Min.X, 0, d.Min.Y);
            Vector3 br = new Vector3(d.Max.X, 0, d.Min.Y);

            float side = d.Max.X - d.Min.X;

            float dist_tl = (pos - tl).LengthSquared;
            float dist_tr = (pos - tr).LengthSquared;
            float dist_bl = (pos - bl).LengthSquared;
            float dist_br = (pos - br).LengthSquared;

            float min_dist0 = System.Math.Min(dist_tl, dist_tr);
            float min_dist1 = System.Math.Min(dist_bl, dist_br);
            float min_dist = System.Math.Min(min_dist0, min_dist1);

            bool tl_vis = Vector3.Dot(Vector3.Normalize(tl - pos), dir) >= 0;
            bool tr_vis = Vector3.Dot(Vector3.Normalize(tr - pos), dir) >= 0;
            bool bl_vis = Vector3.Dot(Vector3.Normalize(bl - pos), dir) >= 0;
            bool br_vis = Vector3.Dot(Vector3.Normalize(br - pos), dir) >= 0;


            //Inside tile
            if (dist_tl <= side && dist_tr <= side && dist_bl <= side && dist_br <= side)
                return maxLevels;

            if (!tl_vis && !tr_vis && !bl_vis && !br_vis)
                return -2;

            if (min_dist <= 500)
                return maxLevels;

            if (min_dist <= 1000)
                return maxLevels - 1;

            if (min_dist <= 4000)
                return maxLevels - 2;

            if (min_dist <= 6000)
                return maxLevels - 3;

            if (min_dist <= 10000)
                return maxLevels - 4;

            /*
            for (float i = 0; i < maxLevels; i++)
                if (min_dist <= -250 * (i-6f) * (i - 6f) + (i - 6) + 10500)
                    return maxLevels - (int)i;
            */
            return -2;

        }

        private List<Matrix4> transforms = new List<Matrix4>();
        private void Traverse(QuadTree<TerrainData> tData, int level, Vector3 pos, Vector3 dir)
        {
            if (tData == null)
                return;

            bool endReached = true;
            int maxLevel = GetMaxLevel(tData, pos, dir);

            if (level == maxLevel || level - 1 == maxLevel)
            {
                transforms.Add(Matrix4.Scale(side / (1 << level)) * Matrix4.CreateTranslation(new Vector3(tData.Min.X * quadSide, 0, tData.Min.Y * quadSide)));
                return;
            }

            if (level > maxLevel)
                return;

            for (int i = 0; i < 4; i++)
            {
                if (tData[i] != null)
                {
                    endReached = false;
                    //Check if this is visible and within range
                    //If so, proceed to traverse down it
                    Vector2 tDataCenter = (tData[i].Max - tData[i].Min) * 0.5f + tData[i].Min;
                    Vector3 c = new Vector3(tDataCenter.X, 0, tDataCenter.Y);
                    
                    if (maxLevel > level)
                    {
                        Traverse(tData[i], level + 1, pos, dir);
                    }
                }
            }

            if (endReached)
            {
                //Add this to render
                //transforms.Add(Matrix4.Scale(side / (1 << level)) * Matrix4.CreateTranslation(new Vector3(tData.Min.X * quadSide, 0, tData.Min.Y * quadSide)));
            }
        }

        public void Update(Vector3 pos, Vector3 dir)
        {
            transforms.Clear();
            Traverse(Data, 0, pos / quadSide, dir);

            unsafe
            {
                float* f = (float*)WorldBuffer.Update();
                for (int i = 0; i < transforms.Count; i++)
                {
                    float[] d = (float[])transforms[i];
                    for (int j = 0; j < d.Length; j++)
                    {
                        f[i * 16 + j] = d[j];
                    }
                }
                WorldBuffer.UpdateDone();
            }

            queue.ClearAndBeginRecording();
            queue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = transforms.Count, Mesh = quad } },
                State = state
            });
            queue.EndRecording();
        }

        public void Draw()
        {
            queue.Submit();
        }
    }
}
