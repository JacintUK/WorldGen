using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloTK
{
    internal class Mesh<TVertex> : IMesh where TVertex : struct, IVertex
    {
        public TVertex[] vertices;
        VertexFormat vertexFormat;
        public int Length { get { return vertices.Length; } }
        public VertexFormat VertexFormat { get { return vertexFormat; } }

        public Mesh(TVertex[] vertices )
        {
            this.vertexFormat = vertices[0].GetVertexFormat();
            this.vertices = vertices;
        }
    }
}
