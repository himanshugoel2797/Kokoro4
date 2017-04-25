using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine
{
    public class GameObjectCollection : IDictionary<string, GameObject>
    {
        private Dictionary<string, GameObject> gameObject;

        public GameObjectCollection()
        {
            gameObject = new Dictionary<string, GameObject>();
        }

        public GameObject this[string key]
        {
            get
            {
                return gameObject[key];
            }
            set
            {
                gameObject[key] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IDictionary<string, GameObject>)gameObject).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<string, GameObject>)gameObject).IsReadOnly;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return ((IDictionary<string, GameObject>)gameObject).Keys;
            }
        }

        public ICollection<GameObject> Values
        {
            get
            {
                return ((IDictionary<string, GameObject>)gameObject).Values;
            }
        }

        public void Add(GameObject obj)
        {
            Add(obj.Name, obj);
        }

        public void Add(KeyValuePair<string, GameObject> item)
        {
            ((IDictionary<string, GameObject>)gameObject).Add(item);
        }

        public void Add(string key, GameObject obj)
        {
            gameObject.Add(key, obj);
        }

        public void Clear()
        {
            ((IDictionary<string, GameObject>)gameObject).Clear();
        }

        public bool Contains(KeyValuePair<string, GameObject> item)
        {
            return ((IDictionary<string, GameObject>)gameObject).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, GameObject>)gameObject).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, GameObject>[] array, int arrayIndex)
        {
            ((IDictionary<string, GameObject>)gameObject).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, GameObject>> GetEnumerator()
        {
            return ((IDictionary<string, GameObject>)gameObject).GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, GameObject> item)
        {
            return ((IDictionary<string, GameObject>)gameObject).Remove(item);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, GameObject>)gameObject).Remove(key);
        }

        public bool TryGetValue(string key, out GameObject value)
        {
            return ((IDictionary<string, GameObject>)gameObject).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, GameObject>)gameObject).GetEnumerator();
        }
    }
}
