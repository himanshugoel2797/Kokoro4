using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public class UIContainer : UIControl
    {
        UIRenderer renderer;

        public UIContainer()
        {
            renderer = new UIRenderer();
        }

        public override void Draw()
        {
            renderer.Apply(new Math.Vector4(1, 1, 1, 1), this.GlobalPosition, this.Size);
        }
    }
}
