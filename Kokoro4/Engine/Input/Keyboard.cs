using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Input.LowLevel;
using System.IO;
using System.Xml.Serialization;

namespace Kokoro.Engine.Input
{
    /// <summary>
    /// Provides methods to obtain and handle keyboard input
    /// </summary>
    public class Keyboard
    {
        private Dictionary<string, Key> KeyMap;
        private Dictionary<string, Action> upHandlers;
        private Dictionary<string, Action> downHandlers;

        private static Dictionary<Key, Action> _supHandlers;
        private static Dictionary<Key, Action> _sdownHandlers;

        static Keyboard()
        {
            _sdownHandlers = new Dictionary<Key, Action>();
            _supHandlers = new Dictionary<Key, Action>();
        }

        public Keyboard(string configFile)
        {
            Stream s = File.OpenRead(configFile);
            KeyMap = new Dictionary<string, Key>();
            XmlSerializer xSer = new XmlSerializer(typeof(Dictionary<string, Key>));
            KeyMap = (Dictionary<string, Key>)xSer.Deserialize(s);

            upHandlers = new Dictionary<string, Action>();
            downHandlers = new Dictionary<string, Action>();
        }

        public void Register(string name, Action downHandler, Action upHandler, Key defaultKey)
        {
            if (!KeyMap.ContainsKey(name))
                KeyMap[name] = defaultKey;

            if(upHandler != null)upHandlers[name] = upHandler;
            if(downHandler != null)downHandlers[name] = downHandler;
        }

        public Keyboard()
        {
            KeyMap = new Dictionary<string, Key>();
            upHandlers = new Dictionary<string, Action>();
            downHandlers = new Dictionary<string, Action>();
        }

        public void SaveKeyMap(string configFile)
        {
            Stream s = File.Open(configFile, FileMode.Create);
            XmlSerializer xSer = new XmlSerializer(KeyMap.GetType());
            xSer.Serialize(s, KeyMap);
        }

        public bool IsKeyReleased(string name)
        {
            return IsKeyReleased(KeyMap[name]);
        }

        public bool IsKeyDown(string name)
        {
            return IsKeyDown(KeyMap[name]);
        }

        public void Forward()
        {
            _supHandlers.Clear();
            foreach(var kvp in upHandlers)
            {
                _supHandlers[KeyMap[kvp.Key]] = kvp.Value;
            }

            _sdownHandlers.Clear();
            foreach (var kvp in downHandlers)
            {
                _sdownHandlers[KeyMap[kvp.Key]] = kvp.Value;
            }
        }

        internal static void Update()
        {
            InputLL.UpdateKeyboard();

            foreach (KeyValuePair<Key, Action> handler in _sdownHandlers)
            {
                if (IsKeyDown(handler.Key)) handler.Value();
            }
        }

        /// <summary>
        /// Check if a key is pressed
        /// </summary>
        /// <param name="k">The key to test</param>
        /// <returns>A boolean describing whether the key is pressed or not</returns>
        internal static bool IsKeyReleased(Key k)
        {
            return InputLL.KeyReleased(k);
        }

        internal static bool IsKeyDown(Key k)
        {
            return InputLL.KeyDown(k);
        }

    }
}
