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

                camera = new FirstPersonCamera(keybd, Vector3.UnitX, Vector3.UnitY, "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 20000, 20000);

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("heightmap.png", 1);
                tex = new Texture();
                tex.SetData(bitmapSrc, 0);

                handle = tex.GetHandle(TextureSampler.Default);
                handle.SetResidency(TextureResidency.Resident);

                terrainRenderer = new TerrainRenderer(5000, grp, Framebuffer.Default, handle);

                inited = true;
            }

            bool terrainUpdateNeeded = false;

            if (camPos != camera.Position)
            {
                terrainUpdateNeeded = true;
            }
            camPos = camera.Position;
            camDir = camera.Direction;

            if (terrainUpdateNeeded) terrainRenderer.Update(camPos, camDir);
            terrainRenderer.Draw(camera.View, camera.Projection);

        }

        public void Update(double interval)
        {
            camera?.Update(interval);

        }
    }
}