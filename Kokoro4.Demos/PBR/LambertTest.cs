using Kokoro.Engine;
using Kokoro.Engine.Cameras;
using Kokoro.Engine.Graphics;
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

        private UniformBuffer Textures;
        private ShaderProgram Shader;
        private FlatGenerator Renderer;

        private Keyboard keybd;

        public void Enter(IState prev)
        {
            if (keybd == null)
            {
                keybd = new Keyboard();
                keybd.Register("ToggleWireframe", null, null, Key.W);
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

                //TODO: Scene graph based on joint relationships
                //TODO: Update scene graph based on animations
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
                    Textures = new UniformBuffer(false);

                    var b_ptr = Textures.Update();
                    long* l_ptr = (long*)b_ptr;
                    l_ptr[0] = Texture.Default.GetHandle(TextureSampler.Default);
                    Textures.UpdateDone();

                    Texture.Default.GetHandle(TextureSampler.Default).SetResidency(Residency.Resident);
                }

                Shader = new ShaderProgram(ShaderSource.Load(ShaderType.VertexShader, "Shaders/Lambert/vertex.glsl"), ShaderSource.Load(ShaderType.FragmentShader, "Shaders/Lambert/fragment.glsl"));
                Renderer = new FlatGenerator(Framebuffer.Default, Shader, new UniformBuffer[] { Textures }, null);
                Renderer.Render(graphRoot, 1);
                inited = true;
            }

            if (keybd.IsKeyReleased("ToggleWireframe"))
            {
                GraphicsDevice.Wireframe = !GraphicsDevice.Wireframe;
            }

            Shader.Set("View", EngineManager.View);
            Shader.Set("Projection", EngineManager.Projection);
            Renderer.Submit(1);
        }

        public void Update(double interval)
        {
            camera?.Update(interval);
        }
    }
}
