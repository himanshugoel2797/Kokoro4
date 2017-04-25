using Kokoro.Engine.Graphics;
using Kokoro.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine
{
    public class GameObject : EngineRenderable
    {
        public Mesh MeshData { get; private set; }
        public Material MaterialData { get; private set; }
        public PhysicsObject PhysicsData { get; private set; }

        public IEnumerable<GameObject> Children { get; set; }

        public override void Dispose()
        {

        }
    }
}
