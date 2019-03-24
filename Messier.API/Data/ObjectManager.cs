using Messier.Base.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base.Data
{
    public class ObjectManager
    {
        public static ObjectManager Default { get => objectManager; }
        static ObjectManager objectManager;
        static ObjectManager()
        {
            objectManager = new ObjectManager();
        }

        Dictionary<string, IConfigurable> configurables;

        public ObjectManager()
        {
            configurables = new Dictionary<string, IConfigurable>();
        }

        public bool AddObject(IConfigurable configurable)
        {
            if (!configurables.ContainsKey(configurable.Name))
            {
                configurables[configurable.Name] = configurable;
                return true;
            }
            return false;
        }

        public IConfigurable this[string name]
        {
            get
            {
                return configurables[name];
            }
        }
    }
}
