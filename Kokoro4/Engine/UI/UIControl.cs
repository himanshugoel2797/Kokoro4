using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public class UIControl
    {
        public UIControl Parent { get; set; }
        public List<UIControl> Controls { get; set; }

        public bool Visible { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public Vector2 GlobalPosition
        {
            get
            {
                if (Parent == null)
                    return Position;

                return Parent.GlobalPosition + Position;
            }
        }

        protected UIRenderer renderer;

        public UIControl()
        {
            Controls = new List<UIControl>();
            Visible = true;
            renderer = new UIRenderer();
        }

        public virtual void Draw()
        {
            if (Visible)
            {
                for (int i = 0; i < Controls.Count; i++)
                {
                    Controls[i].Draw();
                }
            }
        }

        public virtual void Update()
        {

        }

    }
}
