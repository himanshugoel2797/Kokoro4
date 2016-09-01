using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor.Mesh
{
    public static class MeshProcessor
    {

        public static void Preprocess(string[] args)
        {
            for(int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-f":
                    case "-file":
                        //File name
                        break;
                    case "-st":
                    case "-static":
                        //Mesh is static
                        //This enables mesh slicing
                        break;
                    case "-sc":
                    case "-scale":
                        //Scale the mesh by this value to convert it to world scale
                        break;
                    case "-o":
                    case "-out":
                        //Output file
                        break;
                }
            }

            //TODO optimize all meshes for vertex cache performance and reduced overdraw using AMD's Tootle
        }
    }
}
