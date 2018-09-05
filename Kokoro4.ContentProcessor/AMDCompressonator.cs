using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro4.ContentProcessor
{
    class AMDCompressonator
    {
        public static int Invoke(params string[] args)
        {
            string netArgs = "";
            foreach (string s in args)
                netArgs += s + " ";

            var path = Environment.GetEnvironmentVariable("COMPRESSONATOR_ROOT");
            var proc = Process.Start(Path.Combine(path, "CompressonatorCLI.exe"), netArgs);
            while(!proc.WaitForExit(250));
            return proc.ExitCode;
        }

        public static void Compress(string inPath, string outPath, int compressLevel, int mipLevels)
        {
            if (Invoke($"-fd BC{compressLevel}", mipLevels == 0 ? "-nomipmap" : $"-mipLevels {mipLevels}",  inPath, outPath) != 0)
                throw new Exception("Compression Failure.");
        }
    }
}
