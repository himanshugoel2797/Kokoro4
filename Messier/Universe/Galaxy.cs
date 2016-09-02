using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Universe
{
    class Galaxy
    {
        Bitmap galaxy_density_mask;
        Bitmap galaxy_star_seeds;
        Bitmap galaxy_gas_map;

        //Track 3x3 points centered around the player position
        //Use the density value stored in the texture to pick a number of stars in the region
        //Use the seed to seed the star systems
        //Use the density and the seed to decide star properties, denser regions have more active, younger stars
        //Use the gas map to distribute resources to the stars
        //Really dense areas should get nebulae
    }
}
