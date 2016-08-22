using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public class UIContainer : UIControl
    {
        public override void Draw()
        {
            if (Visible)
            {
                renderer.Apply(new Math.Vector4(0.4f, 0.4f, 0.4f, 0.4f), this.GlobalPosition, this.Size);
                base.Draw();
            }
        }
    }
}
