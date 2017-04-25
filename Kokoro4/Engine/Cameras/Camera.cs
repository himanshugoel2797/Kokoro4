using Kokoro.Engine;
using Kokoro.Engine.Graphics;
using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Cameras
{
    public class Camera : EngineComponent
    {
        /// <summary>
        /// The Camera's View Matrix
        /// </summary>
        public Matrix4 View { get; internal set; }

        /// <summary>
        /// The Camera's Projection Matrix
        /// </summary>
        public Matrix4 Projection { get; internal set; }

        public List<RenderPass> PostProcessingEffects { get; set; }

        public ulong LayerMask { get; set; }

        private Vector3 pos;
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
        public Camera(string name)
        {
            View = Matrix4.LookAt(new Vector3(-1, 0, 0), Vector3.Zero, Vector3.UnitY);
            Position = -Vector3.UnitX;
            PostProcessingEffects = new List<RenderPass>();
            SetProjection((float)(114.0f/360.0f * System.Math.PI), 16f / 9f, 0.1f);  //FOV = 90
            this.Name = name;
        }

        /// <summary>
        /// Update the camera instance
        /// </summary>
        /// <param name="interval">The time elapsed in ticks since the last update</param>
        public override void Update(double interval)
        {

        }

        public virtual void Render(double interval, SceneGraph.Node scene)
        {
            //Create buckets of meshes for each shader, take the ones in the same buffer and render them in a single multidraw call, possibly running a compute shader first to upload relevant sparse texture data and perform culling

            //Render all of the opaque meshes to the GBuffer


            //Forward render all the transparent meshes


            //In the end apply all the passes in the order they have been added
        }

        public void SetProjection(float fov, float aspectRatio, float nearClip)
        {
            Projection = Matrix4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearClip);//, 10000000000);
        }

        public override void Dispose()
        {

        }
    }
}
