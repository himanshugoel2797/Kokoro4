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
        public Dictionary<string, Key> KeyMap;
        private static Dictionary<Key, Action> handlers;

        static Keyboard() { handlers = new Dictionary<Key, Action>(); }

        public Keyboard(string configFile)
        {
            Stream s = File.OpenRead(configFile);
            KeyMap = new Dictionary<string, Key>();
            XmlSerializer xSer = new XmlSerializer(typeof(Dictionary<string, Key>));
            KeyMap = (Dictionary<string, Key>)xSer.Deserialize(s);
        }

        public void SaveKeyMap(string configFile)
        {
            Stream s = File.Open(configFile, FileMode.Create);
            XmlSerializer xSer = new XmlSerializer(KeyMap.GetType());
            xSer.Serialize(s, KeyMap);
        }

        public bool IsKeyPressed(string name)
        {
            return IsKeyPressed(KeyMap[name]);
        }

        internal static void Update()
        {
            InputLL.UpdateKeyboard();

            foreach (KeyValuePair<Key, Action> handler in handlers)
            {
                if (IsKeyPressed(handler.Key)) handler.Value();
            }
        }

        /// <summary>
        /// Check if a key is pressed
        /// </summary>
        /// <param name="k">The key to test</param>
        /// <returns>A boolean describing whether the key is pressed or not</returns>
        internal static bool IsKeyPressed(Key k)
        {
            return InputLL.KeyDown(k);
        }

        /// <summary>
        /// Register a Key event handler
        /// </summary>
        /// <param name="handler">The handler to register</param>
        /// <param name="k">The key to register it to</param>
        internal static void RegisterKeyHandler(Action handler, Key k)
        {
            if (!handlers.ContainsKey(k)) handlers.Add(k, handler);
            else handlers[k] += handler;
        }

    }
}
