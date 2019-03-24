using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base.Modules
{
    public interface IConfigurable
    {
        string Name { get; set; }

        void Populate(string key, string value);
        void Populate(string key, IConfigurable value);
    }
}
