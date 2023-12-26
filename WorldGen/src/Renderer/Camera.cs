using System;
using OpenTK.Mathematics;

namespace WorldGen
{
    class Camera
    {
        private Vector3 position = new Vector3();
        public enum Mode
        {
            Perspective,
            Orthographic
        }
        public Mode mode = Mode.Perspective;
        public Vector3 Position { get { return position; } set { position = value; } }
        public Matrix4 View { get; private set; }
        public Matrix4 Projection { get; private set; }
        public float fieldOfView = (float)Math.PI / 4.0f;
        public float Width { get; set; }
        public float Height { get; set; }

        public void Update()
        {
            View = Matrix4.LookAt(position, Vector3.Zero, Vector3.UnitY);
            switch (mode)
            {
                case Mode.Perspective:
                    Projection = Matrix4.CreatePerspectiveFieldOfView(fieldOfView, Width / (float)Height, 0.1f, 100.0f);
                    break;
                case Mode.Orthographic:
                    Projection = Matrix4.CreateOrthographic(Width, Height, -100.0f, 100.0f);
                    break;
            }
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
        private float Clamp(float z, float v1, float v2)
        {
            return (float)Math.Min(v2, Math.Max(z, v1));
        }
    }
}
