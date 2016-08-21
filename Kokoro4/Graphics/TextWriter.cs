using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDIGraphics = System.Drawing.Graphics;

namespace Kokoro.Graphics
{
    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8
    }

    /// <summary>
    /// Converts strings into renderable textures
    /// </summary>
    public class TextDrawer
    {
        static PrivateFontCollection fonts = new PrivateFontCollection();

        public static void AddFont(Stream font)
        {
            byte[] fnt = new byte[font.Length];
            font.Read(fnt, 0, fnt.Length);
            unsafe
            {
                fixed(byte *fontData = fnt)
                {
                    fonts.AddMemoryFont((IntPtr)fontData, fnt.Length);
                }
            }
        }

        public static void AddFont(string path)
        {
            fonts.AddFontFile(path);
        }

        public static TextDrawer CreateWriter(string family, FontStyle style)
        {
            return new TextDrawer(family, style);
        }


        string familyName;
        private TextDrawer(string family, FontStyle s)
        {
            familyName = family + " ";

            if (s.HasFlag(FontStyle.Regular)) familyName += "Regular";
            if (s.HasFlag(FontStyle.Bold)) familyName += "Bold";
            if (s.HasFlag(FontStyle.Italic)) familyName += "Italic";
            if (s.HasFlag(FontStyle.Underline)) familyName += "Underline";
            if (s.HasFlag(FontStyle.Strikeout)) familyName += "Strikeout";
        }

        public BitmapTextureSource Write(string s, float size, Color fg)
        {
            Bitmap tmp = new Bitmap(1, 1);
            StringFormat fmt = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            GDIGraphics tmpG = GDIGraphics.FromImage(tmp);
            Font fnt = new Font(familyName, size);
            SizeF si = tmpG.MeasureString(s, fnt);
            tmpG.Dispose();
            tmp.Dispose();

            Bitmap bmp = new Bitmap((int)si.Width, (int)si.Height);
            bmp.MakeTransparent();

            tmpG = GDIGraphics.FromImage(bmp);
            tmpG.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            tmpG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            tmpG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            tmpG.DrawString(s, fnt, new SolidBrush(fg), 0, 0);
            tmpG.Flush();
            tmpG.Dispose();

            var t = new BitmapTextureSource(bmp, 0);
            bmp.Dispose();
            return t;
        }
    }
}
