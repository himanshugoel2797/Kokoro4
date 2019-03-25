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
        struct VRComponents
        {
            public Matrix4 Projection;
            public Mesh[] model;
            public Texture[] model_tex;
        }

        VRClient vr;
        VRComponents[] vr_data;
        RenderQueue clearQ;

        MeshGroup meshGroup;
        Mesh sphere;
        Matrix4[] spherePos;
        ShaderStorageBuffer sphereTransforms;
        UniformBuffer sphereTexture, ctrlTexture;
        RenderQueue sphereQ;
        ShaderProgram[] sphereShader, ctrl_shader;
        int transform_off;

        public SphereVRScene()
        {

        }

        public void Enter(IState prev)
        {
            {
                vr = VRClient.Create(ExperienceKind.Standing);
                vr_data = new VRComponents[2];

                for (int i = 0; i < 2; i++)
                {
                    vr_data[i] = new VRComponents()
                    {
                        Projection = vr.GetEyeProjection(VRHand.Get(i), 0.01f)
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
                    sphereTransforms = new ShaderStorageBuffer(256 * sizeof(Matrix4), false);
                    var b_ptr = sphereTransforms.Update();
                    fixed (Matrix4* mats = spherePos)
                    {
                        long* s = (long*)mats;
                        long* d = (long*)b_ptr;

                        for (int i = 0; i < sizeof(Matrix4) * spherePos.Length / sizeof(long); i++)
                        {
                            d[i] = s[i];
                            transform_off = i;
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
                var RenderState0 = new RenderState(vr.LeftFramebuffer, sphereShader[VRHand.Left.Value], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { sphereTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                var RenderState1 = new RenderState(vr.RightFramebuffer, sphereShader[VRHand.Right.Value], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { sphereTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                var RenderState_scr = new RenderState(Framebuffer.Default, sphereShader[VRHand.Right.Value], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { sphereTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);

                sphereQ = new RenderQueue(3, false);
                /*sphereQ.BeginRecording();
                sphereQ.RecordDraw(new RenderQueue.DrawData()
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
                sphereQ.RecordDraw(new RenderQueue.DrawData()
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
                sphereQ.RecordDraw(new RenderQueue.DrawData()
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
                sphereQ.EndRecording();*/
            }
            {
                //Setup input
                vr.InitializeControllers(@"manifests\actions.json", new VRActionSet[]
                {
                    new VRActionSet("/actions/vrworld",
                        new VRAction("pickup", ActionHandleDirection.Input, ActionKind.Digital),
                        new VRAction("activate", ActionHandleDirection.Input, ActionKind.Digital),
                        new VRAction("analog_right", ActionHandleDirection.Input, ActionKind.Analog),
                        new VRAction("analog_left", ActionHandleDirection.Input, ActionKind.Analog),
                        new VRAction("hand_right", ActionHandleDirection.Input, ActionKind.Pose),
                        new VRAction("hand_left", ActionHandleDirection.Input, ActionKind.Pose),
                        new VRAction("menu_right", ActionHandleDirection.Input, ActionKind.Digital),
                        new VRAction("menu_left", ActionHandleDirection.Input, ActionKind.Digital),
                        new VRAction("haptic_right", ActionHandleDirection.Output, ActionKind.Haptic),
                        new VRAction("haptic_left", ActionHandleDirection.Output, ActionKind.Haptic))
                });
            }
        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {
            clearQ.Submit();
            var pose = vr.GetPose();

            var left_poseData = vr.GetPoseData("/actions/vrworld/in/hand_left");
            vr.GetControllerMesh(left_poseData.ActiveOrigin, meshGroup, out var ctrl, out var ctrl_tex);
            if (vr_data[VRHand.Left.Value].model == null)
            {
                vr_data[VRHand.Left.Value].model = ctrl;
                vr_data[VRHand.Left.Value].model_tex = ctrl_tex;

                ctrl_shader = new ShaderProgram[2];
                ctrlTexture = new UniformBuffer(false);
                for (int i = 0; i < 2; i++) ctrl_shader[i] = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Default/fragment.glsl"));
                var RenderState0 = new RenderState(vr.LeftFramebuffer, ctrl_shader[VRHand.Left.Value], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { ctrlTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                var RenderState1 = new RenderState(vr.RightFramebuffer, ctrl_shader[VRHand.Right.Value], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { ctrlTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);
                var RenderState_scr = new RenderState(Framebuffer.Default, ctrl_shader[VRHand.Right.Value], new ShaderStorageBuffer[] { sphereTransforms }, new UniformBuffer[] { ctrlTexture }, true, true, DepthFunc.Greater, 1, 0, BlendFactor.One, BlendFactor.Zero, Vector4.UnitW, 0, CullFaceMode.None);

                int b_inst = 81;
                unsafe
                {
                    byte* b_ptr = ctrlTexture.Update();
                    long* l_ptr = (long*)b_ptr;
                    sphereQ.BeginRecording();
                    for (int i = 0; i < ctrl.Length; i++)
                    {
                        l_ptr[0] = ctrl_tex[i].GetHandle(TextureSampler.Default);
                        ctrl_tex[i].GetHandle(TextureSampler.Default).SetResidency(Residency.Resident);
                        l_ptr++;

                        sphereQ.RecordDraw(new RenderQueue.DrawData()
                        {
                            Meshes = new RenderQueue.MeshData[]
                            {
                                new RenderQueue.MeshData()
                                {
                                    BaseInstance = b_inst,
                                    InstanceCount = 1,
                                    Mesh = ctrl[i]
                                }
                            },
                            State = RenderState_scr
                        });
                        sphereQ.RecordDraw(new RenderQueue.DrawData()
                        {
                            Meshes = new RenderQueue.MeshData[]
                            {
                                new RenderQueue.MeshData()
                                {
                                    BaseInstance = b_inst,
                                    InstanceCount = 1,
                                    Mesh = ctrl[i]
                                }
                            },
                            State = RenderState0
                        });
                        sphereQ.RecordDraw(new RenderQueue.DrawData()
                        {
                            Meshes = new RenderQueue.MeshData[]
                            {
                                new RenderQueue.MeshData()
                                {
                                    BaseInstance = b_inst,
                                    InstanceCount = 1,
                                    Mesh = ctrl[i]
                                }
                            },
                            State = RenderState1
                        });
                        b_inst++;
                    }
                    sphereQ.EndRecording();
                    ctrlTexture.UpdateDone();
                }
            }

            var leftEyeView = vr.GetEyeView(VRHand.Left);
            var rightEyeView = vr.GetEyeView(VRHand.Right);

            sphereShader[VRHand.Left.Value].Set("View", leftEyeView * pose.PoseMatrix);
            sphereShader[VRHand.Right.Value].Set("View", rightEyeView * pose.PoseMatrix);
            ctrl_shader[VRHand.Left.Value].Set("View", leftEyeView * pose.PoseMatrix);
            ctrl_shader[VRHand.Right.Value].Set("View", rightEyeView * pose.PoseMatrix);

            for (int i = 0; i < 2; i++)
            {
                sphereShader[i].Set("Projection", vr_data[i].Projection);
                ctrl_shader[i].Set("Projection", vr_data[i].Projection);
            }

            sphereQ.Submit();

            unsafe
            {
                var mats = vr.GetComponentTransforms(left_poseData.ActiveOrigin);
                for(int i = 0; i < mats.Length; i++)
                {
                    mats[i] = mats[i] * left_poseData.PoseMatrix;
                }

                var b_ptr = sphereTransforms.Update();
                {
                    fixed (Matrix4* left_pose_mat = mats)
                    {
                        long* s = (long*)left_pose_mat;
                        long* d = (long*)b_ptr;

                        for (int i = 0; i < mats.Length * sizeof(Matrix4) / sizeof(long); i++)
                        {
                            d[i + transform_off + 1] = s[i];
                        }
                    }
                }
                sphereTransforms.UpdateDone();
            }

            var orient = left_poseData.Orientation;//.ToAxisAngle();
            Console.WriteLine($"Orient: {orient.X},{orient.Y},{orient.Z},{orient.W}  Pos: {left_poseData.Position.X},{left_poseData.Position.Y},{left_poseData.Position.Z}");

            var btn = vr.GetDigitalData("/actions/vrworld/in/activate");
            if (btn.State) Console.WriteLine("Pressed");

            for (int i = 0; i < 2; i++)
                vr.Submit(VRHand.Get(i));
        }

        public void Update(double interval)
        {
            vr.Update();
        }
    }
}
