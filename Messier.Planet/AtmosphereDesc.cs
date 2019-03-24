using Messier.Base.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Planet
{
    public class AtmosphereDesc : IConfigurable
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Populate(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void Populate(string key, IConfigurable value)
        {
            throw new NotImplementedException();
        }
    }
}
