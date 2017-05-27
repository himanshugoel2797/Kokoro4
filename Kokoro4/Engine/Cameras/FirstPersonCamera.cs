using Kokoro.Engine.Input;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Cameras
{
    /// <summary>
    /// Represents a First Person Camera
    /// </summary>
    public class FirstPersonCamera : Camera
    {//TODO setup collisions

        public Vector3 Direction;
        public Vector3 Up;

        float leftrightRot = MathHelper.PiOver2;
        float updownRot = -MathHelper.Pi / 10.0f;
        public float rotationSpeed = 200f;
        public float moveSpeed = 50000f;
        Vector2 mousePos;
        Vector3 cameraRotatedUpVector;

        /// <summary>
        /// Create a new First Person Camera
        /// </summary>
        /// <param name="Position">The Position of the Camera</param>
        /// <param name="Direction">The Direction the Camera initially faces</param>
        public FirstPersonCamera(Vector3 Position, Vector3 Direction, string name) : base(name)
        {
            this.Position = Position;
            this.Direction = Direction;
            View = Matrix4.LookAt(Position, Position + Direction, Vector3.UnitZ);
            this.Up = Vector3.UnitZ;
        }

        private Matrix4 UpdateViewMatrix()
        {
            Matrix4 cameraRotation = Matrix4.CreateRotationX(updownRot) * Matrix4.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);

            Direction = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = Position + Direction;

            cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            return Matrix4.LookAt(Position, cameraFinalTarget, cameraRotatedUpVector);
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
            Vector3 Right = Vector3.Cross(cameraRotatedUpVector, Direction);

            if (Keyboard.IsKeyDown(Key.Up))
            {
                Position += Direction * (float)(moveSpeed * interval / 10000f);
            }
            else if (Keyboard.IsKeyDown(Key.Down))
            {
                Position -= Direction * (float)(moveSpeed * interval / 10000f);
            }

            if (Keyboard.IsKeyDown(Key.Left))
            {
                Position -= Right * (float)(moveSpeed * interval / 10000f);
            }
            else if (Keyboard.IsKeyDown(Key.Right))
            {
                Position += Right * (float)(moveSpeed * interval / 10000f);
            }

#if DEBUG
            if (Keyboard.IsKeyDown(Key.PageDown))
            {
                Position -= cameraRotatedUpVector * (float)(moveSpeed * interval / 10000f);
            }
            else if (Keyboard.IsKeyDown(Key.PageUp))
            {
                Position += cameraRotatedUpVector * (float)(moveSpeed * interval / 10000f);
            }

            if (Keyboard.IsKeyDown(Key.Home))
            {
                moveSpeed += 0.02f * moveSpeed;
            }
            else if (Keyboard.IsKeyDown(Key.End))
            {
                moveSpeed -= 0.02f * moveSpeed;
            }
#endif
            //View = UpdateViewMatrix();
            View = Matrix4.LookAt(Position, Position + Direction, cameraRotatedUpVector);
            base.Update(interval);
        }
    }
}
