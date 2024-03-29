﻿using System;
using OpenTK.Mathematics;

namespace WorldGen
{
    class Camera
    {
        private Vector3 position = new Vector3();
        public enum ProjectionType
        {
            Perspective,
            Orthographic
        }

        /// <summary>
        /// FreeLook camera points whereever you want, set the rotation accordingly.
        /// Target always looks at target position.
        /// In Target mode, the property DistanceFromObject is used to update the initial Z value
        /// </summary>
        public enum ModeType
        {
            FreeLook,
            Target
        }
        public ProjectionType Projection = ProjectionType.Perspective;
        public ModeType Mode = ModeType.Target;

        public Vector3 Position { get { return position; } set { position = value; } }
        public Matrix4 View { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }

        public float fieldOfView = (float)Math.PI / 4.0f;
        public float Width { get; set; }
        public float Height { get; set; }

        public float DistanceFromObject { set; get; }
        public Quaternion Rotation { set; get; }
        public Vector3 UpVector { set; get; }
        public Vector3 TargetPosition { get; set; }

        public Camera()
        {
            DistanceFromObject = 5.0f;
            Rotation = new Quaternion(-Vector3.UnitZ, 0.0f);
            UpVector = Vector3.UnitY;
        }

        public void Update()
        {
            switch (Mode)
            {
                case ModeType.FreeLook:
                    var t = Matrix4.CreateTranslation(-Position);
                    var tx = Matrix4.CreateFromQuaternion(Rotation);
                    View = tx * t;
                    View.Invert();
                    break;
                case ModeType.Target:
                    View = Matrix4.LookAt(Position, TargetPosition, UpVector);
                    break;
                default:
                    break;
            }
            
            switch (Projection)
            {
                case ProjectionType.Perspective:
                    ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fieldOfView, Width / (float)Height, 0.1f, 100.0f);
                    break;
                case ProjectionType.Orthographic:
                    ProjectionMatrix = Matrix4.CreateOrthographic(Width, Height, -100.0f, 100.0f);
                    break;
            }
        }

        public void ChangeFieldOfView(float delta)
        {
            fieldOfView += delta / 100.0f;
            fieldOfView = Math.Clamp(fieldOfView, 0.1f, (float)Math.PI - 0.1f);
            Update();
        }

        internal Vector4 Unproject(Vector4 ray, ref Matrix4 inverseViewProjection)
        {
            // Warning: var newRay = ray * inverseViewProjection does not do what you think it should!
            ray *= inverseViewProjection;
            if (ray.W != 0)
            {
                ray.Xyz /= ray.W;
            }
            return ray;
        }

        internal void BuildPickingRay(Vector2i screenCoords, ref Vector3 rayOrigin, ref Vector3 rayDirection)
        {
            float normalizedX = 2.0f * screenCoords.X / Width - 1.0f; // -1 -> 1
            float normalizedY = 1.0f - 2.0f * screenCoords.Y / Height;

            // Convert to a point on the near plane (Z=-1)
            Vector4 near = new Vector4(normalizedX, normalizedY, -1.0f, 1.0f);

            Matrix4 pv = View * ProjectionMatrix;
            Matrix4 inversePV = Matrix4.Invert(pv);
            var nearW = Unproject(near, ref inversePV).Xyz;

            rayDirection = nearW-Position;
            rayDirection.Normalize();
            rayOrigin = Position;
        }
    }
}
