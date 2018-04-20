using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if OPENGL
using Kokoro.Graphics.OpenGL;
#elif VULKAN
using Kokoro.Graphics.Vulkan;
#else
#error "Pick a graphics backend by defining either 'OPENGL' or 'VULKAN'"
#endif

namespace Kokoro.Engine.Graphics
{
    public class RenderQueue
    {
        public struct DrawData
        {
            public MeshData[] Meshes;
            public RenderState State;
        }

        public struct MeshData
        {
            public Mesh Mesh;
            public int InstanceCount;
            public int BaseInstance;
        }

        private class Bucket
        {
            public RenderState State;
            public List<MeshData> meshes;
            public uint offset;
        }

        private Dictionary<RenderState, Bucket> buckets;
        private List<RenderState> RenderStates;
        private Dictionary<RenderState, List<MeshGroup>> MeshGroups;

        private ShaderStorageBuffer multiDrawParams;
        private bool isRecording = false;
        private int maxDrawCount = 0;

        private const int EntrySize = 0;
        private bool transient;

        public bool ClearFramebufferBeforeSubmit { get; set; } = false;

        public RenderQueue(int MaxDrawCount, bool transient)
        {
            this.transient = transient;
            buckets = new Dictionary<RenderState, Bucket>();
            RenderStates = new List<RenderState>();
            MeshGroups = new Dictionary<RenderState, List<MeshGroup>>();


            if (MaxDrawCount < 4096)
                MaxDrawCount = 4096;

            maxDrawCount = MaxDrawCount;
            multiDrawParams = new ShaderStorageBuffer(MaxDrawCount * 5 * sizeof(uint), false);
        }

        public void UpdateDrawParams(MeshGroup grp, RenderState state, MeshData data)
        {
            Bucket b = buckets[state];

            int j = 0;
            for (int i = 0; i < b.meshes.Count; i++)
                if (b.meshes[i].Mesh == data.Mesh)
                {
                    j = i;
                    break;
                }

            unsafe
            {
                uint* data_uint = (uint*)multiDrawParams.Update();

                data_uint[b.offset / sizeof(uint) + (j * 5) + 2] = (uint)data.InstanceCount;
                data_uint[b.offset / sizeof(uint) + (j * 5) + 5] = (uint)data.BaseInstance;

                multiDrawParams.UpdateDone();
            }
        }

        public void ClearAndBeginRecording()
        {
            if (isRecording) throw new Exception("Already recording.");

            //Clear the buffer
            buckets.Clear();

            //Clear the render state and mesh group lists
            RenderStates.Clear();
            MeshGroups.Clear();

            isRecording = true;
        }

        public void BeginRecording()
        {
            if (isRecording) throw new Exception("Already recording.");
            isRecording = true;
        }

        public void RecordDraws(DrawData[] draws)
        {
            for (int i = 0; i < draws.Length; i++)
                RecordDraw(draws[i]);
        }

        public void RecordDraw(DrawData draw)
        {
            //Group the meshes by state changes
            //Mesh groups can be switched on the fly now, so no need to group them, instead submit them to a compute shader for further culling.
            for (int i = 0; i < draw.Meshes.Length; i++)
            {
                var meshGrp = draw.Meshes[i].Mesh?.Parent;
                var renderState = draw.State;

                var mesh = draw.Meshes[i];

                if (!buckets.ContainsKey(renderState))
                {
                    buckets[renderState] = new Bucket()
                    {
                        State = renderState,
                        meshes = new List<MeshData>()
                    };

                    if (!RenderStates.Contains(renderState)) RenderStates.Add(renderState);
                    if (!MeshGroups.ContainsKey(renderState))
                    {
                        MeshGroups[renderState] = new List<MeshGroup>();
                    }

                    if (!MeshGroups[renderState].Contains(meshGrp) && meshGrp != null)
                    {
                        MeshGroups[renderState].Add(meshGrp);
                    }
                }

                buckets[renderState].meshes.Add(mesh);
            }
        }

        public void EndRecording()
        {
            if (!isRecording) throw new Exception("Not Recording.");

            //Determine if the currently allocated multidraw buffer is large enough

            //Also, perform triple buffering to avoid synchronization if the queue has been hinted as being dynamic


            //Take all the recorded draws in the list and push them into a multidrawindirect buffer for fast draw dispatch
            //Iterate through the list of buckets and build the list of draws to submit
            unsafe
            {
                byte* data = multiDrawParams.Update();

                //Start computing and writing all the data
                uint* data_ui = (uint*)data;

                var bkts = buckets.Values.ToArray();
                for (int i = 0; i < buckets.Count; i++)
                {
                    var bkt = bkts[i];

                    bkt.offset = (uint)data_ui - (uint)data;
                    data_ui[0] = (uint)bkt.meshes.Count;

                    //Index 0 contains the draw count, so all the draw commands themselves are at an offset of 1
                    for (int j = 0; j < bkt.meshes.Count; j++)
                    {
                        var mesh = bkt.meshes[j];

                        if (mesh.Mesh == null)
                            continue;

                        data_ui[(j * 5) + 1] = (uint)mesh.Mesh.IndexCount;   //count
                        data_ui[(j * 5) + 2] = (uint)mesh.InstanceCount;   //instanceCount
                        data_ui[(j * 5) + 3] = (uint)mesh.Mesh.StartOffset;   //first
                        data_ui[(j * 5) + 4] = (uint)mesh.Mesh.StartOffset;   //baseVertex
                        data_ui[(j * 5) + 5] = (uint)mesh.BaseInstance;   //baseInstance
                    }

                    //Move the data pointer forward
                    data_ui += (1 + bkt.meshes.Count);
                }
            }

            //Push the updates
            multiDrawParams.UpdateDone();

            isRecording = false;
        }

        public void Submit()
        {
            if (isRecording)
                throw new Exception("Stop recording before submitting!");

            if (!transient)
                while (!multiDrawParams.IsReady) ;    //Wait for the multidraw buffer to finish updating  


            //Submit the multidraw calls
            for (int i = 0; i < RenderStates.Count; i++)
            {
                if (ClearFramebufferBeforeSubmit)
                {
                    EngineManager.SetRenderState(RenderStates[i]);
                    GraphicsDevice.Clear();
                }

                for (int j = 0; j < MeshGroups[RenderStates[i]].Count; j++)
                {
                    var bkt = buckets[RenderStates[i]];
                    if (bkt.meshes[0].Mesh == null)
                        continue;

                    EngineManager.SetRenderState(bkt.State); //State has been already set if ClearFramebufferBeforeSubmit is true.

                    EngineManager.SetCurrentMeshGroup(bkt.meshes[0].Mesh.Parent);

                    GraphicsDevice.SetMultiDrawParameterBuffer(multiDrawParams);
                    GraphicsDevice.SetParameterBuffer(multiDrawParams);

                    GraphicsDevice.MultiDrawIndirectCount(PrimitiveType.Triangles, bkt.offset + sizeof(uint), bkt.offset, maxDrawCount, true);

                    //Ensure the buffers aren't in use before next update
                    RenderState state = bkt.State;

                    for (int k = 0; state.ShaderStorageBufferBindings != null && k < state.ShaderStorageBufferBindings.Length; k++)
                    {
                        state.ShaderStorageBufferBindings[k].UpdateDone();
                    }
                    for (int k = 0; state.UniformBufferBindings != null && k < state.UniformBufferBindings.Length; k++)
                    {
                        state.UniformBufferBindings[k].UpdateDone();
                    }
                }
            }
        }

    }
}
