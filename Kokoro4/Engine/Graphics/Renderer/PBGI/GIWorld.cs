using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics.Renderer.PBGI
{
    public class GIWorld
    {
        //Load point cloud datasets into this octree
        //Octree is used to process node updates limited to around specified location
        //Specify two radii, a 'bounce radius' and a 'light radius'
        //Bounce radius - radius in which light bounces are performed
        //Light radius - radius from which lights are sampled

        //Octree updates a gpu buffer specifying which octree nodes to consider for bounces
        //One compute shader updates the directional lighting information at one level above the leaf nodes
        //Another compute shader propogates that lighting information down to individual points 
    }
}
