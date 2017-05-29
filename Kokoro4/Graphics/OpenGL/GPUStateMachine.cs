using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.OpenGL
{
    public static class GPUStateMachine
    {
        static Stack<int> vertexArrays, framebuffers;
        static List<Dictionary<TextureTarget, Stack<int>>> boundTextures;
        static Dictionary<BufferTarget, List<Stack<int>>> boundBuffers;

        static GPUStateMachine()
        {
            boundBuffers = new Dictionary<BufferTarget, List<Stack<int>>>();
            boundBuffers[BufferTarget.TransformFeedbackBuffer] = new List<Stack<int>>();
            boundBuffers[BufferTarget.UniformBuffer] = new List<Stack<int>>();
            boundBuffers[BufferTarget.ShaderStorageBuffer] = new List<Stack<int>>();

            for (int i = 0; i < 8; i++)
            {
                boundBuffers[BufferTarget.TransformFeedbackBuffer].Add(new Stack<int>());
                boundBuffers[BufferTarget.TransformFeedbackBuffer][i].Push(0);

                boundBuffers[BufferTarget.UniformBuffer].Add(new Stack<int>());
                boundBuffers[BufferTarget.UniformBuffer][i].Push(0);

                boundBuffers[BufferTarget.ShaderStorageBuffer].Add(new Stack<int>());
                boundBuffers[BufferTarget.ShaderStorageBuffer][i].Push(0);
            }

            boundBuffers[BufferTarget.ArrayBuffer] = new List<Stack<int>>();
            boundBuffers[BufferTarget.ArrayBuffer].Add(new Stack<int>());
            boundBuffers[BufferTarget.ArrayBuffer][0].Push(0);

            boundBuffers[BufferTarget.TextureBuffer] = new List<Stack<int>>();
            boundBuffers[BufferTarget.TextureBuffer].Add(new Stack<int>());
            boundBuffers[BufferTarget.TextureBuffer][0].Push(0);
            
            boundBuffers[BufferTarget.PixelUnpackBuffer] = new List<Stack<int>>();
            boundBuffers[BufferTarget.PixelUnpackBuffer].Add(new Stack<int>());
            boundBuffers[BufferTarget.PixelUnpackBuffer][0].Push(0);
            
            boundBuffers[BufferTarget.PixelPackBuffer] = new List<Stack<int>>();
            boundBuffers[BufferTarget.PixelPackBuffer].Add(new Stack<int>());
            boundBuffers[BufferTarget.PixelPackBuffer][0].Push(0);

            boundBuffers[BufferTarget.ElementArrayBuffer] = new List<Stack<int>>();
            boundBuffers[BufferTarget.ElementArrayBuffer].Add(new Stack<int>());
            boundBuffers[BufferTarget.ElementArrayBuffer][0].Push(0);

            boundTextures = new List<Dictionary<TextureTarget, Stack<int>>>();
            for (int i = 0; i < 8; i++)
            {
                boundTextures.Add(new Dictionary<TextureTarget, Stack<int>>());
                boundTextures[i][TextureTarget.Texture2D] = new Stack<int>();
                boundTextures[i][TextureTarget.Texture2D].Push(0);
                boundTextures[i][TextureTarget.TextureCubeMap] = new Stack<int>();
                boundTextures[i][TextureTarget.TextureCubeMap].Push(0);
                boundTextures[i][TextureTarget.TextureBuffer] = new Stack<int>();
                boundTextures[i][TextureTarget.TextureBuffer].Push(0);
            }

            vertexArrays = new Stack<int>();
            vertexArrays.Push(0);

            framebuffers = new Stack<int>();
            framebuffers.Push(0);
        }

        #region Buffer object state
        public static void BindBuffer(BufferTarget target, int id)
        {
            if (target == BufferTarget.TransformFeedbackBuffer) throw new Exception("Incorrect Function Called, Use Overload for TransformFeedbackBuffers");
            if (boundBuffers[target][0].Count == 0) boundBuffers[target][0].Push(0);

            if (boundBuffers[target][0].Peek() != id || id == 0) GL.BindBuffer(target, id);
            boundBuffers[target][0].Push(id);
        }

        public static void UnbindBuffer(BufferTarget target)
        {
            boundBuffers[target][0].Pop();
            BindBuffer(target, boundBuffers[target][0].Pop());
        }
        #endregion

        #region Uniform buffer object state
        public static void BindBuffer(BufferTarget target, int id, int index, IntPtr off, IntPtr size)
        {
            if (target != BufferTarget.TransformFeedbackBuffer && target != BufferTarget.UniformBuffer && target != BufferTarget.ShaderStorageBuffer) throw new Exception("Incorrect Function Called, Use other Overload");
            if (boundBuffers[target][index].Count == 0) boundBuffers[target][index].Push(0);

            //if (boundBuffers[target][index].Peek() != id || id == 0)
            {
                if (size == IntPtr.Zero) GL.BindBufferBase((BufferRangeTarget)target, index, id);
                else GL.BindBufferRange((BufferRangeTarget)target, index, id, off, size);
            }
            boundBuffers[target][index].Push(id);
        }

        public static void UnbindBuffer(BufferTarget target, int index)
        {
            boundBuffers[target][index].Pop();
            BindBuffer(target, boundBuffers[target][index].Pop());
        }
        #endregion

        #region Texture state
        public static void BindTexture(int index, TextureTarget target, int id)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + index);
            if (boundTextures[index][target].Count == 0) boundTextures[index][target].Push(0);

            if (boundTextures[index][target].Peek() != id || id == 0) GL.BindTexture(target, id);
            boundTextures[index][target].Push(id);
        }

        public static void UnbindTexture(int index, TextureTarget target)
        {
            boundTextures[index][target].Pop();
            BindTexture(index, target, boundTextures[index][target].Pop());
        }
        #endregion

        #region Vertex Array State
        public static void BindVertexArray(int id)
        {
            if (vertexArrays.Count == 0) vertexArrays.Push(0);

            if (vertexArrays.Peek() != id || id == 0) GL.BindVertexArray(id);
            vertexArrays.Push(id);
        }

        public static void UnbindVertexArray()
        {
            vertexArrays.Pop();
            BindVertexArray(vertexArrays.Pop());
        }
        #endregion

        #region Framebuffer State
        public static void BindFramebuffer(int id)
        {
            if (framebuffers.Count == 0) framebuffers.Push(0);

            if (framebuffers.Peek() != id || id == 0) GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            framebuffers.Push(id);
        }

        public static void UnbindFramebuffer()
        {
            framebuffers.Pop();
            BindFramebuffer(framebuffers.Pop());
        }
        #endregion

        #region Viewport State
        static Vector4 viewport;
        public static void SetViewport(int x, int y, int width, int height)
        {
            viewport.X = x;
            viewport.Y = y;
            viewport.Z = width;
            viewport.W = height;
            GL.Viewport(x, y, width, height);
        }
        #endregion
    }
}
