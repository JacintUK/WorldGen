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
    struct Vertex3DColorUV : INormalVertex, IPositionVertex, ITextureCoordinateVertex, IColorVertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
        public Vector4 color;
        public Color Color 
        {
            get { return Color.FromArgb((int)(color.W*255), (int)(color.X*255), (int)(color.Y*255), (int)(color.Z*255) ); }
            set { this.color = new Vector4(value.R/255.0f, value.G/255.0f, value.B/255.0f, value.A/255.0f); }
        }

        public Vertex3DColorUV(Vector3 position, Vector3 normal, Vector2 uv, Vector4 color)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
            this.color = color;
        }
        public Vertex3DColorUV(Vector3 position, Vector3 normal, Vector2 uv, Color color)
        {
            this.position = position;
            this.normal = normal;
            this.uv = uv;
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
        public void SetTextureCoordinates(Vector2 texCoords)
        {
            this.uv = texCoords;
        }
        public void SetColor(Vector4 color)
        {
            this.color = color;
        }
        public Vector2 GetTextureCoordinates()
        {
            return this.uv;
        }
    }
}
