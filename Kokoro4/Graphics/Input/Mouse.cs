using Messier.Graphics.Input.LowLevel;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics.Input
{
    /// <summary>
    /// Stores the states of the Mouse buttons
    /// </summary>
    public struct MouseButtons
    {
        public bool Left;
        public bool Right;
        public bool Middle;
    }

    /// <summary>
    /// Provides mehtods to obtain and handle mouse input
    /// </summary>
    public class Mouse
    {
        private static Vector2 prevMouse;
        private static Vector2 curMouse;
        private static readonly object locker = new object();

        /// <summary>
        /// The position of the mouse in Window Coordinates
        /// </summary>
        public static Vector2 MousePos
        {
            get
            {
                return curMouse;
            }
            set
            {
                InputLL.SetMousePos(value);
            }
        }
        /// <summary>
        /// The position of the mouse relative to the previous frame
        /// </summary>
        public static Vector2 MouseDelta { get; private set; }
        /// <summary>
        /// The status of the mouse buttons
        /// </summary>
        public static MouseButtons ButtonsDown { get; private set; }
        /// <summary>
        /// The mouse position in normalized device coordinates
        /// </summary>
        public static Vector2 NDMousePos
        {
            get; private set;
        }
        /// <summary>
        /// The projection matrix to convert mouse coordinates from screen space to normalized device coordinates
        /// </summary>
        public static Matrix4 MouseProjection { get; private set; }

        static Mouse()
        {
            MouseProjection = Matrix4.CreateOrthographicOffCenter(0, 1, 1, 0, 0.01f, 1);
        }

        internal static void Update()
        {
            lock (locker)
            {
                prevMouse = curMouse;
                curMouse = InputLL.UpdateMouse();

                MouseDelta = prevMouse - curMouse;

                ButtonsDown = new MouseButtons()
                {
                    Left = InputLL.LeftMouseButtonDown(),
                    Right = InputLL.RightMouseButtonDown(),
                    Middle = InputLL.MiddleMouseButtonDown()
                };

                NDMousePos = InputLL.GetNDMousePos(curMouse);
            }

        }

    }
}
