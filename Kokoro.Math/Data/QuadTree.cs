using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Math.Data
{
    public class QuadTree
    {
        public QuadTree TopLeft { get; private set; }
        public QuadTree TopRight { get; private set; }
        public QuadTree BottomLeft { get; private set; }
        public QuadTree BottomRight { get; private set; }

        public Vector2 Min { get; private set; }
        public Vector2 Max { get; private set; }

        public QuadTree(Vector2 min, Vector2 max)
        {
            this.Max = max;
            this.Min = min;
        }

        //Sample grid, isolevel is density after which surface is present
        //Store 8 samples per grid, sharing samples
        //Implement procedural generation algorithms in Kokoro.Native

        public void Split()
        {
            Vector2 ml = new Vector2(Min.X, (Max.Y - Min.Y) * 0.5f + Min.Y);
            Vector2 tm = new Vector2((Max.X - Min.X) * 0.5f + Min.X, Max.Y);

            Vector2 mr = new Vector2(Max.X, (Max.Y - Min.Y) * 0.5f + Min.Y);
            Vector2 bm = new Vector2((Max.X - Min.X) * 0.5f + Min.X, Min.Y);

            Vector2 c = new Vector2((Max.X - Min.X) * 0.5f + Min.X, (Max.Y - Min.Y) * 0.5f + Min.Y);

            TopLeft = new QuadTree(ml, tm);
            TopRight = new QuadTree(c, Max);
            BottomLeft = new QuadTree(Min, c);
            BottomRight = new QuadTree(bm, mr);
        }
    }
}
