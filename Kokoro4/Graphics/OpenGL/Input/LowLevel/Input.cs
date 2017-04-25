using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kokoro.Math;
using OpenTK.Input;

namespace Kokoro.Input.LowLevel
{
    static class InputLL
    {
        static InputLL()
        {
            locker = new object();
            msLocker = new object();
        }

        static bool foc;
        public static Vector2 xy;
        public static Vector2 dim;
        internal static void IsFocused(bool focused)
        {
            foc = focused;
        }
        internal static void SetWinXY(int x, int y, int width, int height)
        {
            xy = new Vector2(x, y);
            dim = new Vector2(width, height);
        }

        #region Keyboard
        static KeyboardState kbdState;
        static object locker;
        public static void UpdateKeyboard()
        {
            lock (locker)
            {
                kbdState = OpenTK.Input.Keyboard.GetState();
            }
        }

        public static bool KeyDown(Kokoro.Engine.Input.Key k)
        {
            if (!foc) return false;
            lock (locker)
            {
                return kbdState[(Key)k];
            }
        }
        #endregion

        #region Mouse
        static MouseState msState;
        static object msLocker;
        public static Vector2 UpdateMouse()
        {
            lock (msLocker)
            {
                msState = OpenTK.Input.Mouse.GetCursorState();
                return new Vector2(msState.X - xy.X, msState.Y - xy.Y);
            }
        }
        public static bool LeftMouseButtonDown()
        {
            if (!foc) return false; lock (msLocker) { return msState.IsButtonDown(MouseButton.Left); }
        }
        public static bool RightMouseButtonDown()
        {
            if (!foc) return false; lock (msLocker) { return msState.IsButtonDown(MouseButton.Right); }
        }
        public static bool MiddleMouseButtonDown()
        {
            if (!foc) return false; lock (msLocker) { return msState.IsButtonDown(MouseButton.Middle); }
        }

        public static bool LeftMouseButtonUp()
        {
            if (!foc) return false; lock (msLocker) { return msState.IsButtonUp(MouseButton.Left); }
        }
        public static bool RightMouseButtonUp()
        {
            if (!foc) return false; lock (msLocker) { return msState.IsButtonUp(MouseButton.Right); }
        }
        public static bool MiddleMouseButtonUp()
        {
            if (!foc) return false; lock (msLocker) { return msState.IsButtonUp(MouseButton.Middle); }
        }

        public static float GetScroll()
        {
            if (!foc) return 0; lock (msLocker) { return msState.WheelPrecise; }
        }
        public static void SetMousePos(Vector2 pos) { OpenTK.Input.Mouse.SetPosition(pos.X, pos.Y); }

        public static Vector2 GetNDMousePos(Vector2 mousePos)
        {
            Vector2 relativePos = mousePos;
            if (relativePos.X < 0) relativePos.X = 0;
            if (relativePos.Y < 0) relativePos.Y = 0;
            if (relativePos.X > dim.X) relativePos.X = dim.X;
            if (relativePos.Y > dim.Y) relativePos.Y = dim.Y;

            relativePos.X /= dim.X;
            relativePos.Y /= dim.Y;
            return relativePos;
        }
        #endregion
    }
}