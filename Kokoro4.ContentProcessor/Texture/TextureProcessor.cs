using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        private enum TextureProcessingTasks
        {
            Unknown,
            DerivativeMap,
            ColorCompression,
            DepthCompression,
            SphericalHarmonicSolver,
        }

        public static void Preprocess(string[] args)
        {
            int sh_band_cnt = 0;
            bool calc_mips = true;
            string output_file = null;
            string[] input_files = null;
            TextureProcessingTasks task = TextureProcessingTasks.Unknown;

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    switch (args[i])
                    {
                        case "-f":
                        case "-file":
                            //File name
                            if (input_files == null)
                                input_files = new string[] { args[i + 1] };
                            else
                            {
                                Console.WriteLine("Ambigiuous input file specification.");
                                return;
                            }
                            break;
                        case "-cb_px":
                            //Cubemap positive X
                            throw new NotImplementedException();
                            break;
                        case "-cb_py":
                            //Cubemap positive Y
                            throw new NotImplementedException();
                            break;
                        case "-cb_pz":
                            //Cubemap positive Z
                            throw new NotImplementedException();
                            break;
                        case "-cb_nx":
                            //Cubemap negative X
                            throw new NotImplementedException();
                            break;
                        case "-cb_ny":
                            //Cubemap negative Y
                            throw new NotImplementedException();
                            break;
                        case "-cb_nz":
                            //Cubemap negative Z
                            throw new NotImplementedException();
                            break;
                        case "-o":
                        case "-out":
                            //Output file
                            output_file = args[i + 1];
                            break;
                        case "-norm":
                            //Convert to a derivative map
                            if (task == TextureProcessingTasks.Unknown)
                                task = TextureProcessingTasks.DerivativeMap;
                            else
                                throw new NotSupportedException();
                            break;
                        case "-depth":
                            if (task == TextureProcessingTasks.Unknown)
                                task = TextureProcessingTasks.DepthCompression;
                            else
                                throw new NotSupportedException();
                            break;
                        case "-color":
                            //Compress to an appropriate format
                            if (task == TextureProcessingTasks.Unknown)
                                task = TextureProcessingTasks.ColorCompression;
                            else
                                throw new NotSupportedException();
                            break;
                        case "-nomips":
                            //Don't precalculate mipmaps
                            break;
                        case "-array":
                            //Following strings are a list of files to be put in the packed array texture.
                            throw new NotImplementedException();
                            break;
                        case "-sh":
                            //Compute the specified number of SH bands
                            try
                            {
                                sh_band_cnt = int.Parse(args[i + 1]);
                                if (task == TextureProcessingTasks.Unknown)
                                    task = TextureProcessingTasks.SphericalHarmonicSolver;
                                else
                                    throw new NotSupportedException();
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Expected positive non-zero integer after '-sh' argument.");
                                return;
                            }
                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine($"Expected additional arguments after '{args[i]}'.");
                    return;
                }
                catch (NotImplementedException)
                {
                    Console.WriteLine($"The '{args[i]}' option is unimplemented or unsupported in the specified combination of options.");
                    return;
                }
                catch (NotSupportedException)
                {
                    Console.WriteLine($"The '{args[i]}' option is unsupported in the specified combination of options.");
                    return;
                }
            }

            if (input_files == null)
            {
                Console.WriteLine("No input files specified, exiting.");
                return;
            }

            if (output_file == null)
            {
                Console.WriteLine("No output file specified, exiting.");
                return;
            }

            //Cubemaps stored as BC6H
            //All other maps are compressed with the same format in order to allow packing into array textures.
            for (int i = 0; i < input_files.Length; i++)
            {
                if (!File.Exists(input_files[i]))
                {
                    Console.WriteLine($"File {input_files[i]} does not exist, skipping.");
                    continue;
                }

                Bitmap bmp = (Bitmap)Image.FromFile(input_files[i]);

                switch (task)
                {
                    case TextureProcessingTasks.Unknown:
                        Console.WriteLine("Task not specified, Exiting.");
                        return;
                    case TextureProcessingTasks.SphericalHarmonicSolver:
                        {
                            if (sh_band_cnt <= 0)
                                sh_band_cnt = 9;

                            SphericalHarmonics sh = new SphericalHarmonics();
                            sh.Compute(bmp, sh_band_cnt);
                            sh.Save(output_file);
                        }
                        break;
                    case TextureProcessingTasks.ColorCompression:
                    case TextureProcessingTasks.DepthCompression:
                        throw new NotImplementedException();
                        break;
                    case TextureProcessingTasks.DerivativeMap:
                        {
                            DerivativeMap sh = new DerivativeMap();
                            sh.Compute(bmp);
                            sh.Save(output_file);
                        }
                        break;
                }

                bmp.Dispose();
            }
        }
    }
}
