using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.UI
{
    public class UIRoot : IDisposable
    {
        internal static List<UIRoot> rootInstances = new List<UIRoot>();
        
        public bool Visible { get; set; }
        public bool Enabled { get; set; }

        public List<UIControl> Children { get; set; }

        public UIRoot()
        {
            Children = new List<UIControl>();
            rootInstances.Add(this);
        }

        public void Update(double interval)
        {

        }

        public void Render(double interval)
        {
            if (!Visible) return;

            for(int i = 0; i < Children.Count; i++)
            {
                if (Children[i].Visible)
                {
                    Children[i].Paint(Children[i]);
                }
            }
        }

        #region IDisposable
        private bool isDisposed = false;
        public void Dispose()
        {
            if (!isDisposed)
            {
                rootInstances.Remove(this);
                isDisposed = true;
            }
        }
        #endregion
    }
}
