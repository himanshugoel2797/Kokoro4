using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public static class InverseDepth
    {
        private const int far = 1;
        private const int near = 0;

        public static int Near => near;

        public static int Far => far;
    }
}
