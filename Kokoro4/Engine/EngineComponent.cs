using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine
{
    public abstract class EngineComponent : IDisposable
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }

        public virtual void Initialize()
        {

        }

        public virtual void Update(double interval)
        {

        }

        public abstract void Dispose();
    }
}
