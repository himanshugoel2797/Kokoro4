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
        ShaderStorageBuffer WorldBuffer;
        Camera cam;
        const int quadSide = 10;
        int maxLevels = 1;

        public TerrainRenderer(float side, MeshGroup grp, Camera c, RenderState state, ShaderStorageBuffer ssbo)
        {
            int len = 256;

            quad = QuadFactory.Create(grp, quadSide, quadSide);
            Data = new QuadTree<TerrainData>(new Math.Vector2(side * -0.5f, side * -0.5f), new Math.Vector2(side * 0.5f, side * 0.5f));
            this.side = side;
            this.state = state;

            cam = c;
            WorldBuffer = ssbo;
            queue = new RenderQueue(len);
            queue.ClearFramebufferBeforeSubmit = true;
        }

        private DistanceState GetMaxLevel(ref QuadTree<TerrainData> d, int level, Vector3 pos, Vector3 dir)
        {
            Vector3 tl = new Vector3(d.Min.X, 0, d.Max.Y);
            Vector3 tr = new Vector3(d.Max.X, 0, d.Max.Y);
            Vector3 bl = new Vector3(d.Min.X, 0, d.Min.Y);
            Vector3 br = new Vector3(d.Max.X, 0, d.Min.Y);

            float side = System.Math.Abs((d.Max.X - d.Min.X));
            float dist_side = side;

            float dist_tl = (pos - tl).LengthSquared;
            float dist_tr = (pos - tr).LengthSquared;
            float dist_bl = (pos - bl).LengthSquared; 
            float dist_br = (pos - br).LengthSquared;

            float min_dist0 = System.Math.Min(dist_tl, dist_tr);
            float min_dist1 = System.Math.Min(dist_bl, dist_br);
            float min_dist = System.Math.Min(min_dist0, min_dist1);

            float tl_a = Vector3.Dot(Vector3.Normalize(tl - pos), dir);
            float tr_a = Vector3.Dot(Vector3.Normalize(tr - pos), dir);
            float bl_a = Vector3.Dot(Vector3.Normalize(bl - pos), dir);
            float br_a = Vector3.Dot(Vector3.Normalize(br - pos), dir);

            float min_a0 = System.Math.Max(System.Math.Abs(tl_a), System.Math.Abs(tr_a));
            float min_a1 = System.Math.Max(System.Math.Abs(bl_a), System.Math.Abs(br_a));
            float min_a = System.Math.Max(min_a0, min_a1);

            bool tl_vis = tl_a >= 0;
            bool tr_vis = tr_a >= 0;
            bool bl_vis = bl_a >= 0;
            bool br_vis = br_a >= 0;


            //Inside tile
            if (dist_tl <= side && dist_tr <= side && dist_bl <= side && dist_br <= side)
                return (d.IsLeaf ? DistanceState.Stop : DistanceState.Visible);


            if (!tl_vis && !tr_vis && !bl_vis && !br_vis)
                return DistanceState.Load;

            //TODO: find visibility bug


            //subdivide when very close to the current level relative to the side
            if (min_dist <= dist_side * dist_side && d.IsLeaf)
            {
                d.Split();
                maxLevels++;
            }

            Matrix4 vp = cam.View * cam.Projection;
            Vector2 tl_s = Vector3.TransformPerspective(tl, vp).Xy;
            Vector2 tr_s = Vector3.TransformPerspective(tr, vp).Xy;
            Vector2 bl_s = Vector3.TransformPerspective(bl, vp).Xy;
            Vector2 br_s = Vector3.TransformPerspective(br, vp).Xy;

            float len0 = (tl_s - tr_s).LengthSquared;
            float len1 = (tr_s - br_s).LengthSquared;

            bool tl_isIn = tl_s.X >= -1 && tl_s.X <= 1 && tl_s.Y >= -1 && tl_s.Y <= 1;
            bool tr_isIn = tr_s.X >= -1 && tr_s.X <= 1 && tr_s.Y >= -1 && tr_s.Y <= 1;
            bool bl_isIn = bl_s.X >= -1 && bl_s.X <= 1 && bl_s.Y >= -1 && bl_s.Y <= 1;
            bool br_isIn = br_s.X >= -1 && br_s.X <= 1 && br_s.Y >= -1 && br_s.Y <= 1;

            float area = len0 * len1;

            //Stop going deeper if all the corners of the current level are already visible
            //if (tl_isIn && tr_isIn && bl_isIn && br_isIn)
            //    return DistanceState.Stop;

            if (min_dist <= dist_side * dist_side)
            {
                //if (min_dist <= dist_side * dist_side / 80)
                    return DistanceState.Visible;
                //else
                //    return DistanceState.Stop;
            }

            return DistanceState.Invisible;

        }

        private List<Matrix4> transforms = new List<Matrix4>();
        private void Traverse(QuadTree<TerrainData> tData, int level, Vector3 pos, Vector3 dir, DistanceState parentState)
        {
            if (tData == null)
                return;

            bool endReached = true;
            DistanceState maxLevel = GetMaxLevel(ref tData, level, pos, dir);

            if (maxLevel == DistanceState.Load)
            {
                maxLevel = DistanceState.Stop;
            }

            if ((maxLevel == DistanceState.Stop) | (parentState == DistanceState.Visible && maxLevel == DistanceState.Invisible) | (maxLevel == DistanceState.Visible && tData.IsLeaf))
            {
                transforms.Add(Matrix4.Scale(side / (1 << level)) * Matrix4.CreateTranslation(new Vector3(tData.Min.X * quadSide, 0, tData.Min.Y * quadSide)));
                return;
            }

            if (maxLevel == DistanceState.Invisible)
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

                    {
                        Traverse(tData[i], level + 1, pos, dir, maxLevel);
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
            Traverse(Data, 0, pos / quadSide, dir, DistanceState.Visible);

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
