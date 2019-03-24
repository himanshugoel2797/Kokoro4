using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Base
{
    public static class Logger
    {
        static StreamWriter logger;
        static Logger()
        {
            logger = new StreamWriter("log.txt");
        }

        public static void Log(string value)
        {
            logger.WriteLine(value);
            logger.Flush();
        }

        public static void Error(string value)
        {
            logger.WriteLine($"ERROR: {value}");
            logger.Flush();
        }
    }
}
