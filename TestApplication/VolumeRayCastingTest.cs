using KinectGestureInterface;
using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Graphics.Renderer;
using Kokoro.Engine.Input;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{
    class VolumeRayCastingTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh cube, fsq;
        private RenderState clearState;
        private RenderQueue clearQueue;

        private Texture volumeData;
        private TextureHandle volumeDataHandle, backFaceTexHndl, frontFaceTexHndl;

        private ForwardPlus forwardPlus;

        private RenderState frontFaceState, backFaceState, rayCastState;
        private RenderQueue frontFaceQueue, backFaceQueue, rayCastQueue;
        private Framebuffer frontFaceFB, backFaceFB;
        private Texture frontFaceTex, backFaceTex;

        private VoxelRayCaster caster;

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {
        }

        public void Render(double interval)
        {
            if (!inited)
            {
                //Setup rendering infrastructure
                camera = new FirstPersonCamera(new Keyboard(), Vector3.UnitX, Vector3.UnitY, "FPV")
                {
                    Enabled = true
                };
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                //Load in the volumetric data
                volumeData = new Texture();
                var volSrc = VolumeDataTextureSource.Load("../../../CThead/CThead.", 256, 256, 113);
                volumeData.SetData(volSrc, 0);

                var sampler = new TextureSampler();
                sampler.SetEnableLinearFilter(true);
                sampler.SetAnisotropicFilter(16);

                volumeDataHandle = volumeData.GetHandle(sampler);
                volumeDataHandle.SetResidency(Residency.Resident);

                //Setup the mesh data
                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 1000, 1000);
                //cube = CubeFactory.Create(grp);
                //fsq = FullScreenQuadFactory.Create(grp);

                //Setup forward plus renderer
                forwardPlus = new ForwardPlus(1, 1, 1280, 720, grp);
                caster = new VoxelRayCaster(forwardPlus.Width, forwardPlus.Height, volumeData, grp, forwardPlus.TargetFramebuffer);

                #region Clear
                clearState = new RenderState(forwardPlus.TargetFramebuffer, null, null, null, true, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.None);
                clearQueue = new RenderQueue(10, false);
                clearQueue.ClearAndBeginRecording();
                clearQueue.ClearFramebufferBeforeSubmit = true;
                clearQueue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 0, Mesh = null } },
                    State = clearState
                });
                clearQueue.EndRecording();
                #endregion
                /*
                #region Front Face
                frontFaceTex = new Texture();
                frontFaceTex.SetData(new FramebufferTextureSource(forwardPlus.Width, forwardPlus.Height, 1) { InternalFormat = PixelInternalFormat.Rgba8, PixelType = PixelType.Float }, 0);

                frontFaceFB = new Framebuffer(forwardPlus.Width, forwardPlus.Height);
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
                backFaceTex.SetData(new FramebufferTextureSource(forwardPlus.Width, forwardPlus.Height, 1) { InternalFormat = PixelInternalFormat.Rgba8, PixelType = PixelType.Float }, 0);

                backFaceFB = new Framebuffer(forwardPlus.Width, forwardPlus.Height);
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
                rayCastState = new RenderState(forwardPlus.TargetFramebuffer, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/FrameBuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/VolumeRayCast/raycast.glsl")), null, null, false, true, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.Back);
                rayCastQueue = new RenderQueue(10, false);
                rayCastQueue.ClearAndBeginRecording();
                rayCastQueue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = fsq } },
                    State = rayCastState
                });
                rayCastQueue.EndRecording();

                backFaceTexHndl = backFaceTex.GetHandle(TextureSampler.Default);
                backFaceTexHndl.SetResidency(Residency.Resident);

                frontFaceTexHndl = frontFaceTex.GetHandle(TextureSampler.Default);
                frontFaceTexHndl.SetResidency(Residency.Resident);

                rayCastState.ShaderProgram.Set("VolumeData", volumeDataHandle);
                rayCastState.ShaderProgram.Set("BackFaces", backFaceTexHndl);
                rayCastState.ShaderProgram.Set("FrontFaces", frontFaceTexHndl);
                #endregion
    */
                inited = true;
            }

            /*
            frontFaceState.ShaderProgram.Set("World", Matrix4.Identity);
            frontFaceState.ShaderProgram.Set("View", camera.View);
            frontFaceState.ShaderProgram.Set("Projection", camera.Projection);

            backFaceState.ShaderProgram.Set("World", Matrix4.Identity);
            backFaceState.ShaderProgram.Set("View", camera.View);
            backFaceState.ShaderProgram.Set("Projection", camera.Projection);

            rayCastState.ShaderProgram.Set("World", Matrix4.Identity);
            rayCastState.ShaderProgram.Set("View", camera.View);
            rayCastState.ShaderProgram.Set("Projection", camera.Projection);
            */

            clearQueue.Submit();    //Clear the display

            /*
            //Render the back faces of the cube
            backFaceQueue.Submit();

            //Render the front faces of the cube
            frontFaceQueue.Submit();

            //Perform the ray cast rendering
            rayCastQueue.Submit();
            */

            caster.Draw(Matrix4.Identity, EngineManager.View, EngineManager.Projection, 0);
            forwardPlus.SubmitDraw();   //Update the screen
        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
