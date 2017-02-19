using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace HelloTK
{
    internal class Mesh<TVertex> : IMesh where TVertex : struct, IVertex
    {
        public TVertex[] vertices;
        VertexFormat vertexFormat;
        public int Length { get { return vertices.Length; } }
        public VertexFormat VertexFormat { get { return vertexFormat; } }

        public Mesh(TVertex[] vertices)
        {
            this.vertexFormat = vertices[0].GetVertexFormat();
            this.vertices = vertices;
        }

    }

    internal class MeshAttr
    { 
        public static Vector3 GetPosition<TVertex2>(ref TVertex2 vertex) where TVertex2 : struct, IVertex
        {
            IPositionVertex ipv = vertex as IPositionVertex;
            if (ipv != null)
            {
                return ipv.GetPosition();
            }
            return Vector3.Zero;
        }

        public static void SetPosition<TVertex2>(ref TVertex2 vert, ref Vector3 pos) where TVertex2 : struct, IVertex
        {
            IPositionVertex ipv = vert as IPositionVertex;
            if (ipv != null)
            {
                ipv.SetPosition(pos);
                vert = (TVertex2)ipv;
            }
        }

        public static void SetNormal<TVertex2>(ref TVertex2 vertex, Vector3 normal) where TVertex2 : struct, IVertex
        {
            INormalVertex inv = vertex as INormalVertex;
            if (inv != null)
            {
                inv.SetNormal(normal);
                vertex = (TVertex2)inv;
            }
        }

        public static void SetUV<TVertex2>(ref TVertex2 vertex, Vector2 uv) where TVertex2 : struct, IVertex
        {
            ITextureCoordinateVertex inv = vertex as ITextureCoordinateVertex;
            if (inv != null)
            {
                inv.SetTextureCoordinates(uv);
                vertex = (TVertex2)inv;
            }
        }
        public static void SetColor<TVertex2>(ref TVertex2 vertex, Vector4 color) where TVertex2 : struct, IVertex
        {
            IColorVertex inv = vertex as IColorVertex;
            if (inv != null)
            {
                inv.SetColor(color);
                vertex = (TVertex2)inv;
            }
        }

        public static Vector4 GetColor<TVertex2>(ref TVertex2 vertex) where TVertex2 : struct, IVertex
        {
            IColorVertex ipv = vertex as IColorVertex;
            if (ipv != null)
            {
                return ipv.GetColor();
            }
            return Vector4.Zero;
        }

    }
}
