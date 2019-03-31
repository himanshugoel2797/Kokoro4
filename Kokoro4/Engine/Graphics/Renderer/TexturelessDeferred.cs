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
        public static readonly LightShaderIndex Count = new LightShaderIndex(1);

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
            public Texture Accumulator { get; internal set; }

            public ShaderProgram[] Programs { get; internal set; }
            public RenderState StaticMeshRender { get; internal set; }
            public Matrix4 Projection { get; internal set; }

            public GPULightRaster LightRaster { get; internal set; }

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
        public const int TileSz = 16;
        public const int MaxLights = 32;
        public const int MaxMaterials = 4096;
        public const int MaxTotalLights = 4096;

        MeshGroup fst_mem;
        Mesh fst;
        RenderQueue que;
        ShaderStorageBuffer materialParams;


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
                    //TODO: potentially bugged if width and height are not multiples of TileSz
                    //Light clustering
                    Resources[i].LightRaster = new GPULightRaster(w / TileSz, h / TileSz, MaxLights, MaxTotalLights);
                }

                {
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
                        InternalFormat = PixelInternalFormat.Rgba16f,
                        PixelType = PixelType.HalfFloat
                    }, 0);

                    //Material ID buffer
                    Resources[i].MaterialIDs = new Texture();
                    Resources[i].MaterialIDs.SetData(new FramebufferTextureSource(w, h, 1)
                    {
                        InternalFormat = PixelInternalFormat.Rgba16f,
                        PixelType = PixelType.HalfFloat,
                    }, 0);

                    //Assemble GBuffer
                    Resources[i].GBuffer = new Framebuffer(w, h);
                    Resources[i].GBuffer[FramebufferAttachment.DepthAttachment] = Resources[i].Depth;
                    Resources[i].GBuffer[FramebufferAttachment.ColorAttachment0 + OutputUVAttachment] = Resources[i].UVs;
                    Resources[i].GBuffer[FramebufferAttachment.ColorAttachment0 + MaterialIDAttachment] = Resources[i].MaterialIDs;

                    //GBuffer render state
                    Resources[i].Programs = new ShaderProgram[ProgramIndex.Count];
                    Resources[i].Programs[ProgramIndex.StaticMesh] = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Default/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/TexturelessDeferred/gbuffer_static_frag.glsl"));
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

            fst_mem = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 30, 30);
            fst = FullScreenQuadFactory.Create(fst_mem);

            int tile_cnt = (w / TileSz) * (h / TileSz);

            lightShaders = new ShaderProgram[LightShaderIndex.Count];
            lightShaders[LightShaderIndex.Point] = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/PBR/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/PBR/point_light.glsl", $"#define MAX_TOTAL_LIGHT_CNT {MaxTotalLights}\n#define MAX_LIGHT_CNT {MaxLights}\n#define MAT_CNT {MaxMaterials}\n"));
            materialParams = new ShaderStorageBuffer(16384, true);
            que = new RenderQueue(100, true);

            Resize(w, h);
        }

        public int RegisterMaterial(Material m)
        {
            if (!Materials.ContainsKey((short)m.TypeIndex)) Materials[(short)m.TypeIndex] = new List<Material>();
            Materials[(short)m.TypeIndex].Add(m);
            return (ushort)(m.TypeIndex << 10) | (ushort)(Materials[(short)m.TypeIndex].Count - 1);
        }

        public void RegisterLight(ILight l)
        {
            lights.Add(l);
            for (int i = 0; i < Resources.Length; i++)
                Resources[i].LightRaster.AddLight(l);
        }

        public void Update(Matrix4[] view)
        {
            if (view.Length != Outputs.Length) throw new Exception("View matrix array must be the same length as the number of outputs.");

            for (int i = 0; i < Outputs.Length; i++)
                for (int j = 0; j < ProgramIndex.Count; j++)
                    Resources[i].Programs[j].Set("View", view[i]);
        }

        public void Submit(Matrix4[] view)
        {
            if (view.Length != Outputs.Length) throw new Exception("View matrix array must be the same length as the number of outputs.");

            var mat_arr = Materials.ToArray();
            for (int i = 0; i < Outputs.Length; i++)
            {
                //Compute the corners
                var VP = view[i] * Resources[i].Projection;
                var iVP = Matrix4.Invert(VP);

                //Prepare the cpu raster
                Resources[i].LightRaster.Render(view[i], Resources[i].Projection);

                //Group the lights based on type
                Dictionary<int, List<ILight>> groupedLights = new Dictionary<int, List<ILight>>();
                for (int j = 0; j < lights.Count; j++)
                {
                    if (!groupedLights.ContainsKey(lights[j].TypeIndex))
                        groupedLights[lights[j].TypeIndex] = new List<ILight>();
                    groupedLights[lights[j].TypeIndex].Add(lights[j]);

                    //Render the lights to the cpu raster
                    if (lights[j].TypeIndex == LightShaderIndex.Point)
                    {
                        var pL = (PointLight)lights[j];
                    }
                }

                for (int q = 0; q < groupedLights.Count; q++)
                {
                    var lightGroup = groupedLights.ElementAt(q).Value;
                    var light_type_index = groupedLights.ElementAt(q).Key;
                    if (lightGroup.Count == 0)
                        continue;

                    //Build a render state
                    RenderState state = new RenderState(Resources[i].AccumulatorBuffer, lightShaders[LightShaderIndex.Get(light_type_index)], new ShaderStorageBuffer[] { materialParams, Resources[i].LightRaster.LightData }, null, false, true, DepthFunc.Always, InverseDepth.Far, InverseDepth.Near, BlendFactor.One, BlendFactor.One, Vector4.Zero, 0, CullFaceMode.Back);

                    //Invoke the light shaders for each specified material type
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
                        var depthBuf = Resources[i].Depth.GetHandle(TextureSampler.Default);
                        depthBuf.SetResidency(Residency.Resident);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("depthBuf", depthBuf);

                        var uvBuf = Resources[i].UVs.GetHandle(TextureSampler.Default);
                        uvBuf.SetResidency(Residency.Resident);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("uvBuf", uvBuf);

                        var mID_Buf = Resources[i].MaterialIDs.GetHandle(TextureSampler.Default);
                        mID_Buf.SetResidency(Residency.Resident);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("mID_Buf", mID_Buf);

                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("im_sz", new Vector2(Width, Height));
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("mat_type", matType.Key);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("iVP", iVP);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("VP", VP);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("tile_sz", TileSz);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("LightIndices", Resources[i].LightRaster.LightIndicesImage);

                        if (light_type_index == LightShaderIndex.Point)
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("min_intensity", PointLight.Threshold);

                        que.ClearFramebufferBeforeSubmit = true;
                        que.ClearAndBeginRecording();
                        que.RecordDraw(new RenderQueue.DrawData()
                        {
                            Meshes = new RenderQueue.MeshData[]
                            {
                                        new RenderQueue.MeshData()
                                        {
                                            BaseInstance = 0,
                                            InstanceCount = MaxLights,
                                            Mesh = fst
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
