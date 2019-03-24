using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base.Data
{
    public class FileManager
    {
        public static string ReadAllText(string file)
        {
            return File.ReadAllText(file);
        }

        public static void WriteAllText(string file, string contents)
        {
            File.WriteAllText(file, contents);
        }
    }
}
