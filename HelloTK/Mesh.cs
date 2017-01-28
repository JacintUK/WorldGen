using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloTK
{
    class Mesh<TVertex> where TVertex : struct
    {
        public TVertex[] vertices;
        VertexFormat vertexFormat;
        //public TVertex[] Vertices { get { return vertices; } }
        public int Length { get { return vertices.Length; } }
        public VertexFormat VertexFormat { get { return vertexFormat; } }

        public Mesh(TVertex[] vertices, VertexFormat vertexFormat )
        {
            this.vertexFormat = vertexFormat;
            this.vertices = vertices;
        }
    }
}
