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
    internal static class MemoryAllocator
    {
        //Net Vertex Count:
        //49,000,000 vertices
        //3 components for vertices, 2 for UVs, 2 for normals, 1 for Indices
        // (750 * 40960 + 1000*16384 + 363*4096 + 300*1024 + 304*256 + 302*64 + 300*16))
        //NOTE: Indices are 16 bit unsigned integers, this means that a mesh may not have more than 65536 vertices, allowing 251 meshes to be loaded into memory at any given moment

        const int vertex_cnt = 49000000;
        const int index_cnt = (int)(vertex_cnt);

        static GPUBuffer vertices;
        static GPUBuffer uvs;
        static GPUBuffer normals;
        static GPUBuffer indices;
        internal static VertexArray varray;

        static Dictionary<int, bool[]> usedBlocks;
        static Dictionary<int, int> baseOffsets;

        public enum IntPtrIndex
        {
            Index = 0,
            Vertex = 1,
            UV = 2,
            Normal = 3
        }

        static MemoryAllocator()
        {
            vertices = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer, vertex_cnt * 3 * sizeof(float), false);
            uvs = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer, vertex_cnt * 2 * sizeof(ushort), false);
            normals = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer, vertex_cnt * sizeof(uint), false);
            indices = new GPUBuffer(OpenTK.Graphics.OpenGL.BufferTarget.ElementArrayBuffer, index_cnt * sizeof(ushort), false);

            usedBlocks = new Dictionary<int, bool[]>();

            usedBlocks[40960] = new bool[750];
            usedBlocks[16384] = new bool[1000];
            usedBlocks[4096] = new bool[363];
            usedBlocks[1024] = new bool[300];
            usedBlocks[256] = new bool[304];
            usedBlocks[64] = new bool[302];
            usedBlocks[16] = new bool[300];

            baseOffsets = new Dictionary<int, int>();
            baseOffsets[40960] = 0;
            baseOffsets[16384] = usedBlocks[40960].Length * 40960;
            baseOffsets[4096] = baseOffsets[16384] + usedBlocks[16384].Length * 16384;
            baseOffsets[1024] = baseOffsets[4096] + usedBlocks[4096].Length * 4096;
            baseOffsets[256] = baseOffsets[1024] + usedBlocks[1024].Length * 1024;
            baseOffsets[64] = baseOffsets[256] + usedBlocks[256].Length * 256;
            baseOffsets[16] = baseOffsets[64] + usedBlocks[64].Length * 64;

            varray = new VertexArray();
            varray.SetBufferObject(0, vertices, 3, OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float, false);
            varray.SetBufferObject(1, uvs, 2, OpenTK.Graphics.OpenGL.VertexAttribPointerType.UnsignedShort, true);
            varray.SetBufferObject(2, normals, 4, OpenTK.Graphics.OpenGL.VertexAttribPointerType.UnsignedInt2101010Rev, true);
            varray.SetElementBufferObject(indices);
            GraphicsDevice.SetVertexArray(varray);

            //NOTE: Meshes are automatically expelled from memory as needed, the engine knows about which meshes should be visible based on the player's position in the scene, thus it can handle evicting meshes as necessary
            //Have multiple block sizes, 65536, 16384, 4096, 1024, 256, 64, 16
        }

        private static int AllocateBlock(int cnt)
        {
            //Find the smallest value larger than cnt
            int max = int.MaxValue;

            for(int i = 0; i < usedBlocks.Keys.Count; i++)
            {
                int val = usedBlocks.Keys.ElementAt(i);
                if (val < max && val >= cnt)
                {
                    max = val;
                }
            }

            int sub_index = -1;

            //Allocate the memory by calculating the offset from the main buffer
            for(int i = 0; i < usedBlocks[max].Length; i++)
            {
                if (!usedBlocks[max][i])
                {
                    sub_index = i;
                    usedBlocks[max][i] = true;
                }
            }

            //sub_index found, now start calculating offsets
            return baseOffsets[max] + sub_index * max;
        }

        public static IntPtr[] AllocateMemory(int cnt, out int offset)
        {
            offset = AllocateBlock(cnt);

            IntPtr[] result = new IntPtr[4];
            result[(int)IntPtrIndex.Index] = indices.GetPtr() + (offset * sizeof(ushort));
            result[(int)IntPtrIndex.Vertex] = vertices.GetPtr() + (offset * 3 * sizeof(float));
            result[(int)IntPtrIndex.UV] = uvs.GetPtr() + (offset * 2 * sizeof(float));
            result[(int)IntPtrIndex.Normal] = normals.GetPtr() + (offset * sizeof(uint));

            return result;
        }

        public static void FlushBuffer(IntPtrIndex idx, int offset, int size)
        {
            switch (idx)
            {
                case IntPtrIndex.Index:
                    indices.FlushBuffer(indices.GetPtr() + (offset * sizeof(ushort)), size);
                    break;
                case IntPtrIndex.Normal:
                    normals.FlushBuffer(normals.GetPtr() + (offset * sizeof(uint)), size);
                    break;
                case IntPtrIndex.UV:
                    uvs.FlushBuffer(uvs.GetPtr() + (offset * 2 * sizeof(float)), size);
                    break;
                case IntPtrIndex.Vertex:
                    vertices.FlushBuffer(vertices.GetPtr() + (offset * 3 * sizeof(float)), size);
                    break;
            }

            //Place a fence here to allow freeing to be deferred to when the resource is no longer in use
        }

        public static void FreeMemory(int offset)
        {
            //Find the smallest value larger than cnt
            int max = 0;

            for (int i = 0; i < usedBlocks.Keys.Count; i++)
            {
                int val = usedBlocks.Keys.ElementAt(i);
                if (val <= offset && val > max)
                {
                    max = val;
                }
            }

            int sub_offset = (offset - baseOffsets[max]) / max;
            if (!usedBlocks[max][sub_offset]) throw new Exception();
            else usedBlocks[max][sub_offset] = false;
        }

    }
}
