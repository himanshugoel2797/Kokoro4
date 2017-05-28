using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
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
        private TerrainRenderer terrainRenderer;
        private RenderState state;
        private RenderQueue queue;
        private UniformBuffer textureUBO;
        private ShaderStorageBuffer worldSSBO;
        private Texture tex;
        private TextureHandle handle;

        private Vector3 camPos, camDir;
        private bool updateCamPos;

        private Keyboard keybd;

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
                keybd = new Keyboard();
                keybd.KeyMap.Add("ToggleCamPos", Key.Z);

                camera = new FirstPersonCamera(Vector3.UnitX, Vector3.UnitY, "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 20000, 20000);

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("test.png", 1);
                tex = new Texture();
                tex.SetData(bitmapSrc, 0);
                textureUBO = new UniformBuffer();
                worldSSBO = new ShaderStorageBuffer(16 * sizeof(float) * 2048);
                handle = tex.GetHandle(TextureSampler.Default);
                handle.SetResidency(TextureResidency.Resident);


                GraphicsDevice.Wireframe = true;
                unsafe
                {
                    long* l = (long*)textureUBO.Update();
                    l[0] = handle;
                    textureUBO.UpdateDone();
                }

                state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Default/fragment.glsl")), new ShaderStorageBuffer[] { worldSSBO }, new UniformBuffer[] { textureUBO }, true, DepthFunc.LEqual, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 1, CullFaceMode.None);
                state.ShaderProgram.SetShaderStorageBufferMapping("transforms", 0);
                state.ShaderProgram.SetUniformBufferMapping("Material_t", 0);

                terrainRenderer = new TerrainRenderer(50000, grp, camera, state, worldSSBO);

                inited = true;
            }

            state.ShaderProgram.Set("View", camera.View);
            state.ShaderProgram.Set("Projection", camera.Projection);

            bool terrainUpdateNeeded = false;

            if (updateCamPos)
            {
                if (camPos != camera.Position | camDir != camera.Direction)
                {
                    terrainUpdateNeeded = true;
                }
                camPos = camera.Position;
                camDir = camera.Direction;
            }

            if (terrainUpdateNeeded) terrainRenderer.Update(camPos, camDir);
            terrainRenderer.Draw();

        }

        public void Update(double interval)
        {
            camera?.Update(interval);

            if (keybd?.IsKeyReleased("ToggleCamPos") == true)
            {
                updateCamPos = !updateCamPos;
            }
        }
    }
}
