﻿using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Input;
using Kokoro.Graphics.OpenGL;
using Kokoro.Graphics.OpenGL.ShaderLibraries;
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
        private Texture tex;
        private TextureHandle handle;
        private TextureCache cache;

        private Vector3 camPos;

        private Keyboard keybd;
        private bool updateCamPos;

        public void Enter(IState prev)
        {

        }

        public void Exit(IState next)
        {
        }

        Vector3 pos;
        double x = 0;
        IEnumerator<Vector3> enume;
        public IEnumerable<Vector3> GetCamPos()
        {
            while (true)
            //for (double x = 0; x < System.Math.PI * 2; x += 0.1)
            {
                x += 0.1;
                x = x % System.Math.PI * 2;

                double y = System.Math.Sqrt(100 - x * x);
                pos = new Vector3((float)x, 0.5f, (float)y);
                yield return pos;
            }
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

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 20000, 20000);

                BitmapTextureSource bitmapSrc = new BitmapTextureSource("heightmap.png", 1);
                tex = new Texture();
                tex.SetData(bitmapSrc, 0);

                handle = tex.GetHandle(TextureSampler.Default);
                handle.SetResidency(Residency.Resident);

                //GraphicsDevice.Wireframe = true;

                cache = new TextureCache(1024, 64, 64, 1, PixelFormat.Rgba, PixelInternalFormat.Rgba8, PixelType.Byte);
                terrainRenderer = new TerrainRenderer(5000, grp, new Framebuffer[] { Framebuffer.Default }, 0, 2, 0, ShaderSource.Load(ShaderType.ComputeShader, "Shaders/TerrainSource/compute.glsl", Noise.Name), cache);

                inited = true;
            }


            if (keybd.IsKeyReleased("ToggleCamera"))
            {
                updateCamPos = !updateCamPos;
            }

            if (updateCamPos && camPos != camera.Position)
            {
                camPos = camera.Position;

                terrainRenderer.Update(camPos, camera.Direction);
            }
            terrainRenderer.Draw(camera.View, camera.Projection);

        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
