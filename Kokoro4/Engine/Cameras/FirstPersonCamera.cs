using Kokoro.Graphics.Input;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Key = OpenTK.Input.Key;

namespace Kokoro.Engine.Cameras
{
    /// <summary>
    /// Represents a First Person Camera
    /// </summary>
    public class FirstPersonCamera : Camera
    {//TODO setup collisions

        public Vector3d Direction;
        public Vector3d Up;

        float leftrightRot = MathHelper.PiOver2;
        float updownRot = -MathHelper.Pi / 10.0f;
        public float rotationSpeed = 200f;
        public float moveSpeed = 50000f;
        Vector2 mousePos;
        Vector3d cameraRotatedUpVector;

        /// <summary>
        /// Create a new First Person Camera
        /// </summary>
        /// <param name="Position">The Position of the Camera</param>
        /// <param name="Direction">The Direction the Camera initially faces</param>
        public FirstPersonCamera(Vector3d Position, Vector3d Direction, string name) : base(name)
        {
            this.Position = Position;
            this.Direction = Direction;
            View = Matrix4d.LookAt(Position, Position + Direction, Vector3d.UnitZ);
            this.Up = Vector3d.UnitZ;
        }

        private Matrix4d UpdateViewMatrix()
        {
            Matrix4d cameraRotation = Matrix4d.CreateRotationX(updownRot) * Matrix4d.CreateRotationY(leftrightRot);

            Vector3d cameraOriginalTarget = new Vector3d(0, 0, -1);
            Vector3d cameraOriginalUpVector = new Vector3d(0, 1, 0);

            Direction = Vector3d.Transform(cameraOriginalTarget, cameraRotation);
            Vector3d cameraFinalTarget = Position + Direction;

            cameraRotatedUpVector = Vector3d.Transform(cameraOriginalUpVector, cameraRotation);

            return Matrix4d.LookAt(Position, cameraFinalTarget, cameraRotatedUpVector);
        }

        /// <summary>
        /// Update the camera instance
        /// </summary>
        /// <param name="interval">The time elapsed in ticks since the last update</param>
        public override void Update(double interval)
        {
            if (!Enabled) return;

            if (Mouse.ButtonsDown.Left)
            {
                if (System.Math.Abs(mousePos.X - Mouse.MousePos.X) > 0) leftrightRot -= (float)MathHelper.DegreesToRadians(rotationSpeed * (mousePos.X - Mouse.MousePos.X) * interval / 10000f);
                if (System.Math.Abs(mousePos.Y - Mouse.MousePos.Y) > 0) updownRot -= (float)MathHelper.DegreesToRadians(rotationSpeed * (mousePos.Y - Mouse.MousePos.Y) * interval / 10000f);
            }
            else
            {
                mousePos = Mouse.MousePos;
            }
            UpdateViewMatrix();
            Vector3d Right = Vector3d.Cross(cameraRotatedUpVector, Direction);

            if (Keyboard.IsKeyPressed(Key.Up))
            {
                Position += Direction * (float)(moveSpeed * interval / 10000f);
            }
            else if (Keyboard.IsKeyPressed(Key.Down))
            {
                Position -= Direction * (float)(moveSpeed * interval / 10000f);
            }

            if (Keyboard.IsKeyPressed(Key.Left))
            {
                Position -= Right * (float)(moveSpeed * interval / 10000f);
            }
            else if (Keyboard.IsKeyPressed(Key.Right))
            {
                Position += Right * (float)(moveSpeed * interval / 10000f);
            }

#if DEBUG
            if (Keyboard.IsKeyPressed(Key.PageDown))
            {
                Position -= cameraRotatedUpVector * (float)(moveSpeed * interval / 10000f);
            }
            else if (Keyboard.IsKeyPressed(Key.PageUp))
            {
                Position += cameraRotatedUpVector * (float)(moveSpeed * interval / 10000f);
            }

            if (Keyboard.IsKeyPressed(Key.Home))
            {
                moveSpeed += 0.02f * moveSpeed;
            }
            else if (Keyboard.IsKeyPressed(Key.End))
            {
                moveSpeed -= 0.02f * moveSpeed;
            }
#endif
            //View = UpdateViewMatrix();
            View = Matrix4d.LookAt(Position, Position + Direction, cameraRotatedUpVector);
            base.Update(interval);
        }
    }
}
