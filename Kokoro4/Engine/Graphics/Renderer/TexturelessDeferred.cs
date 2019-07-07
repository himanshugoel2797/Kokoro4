using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kokoro.Graphics.Prefabs;
using Kokoro.Engine.Graphics.Lights;
using Kokoro.Engine.Graphics.Materials;

#if DEBUG
using Kokoro.Graphics.OpenGL;
#endif

namespace Kokoro.Engine.Graphics.Renderer
{
    public class LightShaderIndex
    {
        public static readonly LightShaderIndex Point = new LightShaderIndex(0);
        public static readonly LightShaderIndex Directional = new LightShaderIndex(1);
        public static readonly LightShaderIndex Count = new LightShaderIndex(2);

        public int Value { get; private set; }
        private LightShaderIndex(int idx)
        {
            Value = idx;
        }

        public static LightShaderIndex Get(int idx)
        {
            return new LightShaderIndex(idx);
        }

        public static implicit operator int(LightShaderIndex idx)
        {
            return idx.Value;
        }
    }

    public class TexturelessDeferred
    {
        public class ProgramIndex
        {
            public static readonly ProgramIndex StaticMesh = new ProgramIndex(0);
            public static readonly ProgramIndex Count = new ProgramIndex(1);

            public int Value { get; private set; }
            private ProgramIndex(int idx)
            {
                Value = idx;
            }

            public static implicit operator int(ProgramIndex idx)
            {
                return idx.Value;
            }
        }

        public struct FramebufferData
        {
            public Framebuffer Output { get; internal set; }
            public Framebuffer GBuffer { get; internal set; }
            public Framebuffer AccumulatorBuffer { get; internal set; }

            public Texture Depth { get; internal set; }
            public Texture UVs { get; internal set; }
            public Texture MaterialIDs { get; internal set; }
            public Texture WorldPos { get; internal set; }
            public Texture Accumulator { get; internal set; }
            public Texture Reflection_MaterialIDs { get; internal set; }
            public Texture Reflection_UVs { get; internal set; }
            public TextureSampler ReflectionSampler { get; internal set; }
            public ShaderProgram ReflectionProgram { get; internal set; }
            public ShaderProgram[] Programs { get; internal set; }
            public RenderState StaticMeshRender { get; internal set; }
            public Matrix4 Projection { get; internal set; }

            internal BoundingFrustum Bounds { get; set; }
        }

        private List<ILight> lights;
        private Matrix4[] proj;
        private ShaderProgram[] lightShaders;

        public Framebuffer[] Outputs { get; private set; }
        public FramebufferData[] Resources { get; private set; }
        public IReadOnlyList<ILight> Lights { get => lights; }
        public Dictionary<short, List<Material>> Materials { get; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public const int OutputUVAttachment = 0;
        public const int MaterialIDAttachment = 1;
        public const int WorldPosAttachment = 2;
        public const int TileSz = 16;
        public const int MaxLights = 32;
        public const int MaxMaterials = 4096;
        public const int MaxTotalLights = 4096;

        MeshGroup mesh_mem;
        Mesh fst, sphere;
        RenderQueue que;
        ShaderStorageBuffer materialParams, lightParams;


        public void Resize(int w, int h)
        {
            Width = w;
            Height = h;

            Resources = new FramebufferData[Outputs.Length];
            for (int i = 0; i < Outputs.Length; i++)
            {
                Resources[i].Output = Outputs[i];
                Resources[i].Projection = proj[i];

                {
                    //Reflection Maps
                    Resources[i].Reflection_MaterialIDs = new Texture();
                    Resources[i].Reflection_MaterialIDs.SetData(new FramebufferTextureSource(w, h, 1)
                    {
                        Format = PixelFormat.RedInteger,
                        InternalFormat = PixelInternalFormat.R32ui,
                        PixelType = PixelType.UnsignedInt
                    }, 0);

                    Resources[i].Reflection_UVs = new Texture();
                    Resources[i].Reflection_UVs.SetData(new FramebufferTextureSource(w, h, 1)
                    {
                        Format = PixelFormat.Rgba,
                        InternalFormat = PixelInternalFormat.Rgba16f,
                        PixelType = PixelType.HalfFloat
                    }, 0);

                    Resources[i].ReflectionSampler = new TextureSampler();
                    Resources[i].ReflectionSampler.SetEnableLinearFilter(true);
                    Resources[i].ReflectionSampler.SetTileMode(false, false);

                    Resources[i].ReflectionProgram = new ShaderProgram(
                        ShaderSource.Load(ShaderType.ComputeShader, "Shaders/Lighting/ReflectionTracing/compute.glsl", $"#define BOUNCE_CNT ({1})\n#define WIDTH ({w})\n#define HEIGHT ({h})\n"));

                    //Accumulator
                    Resources[i].Accumulator = new Texture();
                    Resources[i].Accumulator.SetData(new FramebufferTextureSource(w, h, 1)
                    {
                        InternalFormat = PixelInternalFormat.Rgba16f,
                        PixelType = PixelType.HalfFloat
                    }, 0);

                    Resources[i].AccumulatorBuffer = new Framebuffer(w, h);
                    Resources[i].AccumulatorBuffer[FramebufferAttachment.ColorAttachment0] = Resources[i].Accumulator;
                    //Depth
                    Resources[i].Depth = new Texture();
                    Resources[i].Depth.SetData(new DepthTextureSource(w, h)
                    {
                        InternalFormat = PixelInternalFormat.DepthComponent32f
                    }, 0);

                    //UV data + deriv buffer
                    Resources[i].UVs = new Texture();
                    Resources[i].UVs.SetData(new FramebufferTextureSource(w, h, 1)
                    {
                        Format = PixelFormat.Bgra,
                        InternalFormat = PixelInternalFormat.Rgba16f,
                        PixelType = PixelType.HalfFloat
                    }, 0);

                    //Material ID buffer
                    Resources[i].MaterialIDs = new Texture();
                    Resources[i].MaterialIDs.SetData(new FramebufferTextureSource(w, h, 1)
                    {
                        Format = PixelFormat.RedInteger,
                        InternalFormat = PixelInternalFormat.R32ui,
                        PixelType = PixelType.UnsignedInt,
                    }, 0);

                    //Material ID buffer
                    Resources[i].WorldPos = new Texture();
                    Resources[i].WorldPos.SetData(new FramebufferTextureSource(w, h, 1)
                    {
                        Format = PixelFormat.Rgba,
                        InternalFormat = PixelInternalFormat.Rgb32f,
                        PixelType = PixelType.Float,
                    }, 0);

                    //Assemble GBuffer
                    Resources[i].GBuffer = new Framebuffer(w, h);
                    Resources[i].GBuffer[FramebufferAttachment.DepthAttachment] = Resources[i].Depth;
                    Resources[i].GBuffer[FramebufferAttachment.ColorAttachment0 + OutputUVAttachment] = Resources[i].UVs;
                    Resources[i].GBuffer[FramebufferAttachment.ColorAttachment0 + MaterialIDAttachment] = Resources[i].MaterialIDs;
                    Resources[i].GBuffer[FramebufferAttachment.ColorAttachment0 + WorldPosAttachment] = Resources[i].WorldPos;

                    //GBuffer render state
                    Resources[i].Programs = new ShaderProgram[ProgramIndex.Count];
                    Resources[i].Programs[ProgramIndex.StaticMesh] = new ShaderProgram(
                        ShaderSource.Load(ShaderType.VertexShader, "Shaders/Lighting/TexturelessDeferred/vertex.glsl"),
                        ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Lighting/TexturelessDeferred/fragment.glsl"));
                    Resources[i].Programs[ProgramIndex.StaticMesh].Set("Projection", proj[i]);

                }
            }
        }

        public TexturelessDeferred(int w, int h, Framebuffer[] destFb, Matrix4[] proj)
        {
            if (w % TileSz != 0)
                w += TileSz - w % TileSz;

            if (h % TileSz != 0)
                h += TileSz - h % TileSz;

            Outputs = destFb;
            lights = new List<ILight>();
            Materials = new Dictionary<short, List<Material>>();
            this.proj = proj;

            //Generate proper geometry to handle each light type
            mesh_mem = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 3000, 3000);
            fst = FullScreenQuadFactory.Create(mesh_mem);
            sphere = SphereFactory.Create(mesh_mem, 10);

            int tile_cnt = (w / TileSz) * (h / TileSz);

            lightShaders = new ShaderProgram[LightShaderIndex.Count];
            lightShaders[LightShaderIndex.Point] = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Lighting/Metalness/Point/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Lighting/Metalness/Point/fragment.glsl", $"#define MAX_TOTAL_LIGHT_CNT {MaxTotalLights}\n#define MAX_LIGHT_CNT {MaxLights}\n#define MAT_CNT {MaxMaterials}\n"));
            lightShaders[LightShaderIndex.Directional] = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Lighting/Metalness/Directional/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Lighting/Metalness/Directional/fragment.glsl", $"#define MAX_TOTAL_LIGHT_CNT {MaxTotalLights}\n#define MAX_LIGHT_CNT {MaxLights}\n#define MAT_CNT {MaxMaterials}\n"));
            materialParams = new ShaderStorageBuffer(16384, true);
            lightParams = new ShaderStorageBuffer(16384, true);
            que = new RenderQueue(100, true);

            Resize(w, h);
        }

        public int RegisterMaterial(Material m)
        {
            if (!Materials.ContainsKey((short)m.TypeIndex))
                Materials[(short)m.TypeIndex] = new List<Material>();
            Materials[(short)m.TypeIndex].Add(m);
            return (ushort)(m.TypeIndex << 10) | (ushort)(Materials[(short)m.TypeIndex].Count - 1);
        }

        public void RegisterLight(ILight l)
        {
            lights.Add(l);
        }

        public void Update(Matrix4[] view)
        {
            if (view.Length != Outputs.Length) throw new Exception("View matrix array must be the same length as the number of outputs.");

            for (int i = 0; i < Outputs.Length; i++)
                for (int j = 0; j < ProgramIndex.Count; j++)
                    Resources[i].Programs[j].Set("View", view[i]);
        }

        public void Submit(Matrix4[] view, Vector3[] viewPos)
        {
            if (view.Length != Outputs.Length) throw new Exception("View matrix array must be the same length as the number of outputs.");

            bool clear = true;
            var mat_arr = Materials.ToArray();
            for (int i = 0; i < Outputs.Length; i++)
            {
                //Compute the corners
                var VP = view[i] * Resources[i].Projection;
                var iVP = Matrix4.Invert(VP);

                //Compute reflection map
                {
                    var r_mID = Resources[i].Reflection_MaterialIDs.GetImageHandle(0, 0, PixelInternalFormat.R32ui);
                    r_mID.SetResidency(Residency.Resident, AccessMode.Write);
                    Resources[i].ReflectionProgram.Set("reflectionMap_matID", r_mID);

                    var r_uv = Resources[i].Reflection_UVs.GetImageHandle(0, 0, PixelInternalFormat.Rgba16f);
                    r_uv.SetResidency(Residency.Resident, AccessMode.Write);
                    Resources[i].ReflectionProgram.Set("reflectionMap_uv", r_uv);

                    var worldPos = Resources[i].WorldPos.GetHandle(Resources[i].ReflectionSampler);
                    worldPos.SetResidency(Residency.Resident);
                    Resources[i].ReflectionProgram.Set("worldPosMap", worldPos);

                    var uvBuf = Resources[i].UVs.GetHandle(Resources[i].ReflectionSampler);
                    uvBuf.SetResidency(Residency.Resident);
                    Resources[i].ReflectionProgram.Set("uvNormMap", uvBuf);

                    var mID_Buf = Resources[i].MaterialIDs.GetHandle(Resources[i].ReflectionSampler);
                    mID_Buf.SetResidency(Residency.Resident);
                    Resources[i].ReflectionProgram.Set("colorMap", mID_Buf);

                    Resources[i].ReflectionProgram.Set("VP", VP);
                    Resources[i].ReflectionProgram.Set("viewPos", viewPos[i]);

                    EngineManager.DispatchSyncComputeJob(Resources[i].ReflectionProgram, Width, Height, 1);
                }

                //Group the lights based on type
                Dictionary<int, List<ILight>> groupedLights = new Dictionary<int, List<ILight>>();
                for (int j = 0; j < lights.Count; j++)
                {
                    if (!groupedLights.ContainsKey(lights[j].TypeIndex))
                        groupedLights[lights[j].TypeIndex] = new List<ILight>();
                    groupedLights[lights[j].TypeIndex].Add(lights[j]);
                }

                for (int q = 0; q < groupedLights.Count; q++)
                {
                    var lightGroup = groupedLights.ElementAt(q).Value;
                    var light_type_index = groupedLights.ElementAt(q).Key;
                    if (lightGroup.Count == 0)
                        continue;

                    unsafe
                    {
                        var b_ptr = lightParams.Update();
                        switch (light_type_index)
                        {
                            case 0: //Point
                                {
                                    float* f_ptr = (float*)b_ptr;
                                    for (int idx = 0; idx < lightGroup.Count; idx++)
                                    {
                                        f_ptr[0] = (lightGroup[idx] as PointLight).Position.X;
                                        f_ptr[1] = (lightGroup[idx] as PointLight).Position.Y;
                                        f_ptr[2] = (lightGroup[idx] as PointLight).Position.Z;
                                        f_ptr[3] = (lightGroup[idx] as PointLight).MaxEffectiveRadius;
                                        f_ptr[4] = (lightGroup[idx] as PointLight).Color.X;
                                        f_ptr[5] = (lightGroup[idx] as PointLight).Color.Y;
                                        f_ptr[6] = (lightGroup[idx] as PointLight).Color.Z;
                                        f_ptr[7] = (lightGroup[idx] as PointLight).Intensity;

                                        f_ptr += 8;
                                    }
                                }
                                break;
                        }

                        lightParams.UpdateDone();
                    }

                    //Build a render state
                    RenderState state = new RenderState(Resources[i].AccumulatorBuffer, lightShaders[LightShaderIndex.Get(light_type_index)], new ShaderStorageBuffer[] { materialParams, lightParams }, null, false, true, DepthFunc.Always, InverseDepth.Far, InverseDepth.Near, BlendFactor.One, BlendFactor.One, Vector4.Zero, 0, CullFaceMode.None);

                    //Invoke the light shaders for each specified material type
                    for (int light_base_idx = 0; light_base_idx < lightGroup.Count; light_base_idx += MaxLights)
                        for (int mat_idx = 0; mat_idx < mat_arr.Length; mat_idx++)
                        {
                            var matType = mat_arr[mat_idx];
                            //Fill an SSBO with all the parameters
                            unsafe
                            {
                                var b_ptr = materialParams.Update();
                                for (int j = 0; j < matType.Value.Count; j++)
                                {
                                    matType.Value[j].MakeResident();

                                    int prop_sz = 0;
                                    for (int k = 0; k < matType.Value.First().PropertyCount; k++)
                                    {
                                        var mat_props = matType.Value[j].GetProperty(k);
                                        fixed (byte* mat_props_b = mat_props)
                                            Buffer.MemoryCopy(mat_props_b, b_ptr, mat_props.Length, mat_props.Length);

                                        prop_sz += mat_props.Length;
                                        b_ptr += mat_props.Length;
                                    }
                                }

                                materialParams.UpdateDone();
                            }

                            //Provide the depth buffer, UV buffer and material ID buffer as inputs
                            var _r_mID = Resources[i].Reflection_MaterialIDs.GetHandle(TextureSampler.Default);
                            _r_mID.SetResidency(Residency.Resident);
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("r_mID", _r_mID);

                            var _r_uv = Resources[i].Reflection_UVs.GetHandle(TextureSampler.Default);
                            _r_uv.SetResidency(Residency.Resident);
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("r_uv", _r_uv);

                            var worldPos = Resources[i].WorldPos.GetHandle(Resources[i].ReflectionSampler);
                            worldPos.SetResidency(Residency.Resident);
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("worldBuf", worldPos);

                            var uvBuf = Resources[i].UVs.GetHandle(Resources[i].ReflectionSampler);
                            uvBuf.SetResidency(Residency.Resident);
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("uvBuf", uvBuf);

                            var mID_Buf = Resources[i].MaterialIDs.GetHandle(Resources[i].ReflectionSampler);
                            mID_Buf.SetResidency(Residency.Resident);
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("mID_Buf", mID_Buf);

                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("im_sz", new Vector2(Width, Height));
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("viewPos", viewPos[i]);
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("VP", VP);
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("tile_sz", TileSz);
                            //lightShaders[LightShaderIndex.Get(light_type_index)].Set("LightIndices", Resources[i].LightRaster.LightIndicesImage);

                            if (light_type_index == LightShaderIndex.Point)
                                lightShaders[LightShaderIndex.Get(light_type_index)].Set("min_intensity", PointLight.Threshold);

                            que.ClearFramebufferBeforeSubmit = clear;
                            if (clear) clear = false;
                            que.ClearAndBeginRecording();
                            que.RecordDraw(new RenderQueue.DrawData()
                            {
                                Meshes = new RenderQueue.MeshData[]
                                {
                                        new RenderQueue.MeshData()
                                        {
                                            BaseInstance = light_base_idx,
                                            InstanceCount = System.Math.Min(MaxLights, lightGroup.Count - light_base_idx),
                                            Mesh = light_type_index == LightShaderIndex.Point ? sphere : fst
                                        }
                                },
                                State = state
                            });
                            que.EndRecording();

                            //Submit a full screen quad to compute the lighting
                            que.Submit();

                            for (int j = 0; j < matType.Value.Count; j++)
                                matType.Value[j].MakeNonResident();
                        }
                }

            }
        }
    }
}
