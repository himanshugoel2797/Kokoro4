using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Graphics.OpenGL;
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
    class QuadTreeTerrainTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh mesh;
        private RenderState state;
        private RenderQueue queue;
        private UniformBuffer textureUBO;
        private ShaderStorageBuffer worldSSBO;
        private Texture tex;
        private TextureStreamer.TextureStream stream;
        private TextureHandle handle;

        private TextureStreamer tStreamer;

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
                camera = new FirstPersonCamera(Vector3.UnitX, Vector3.UnitY, "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                tStreamer = new TextureStreamer(10);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 10000, 10000);
                mesh = QuadFactory.Create(grp, 10, 10);

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("test.png", 10);
                stream = tStreamer.UploadTexture(bitmapSrc);
                tex = stream.TargetTexture;
                textureUBO = new UniformBuffer();
                worldSSBO = new ShaderStorageBuffer(16 * sizeof(float));

                GraphicsDevice.Wireframe = true;

                unsafe
                {
                    float* f = (float*)worldSSBO.Update();
                    float[] ident = (float[])Matrix4.Identity;

                    for (int i = 0; i < ident.Length; i++)
                    {
                        f[i] = ident[i];
                    }

                    worldSSBO.UpdateDone();
                }

                state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Default/fragment.glsl")), new ShaderStorageBuffer[] { worldSSBO }, new UniformBuffer[] { textureUBO }, true, DepthFunc.LEqual, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 1, CullFaceMode.None);
                state.ShaderProgram.SetShaderStorageBufferMapping("transforms", 0);
                state.ShaderProgram.SetUniformBufferMapping("Material_t", 0);

                queue = new RenderQueue(10);

                queue.ClearAndBeginRecording();
                queue.ClearFramebufferBeforeSubmit = true;
                queue.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = mesh } },
                    State = state
                });
                queue.EndRecording();

                inited = true;
            }

            state.ShaderProgram.Set("View", camera.View);
            state.ShaderProgram.Set("Projection", camera.Projection);

            if (stream != null)
            {
                handle?.SetResidency(TextureResidency.NonResident);
                TextureSampler sampler = stream.TargetSampler;
                handle = tex.GetHandle(sampler);
                handle.SetResidency(TextureResidency.Resident);

                GC.Collect();

                unsafe
                {
                    long* l = (long*)textureUBO.Update();
                    l[0] = handle;
                    textureUBO.UpdateDone();
                }

                if (stream.IsDone)
                {
                    stream.Free();
                    stream = null;
                }
            }


            queue.Submit();

        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
