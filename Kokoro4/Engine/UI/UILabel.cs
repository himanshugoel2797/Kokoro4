using Kokoro.Engine.Graphics;
using Kokoro.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public class UILabel : UIControl
    {
        private bool _dirty = true;
        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value) _dirty = true;
                _text = value;
            }
        }

        private float _textSize;
        public float TextSize
        {
            get
            {
                return _textSize;
            }
            set
            {
                if (_textSize != value) _dirty = true;
                _textSize = value;
            }
        }

        private System.Drawing.Color _textColor;
        public System.Drawing.Color TextColor
        {
            get
            {
                return _textColor;
            }
            set
            {
                if (_textColor != value) _dirty = true;
                _textColor = value;
            }
        }

        private Texture textData;

        public override void Draw()
        {
            if (Visible)
            {
                if (_dirty)
                {
                    _dirty = false;
                    if (textData != null)
                        textData.Dispose();

                    textData = new Texture();

                    TextDrawer drawer = TextDrawer.CreateWriter("Times New Roman", FontStyle.Bold);
                    textData.SetData(drawer.Write(Text, TextSize, TextColor), 0);
                    Size = new Math.Vector2(textData.Width, textData.Height);
                }

                renderer.Apply(textData, GlobalPosition, Size);
            }
        }
    }
}
