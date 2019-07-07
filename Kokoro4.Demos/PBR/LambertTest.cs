using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
using Kokoro.Engine.Graphics.Effects;
using Kokoro.Engine.Graphics.Lights;
using Kokoro.Engine.Graphics.Materials;
using Kokoro.Engine.Graphics.Renderer;
using Kokoro.Engine.Input;
using Kokoro.Graphics.OpenGL;
using Kokoro.Graphics.Prefabs;
using Kokoro.Math;
using Kokoro.SceneGraph;
using Kokoro.SceneGraph.Generators;
using Kokoro.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.Demos.PBR
{
    public class LambertTest : IState
    {
        private bool inited = false;
        private FirstPersonCamera camera;
        private MeshGroup grp;
        private Mesh sphere;

        private ShaderStorageBuffer Textures;
        private ShaderProgram Shader;
        private FlatGenerator Renderer;
        private TexturelessDeferred Deferred;
        private ReflectionTracing Reflection;
        private Framebuffer ReflectionBuffer;

        private Keyboard keybd;

        public void Enter(IState prev)
        {
            if (keybd == null)
            {
                keybd = new Keyboard();
                keybd.Register("ToggleWireframe", null, () => GraphicsDevice.Wireframe = !GraphicsDevice.Wireframe, Key.W);
            }
            keybd.Forward();
        }

        public void Exit(IState next)
        {

        }

        public void Render(double interval)
        {
            if (!inited)
            {
                camera = new FirstPersonCamera(keybd, new Vector3(43.74f, 15.3f, 9.07f), new Vector3(-0.951f, -0.309f, 4.1572e-08f), "FPV");
                camera.Enabled = true;
                EngineManager.AddCamera(camera);
                EngineManager.SetVisibleCamera(camera.Name);

                grp = new MeshGroup(MeshGroupVertexFormat.X32F_Y32F_Z32F, 30000, 30000);
                sphere = SphereFactory.Create(grp);

                Node graphRoot = new Node(null, "Root", 1);

                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                    {
                        var node = new Node(graphRoot, $"{x},{y}", 1);
                        node.Visible = true;
                        node.Transform = Matrix4.CreateTranslation(x * 3, 0, y * 3);
                        node.Mesh = sphere;
                    }
                graphRoot.UpdateTree();

                //TODO: Physics engine uses the above scene graph
                //TODO: The physics engine then builds a render graph from its updated scene graph
                //TODO: Use the scenegraph to implement an octree construction step for frustum culling
                //TODO: Dump the render graph

                //TODO: First update scene graph implementation
                //TODO: Then write the code to convert physics scene graph to graphics scene graph
                //TODO: Perform lighting updates with the lights in the scene graph
                //TODO: Implement reflection map rendering
                //TODO: Implement reflection convolution
                //TODO: Implement PBR
                //TODO: Implement forward plus for rendering the graph
                //TODO: Implement physics updates on the scene graph, now that visualization is working
                //TODO: Implement collisions on the scene graph
                //TODO: Implement animations on the scene graph (look into importing animation paths)
                //TODO: Implement soft body physics

                unsafe
                {
                    Textures = new ShaderStorageBuffer(4096, false);

                    var b_ptr = Textures.Update();
                    uint* l_ptr = (uint*)b_ptr;
                    l_ptr[0] = 1;
                    Textures.UpdateDone();

                    Texture.Default.GetHandle(TextureSampler.Default).SetResidency(Residency.Resident);
                }

                Deferred = new TexturelessDeferred(1280, 720, new Framebuffer[] { Framebuffer.Default }, new Matrix4[] { EngineManager.VisibleCamera.Projection });
                /*Deferred.RegisterLight(new PointLight()
                {
                    Color = Vector3.One,
                    Intensity = 10,
                    Position = Vector3.UnitY * 2,
                    Radius = 1
                });*/
                Deferred.RegisterLight(new PointLight()
                {
                    Color = Vector3.One,
                    Intensity = 1000,
                    Position = Vector3.UnitY * 10 + Vector3.UnitX * 13.5f + Vector3.UnitZ * 13.5f,
                    Radius = 1
                });

                Deferred.RegisterMaterial(new PBRMetalnessMaterial("SphereMat")
                {
                    Albedo = Texture.Default,
                    MetalRoughnessDerivative = Texture.Default,
                    AlbedoSampler = TextureSampler.Default,
                    MetalRoughnessDerivativeSampler = TextureSampler.Default,
                    Enabled = true,
                });

                Shader = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Lighting/TexturelessDeferred/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Lighting/TexturelessDeferred/fragment.glsl"));

                Renderer = new FlatGenerator(Deferred.Resources[0].GBuffer, Shader, null, new ShaderStorageBuffer[] { Textures });
                Renderer.Render(graphRoot, 1);

                //Reflection = new ReflectionTracing(1280, 720, Deferred.Resources[0].WorldPos, Deferred.Resources[0].UVs, Deferred.Resources[0].MaterialIDs);
                //ReflectionBuffer = new Framebuffer(1280, 720);
                //ReflectionBuffer[FramebufferAttachment.ColorAttachment0] = Reflection.MaterialIDs;


                inited = true;
            }

            Shader.Set("View", EngineManager.View);
            Shader.Set("Projection", EngineManager.Projection);
            Renderer.Submit(1);
            Deferred.Submit(new Matrix4[] { EngineManager.View }, new Vector3[] { camera.Position });
            //Reflection.Render(new Matrix4[] { EngineManager.View }, new Matrix4[] { EngineManager.Projection }, new Vector3[] { camera.Position });
            Framebuffer.Default.Blit(Deferred.Resources[0].AccumulatorBuffer, true, false, true);
            //Framebuffer.Default.Blit(ReflectionBuffer, true, false, true);
        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
