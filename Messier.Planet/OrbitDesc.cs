using Messier.Base.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Planet
{
    public class OrbitDesc : IConfigurable
    {
        public string Name { get; set; }

        //Parent body
        //Orbital Inclination
        //Orbital velocity
        //Distance
        //Phase
        //Eccentrictiy

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
