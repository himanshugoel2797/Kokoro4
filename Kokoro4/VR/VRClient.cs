using Kokoro.Engine;
using Kokoro.Engine.Graphics;
using Kokoro.Graphics.OpenGL;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Valve.VR;

namespace Kokoro.VR
{
    public class VRHand
    {
        public static readonly VRHand Left = new VRHand(0);
        public static readonly VRHand Right = new VRHand(1);

        public int Value { get; }

        private VRHand(int i)
        {
            Value = i;
        }

        public static VRHand Get(int i)
        {
            if (i == Left.Value)
                return Left;
            else if (i == Right.Value)
                return Right;
            throw new ArgumentException();
        }

        public override bool Equals(object obj)
        {
            var hand = obj as VRHand;
            return hand != null &&
                   Value == hand.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        public static implicit operator EVREye(VRHand hand)
        {
            return (hand == Left) ? EVREye.Eye_Left : EVREye.Eye_Right;
        }

        public static bool operator ==(VRHand a, VRHand b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(VRHand a, VRHand b)
        {
            return a.Value != b.Value;
        }
    }

    public enum ActionHandleDirection
    {
        Input = 0,
        Output = 1,
    }

    public enum ActionKind
    {
        Unknown,
        Digital,
        Analog,
        Pose,
        Haptic
    }

    public enum ExperienceKind
    {
        Standing,
        Seated,
    }

    public class VRActionSet
    {
        public string Name { get; }
        public VRAction[] Actions { get; }

        public VRActionSet(string name, params VRAction[] actions)
        {
            Name = name;
            Actions = actions;
            for (int i = 0; i < actions.Length; i++)
                actions[i].Name = name + actions[i].Name;
        }
    }

    public class VRAction
    {
        public string Name { get; internal set; }
        public ActionHandleDirection Direction { get; }
        public ActionKind Kind { get; }

        public VRAction(string name, ActionHandleDirection direction, ActionKind kind)
        {
            Name = (direction == ActionHandleDirection.Input ? "/in/" : "/out/") + name;
            Direction = direction;
            Kind = kind;
        }
    }

    public class VRClient : IDisposable
    {
        #region Actions
        struct ActionHandle
        {
            public bool isActive;
            public ulong handle;
            public string name;
            public ActionKind kind;
            public ActionHandleDirection direction;
        }

        struct ActionSet
        {
            public string name;
            public ulong handle;
            public ActionHandle[] actionHandles;
        }

        private ActionSet[] ActionSets;
        #endregion

        #region Action Data
        public struct AnalogData
        {
            public Vector3 Position { get; private set; }
            public Vector3 Delta { get; private set; }
            public float TimeOffset { get; private set; }

            internal AnalogData(Vector3 pos, Vector3 delta, float timeOff)
            {
                Position = pos;
                Delta = delta;
                TimeOffset = timeOff;
            }
        }

        public struct DigitalData
        {
            public bool State { get; private set; }
            public bool Changed { get; private set; }
            public float TimeOffset { get; private set; }

            internal DigitalData(bool state, bool changed, float timeOff)
            {
                State = state;
                Changed = changed;
                TimeOffset = timeOff;
            }
        }

        public struct PoseData
        {
            public Matrix4 PoseMatrix { get; private set; }
            public Vector3 Position { get; private set; }
            public Quaternion Orientation { get; private set; }
            public Vector3 Velocity { get; private set; }
            public Vector3 AngularVelocity { get; private set; }
            public ulong ActiveOrigin { get; private set; }

            public float TimeOffset { get; private set; }

            internal PoseData(HmdMatrix34_t mat, bool hmd, float timeOff, HmdVector3_t vel, HmdVector3_t angular_vel, ulong origin)
            {
                PoseMatrix = new Matrix4(mat.m0, mat.m4, mat.m8, 0,
                   mat.m1, mat.m5, mat.m9, 0,
                   mat.m2, mat.m6, mat.m10, 0,
                   mat.m3, mat.m7, mat.m11, 1);
                if (hmd) PoseMatrix = Matrix4.Invert(PoseMatrix);

                Position = PoseMatrix.Row3.Xyz;
                Orientation = new Quaternion(new Matrix3(
                    PoseMatrix.M11, PoseMatrix.M12, PoseMatrix.M13,
                    PoseMatrix.M21, PoseMatrix.M22, PoseMatrix.M23,
                    PoseMatrix.M31, PoseMatrix.M32, PoseMatrix.M33
                    ));

                Velocity = new Vector3(vel.v0, vel.v1, vel.v2);
                AngularVelocity = new Vector3(angular_vel.v0, angular_vel.v1, angular_vel.v2);

                ActiveOrigin = origin;
                TimeOffset = timeOff;
            }
        }
        #endregion

        #region Render Model Management
        struct renderModel
        {
            public int texId;
            public Mesh mesh;
        }
        private Dictionary<int, Texture> renderTextures;
        private Dictionary<string, renderModel> renderModels;
        private Dictionary<ulong, string[]> controllers;
        #endregion

        private CVRSystem vr;
        private CVRCompositor cr;
        private const int HMD_Idx = 0;
        private Texture_t leftEye;
        private Texture_t rightEye;
        private VRTextureBounds_t defaultBounds;
        private ExperienceKind experienceKind;

        public Framebuffer LeftFramebuffer { get; private set; }
        public Framebuffer RightFramebuffer { get; private set; }

        public Texture LeftColorTexture { get; private set; }
        public Texture RightColorTexture { get; private set; }


        public int Width { get; private set; }
        public int Height { get; private set; }

        private VRClient(CVRSystem vr, ExperienceKind expKind)
        {
            this.vr = vr;
            this.experienceKind = expKind;
            cr = OpenVR.Compositor;

            uint width = 0, height = 0;
            vr.GetRecommendedRenderTargetSize(ref width, ref height);

            Width = (int)width;
            Height = (int)height;

            renderModels = new Dictionary<string, renderModel>();
            renderTextures = new Dictionary<int, Texture>();
            controllers = new Dictionary<ulong, string[]>();

            LeftFramebuffer = new Framebuffer((int)width, (int)height);
            {
                FramebufferTextureSource color = new FramebufferTextureSource((int)width, (int)height, 1)
                {
                    InternalFormat = PixelInternalFormat.Rgba8,
                    PixelType = PixelType.UnsignedByte
                };
                Texture colorTex = new Texture();
                colorTex.SetData(color, 0);
                LeftColorTexture = colorTex;

                DepthTextureSource depth = new DepthTextureSource((int)width, (int)height)
                {
                    InternalFormat = PixelInternalFormat.DepthComponent32
                };
                Texture depthTex = new Texture();
                depthTex.SetData(depth, 0);

                LeftFramebuffer[FramebufferAttachment.DepthAttachment] = depthTex;
                LeftFramebuffer[FramebufferAttachment.ColorAttachment0] = colorTex;
            }

            RightFramebuffer = new Framebuffer((int)width, (int)height);
            {
                FramebufferTextureSource color = new FramebufferTextureSource((int)width, (int)height, 1)
                {
                    InternalFormat = PixelInternalFormat.Rgba8,
                    PixelType = PixelType.UnsignedByte
                };
                Texture colorTex = new Texture();
                colorTex.SetData(color, 0);
                RightColorTexture = colorTex;

                DepthTextureSource depth = new DepthTextureSource((int)width, (int)height)
                {
                    InternalFormat = PixelInternalFormat.DepthComponent32
                };
                Texture depthTex = new Texture();
                depthTex.SetData(depth, 0);

                RightFramebuffer[FramebufferAttachment.DepthAttachment] = depthTex;
                RightFramebuffer[FramebufferAttachment.ColorAttachment0] = colorTex;
            }

            leftEye = new Texture_t()
            {
                handle = (IntPtr)LeftColorTexture.id,
                eColorSpace = EColorSpace.Auto,
                eType = ETextureType.OpenGL
            };

            rightEye = new Texture_t()
            {
                handle = (IntPtr)RightColorTexture.id,
                eColorSpace = EColorSpace.Auto,
                eType = ETextureType.OpenGL
            };

            defaultBounds = new VRTextureBounds_t()
            {
                uMin = 0,
                vMin = 0,
                uMax = 1,
                vMax = 1,
            };

            GraphicsDevice.Cleanup.Add(Dispose);
        }

        public Matrix4 GetEyeProjection(VRHand hand, float nearZ)
        {
            float t = 0, b = 0, l = 0, r = 0;

            vr.GetProjectionRaw(hand, ref l, ref r, ref t, ref b);
            var m = Matrix4.CreatePerspectiveOffCenter(l, r, b, t, nearZ);
            return m;
        }

        public Matrix4 GetEyeView(VRHand hand)
        {
            var mat = vr.GetEyeToHeadTransform(hand);

            return Matrix4.Invert(new Matrix4(mat.m0, mat.m4, mat.m8, 0,
                               mat.m1, mat.m5, mat.m9, 0,
                               mat.m2, mat.m6, mat.m10, 0,
                               mat.m3, mat.m7, mat.m11, 1));
        }

        public void Clear()
        {
            cr.ClearLastSubmittedFrame();
        }

        public void Submit(VRHand hand)
        {
            EVRCompositorError err = EVRCompositorError.None;
            if (hand == VRHand.Left)
                err = cr.Submit(EVREye.Eye_Left, ref leftEye, ref defaultBounds, EVRSubmitFlags.Submit_Default);
            else
                err = cr.Submit(EVREye.Eye_Right, ref rightEye, ref defaultBounds, EVRSubmitFlags.Submit_Default);
        }

        public PoseData GetPose()
        {
            var tPose = new TrackedDevicePose_t[1];
            var gPose = new TrackedDevicePose_t[0];

            
            var err = cr.WaitGetPoses(tPose, gPose);
            if (err != EVRCompositorError.None)
                throw new Exception();

            var mat = tPose[0].mDeviceToAbsoluteTracking;

            return new PoseData(mat, true, 0, tPose[0].vVelocity, tPose[0].vAngularVelocity, 0);
        }

        #region Input Management
        public void InitializeControllers(string manifestPath, VRActionSet[] actionSets)
        {
            if (!System.IO.Path.IsPathRooted(manifestPath))
                manifestPath = System.IO.Path.GetFullPath(manifestPath);
            var err = OpenVR.Input.SetActionManifestPath(manifestPath);
            if (err != EVRInputError.None) throw new Exception(err.ToString());

            ActionSet[] action_sets = new ActionSet[actionSets.Length];
            for (int j = 0; j < actionSets.Length; j++)
            {
                ActionHandle[] action_handles = new ActionHandle[actionSets[j].Actions.Length];
                for (int i = 0; i < actionSets[j].Actions.Length; i++)
                {
                    err = OpenVR.Input.GetActionHandle(actionSets[j].Actions[i].Name, ref action_handles[i].handle);
                    if (err != EVRInputError.None) throw new Exception(err.ToString());

                    action_handles[i].name = actionSets[j].Actions[i].Name;
                    action_handles[i].direction = actionSets[j].Actions[i].Direction;
                    action_handles[i].kind = actionSets[j].Actions[i].Kind;
                }

                err = OpenVR.Input.GetActionSetHandle(actionSets[j].Name, ref action_sets[j].handle);
                if (err != EVRInputError.None) throw new Exception(err.ToString());

                action_sets[j].name = actionSets[j].Name;
                action_sets[j].actionHandles = action_handles;
            }

            ActionSets = action_sets;
        }

        public void Update()
        {
            var actionSet = new VRActiveActionSet_t[ActionSets.Length];
            for (int i = 0; i < ActionSets.Length; i++)
                actionSet[i].ulActionSet = ActionSets[i].handle;

            var err = OpenVR.Input.UpdateActionState(actionSet, (uint)Marshal.SizeOf(typeof(VRActiveActionSet_t)));
            if (err != EVRInputError.None) throw new Exception(err.ToString());

        }

        public AnalogData GetAnalogData(string actionName)
        {
            var actionParts = actionName.Split('/');
            var actionSetName = "/" + actionParts[1] + "/" + actionParts[2];

            for (int i = 0; i < ActionSets.Length; i++)
            {
                if (ActionSets[i].name != actionSetName) continue;

                for (int j = 0; j < ActionSets[i].actionHandles.Length; j++)
                {
                    if (ActionSets[i].actionHandles[j].name != actionName) continue;

                    if (ActionSets[i].actionHandles[j].kind != ActionKind.Analog)
                        throw new ArgumentException("Specified action is not analog.");

                    InputAnalogActionData_t actionData = new InputAnalogActionData_t();
                    OpenVR.Input.GetAnalogActionData(ActionSets[i].actionHandles[j].handle, ref actionData, (uint)Marshal.SizeOf(typeof(InputAnalogActionData_t)), OpenVR.k_ulInvalidInputValueHandle);
                    return new AnalogData(new Vector3(actionData.x, actionData.y, actionData.z), new Vector3(actionData.deltaX, actionData.deltaY, actionData.deltaZ), actionData.fUpdateTime);
                }
            }

            throw new ArgumentException("Action not found.");
        }

        public DigitalData GetDigitalData(string actionName)
        {
            var actionParts = actionName.Split('/');
            var actionSetName = "/" + actionParts[1] + "/" + actionParts[2];

            for (int i = 0; i < ActionSets.Length; i++)
            {
                if (ActionSets[i].name != actionSetName) continue;

                for (int j = 0; j < ActionSets[i].actionHandles.Length; j++)
                {
                    if (ActionSets[i].actionHandles[j].name != actionName) continue;

                    if (ActionSets[i].actionHandles[j].kind != ActionKind.Digital)
                        throw new ArgumentException("Specified action is not digital.");

                    InputDigitalActionData_t actionData = new InputDigitalActionData_t();
                    OpenVR.Input.GetDigitalActionData(ActionSets[i].actionHandles[j].handle, ref actionData, (uint)Marshal.SizeOf(typeof(InputDigitalActionData_t)), OpenVR.k_ulInvalidInputValueHandle);
                    return new DigitalData(actionData.bState, actionData.bChanged, actionData.fUpdateTime);
                }
            }

            throw new ArgumentException("Action not found.");
        }

        public PoseData GetPoseData(string actionName, float timeOff = 0)
        {
            var actionParts = actionName.Split('/');
            var actionSetName = "/" + actionParts[1] + "/" + actionParts[2];

            for (int i = 0; i < ActionSets.Length; i++)
            {
                if (ActionSets[i].name != actionSetName) continue;

                for (int j = 0; j < ActionSets[i].actionHandles.Length; j++)
                {
                    if (ActionSets[i].actionHandles[j].name != actionName) continue;

                    if (ActionSets[i].actionHandles[j].kind != ActionKind.Pose)
                        throw new ArgumentException("Specified action is not pose.");

                    InputPoseActionData_t actionData = new InputPoseActionData_t();
                    var err = OpenVR.Input.GetPoseActionData(ActionSets[i].actionHandles[j].handle, (experienceKind == ExperienceKind.Seated) ? ETrackingUniverseOrigin.TrackingUniverseSeated : ETrackingUniverseOrigin.TrackingUniverseStanding, timeOff, ref actionData, (uint)Marshal.SizeOf(typeof(InputPoseActionData_t)), OpenVR.k_ulInvalidInputValueHandle);
                    if (err != EVRInputError.None) throw new Exception(err.ToString());

                    return new PoseData(actionData.pose.mDeviceToAbsoluteTracking, false, timeOff, actionData.pose.vVelocity, actionData.pose.vAngularVelocity, actionData.activeOrigin);
                }
            }

            throw new ArgumentException("Action not found.");
        }

        public void SendHapticPulse(string actionName, float secsFromNow, float durationSecs, float freq, float amplitude)
        {
            var actionParts = actionName.Split('/');
            var actionSetName = "/" + actionParts[1] + "/" + actionParts[2];

            for (int i = 0; i < ActionSets.Length; i++)
            {
                if (ActionSets[i].name != actionSetName) continue;

                for (int j = 0; j < ActionSets[i].actionHandles.Length; j++)
                {
                    if (ActionSets[i].actionHandles[j].name != actionName) continue;

                    if (ActionSets[i].actionHandles[j].kind != ActionKind.Haptic)
                        throw new ArgumentException("Specified action is not haptic.");

                    OpenVR.Input.TriggerHapticVibrationAction(ActionSets[i].actionHandles[j].handle, secsFromNow, durationSecs, freq, amplitude, OpenVR.k_ulInvalidInputValueHandle);
                    return;
                }
            }

            throw new ArgumentException("Action not found.");
        }
        #endregion

        #region Render Model Management
        public Matrix4[] GetComponentTransforms(ulong activeOrigin)
        {
            InputOriginInfo_t info = new InputOriginInfo_t();
            var err = OpenVR.Input.GetOriginTrackedDeviceInfo(activeOrigin, ref info, (uint)Marshal.SizeOf(typeof(InputOriginInfo_t)));
            if (err != EVRInputError.None) throw new Exception(err.ToString());

            if (!controllers.ContainsKey(activeOrigin))
                throw new ArgumentException("Must load the mesh using GetControllerMesh first");

            var names = controllers[activeOrigin];

            Matrix4[] transforms = new Matrix4[names.Length - 1];
            for (int i = 1; i < names.Length; i++)
            {   
                RenderModel_ControllerMode_State_t pState = new RenderModel_ControllerMode_State_t();
                RenderModel_ComponentState_t pComponentState = new RenderModel_ComponentState_t();
                OpenVR.RenderModels.GetComponentStateForDevicePath(names[0], names[i], info.devicePath, ref pState, ref pComponentState);
                {
                    var mat = pComponentState.mTrackingToComponentRenderModel;
                    transforms[i - 1] = (new Matrix4(mat.m0, mat.m4, mat.m8, 0,
                       mat.m1, mat.m5, mat.m9, 0,
                       mat.m2, mat.m6, mat.m10, 0,
                       mat.m3, mat.m7, mat.m11, 1));
                }
            }

            return transforms;
        }

        public void GetControllerMesh(ulong activeOrigin, MeshGroup group, out Mesh[] meshes, out Texture[] texs)
        {
            InputOriginInfo_t info = new InputOriginInfo_t();
            var err = OpenVR.Input.GetOriginTrackedDeviceInfo(activeOrigin, ref info, (uint)Marshal.SizeOf(typeof(InputOriginInfo_t)));
            if (err != EVRInputError.None) throw new Exception(err.ToString());
            
            StringBuilder builder = new StringBuilder(1024);
            ETrackedPropertyError track_prop_err = ETrackedPropertyError.TrackedProp_Success;
            vr.GetStringTrackedDeviceProperty(info.trackedDeviceIndex, ETrackedDeviceProperty.Prop_RenderModelName_String, builder, 1024, ref track_prop_err);

            var str = builder.ToString();

            var comp_cnt = OpenVR.RenderModels.GetComponentCount(str);
            var meshes0 = new List<Mesh>();
            var texs0 = new List<Texture>();
            List<string> names = new List<string>();
            names.Add(str);

            for (uint q = 0; q < comp_cnt; q++)
            {
                StringBuilder comp_name_bldr = new StringBuilder(1024);
                StringBuilder comp_rm_name_bldr = new StringBuilder(1024);
                OpenVR.RenderModels.GetComponentName(str, q, comp_name_bldr, 1024);
                if (OpenVR.RenderModels.GetComponentRenderModelName(str, comp_name_bldr.ToString(), comp_rm_name_bldr, 1024) == 0)
                    continue;

                Texture texs_l;
                Mesh meshes_l;

                //Load the specific rendermodel
                if (!renderModels.ContainsKey(group.varray.id + "_" + comp_name_bldr.ToString() + "_" + str))
                {
                    unsafe
                    {
                        IntPtr rM_ptr = IntPtr.Zero;
                        EVRRenderModelError rM_err = EVRRenderModelError.None;
                        while (true)
                        {
                            rM_err = OpenVR.RenderModels.LoadRenderModel_Async(comp_rm_name_bldr.ToString(), ref rM_ptr);
                            if (rM_err != EVRRenderModelError.Loading)
                                break;

                            Thread.Sleep(1);
                        }
                        RenderModel_t* rM = (RenderModel_t*)rM_ptr;

                        if (rM_err != EVRRenderModelError.None)
                            throw new Exception($"Unable to load render model \"{comp_rm_name_bldr.ToString()}\"");

                        IntPtr rT_ptr = IntPtr.Zero;
                        EVRRenderModelError rT_err = EVRRenderModelError.None;
                        if (!renderTextures.ContainsKey(rM->diffuseTextureId))
                            while (true)
                            {
                                rT_err = OpenVR.RenderModels.LoadTexture_Async(rM->diffuseTextureId, ref rT_ptr);
                                if (rT_err != EVRRenderModelError.Loading)
                                    break;

                                Thread.Sleep(1);
                            }
                        RenderModel_TextureMap_t* rT = (RenderModel_TextureMap_t*)rT_ptr;

                        if (rT_err != EVRRenderModelError.None)
                        {
                            OpenVR.RenderModels.FreeRenderModel(rM_ptr);
                            throw new Exception($"Unable to load render model \"{comp_rm_name_bldr.ToString()}\"");
                        }
                        else
                        {
                            //Load the data into temp structures and free the API data
                            ushort* indices = (ushort*)rM->rIndexData;
                            RenderModel_Vertex_t* vertex = (RenderModel_Vertex_t*)rM->rVertexData;

                            float[] verts = new float[rM->unVertexCount * 3];
                            float[] uvs = new float[rM->unVertexCount * 2];
                            uint[] norms = new uint[rM->unVertexCount];
                            ushort[] inds = new ushort[rM->unTriangleCount * 3];

                            for (int i = 0; i < rM->unVertexCount; i++)
                            {
                                verts[i * 3] = vertex[i].vPosition.v0;
                                verts[i * 3 + 1] = vertex[i].vPosition.v1;
                                verts[i * 3 + 2] = vertex[i].vPosition.v2;

                                uvs[i * 2] = (vertex[i].rfTextureCoord0);
                                uvs[i * 2 + 1] = (1 - vertex[i].rfTextureCoord1);

                                norms[i] = Mesh.CompressNormal(vertex[i].vNormal.v0, vertex[i].vNormal.v1, vertex[i].vNormal.v2);
                            }

                            for (int i = 0; i < rM->unTriangleCount * 3; i++)
                            {
                                inds[i] = indices[i];
                            }

                            var m = new Mesh(group, verts, uvs, norms, inds);

                            if (!renderTextures.ContainsKey(rM->diffuseTextureId))
                            {
                                byte* im_dat = (byte*)rT->rubTextureMapData;

                                Bitmap bmp = new Bitmap(rT->unWidth, rT->unHeight);
                                for (int y = 0; y < rT->unHeight; y++)
                                    for (int x = 0; x < rT->unWidth; x++)
                                    {
                                        bmp.SetPixel(x, y, Color.FromArgb(*(im_dat++), *(im_dat++), *(im_dat++), *(im_dat++)));
                                    }

                                BitmapTextureSource textureSource = new BitmapTextureSource(bmp, 1);
                                var t = new Texture();
                                t.SetData(textureSource, 0);
                                renderTextures[rM->diffuseTextureId] = t;
                                texs_l = t;
                            }
                            else
                            {
                                texs_l = renderTextures[rM->diffuseTextureId];
                            }

                            meshes_l = m;
                            renderModels[group.varray.id + "_" + comp_name_bldr.ToString() + "_" + str] = new renderModel()
                            {
                                mesh = m,
                                texId = rM->diffuseTextureId
                            };
                        }
                    }
                }
                else
                {
                    texs_l = renderTextures[renderModels[group.varray.id + "_" + comp_name_bldr.ToString() + "_" + str].texId];
                    meshes_l = renderModels[group.varray.id + "_" + comp_name_bldr.ToString() + "_" + str].mesh;
                }

                meshes0.Add(meshes_l);
                texs0.Add(texs_l);
                names.Add(comp_name_bldr.ToString());
            }

            meshes = meshes0.ToArray();
            texs = texs0.ToArray();
            controllers[activeOrigin] = names.ToArray();
        }
        #endregion

        public static VRClient Create(ExperienceKind experienceKind)
        {
            EVRInitError err = EVRInitError.None;
            var sys = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Scene);

            if (err != EVRInitError.None)
                throw new Exception("Failed to initialize OpenVR.");


            return new VRClient(sys, experienceKind);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                OpenVR.Shutdown();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~VRClient()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
