using Kokoro.Engine;
using Kokoro.Engine.Graphics;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.StateMachine;
using Kokoro.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRWorld.Base
{
    class SphereVRScene : IState
    {
        const int Left = 0;
        const int Right = 1;

        struct VRComponents
        {
            public Matrix4 Projection;
        }

        VRRenderer vr;
        VRComponents[] vr_data;
        RenderQueue clearQ;

        MeshGroup meshGroup;
        Mesh sphere;
        Matrix4[] spherePos;
        ShaderStorageBuffer sphereTransforms;
        UniformBuffer sphereTexture;
        RenderQueue[] sphereQ;
        ShaderProgram[] sphereShader;

        public SphereVRScene()
        {

        }

        public void Enter(IState prev)
        {
            {
                vr = VRRenderer.Create();
                vr_data = new VRComponents[2];

                for (int i = 0; i < 2; i++)
                {
                    vr_data[i] = new VRComponents()
                    {
                        Projection = vr.GetEyeProjection(i == Left, 0.01f)
                    };
                }

                var RenderState0 = new RenderState(vr.LeftFramebuffer, null, null, null, true, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);
                var RenderState1 = new RenderState(vr.RightFramebuffer, null, null, null, true, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);
                var RenderState_scr = new RenderState(Framebuffer.Default, null, null, null, true, true, DepthFunc.Always, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.One, 0, CullFaceMode.None);
                clearQ = new RenderQueue(3, false);
                clearQ.BeginRecording();
                clearQ.ClearFramebufferBeforeSubmit = true;
                clearQ.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = 0,
                            Mesh = null
                        }
                    },
                    State = RenderState_scr
                });
                clearQ.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = 0,
                            Mesh = null
                        }
                    },
                    State = RenderState0
                });
                clearQ.RecordDraw(new RenderQueue.DrawData()
                {
                    Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = 0,
                            Mesh = null
                        }
                    },
                    State = RenderState1
                });
                clearQ.EndRecording();
            }

            {
                meshGroup = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 30000, 30000);
                sphere = SphereFactory.Create(meshGroup);
                spherePos = new Matrix4[9 * 9];
                for (int y = 0; y < 9; y++)
                    for (int x = 0; x < 9; x++)
                        spherePos[y * 9 + x] = Matrix4.CreateTranslation((x - 5) * 3, 0, (y - 5) * 3);

                unsafe
                {
                    sphereTransforms = new ShaderStorageBuffer(spherePos.Length * sizeof(Matrix4), false);
                    var b_ptr = sphereTransforms.Update();
                    fixed (Matrix4* mats = spherePos)
                    {
                        long* s = (long*)mats;
                        long* d = (long*)b_ptr;

                        for (int i = 0; i < sizeof(Matrix4) * spherePos.Length / sizeof(long); i++)
                        {
                            d[i] = s[i];
                        }
                    }
                    sphereTransforms.UpdateDone();

                    sphereTexture = new UniformBuffer(false);

                    b_ptr = sphereTexture.Update();
                    long* l_ptr = (long*)b_ptr;
                    l_ptr[0] = Texture.Default.GetHandle(TextureSampler.Default);
                    sphereTexture.UpdateDone();

                    Texture.Default.GetHandle(TextureSampler.Default).SetResidency(Residency.Resident);
                }

                sphereShader = new ShaderProgram[2];
                for (int i = 0; i < 2; i++) sphereShader[i] = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Default/fragment.glsl"));
                var RenderState0 = new RenderState(vr.LeftFramebuffer, sphereShader[Left], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { sphereTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                var RenderState1 = new RenderState(vr.RightFramebuffer, sphereShader[Right], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { sphereTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                var RenderState_scr = new RenderState(Framebuffer.Default, sphereShader[Right], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { sphereTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);

                sphereQ = new RenderQueue[3];

                for (int i = 0; i < 3; i++)
                {
                    sphereQ[i] = new RenderQueue(3, false);
                    sphereQ[i].BeginRecording();
                    if (i == 0) sphereQ[i].RecordDraw(new RenderQueue.DrawData()
                    {
                        Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = spherePos.Length,
                            Mesh = sphere
                        }
                    },
                        State = RenderState_scr
                    });
                    if (i == 0) sphereQ[i].RecordDraw(new RenderQueue.DrawData()
                    {
                        Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = spherePos.Length,
                            Mesh = sphere
                        }
                    },
                        State = RenderState1
                    });
                    if (i == 0) sphereQ[i].RecordDraw(new RenderQueue.DrawData()
                    {
                        Meshes = new RenderQueue.MeshData[] {
                        new RenderQueue.MeshData(){
                            BaseInstance = 0,
                            InstanceCount = spherePos.Length,
                            Mesh = sphere
                        }
                    },
                        State = RenderState0
                    });
                    sphereQ[i].EndRecording();
                }
            }
        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {
            clearQ.Submit();
            var pose = vr.GetPose();


            var leftEyeView = vr.GetEyeView(true);
            var rightEyeView = vr.GetEyeView(false);

            sphereShader[Left].Set("View", leftEyeView * pose);
            sphereShader[Right].Set("View", rightEyeView * pose);

            for (int i = 0; i < 2; i++)
                sphereShader[i].Set("Projection", vr_data[i].Projection);

            sphereQ[0].Submit();
            //sphereQ[1].Submit();
            //sphereQ[2].Submit();

            for (int i = 0; i < 2; i++)
                vr.Submit(i == Left);
        }

        public void Update(double interval)
        {

        }
    }
}
