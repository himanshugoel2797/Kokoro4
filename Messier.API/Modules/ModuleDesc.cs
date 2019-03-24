using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base.Modules
{
    public abstract class ModuleDesc
    {
        public string ModuleNamespace { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool Initialized { get; private set; }

        internal ModuleDesc() { }

        public ModuleDesc(string name, string @namespace, string desc)
        {
            Name = name;
            ModuleNamespace = @namespace;
            Description = desc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if exception.</returns>
        public virtual void Initialize()
        {
            Initialized = true;
        }
    }
}
