using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor
{
    class FileManager
    {
        private const string ShaderDir = "Shader";
        private const string MaterialDir = "Material";
        private const string MeshDir = "Mesh";

        private static string[] ShaderExtns = { "vert", "frag", "tess", "hull", "comp" };
        private static string[] MaterialExtns = { "mat" };
        private static string[] MeshExtns = { "fbx" };

        public static void CompileDirectory(string dir, string targetDir)
        {
            var dirs = Directory.EnumerateDirectories(dir);

            foreach(string dirName in dirs)
            {

                switch(dirName)
                {
                    case ShaderDir:
                        {
                            var files = Directory.EnumerateFiles(Path.Combine(dir, ShaderDir));
                            foreach(string fileName in files)
                            {
                                if(ShaderExtns.Contains(Path.GetExtension(fileName)))
                                {
                                    Shader.ShaderProcessor.Preprocess(Path.Combine(dir, ShaderDir, fileName), Path.Combine(dir, ShaderDir), targetDir);
                                }
                            }
                        }
                        break;
                    case MaterialDir:
                        {
                            var files = Directory.EnumerateFiles(Path.Combine(dir, MaterialDir));
                            foreach (string fileName in files)
                            {
                                if (MaterialExtns.Contains(Path.GetExtension(fileName)))
                                {
                                    Material.MaterialProcessor.Preprocess(Path.Combine(dir, MaterialDir, fileName), Path.Combine(dir, MaterialDir), targetDir);
                                }
                            }
                        }
                        break;
                    case MeshDir:
                        {
                            var files = Directory.EnumerateFiles(Path.Combine(dir, MeshDir));
                            foreach (string fileName in files)
                            {
                                if (MeshExtns.Contains(Path.GetExtension(fileName)))
                                {
                                    Mesh.MeshProcessor.Preprocess(Path.Combine(dir, MeshDir, fileName), 1, targetDir);
                                }
                            }
                        }
                        break;
                }
            }

        }
    }
}
