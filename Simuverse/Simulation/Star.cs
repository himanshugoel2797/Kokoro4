using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simuverse.Simulation
{
    public enum StarClass
    {
        O, //Most luminous
        B, //
        A, //
        F, //
        G, //
        K, //
        M, //Red dwarf (least luminous)

        W, //Wolf-Rayet star

        L, //Brown dwarf
        T, //Methane star???
        Y, //sub-brown dwarf
        C, //Carbon star
        D, //White dwarf
        BH //Black hole
    }

    public class Star
    {
        public StarClass Class { get; private set; }
        public string Name { get; private set; }
        public int Seed { get; private set; }

        public double Mass { get; private set; }
        public int Temperature { get; private set; }

        public Vector3 BaseColor { get; private set; }

        public Star(int seed, string name)
        {
            Seed = seed;
            Name = name;

            //Determine class

            //Determine mass

            //Determine temperature

            //Calculate color based on the above.
        }
    }
}
