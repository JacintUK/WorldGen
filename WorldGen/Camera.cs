using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using static OpenTK.MathHelper;

namespace WorldGenerator
{
    class Camera
    {
        private Vector3 position = new Vector3();
        public Vector3 Position { get { return position; } set { position = value; } }
        public Matrix4 View { get; set; }
        public Matrix4 Projection { get; set; }
        public float fieldOfView = (float)Math.PI / 4.0f;
        public float Width { get; set; }
        public float Height { get; set; }

        public void Update()
        {
            View = Matrix4.LookAt(position, Vector3.Zero, Vector3.UnitY);
            Projection = Matrix4.CreatePerspectiveFieldOfView(fieldOfView, Width / (float)Height, 0.1f, 100.0f);
        }

        public void ChangeZoom(float delta)
        {
            position.Z += delta / 100.0f;
            position.Z = Clamp(position.Z, -1, 10);
            Update();
        }

        public void ChangeFieldOfView(float delta)
        {
            fieldOfView += delta / 100.0f;
            fieldOfView = Clamp(fieldOfView, 0.1f, (float)Math.PI - 0.1f);
            Update();
        }
    }
}
