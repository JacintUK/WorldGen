using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace HelloTK
{
    struct Vertex3DColor : INormalVertex, IPositionVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector4 color;
        public static int SizeInBytes { get { return Vector2.SizeInBytes + Vector3.SizeInBytes + Vector4.SizeInBytes; } }
        public Color Color 
        {
            get { return Color.FromArgb((int)(color.W*255), (int)(color.X*255), (int)(color.Y*255), (int)(color.Z*255) ); }
            set { this.color = new Vector4(value.R/255.0f, value.G/255.0f, value.B/255.0f, value.A/255.0f); }
        }

        public Vertex3DColor(Vector3 position, Vector3 normal, Vector4 color)
        {
            this.position = position;
            this.normal = normal;
            this.color = color;
        }
        public Vertex3DColor(Vector3 position, Vector3 normal, Color color)
        {
            this.position = position;
            this.normal = normal;
            this.color = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f); 
        }
        public Vector3 GetPosition()
        {
            return this.position;
        }
        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }
        public void SetNormal(Vector3 normal)
        {
            this.normal = normal;
        }
    }
}
