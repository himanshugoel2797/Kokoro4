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

        class LightShaderIndex
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

            public CPULightRaster LightRaster { get; internal set; }

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


        MeshGroup fst_mem;
        Mesh fst;
        RenderQueue que;
        ShaderStorageBuffer light_ssbo;
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
                    Resources[i].LightRaster = new CPULightRaster(w / TileSz, h / TileSz, MaxLights);
                }
                
                {
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

                    //Accumulator
                    Resources[i].Accumulator = new Texture();
                    Resources[i].Accumulator.SetData(new FramebufferTextureSource(w, h, 1)
                    {
                        InternalFormat = PixelInternalFormat.Rgba16f,
                        PixelType = PixelType.Float
                    }, 0);

                    Resources[i].AccumulatorBuffer = new Framebuffer(w, h);
                    Resources[i].AccumulatorBuffer[FramebufferAttachment.ColorAttachment0] = Resources[i].Accumulator;
                }
            }
        }

        public TexturelessDeferred(int w, int h, Framebuffer[] destFb, Matrix4[] proj)
        {
            Outputs = destFb;
            lights = new List<ILight>();
            Materials = new Dictionary<short, List<Material>>();
            this.proj = proj;

            fst_mem = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 30, 30);
            fst = FullScreenQuadFactory.Create(fst_mem);

            int tile_cnt = (w / TileSz + 1) * (h / TileSz + 1);

            lightShaders = new ShaderProgram[LightShaderIndex.Count];
            lightShaders[LightShaderIndex.Point] = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/PBR/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/PBR/point_light.glsl", $"#define MAX_LIGHT_CNT {MaxLights * tile_cnt}\n#define MAT_CNT {10}\n"));
            light_ssbo = new ShaderStorageBuffer(MaxLights * tile_cnt * 64, true);
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
        }

        public void Update(Matrix4[] view)
        {
            if (view.Length != Outputs.Length) throw new Exception("View matrix array must be the same length as the number of outputs.");
            var mat_arr = Materials.ToArray();
            for (int i = 0; i < Outputs.Length; i++)
            {
                for (int j = 0; j < ProgramIndex.Count; j++)
                    Resources[i].Programs[j].Set("View", view[i]);

                //Compute the corners
                var iVP = Matrix4.Invert(Resources[i].Projection * view[i]);
                var VP = Resources[i].Projection * view[i];

                //Prepare the cpu raster
                Resources[i].LightRaster.StartRender(VP);
                
                //First filter lights by global aabb
                //Then render them out

                //Compute bounds for each tile
                List<BoundingBox> bounds = new List<BoundingBox>();
                for (int y = 0; y < Height; y += TileSz)
                    for (int x = 0; x < Width; x += TileSz)
                    {
                        float x_ndc = (float)x / Width * 2 - 1;
                        float y_ndc = (float)y / Height * 2 - 1;
                        float w_ndc = 16.0f / Width * 2 - 1;
                        float h_ndc = 16.0f / Height * 2 - 1;

                        var frust_corners = new Vector4[]
                        {
                            new Vector4(x_ndc, y_ndc, 0, 1),
                            new Vector4(x_ndc, y_ndc, 1, 1),
                            new Vector4(x_ndc, y_ndc + h_ndc, 0, 1),
                            new Vector4(x_ndc, y_ndc + h_ndc, 1, 1),
                            new Vector4(x_ndc + w_ndc, y_ndc, 0, 1),
                            new Vector4(x_ndc + w_ndc, y_ndc, 1, 1),
                            new Vector4(x_ndc + w_ndc, y_ndc + h_ndc, 0, 1),
                            new Vector4(x_ndc + w_ndc, y_ndc + h_ndc, 1, 1),
                        };

                        //Faces = ((x,y,0),(x+,y,0),(x+,y+,0),(x,y+,0))
                        //        ((x,y,1),(x+,y,1),(x+,y+,1),(x,y+,1))
                        //        ((x,y,0),(x,y+,0),(x,y+,1),(x,y,1))
                        //        ((x+,y,0),(x+,y+,0),(x+,y+,1),(x+,y,1))
                        //        ((x,y,0),(x+,y,0),(x+,y,1),(x,y,1))
                        //        ((x,y+,0),(x+,y+,0),(x+,y+,1),(x+,y,1))
                        //Store plane as normal, distance
                        Vector3 max = Vector3.One * float.MinValue;
                        Vector3 min = Vector3.One * float.MaxValue;
                        for (int q = 0; q < frust_corners.Length; q++)
                        {
                            var m = Vector4.Transform(frust_corners[q], iVP);
                            m = m / m.W;
                            frust_corners[q] = m;

                            if (m.X > max.X) max.X = m.X;
                            if (m.Y > max.Y) max.Y = m.Y;
                            if (m.Z > max.Z) max.Z = m.Z;

                            if (m.X < min.X) min.X = m.X;
                            if (m.Y < min.Y) min.Y = m.Y;
                            if (m.Z < min.Z) min.Z = m.Z;
                        }

                        var bbox = new BoundingBox(min, max);

                        bounds.Add(bbox);
                    }

                //Group the lights based on type
                Dictionary<int, List<ILight>> groupedLights = new Dictionary<int, List<ILight>>();
                for (int j = 0; j < lights.Count; j++)
                {
                    if (!groupedLights.ContainsKey(lights[j].TypeIndex))
                        groupedLights[lights[j].TypeIndex] = new List<ILight>();
                    groupedLights[lights[j].TypeIndex].Add(lights[j]);
                }

                //Determine maximum size for material SSBO
                int max_size = 0;
                int max_mat_cnt = 10;
                foreach (KeyValuePair<short, List<Material>> matType in Materials)
                {
                    if (matType.Value.First().PropertySize * matType.Value.Count > max_size)
                        max_size = matType.Value.First().PropertySize * matType.Value.Count;
                    if (matType.Value.Count > max_mat_cnt)
                        max_mat_cnt = matType.Value.Count;
                }


                for (int q = 0; q < groupedLights.Count; q++)
                {
                    var lightGroup = groupedLights.ElementAt(q).Value;
                    var light_type_index = groupedLights.ElementAt(q).Key;

                    int maxLights = 0;  //Track the largest required light information buffer

                    //Cluster lights into tiles
                    List<List<ILight>> visibleLights = new List<List<ILight>>();
                    for (int k = 0; k < bounds.Count; k++)
                    {
                        visibleLights.Add(new List<ILight>());
                        for (int j = 0; j < lightGroup.Count; j++)
                            if (lightGroup[j].Intersect(bounds[k]))
                            {
                                visibleLights[k].Add(lightGroup[j]);
                                if (visibleLights[k].Count > maxLights)
                                    maxLights = visibleLights[k].Count;
                            }
                    }

                    if (maxLights == 0) //Being 0 means that there were no lights that passed the test
                        continue;


                    //Invoke the light shaders for each specified material type

                    for (int mat_idx = 0; mat_idx < mat_arr.Length; mat_idx++)
                    {
                        var matType = mat_arr[mat_idx];
                        //Fill an SSBO with all the parameters
                        unsafe
                        {
                            var b_ptr = materialParams.Update();
                            for (int k = 0; k < matType.Value.First().PropertyCount; k++)
                            {
                                int prop_sz = 0;
                                for (int j = 0; j < matType.Value.Count; j++)
                                {
                                    matType.Value[j].MakeResident();
                                    var mat_props = matType.Value[j].GetProperty(k);
                                    fixed (byte* mat_props_b = mat_props)
                                        Buffer.MemoryCopy(mat_props_b, b_ptr, mat_props.Length, mat_props.Length);

                                    prop_sz = mat_props.Length;
                                    b_ptr += mat_props.Length;
                                }

                                for (int j = matType.Value.Count; j < max_mat_cnt; j++)
                                    b_ptr += prop_sz;
                            }

                            materialParams.UpdateDone();
                        }


                        //Build a render state
                        RenderState state = new RenderState(Resources[i].AccumulatorBuffer, lightShaders[LightShaderIndex.Get(light_type_index)], new ShaderStorageBuffer[] { materialParams, light_ssbo }, null, false, true, DepthFunc.Always, InverseDepth.Far, InverseDepth.Near, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, 0, CullFaceMode.Back);

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
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("iVP", iVP);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("VP", VP);
                        lightShaders[LightShaderIndex.Get(light_type_index)].Set("tile_sz", TileSz);

                        if (light_type_index == LightShaderIndex.Point)
                            lightShaders[LightShaderIndex.Get(light_type_index)].Set("min_intensity", PointLight.Threshold);

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

                        //Upload the lights
                        unsafe
                        {
                            var l_b_ptr = light_ssbo.Update();
                            float* l_f_ptr = (float*)l_b_ptr;
                            for (int idx = 0; idx < bounds.Count; idx++)
                            {
                                var bound = bounds[idx];
                                var lightSet = visibleLights[idx++];
                                if (light_type_index == LightShaderIndex.Point)
                                {
                                    for (int l_p_idx = 0; l_p_idx < 2; l_p_idx++)
                                    {
                                        if (lightSet.Count == 0)
                                        {
                                            l_f_ptr += 4 * MaxLights;
                                            continue;
                                        }

                                        for (int l_idx = 0; l_idx < lightSet.Count; l_idx++)
                                        {
                                            var l = lightSet[l_idx];
                                            var p_l = (PointLight)l;
                                            if (l_p_idx == 0)
                                            {
                                                l_f_ptr[0] = p_l.Position.X;
                                                l_f_ptr[1] = p_l.Position.Y;
                                                l_f_ptr[2] = p_l.Position.Z;
                                                l_f_ptr[3] = p_l.Radius;
                                                l_f_ptr += 4;
                                            }
                                            else if (l_p_idx == 1)
                                            {
                                                l_f_ptr[0] = p_l.Color.X;
                                                l_f_ptr[1] = p_l.Color.Y;
                                                l_f_ptr[2] = p_l.Color.Z;
                                                l_f_ptr[3] = p_l.Intensity;
                                                l_f_ptr += 4;
                                            }
                                        }

                                        l_f_ptr += 4 * (MaxLights - lightSet.Count);
                                    }
                                }
                            }
                            light_ssbo.UpdateDone();
                        }

                        //Submit a draw to compute the lighting
                        que.Submit();

                        for (int j = 0; j < matType.Value.Count; j++)
                            matType.Value[j].MakeNonResident();
                    }
                }

            }
        }

    }
}
