using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor.Material
{
    public static class MaterialProcessor
    {
        public static void Preprocess(string[] args)
        {
            Dictionary<string, string> OptionMap = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-o":
                    case "-out":
                        //Output file
                        if (i + 1 >= args.Length) throw new ArgumentException();
                        OptionMap["-out"] = args[++i];
                        break;
                    case "-metalness":
                        if (i + 1 >= args.Length) throw new ArgumentException();
                        OptionMap["-metalness"] = args[++i];
                        break;
                    case "-albedo":
                        if (i + 1 >= args.Length) throw new ArgumentException();
                        OptionMap["-albedo"] = args[++i];
                        break;
                    case "-normal":
                        if (i + 1 >= args.Length) throw new ArgumentException();
                        OptionMap["-normal"] = args[++i];
                        break;
                    case "-cavity":
                        if (i + 1 >= args.Length) throw new ArgumentException();
                        OptionMap["-cavity"] = args[++i];
                        break;
                    case "-ao":
                        if (i + 1 >= args.Length) throw new ArgumentException();
                        OptionMap["-ao"] = args[++i];
                        break;
                    case "-roughness":
                        if (i + 1 >= args.Length) throw new ArgumentException();
                        OptionMap["-roughness"] = args[++i];
                        break;
                }

            }

            //pack ao, cavity and metalness into a single map
            //pack normals into derivative map + roughness
            //compress albedo



        }
    }
}
