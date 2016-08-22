using Kokoro.Engine.UI;
using Kokoro.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier
{
    class Program
    {
        static void Main(string[] args)
        {
            UIContext ui_ctxt = null;
            UIContainer ui_cntnr = null;
            UILabel ui_lbl = null;
            

            GraphicsDevice.Update += (double interval) =>
            {

            };

            GraphicsDevice.Render += (double interval) =>
            {
                GraphicsDevice.CullMode = OpenTK.Graphics.OpenGL.CullFaceMode.Back;
                GraphicsDevice.CullEnabled = false;

                if (ui_ctxt == null)
                {
                    ui_ctxt = new UIContext(GraphicsDevice.WindowSize.Width, GraphicsDevice.WindowSize.Height);
                }

                if (ui_cntnr == null)
                {
                    ui_cntnr = new UIContainer();
                    ui_cntnr.Size = new Kokoro.Math.Vector2(GraphicsDevice.WindowSize.Width / 2, GraphicsDevice.WindowSize.Height / 2);

                    ui_ctxt.Containers.Add(ui_cntnr);
                }

                if (ui_lbl == null)
                {
                    ui_lbl = new UILabel();
                    ui_lbl.Text = "Hello World!";
                    ui_lbl.TextSize = 12;
                    ui_lbl.TextColor = System.Drawing.Color.Beige;

                    ui_cntnr.Controls.Add(ui_lbl);
                }

                GraphicsDevice.ClearColor = new Kokoro.Math.Vector4(0, 0.5f, 1.0f, 0.0f);
                GraphicsDevice.Clear();

                ui_ctxt.Draw();
                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "Messier";
            GraphicsDevice.Run(60, 60);
            GraphicsDevice.Cleanup();
        }
    }
}
