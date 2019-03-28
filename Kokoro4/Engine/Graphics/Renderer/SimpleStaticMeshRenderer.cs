using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Renderer
{
    public class SimpleStaticMeshRenderer
    {

        struct draw
        {
            public int base_inst;
            public int count;
            public short matID;
            public Mesh m;
            public Matrix4[] transforms;
        }

        readonly int max_draw_cnt;
        int count;
        bool transient;
        RenderQueue queue;
        RenderState state;
        ShaderStorageBuffer transforms;
        UniformBuffer materials;
        List<draw> draws;
        List<int> updated;

        public SimpleStaticMeshRenderer(int max_draws, bool transient, ShaderProgram shader, Framebuffer fbuf, bool clear, Vector4 clearColor)
        {
            if (!transient && max_draws > 1024)
            {
                throw new Exception("TODO: Update design to accomodate more than 1024 draws/material IDs by switching to an SSBO instead of UBO");
            }
            else if (transient && max_draws > 256)
            {
                throw new Exception("TODO: Update design to accomodate more than 256 draws/material IDs by switching to an SSBO instead of UBO");
            }

            unsafe
            {
                transforms = new ShaderStorageBuffer(max_draws * sizeof(Matrix4), transient);
                materials = new UniformBuffer(transient);
            }
            state = new RenderState(fbuf, shader, new ShaderStorageBuffer[] { transforms }, new UniformBuffer[] { materials }, true, true, DepthFunc.Greater, InverseDepth.Far, InverseDepth.Near, BlendFactor.One, BlendFactor.Zero, clearColor, InverseDepth.ClearDepth, CullFaceMode.Back);
            queue = new RenderQueue(max_draws, transient);
            queue.ClearFramebufferBeforeSubmit = clear;
            draws = new List<draw>();
            this.transient = transient;
            updated = new List<int>();

            count = 0;
            max_draw_cnt = max_draws;
        }

        public int AddDraw(Mesh m, int inst_cnt, short materialID)
        {
            if (count >= max_draw_cnt) throw new Exception("More draws submitted than allocated.");

            draws.Add(new draw()
            {
                base_inst = count,
                count = inst_cnt,
                m = m,
                matID = materialID,
                transforms = new Matrix4[inst_cnt]
            });

            queue.BeginRecording();
            queue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[]
                {
                    new RenderQueue.MeshData()
                    {
                        BaseInstance = count,
                        InstanceCount = inst_cnt,
                        Mesh = m
                    },
                },
                State = state
            });
            queue.EndRecording();

            var idx = count;
            count += inst_cnt;
            return idx;
        }

        public void Update(int idx, Matrix4 transform)
        {
            if (idx >= count) throw new ArgumentOutOfRangeException("idx is invalid");

            if (!updated.Contains(idx)) updated.Add(idx);
            for (int i = 0; i < draws.Count; i++)
                if (idx >= draws[i].base_inst && idx < draws[i].base_inst + draws[i].count)
                {
                    draws[i].transforms[idx - draws[i].base_inst] = transform;
                    break;
                }
        }

        public void Submit()
        {
            if (updated.Count > 0)
            {
                unsafe
                {
                    byte* b_ptr = transforms.Update();
                    Matrix4* trans_ptr = (Matrix4*)b_ptr;

                    byte* mat_b_ptr = materials.Update();
                    int* mat_ptr = (int*)mat_b_ptr;

                    if (transient)
                    {
                        int o_idx = 0;
                        for (int i = 0; i < draws.Count; i++)
                        {
                            fixed (Matrix4* local_trans = draws[i].transforms)
                                for (int j = 0; j < draws[i].transforms.Length; j++)
                                    trans_ptr[o_idx++] = local_trans[j];

                            mat_ptr[i] = draws[i].matID;
                        }
                    }
                    else
                    {
                        updated.Sort();
                        int u_cnt = updated.Count;
                        for (int i = 0; i < u_cnt; i++)
                            for (int j = 0; j < draws.Count; j++)
                            {
                                if (updated.First() >= draws[j].base_inst && updated.First() < draws[j].base_inst + draws[j].count)
                                {
                                    fixed (Matrix4* local_trans = draws[j].transforms)
                                        trans_ptr[updated.First()] = local_trans[updated.First() - draws[j].base_inst];
                                    updated.RemoveAt(0);
                                    break;
                                }

                                mat_ptr[j] = draws[j].matID;
                            }
                    }

                    materials.UpdateDone();
                    transforms.UpdateDone();
                }
                updated.Clear();
            }

            queue.Submit();
        }
    }
}
