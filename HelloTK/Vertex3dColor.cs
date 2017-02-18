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
    struct Vertex3DColor : IVertex, IPositionVertex, IColorVertex
    {
        public Vector3 position;
        public Vector4 color;

        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3 },
                new Attribute() { Name = "aColor",    Type = Attribute.AType.VECTOR4 }
                });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }

        public Color Color 
        {
            get { return Color.FromArgb((int)(color.W*255), (int)(color.X*255), (int)(color.Y*255), (int)(color.Z*255) ); }
            set { this.color = new Vector4(value.R/255.0f, value.G/255.0f, value.B/255.0f, value.A/255.0f); }
        }

        public Vertex3DColor(Vector3 position, Vector4 color)
        {
            this.position = position;
            this.color = color;
        }
        public Vertex3DColor(Vector3 position, Color color)
        {
            this.position = position;
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
        public Vector4 GetColor()
        {
            return this.color;
        }
        public void SetColor(Vector4 color)
        {
            this.color = color;
        }
    }
}
