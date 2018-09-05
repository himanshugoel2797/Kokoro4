using Kokoro.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Engine.Graphics
{
    public class ProbeBase
    {
        public static Vector3[] LookDirs = new Vector3[]
        {
            Vector3.UnitX,
            -Vector3.UnitX,
            Vector3.UnitY,
            -Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitZ
        };

        public static Vector3[] UpDirs = new Vector3[]
        {
            -Vector3.UnitY,
            -Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitZ,
            -Vector3.UnitY,
            -Vector3.UnitY
        };

        private Vector3 _pos;
        public Vector3 Position
        {
            get
            {
                return _pos;
            }
            set
            {
                _pos = value;

                for (int i = 0; i < ViewMats.Length; i++)
                    ViewMats[i] = Matrix4.LookAt(_pos, _pos + LookDirs[i], UpDirs[i]);
            }
        }

        public Matrix4 Projection { get; private set; }
        public Matrix4[] ViewMats { get; private set; }
        public Vector4[] Viewports { get; private set; }

        public int TargetWidth { get; private set; }
        public int TargetHeight { get; private set; }

        public ProbeBase(int probeRes, float near, float far)
        {
            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), 1, near, far);
            ViewMats = new Matrix4[6];
            Viewports = new Vector4[6];

            for (int i = 0; i < Viewports.Length; i++)
                Viewports[i] = new Vector4(0, i * probeRes, probeRes, probeRes);

            TargetWidth = probeRes;
            TargetHeight = Viewports.Length * probeRes;

            Position = Vector3.Zero;
        }
    }
}
