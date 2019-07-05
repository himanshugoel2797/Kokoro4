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
        Keyboard kbd;

        public const string UpBinding = "FirstPersonCamera.Up";
        public const string DownBinding = "FirstPersonCamera.Down";
        public const string LeftBinding = "FirstPersonCamera.Left";
        public const string RightBinding = "FirstPersonCamera.Right";
        public const string ForwardBinding = "FirstPersonCamera.Forward";
        public const string BackwardBinding = "FirstPersonCamera.Backward";
        public const string AccelerateBinding = "FirstPersonCamera.Accelerate";
        public const string DecelerateBinding = "FirstPersonCamera.Decelerate";

        /// <summary>
        /// Create a new First Person Camera
        /// </summary>
        /// <param name="Position">The Position of the Camera</param>
        /// <param name="Direction">The Direction the Camera initially faces</param>
        public FirstPersonCamera(Keyboard kbd, Vector3 Position, Vector3 Direction, string name) : base(name)
        {
            //Setup default key bindings
            this.kbd = kbd;
            kbd.Register(FirstPersonCamera.ForwardBinding, null, null, Key.Up);
            kbd.Register(FirstPersonCamera.BackwardBinding, null, null, Key.Down);
            kbd.Register(FirstPersonCamera.LeftBinding, null, null, Key.Left);
            kbd.Register(FirstPersonCamera.RightBinding, null, null, Key.Right);
            kbd.Register(FirstPersonCamera.UpBinding, null, null, Key.PageUp);
            kbd.Register(FirstPersonCamera.DownBinding, null, null, Key.PageDown);
            kbd.Register(FirstPersonCamera.AccelerateBinding, null, null, Key.Home);
            kbd.Register(FirstPersonCamera.DecelerateBinding, null, null, Key.End);

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

            if (kbd.IsKeyDown(ForwardBinding))
            {
                Position += Direction * (float)(moveSpeed * interval / 10000f);
            }
            else if (kbd.IsKeyDown(BackwardBinding))
            {
                Position -= Direction * (float)(moveSpeed * interval / 10000f);
            }

            if (kbd.IsKeyDown(LeftBinding))
            {
                Position -= Right * (float)(moveSpeed * interval / 10000f);
            }
            else if (kbd.IsKeyDown(RightBinding))
            {
                Position += Right * (float)(moveSpeed * interval / 10000f);
            }

#if DEBUG
            if (kbd.IsKeyDown(DownBinding))
            {
                Position -= cameraRotatedUpVector * (float)(moveSpeed * interval / 10000f);
            }
            else if (kbd.IsKeyDown(UpBinding))
            {
                Position += cameraRotatedUpVector * (float)(moveSpeed * interval / 10000f);
            }

            if (kbd.IsKeyDown(AccelerateBinding))
            {
                moveSpeed += 0.02f * moveSpeed;
            }
            else if (kbd.IsKeyDown(DecelerateBinding))
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
