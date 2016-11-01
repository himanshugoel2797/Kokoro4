using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor.Texture
{
    public static class TextureProcessor
    {

        public static string GetHelp()
        {
            return "";
        }

        public static void Preprocess(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-f":
                    case "-file":
                        //File name
                        break;
                    case "-cb_px":
                        //Cubemap positive X
                        break;
                    case "-cb_py":
                        //Cubemap positive Y
                        break;
                    case "-cb_pz":
                        //Cubemap positive Z
                        break;
                    case "-cb_nx":
                        //Cubemap negative X
                        break;
                    case "-cb_ny":
                        //Cubemap negative Y
                        break;
                    case "-cb_nz":
                        //Cubemap negative Z
                        break;
                    case "-o":
                    case "-out":
                        //Output file
                        break;
                    case "-norm":
                        //Convert to a derivative map
                        break;
                    case "-depth":
                        
                        break;
                    case "-color":
                        //Compress to an appropriate format
                        break;
                    case "-nomips":
                        //Don't precalculate mipmaps
                        break;
                }
            }

            //Cubemaps stored as BC6H
            //Normal maps stored as BC5
            //Color stored as BC7
            //Depth stored as BC6H

        }
    }
}
