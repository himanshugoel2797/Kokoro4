using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics.Cameras
{
    public class Camera
    {
        /// <summary>
        /// The Camera's View Matrix
        /// </summary>
        public Matrix4 View { get; internal set; }

        /// <summary>
        /// The Camera's Projection Matrix
        /// </summary>
        public Matrix4 Projection { get; internal set; }

        Vector3 pos;
        /// <summary>
        /// The 3D Position of the Camera
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }

        /// <summary>
        /// Create a new Camera object
        /// </summary>
        public Camera()
        {
            View = Matrix4.LookAt(new Vector3(-1, 0, 0), Vector3.Zero, Vector3.UnitY);
            Position = -Vector3.UnitX;
            SetProjection(0.7853f, 16f / 9f, 0.1f, 1000f);  //FOV = 45
        }

        /// <summary>
        /// Update the camera instance
        /// </summary>
        /// <param name="interval">The time elapsed in ticks since the last update</param>
        /// <param name="Context">The current GraphicsContext</param>
        public virtual void Update(double interval, GraphicsContext Context)
        {

        }

        public void SetProjection(float fov, float aspectRatio, float nearClip, float farClip)
        {
            Projection = Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearClip, farClip);
        }
    }
}
