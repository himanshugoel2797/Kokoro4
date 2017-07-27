using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor.Material
{
    public static class MaterialProcessor
    {
        public static string GetHelp()
        {
            return "";
        }

        public static void Preprocess(string src, string srcDir, string dstDir)
        {
            

            //pack ao, cavity and metalness into a single map
            //pack normals into derivative map + roughness
            //compress albedo



        }
    }
}
