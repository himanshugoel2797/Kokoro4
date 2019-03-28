using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public abstract class Material : EngineRenderable
    {
        public ushort TypeIndex { get; protected set; } = 63;
        public abstract int PropertyCount { get; }
        public abstract int PropertySize { get; }
 
        public Material(string name)
        {
            this.Name = name;
        }

        public abstract byte[] GetProperty(int idx);
        public abstract override void Dispose();
        public abstract void MakeResident();
        public abstract void MakeNonResident();
    }
}
