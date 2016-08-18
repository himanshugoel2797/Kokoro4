using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics
{
    public class GraphicsContext
    {
        public Matrix4 View;
        public Matrix4 Projection;
        public Cameras.Camera Camera;
        
        public void Update(double interval)
        {
            Input.LowLevel.InputLL.IsFocused(true);
            Input.Keyboard.Update();
            Input.Mouse.Update();

            Camera.Update(interval, this);

            View = Camera.View;
            Projection = Camera.Projection;
        } 
    }
}
