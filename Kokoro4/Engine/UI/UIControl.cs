using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public abstract class UIControl
    {

        public bool Enabled { get; set; }
        public bool Visible { get; set; }

        public List<UIControl> Children { get; set; }

        public event Action<Vector2, UIControl> MouseOverEvent;
        public event Action<Vector2, UIControl> MouseClickEvent;
        public event Action<Vector2, Vector2, UIControl> MouseDragEvent;
        public event Action<Vector2, UIControl> MouseUpEvent;
        public event Action<Vector2, UIControl> MouseDownEvent;

        public UIControl()
        {
            Children = new List<UIControl>();
        }

        internal virtual void Paint(UIControl control)
        {
            for(int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Visible)
                {
                    Children[i].Paint(Children[i]);
                }
            }
        }
    }
}
