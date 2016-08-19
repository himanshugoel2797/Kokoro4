using Kokoro.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GraphicsDevice.Update += (double interval) =>
            {

            };

            GraphicsDevice.Render += (double interval) =>
            {
                GraphicsDevice.Clear();
                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Run(60, 60);
        }
    }
}
