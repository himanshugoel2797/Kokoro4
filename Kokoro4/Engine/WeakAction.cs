using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine
{
    public class WeakAction
    {
        private List<WeakReference> weakRef;

        public WeakAction()
        {
            weakRef = new List<WeakReference>();
        }

        public void Add(Action a)
        {
            weakRef.Add(new WeakReference(a));
        }

        public void Invoke()
        {
            for (int i = 0; i < weakRef.Count; i++)
            {
                if (weakRef[i].IsAlive)
                {
                    (weakRef[i].Target as Action)?.Invoke();
                }
            }
        }
    }
}
