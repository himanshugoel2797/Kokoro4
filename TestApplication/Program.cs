using Kokoro.Engine.UI;
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
            UIContext ui_ctxt = null;
            UIContainer ui_cntnr = null;

            GraphicsDevice.Update += (double interval) =>
            {

            };

            GraphicsDevice.Render += (double interval) =>
            {
                GraphicsDevice.CullMode = OpenTK.Graphics.OpenGL4.CullFaceMode.Back;
                GraphicsDevice.CullEnabled = false;

                if (ui_ctxt == null)
                {
                    ui_ctxt = new UIContext(GraphicsDevice.WindowSize.Width, GraphicsDevice.WindowSize.Height);
                }

                if(ui_cntnr == null)
                {
                    ui_cntnr = new UIContainer();
                    ui_cntnr.Size = new Kokoro.Math.Vector2(GraphicsDevice.WindowSize.Width, GraphicsDevice.WindowSize.Height);

                    ui_ctxt.Containers.Add(ui_cntnr);
                }

                GraphicsDevice.ClearColor = new Kokoro.Math.Vector4(0, 0.5f, 1.0f, 0.0f);
                GraphicsDevice.Clear();

                ui_ctxt.Draw();
                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "Test Application";
            GraphicsDevice.Run(60, 60);
            GraphicsDevice.Cleanup();
        }
    }
}
