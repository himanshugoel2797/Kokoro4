using Kokoro.Engine.Graphics;
using Kokoro.Engine;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.SceneGraph.Generators
{
    public class FlatGenerator
    {
        ShaderStorageBuffer transforms;
        RenderQueue queue;
        RenderState state;

        const int maxDraws = 4096;

        public FlatGenerator(Framebuffer dest, ShaderProgram prog, UniformBuffer[] ubos, ShaderStorageBuffer[] ssbos)
        {
            transforms = new ShaderStorageBuffer(sizeof(float) * 16 * maxDraws, true);
            var ssbo_set = new List<ShaderStorageBuffer>();
            ssbo_set.Add(transforms);
            if(ssbos != null)ssbo_set.AddRange(ssbos);

            state = new RenderState(dest, prog, ssbo_set.ToArray(), ubos, true, true, DepthFunc.Greater, 1, -1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.Back);
            queue = new RenderQueue(maxDraws, true);
            queue.ClearFramebufferBeforeSubmit = true;
        }

        public void Reset()
        {

        }

        private void traverseTree(ulong mask, Node tree, List<(Matrix4, Mesh)> transforms)
        {
            if ((tree.LayerMask & mask) > 0 && tree.Visible && tree.Mesh != null)
            {
                transforms.Add((tree.NetTransform, tree.Mesh));
            }

            for (int i = 0; i < tree.Children.Count; i++)
                traverseTree(mask, tree.Children[i], transforms);
        }

        class MeshDataEntry
        {
            public int _count;
            public int _base;
            public Mesh _mesh;
        }

        public void Render(Node tree, ulong layerMask)
        {
            //Generate a buffer of all the transforms and their renderables for the chosen set of layers
            List<(Matrix4, Mesh)> transformList = new List<(Matrix4, Mesh)>();
            traverseTree(layerMask, tree, transformList);

            //Sort the transforms by mesh
            var transformArr = transformList.OrderBy(a => a.Item2.GetHashCode()).ToArray();

            //Upload the data
            List<MeshDataEntry> meshData = new List<MeshDataEntry>();
            unsafe
            {
                var b_ptr = transforms.Update();
                long* d = (long*)b_ptr;


                for (int j = 0; j < transformArr.Length; j++)
                {
                    if (meshData.Count == 0 || meshData.Last()._mesh != (transformArr[j].Item2))
                    {
                        meshData.Add(new MeshDataEntry()
                        {
                            _base = (meshData.Count == 0) ? 0 : (meshData.Last()._base + meshData.Last()._count),
                            _count = 1,
                            _mesh = transformArr[j].Item2
                        });
                    }
                    else
                    {
                        meshData[meshData.Count - 1]._count++;
                    }

                    fixed (Matrix4* mats = &transformArr[j].Item1)
                    {
                        long* s = (long*)mats;
                        for (int i = 0; i < sizeof(Matrix4) / sizeof(long); i++)
                        {
                            *(d++) = s[i];
                        }
                    }
                }
                transforms.UpdateDone();
            }

            //Record render calls for anything directly recordable
            queue.ClearAndBeginRecording();
            for(int i = 0; i < meshData.Count; i++)
            {
                queue.RecordDraw(new RenderQueue.DrawData()
                {
                    State = state,
                    Meshes = new RenderQueue.MeshData[]
                    {
                        new RenderQueue.MeshData()
                        {
                            BaseInstance = meshData[i]._base,
                            InstanceCount = meshData[i]._count,
                            Mesh = meshData[i]._mesh
                        }
                    }
                });
            }
            queue.EndRecording();

            //TODO Store remaining render methods
        }

        public void Submit(ulong layerMask)
        {
            //Submit render calls
            queue.Submit();

            //Submit render methods
        }
    }
}
