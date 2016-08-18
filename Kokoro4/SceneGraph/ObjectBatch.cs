using Kokoro.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.SceneGraph
{
    public class ObjectBatch
    {
        struct BatchEntry
        {
            public VertexArray obj;
            public PrimitiveType primType;
            public uint start, count;
            public Texture[] textures;           
        }

        List<BatchEntry> oBatch;
        public ObjectBatch()
        {
            oBatch = new List<BatchEntry>();
        }

        public void AddObject()
        {

        }
    }
}
