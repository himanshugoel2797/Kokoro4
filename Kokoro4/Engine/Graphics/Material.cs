using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class Material : EngineRenderable
    {
        private enum MatTypes
        {
            Int,
            Float,
            Vec2f,
            Vec2i,
            Vec3f,
            Vec3i,
            Vec4f,
            Vec4i,
            Mat4x4,
            Buffer
        }

        private struct MatParams
        {
            public MatTypes Type;
            public object Value;
            public bool Dirty;
        }

        private Dictionary<string, MatParams> Params;

        public bool TransparencyEnabled { get; set; }

        public void SetParameter(string name, int val)
        {
            Params[name] = new MatParams()
            {
                Dirty = true,
                Type = MatTypes.Int,
                Value = val
            };
        }

        public void SetParameter(string name, float val)
        {
            Params[name] = new MatParams()
            {
                Dirty = true,
                Type = MatTypes.Float,
                Value = val
            };
        }

        public void SetParameter(string name, Vector2 val)
        {
            Params[name] = new MatParams()
            {
                Dirty = true,
                Type = MatTypes.Vec2f,
                Value = val
            };
        }

        public void SetParameter(string name, Vector2I val)
        {
            Params[name] = new MatParams()
            {
                Dirty = true,
                Type = MatTypes.Vec2i,
                Value = val
            };
        }

        public void SetParameter(string name, Vector3 val)
        {
            Params[name] = new MatParams()
            {
                Dirty = true,
                Type = MatTypes.Vec3f,
                Value = val
            };
        }


        public void SetParameter(string name, Vector4 val)
        {
            Params[name] = new MatParams()
            {
                Dirty = true,
                Type = MatTypes.Vec4f,
                Value = val
            };
        }

        public void SetParameter(string name, Matrix4 val)
        {
            Params[name] = new MatParams()
            {
                Dirty = true,
                Type = MatTypes.Mat4x4,
                Value = val
            };
        }

        public void SetParameter(string name, DataBuffer val)
        {
            Params[name] = new MatParams()
            {
                Dirty = true,
                Type = MatTypes.Buffer,
                Value = val
            };
        }
    }
}
