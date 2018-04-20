using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class VoxelRayCaster
    {
        private Mesh fsq, cube;
        private TextureHandle volumeDataHandle;
        private RenderState frontFaceState, backFaceState;
        private RenderQueue frontFaceQueue, backFaceQueue;
        private RenderState[] rayCastState;
        private RenderQueue[] rayCastQueue;

        private Framebuffer frontFaceFB, backFaceFB;
        private Texture frontFaceTex, backFaceTex;

        public VoxelRayCaster(int w, int h, Texture data, MeshGroup grp, params Framebuffer[] fbufs)
        {
            cube = CubeFactory.Create(grp);
            fsq = FullScreenQuadFactory.Create(grp);

            var sampler = new TextureSampler();
            sampler.SetEnableLinearFilter(true);
            sampler.SetAnisotropicFilter(16);

            volumeDataHandle = data.GetHandle(sampler);
            volumeDataHandle.SetResidency(Residency.Resident);

            #region Front Face
            frontFaceTex = new Texture();
            frontFaceTex.SetData(new FramebufferTextureSource(w, h, 1) { InternalFormat = PixelInternalFormat.Rgba8, PixelType = PixelType.Float }, 0);

            frontFaceFB = new Framebuffer(w, h);
            frontFaceFB[FramebufferAttachment.ColorAttachment0] = frontFaceTex;

            frontFaceState = new RenderState(frontFaceFB, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/VolumeRayCast/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/VolumeRayCast/fragment.glsl")), null, null, false, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.Back);
            frontFaceQueue = new RenderQueue(10, false);
            frontFaceQueue.ClearAndBeginRecording();
            frontFaceQueue.ClearFramebufferBeforeSubmit = true;
            frontFaceQueue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = cube } },
                State = frontFaceState
            });
            frontFaceQueue.EndRecording();
            #endregion

            #region Back Face
            backFaceTex = new Texture();
            backFaceTex.SetData(new FramebufferTextureSource(w, h, 1) { InternalFormat = PixelInternalFormat.Rgba8, PixelType = PixelType.Float }, 0);

            backFaceFB = new Framebuffer(w, h);
            backFaceFB[FramebufferAttachment.ColorAttachment0] = backFaceTex;

            backFaceState = new RenderState(backFaceFB, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/VolumeRayCast/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/VolumeRayCast/fragment.glsl")), null, null, false, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.Front);
            backFaceQueue = new RenderQueue(10, false);
            backFaceQueue.ClearAndBeginRecording();
            backFaceQueue.ClearFramebufferBeforeSubmit = true;
            backFaceQueue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = cube } },
                State = backFaceState
            });
            backFaceQueue.EndRecording();
            #endregion

            #region Ray Cast
            rayCastState = new RenderState[fbufs.Length];
            rayCastQueue = new RenderQueue[fbufs.Length];

            for (int i = 0; i < fbufs.Length; i++)
            {
                rayCastState[i] = new RenderState(fbufs[i], new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/VolumeRayCast/raycast.glsl")), null, null, false, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.Back);
                rayCastQueue[i] = new RenderQueue(10, false);
                rayCastQueue[i].ClearAndBeginRecording();
                rayCastQueue[i].RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = fsq } },
                    State = rayCastState[i]
                });
                rayCastQueue[i].EndRecording();

                var backFaceTexHndl = backFaceTex.GetHandle(TextureSampler.Default);
                backFaceTexHndl.SetResidency(Residency.Resident);

                var frontFaceTexHndl = frontFaceTex.GetHandle(TextureSampler.Default);
                frontFaceTexHndl.SetResidency(Residency.Resident);

                rayCastState[i].ShaderProgram.Set("VolumeData", volumeDataHandle);
                rayCastState[i].ShaderProgram.Set("BackFaces", backFaceTexHndl);
                rayCastState[i].ShaderProgram.Set("FrontFaces", frontFaceTexHndl);
            }
            #endregion
        }

        public void Draw(Matrix4 World, Matrix4 View, Matrix4 Projection, int idx = 0)
        {
            frontFaceState.ShaderProgram.Set("World", Matrix4.Identity);
            frontFaceState.ShaderProgram.Set("View", View);
            frontFaceState.ShaderProgram.Set("Projection", Projection);

            backFaceState.ShaderProgram.Set("World", Matrix4.Identity);
            backFaceState.ShaderProgram.Set("View", View);
            backFaceState.ShaderProgram.Set("Projection", Projection);

            rayCastState[idx].ShaderProgram.Set("World", Matrix4.Identity);
            rayCastState[idx].ShaderProgram.Set("View", View);
            rayCastState[idx].ShaderProgram.Set("Projection", Projection);

            //Render the back faces of the cube
            backFaceQueue.Submit();

            //Render the front faces of the cube
            frontFaceQueue.Submit();

            //Perform the ray cast rendering
            rayCastQueue[idx].Submit();
        }
    }
}
