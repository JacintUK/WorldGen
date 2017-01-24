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
    struct Vertex
    {
        public Vector2 position;
        public Vector2 texCoords;
        public Vector4 color;
        public static int SizeInBytes { get { return Vector2.SizeInBytes * 2 + Vector4.SizeInBytes; } }
        public Color Color 
        {
            get { return Color.FromArgb(((int)color.W*255), ((int)color.X*255), ((int)color.Y*255), ((int)color.Z*255) ); }
            set { this.color = new Vector4(value.R/255.0f, value.G/255.0f, value.B/255.0f, value.A/255.0f); }
        }

        public Vertex(Vector2 position, Vector2 texCoords, Vector4 color)
        {
            this.position = position;
            this.texCoords = texCoords;
            this.color = color;
        }
        public Vertex(Vector2 position, Vector2 texCoords, Color color)
        {
            this.position = position;
            this.texCoords = texCoords;
            this.color = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f); 
        }

        public static void SetOffsets()
        {
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            GL.VertexPointer(2, VertexPointerType.Float, SizeInBytes, 0);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, SizeInBytes, Vector2.SizeInBytes);
            GL.ColorPointer(4, ColorPointerType.Float, SizeInBytes, Vector2.SizeInBytes*2);
        }
    }
}
