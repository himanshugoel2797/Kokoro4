using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Renderer
{
    class ImperfectMapRenderer
    {
        //Maintain a set of points around the camera for realtime reflections/shadow maps/lighting
        //These are calculated by rendering a large number of probes
        //Each probe renders a cubemap at the lowest LoD, storing world space positions and colors
        //This data is used to feed point cloud renderings for shadows/reflections
        //
    }
}
