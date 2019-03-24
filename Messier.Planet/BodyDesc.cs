using Messier.Base.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Planet
{
    public class BodyDesc : IConfigurable
    {
        public string Name { get; set; }

        //Surface Radius
        public double Radius { get; set; }

        //Mass
        public double Mass { get; set; }

        //Rotation Rate
        public double RotationRate { get; set; }

        //Inclination
        public double Inclination { get; set; }

        //OrbitDesc
        public OrbitDesc Orbit { get; set; }

        //AtmosphereDesc
        public AtmosphereDesc Atmosphere { get; set; }

        public BodyClass BodyClass { get; set; }
        //BodyClass enum
        //  Star: StarDesc
        //  GasGiant : GasGiantDesc
        //  RockyBody : RockyBodyDesc
        //  Asteroid : AsteroidDesc
        //  Comet : CometDesc
        //  Barycenter : BarycenterDesc



        public void Populate(string key, string value)
        {
            switch (key)
            {
                case nameof(Radius):
                    Radius = double.Parse(value);
                    break;
                case nameof(Mass):
                    Mass = double.Parse(value);
                    break;
                case nameof(RotationRate):
                    RotationRate = double.Parse(value);
                    break;
                case nameof(Inclination):
                    Inclination = double.Parse(value);
                    break;
                case nameof(BodyClass):
                    BodyClass = (BodyClass)Enum.Parse(typeof(BodyClass), value);
                    break;
            }
        }

        public void Populate(string key, IConfigurable value)
        {
            switch (key)
            {
                case nameof(OrbitDesc):
                    Orbit = (OrbitDesc)value;
                    break;
                case nameof(AtmosphereDesc):
                    Atmosphere = (AtmosphereDesc)value;
                    break;
            }
        }
    }
}
