using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
using Kokoro.Math;
using Kokoro.StateMachine;

namespace TestApplication
{
    class CPUProcGenTerrainTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private TerrainRenderer terrainRenderer;
        private RenderState state;
        private RenderQueue queue;
        private UniformBuffer textureUBO;
        private ShaderStorageBuffer worldSSBO, texSSBO;
        private Texture tex;
        private TextureHandle handle;

        private Vector3 camPos, camDir;
        private bool updateCamPos = true;

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

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("heightmap.png", 1);
                tex = new Texture();
                tex.SetData(bitmapSrc, 0);
                textureUBO = new UniformBuffer();
                worldSSBO = new ShaderStorageBuffer(16 * 2048, true);
                texSSBO = new ShaderStorageBuffer(16 * 2048, true);

                handle = tex.GetHandle(TextureSampler.Default);
                handle.SetResidency(TextureResidency.Resident);

                unsafe
                {
                    for (int i = 0; i < 4; i++)
                    {
                        long* l = (long*)textureUBO.Update();
                        l[0] = handle;
                        textureUBO.UpdateDone();
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        long* l = (long*)texSSBO.Update();
                        for (int j = 0; j < 2048; j++)
                        {
                            l[j * 2] = handle;
                        }
                        texSSBO.UpdateDone();
                    }
                }

                state = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/TerrainRenderer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/TerrainRenderer/fragment.glsl")), new ShaderStorageBuffer[] { worldSSBO, texSSBO }, null, true, DepthFunc.LEqual, -1, 1, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 1, CullFaceMode.None);
                state.ShaderProgram.SetShaderStorageBufferMapping("transforms", 0);
                state.ShaderProgram.SetShaderStorageBufferMapping("heightmaps", 1);
                
                terrainRenderer = new TerrainRenderer(5000, grp, camera, state, worldSSBO, texSSBO, handle);

                inited = true;
            }

            state.ShaderProgram.Set("View", camera.View);
            state.ShaderProgram.Set("Projection", camera.Projection);

            bool terrainUpdateNeeded = false;


            if (updateCamPos)
            {
                if (camPos != camera.Position)
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