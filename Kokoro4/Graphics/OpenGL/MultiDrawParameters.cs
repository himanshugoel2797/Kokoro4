using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.OpenGL
{
    public struct MultiDrawParameters
    {
        public int First { get; set; }
        public int Count { get; set; }
        public int BaseVertex { get; set; }
    }
}
