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
    class FenceTest : IState
    {
        private bool inited = false;
        List<Fence> fences;
        List<Fence> placedFences;
        UniformBuffer buf;
        Mesh mesh;
        MeshGroup grp;
        RenderQueue q;
        RenderState s;
        Texture tex;
        TextureHandle h;

        private Keyboard keybd;
        Random r;

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


                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 1000, 1000);
                mesh = FullScreenQuadFactory.Create(grp);

                BitmapTextureSource src = new BitmapTextureSource("test.png", 1);
                tex = new Texture();
                tex.SetData(src, 0);

                h = tex.GetHandle(TextureSampler.Default);
                h.SetResidency(Residency.Resident);
                buf = new UniformBuffer(false);


                unsafe
                {
                    for (int i = 0; i < 4; i++)
                    {
                        long* f = (long*)buf.Update();
                        f[0] = h;
                        buf.UpdateDone();
                    }
                }

                s = new RenderState(Framebuffer.Default, new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Graphics/OpenGL/Shaders/Framebuffer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Graphics/OpenGL/Shaders/Default/fragment.glsl")), null, new UniformBuffer[] { buf }, false, DepthFunc.Always, 0, 1, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);
                s.ShaderProgram.SetUniformBufferMapping("Material_t", 0);

                q = new RenderQueue(10, false);

                q.ClearAndBeginRecording();
                q.ClearFramebufferBeforeSubmit = true;
                q.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] { new RenderQueue.MeshData() { BaseInstance = 0, InstanceCount = 1, Mesh = mesh } },
                    State = s
                });
                q.EndRecording();
                System.Threading.Thread.Sleep(1000);

                inited = true;
            }


            q.Submit();
        }

        public void Update(double interval)
        {
        }
    }
}
