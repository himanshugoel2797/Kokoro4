using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine
{
    public class CoroutineManager
    {
        Dictionary<string, Func<IEnumerable<object>>> coroutines;
        Dictionary<string, IEnumerator<object>> states;

        public CoroutineManager()
        {
            coroutines = new Dictionary<string, Func<IEnumerable<object>>>();
            states = new Dictionary<string, IEnumerator<object>>();
        }

        public void AddCoroutine(Func<IEnumerable<object>> routine)
        {
            coroutines.Add(routine.GetType().Name, routine);
        }

        public void Invoke(string routine)
        {
            if (!coroutines.ContainsKey(routine))
                throw new KeyNotFoundException();

            if (!states.ContainsKey(routine))
                states[routine] = coroutines[routine]().GetEnumerator();
            else
                states[routine].MoveNext();
        }
    }
}
