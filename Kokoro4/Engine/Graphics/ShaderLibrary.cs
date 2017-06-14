using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class ShaderLibrary
    {
        private static Dictionary<string, ShaderLibrary> libraries;
        internal List<string> Sources;

        static ShaderLibrary()
        {
            libraries = new Dictionary<string, ShaderLibrary>();
        }

        public static ShaderLibrary Create(string libraryName)
        {
            if (libraries.ContainsKey(libraryName))
                throw new ArgumentException("Library name must be unique");

            var s = new ShaderLibrary(libraryName);
            libraries[libraryName] = s;
            return s;
        }

        public static ShaderLibrary GetLibrary(string libraryName)
        {
            return libraries[libraryName];
        }

        public static ShaderLibrary[] GetLibraries(string[] libraryName)
        {
            List<ShaderLibrary> libs = new List<ShaderLibrary>();
            for (int i = 0; i < libraryName.Length; i++)
                libs.Add(GetLibrary(libraryName[i]));
            return libs.ToArray();
        }


        private ShaderLibrary(string libraryName)
        {
            Sources = new List<string>();
        }

        public void AddSource(string src)
        {
            Sources.Add(src);
        }

        public void AddSourceFile(string file)
        {
            Sources.Add(File.ReadAllText(file));
        }

        public string this[int idx]
        {
            get
            {
                return Sources[idx];
            }
        }
    }
}
