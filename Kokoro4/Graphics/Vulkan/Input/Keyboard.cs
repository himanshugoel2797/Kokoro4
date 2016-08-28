using Kokoro.Graphics.Input.LowLevel;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Graphics.Input
{
    /// <summary>
    /// Provides methods to obtain and handle keyboard input
    /// </summary>
    public static class Keyboard
    {
        private static Dictionary<Key, Action> handlers;
        static Keyboard() { handlers = new Dictionary<Key, Action>(); }

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
        public static bool IsKeyPressed(Key k)
        {
            return InputLL.KeyDown(k);
        }

        /// <summary>
        /// Register a Key event handler
        /// </summary>
        /// <param name="handler">The handler to register</param>
        /// <param name="k">The key to register it to</param>
        public static void RegisterKeyHandler(Action handler, Key k)
        {
            if (!handlers.ContainsKey(k)) handlers.Add(k, handler);
            else handlers[k] += handler;
        }

    }
}
