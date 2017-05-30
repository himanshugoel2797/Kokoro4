using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
using Kokoro.Graphics.OpenGL;
using Kokoro.Math;
using Kokoro.StateMachine;
using System;

namespace TestApplication
{
    class CubeTerrainTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private TerrainRenderer[] terrainRenderers;
        private Texture tex;
        private TextureHandle handle;

        private Vector3 camPos;
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
                keybd.KeyMap["ToggleCamera"] = Key.Z;

                camera = new FirstPersonCamera(keybd, Vector3.UnitX, Vector3.UnitY, "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 100000, 100000);

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("heightmap.png", 1);
                tex = new Texture();
                tex.SetData(bitmapSrc, 0);

                handle = tex.GetHandle(TextureSampler.Default);
                handle.SetResidency(TextureResidency.Resident);

                //GraphicsDevice.Wireframe = true;

                float side = 500;
                float off = side * 0.5f;
                Framebuffer fbuf = Framebuffer.Default;

                terrainRenderers = new TerrainRenderer[] {
                new TerrainRenderer(side, grp, fbuf, 0, 2, off),
                new TerrainRenderer(side, grp, fbuf, 0, 2, -off),
                new TerrainRenderer(side, grp, fbuf, 0, 1, off),
                new TerrainRenderer(side, grp, fbuf, 0, 1, -off),
                new TerrainRenderer(side, grp, fbuf, 1, 2, off),
                new TerrainRenderer(side, grp, fbuf, 1, 2, -off),
                };

                terrainRenderers[0].Queue.ClearFramebufferBeforeSubmit = true;

                inited = true;
            }

            if (keybd.IsKeyReleased("ToggleCamera"))
            {
                updateCamPos = !updateCamPos;
            }

            if (updateCamPos && camPos != camera.Position)
            {
                camPos = camera.Position;

                foreach (TerrainRenderer r in terrainRenderers)
                    r.Update(camPos, camera.Direction);
            }

            foreach (TerrainRenderer r in terrainRenderers)
            {

                //if(Vector3.Dot(camera.Direction, r.Normal) <= 0)
                r.Draw(camera.View, camera.Projection);

            }

        }

        public void Update(double interval)
        {
            camera?.Update(interval);

        }
    }
}