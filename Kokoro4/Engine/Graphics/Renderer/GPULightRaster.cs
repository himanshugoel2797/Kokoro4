using Kokoro.Engine.Graphics.Lights;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Renderer
{
    public class GPULightRaster
    {
        const int LightSz = 64;

        private MeshGroup boundingMeshes;
        private Mesh sphere, fst;

        public ShaderStorageBuffer LightData { get; }
        public Texture LightIndices { get; }
        public ImageHandle LightIndicesImage { get; }
        private Texture visDepth;
        private Framebuffer visDepthBuffer;
        private ShaderProgram LightRendererPrepass;
        private ShaderProgram LightRenderer;
        private ShaderProgram ClearCount;
        private RenderState renderStatePrepass;
        private RenderState renderState;
        private RenderState fstState;
        private RenderQueue renderQueue;
        private readonly int width, height, maxLights, totalLights;
        private List<ILight> lights;
        
        public GPULightRaster(int w, int h, byte max_lights_per_tile, int total_lights)
        {
            //Specialized vertex shader that reads geometry from ssbo to completely eliminate CPU side logic beyond uploading updated light data
            boundingMeshes = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 3000, 3000);
            sphere = Kokoro.Graphics.Prefabs.SphereFactory.Create(boundingMeshes, 18);
            fst = Kokoro.Graphics.Prefabs.FullScreenTriangleFactory.Create(boundingMeshes);

            LightData = new ShaderStorageBuffer(total_lights * LightSz, false);
            LightIndices = new Texture();
            LightIndices.SetData(new RawTextureSource(3, w, h, max_lights_per_tile + 1, 1, PixelFormat.RedInteger, PixelInternalFormat.R32ui, TextureTarget.Texture3D, PixelType.Int), 0);
            LightIndicesImage = LightIndices.GetImageHandle(0, -1, PixelInternalFormat.R32ui);
            LightIndicesImage.SetResidency(Residency.Resident, AccessMode.ReadWrite);

            visDepth = new Texture();
            visDepth.SetData(new DepthTextureSource(w, h)
            {
                InternalFormat = PixelInternalFormat.DepthComponent32f
            }, 0);
            visDepthBuffer = new Framebuffer(w, h);
            visDepthBuffer[FramebufferAttachment.DepthAttachment] = visDepth;

            LightRendererPrepass = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/GPULightRenderer/vertex.glsl"));
            
            LightRenderer = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/GPULightRenderer/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/GPULightRenderer/fragment.glsl"));
            LightRenderer.Set("LightIndices", LightIndicesImage);

            ClearCount = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/FrameBufferTriangle/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/GPULightRenderer/clear_frag.glsl"));
            ClearCount.Set("LightIndices", LightIndicesImage);

            renderStatePrepass = new RenderState(visDepthBuffer, LightRendererPrepass, new ShaderStorageBuffer[] { LightData }, null, true, false, DepthFunc.Greater, InverseDepth.Far, InverseDepth.Near, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, InverseDepth.ClearDepth, CullFaceMode.None);
            renderState = new RenderState(visDepthBuffer, LightRenderer, new ShaderStorageBuffer[] { LightData }, null, false, false, DepthFunc.Equal, InverseDepth.Far, InverseDepth.Near, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, InverseDepth.ClearDepth, CullFaceMode.None);
            fstState = new RenderState(visDepthBuffer, ClearCount, new ShaderStorageBuffer[] { LightData }, null, false, false, DepthFunc.Always, InverseDepth.Far, InverseDepth.Near, BlendFactor.One, BlendFactor.Zero, Vector4.Zero, InverseDepth.ClearDepth, CullFaceMode.Back);

            renderQueue = new RenderQueue(1, false);
            renderQueue.ClearFramebufferBeforeSubmit = true;

            width = w;
            height = h;
            maxLights = max_lights_per_tile;
            totalLights = total_lights;

            lights = new List<ILight>();
        }
        
        public int AddLight(ILight l)
        {
            if (lights.Count >= totalLights)
                return -1;

            int idx = lights.Count;
            lights.Add(l);
            UpdateLight(l, idx);

            renderQueue.ClearAndBeginRecording();
            renderQueue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[]
                {
                    new RenderQueue.MeshData()
                    {
                        BaseInstance = 0,
                        InstanceCount = 1,
                        Mesh = fst
                    }
                },
                State = fstState
            });
            renderQueue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[]
                {
                    new RenderQueue.MeshData()
                    {
                        BaseInstance = 0,
                        InstanceCount = lights.Count,
                        Mesh = sphere
                    }
                },
                State = renderStatePrepass
            });
            renderQueue.RecordDraw(new RenderQueue.DrawData()
            {
                Meshes = new RenderQueue.MeshData[]
                {
                    new RenderQueue.MeshData()
                    {
                        BaseInstance = 0,
                        InstanceCount = lights.Count,
                        Mesh = sphere
                    }
                },
                State = renderState
            });
            renderQueue.EndRecording();

            return idx;
        }

        public void UpdateLight(ILight l, int idx)
        {
            if (idx >= lights.Count) throw new IndexOutOfRangeException();

            lights[idx] = l;
            unsafe
            {
                var b_ptr = LightData.Update();
                var l_ptr = (float*)(b_ptr + idx * LightSz);
                if (l.TypeIndex == LightShaderIndex.Point)
                {
                    var lP = l as PointLight;
                    l_ptr[0] = lP.Position.X;
                    l_ptr[1] = lP.Position.Y;
                    l_ptr[2] = lP.Position.Z;
                    l_ptr[3] = lP.MaxEffectiveRadius;

                    l_ptr[4] = lP.Radius;
                    l_ptr[5] = lP.Intensity;
                    l_ptr[6] = lP.TypeIndex;
                    l_ptr[7] = 0;

                    l_ptr[8] = lP.Color.X;
                    l_ptr[9] = lP.Color.Y;
                    l_ptr[10] = lP.Color.Z;
                    l_ptr[11] = 0.0f;
                }
                LightData.UpdateDone();
            }
        }

        public void Render(Matrix4 view, Matrix4 proj)
        {
            LightRendererPrepass.Set("ViewProj", view * proj);
            LightRenderer.Set("ViewProj", view * proj);
            renderQueue.Submit();
        }
    }
}
