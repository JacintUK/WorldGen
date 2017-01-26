using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace HelloTK
{
    class VertexBuffer// <TVertex> where TVertex : struct
    {
        int vboId;
        int numVertices;
        Vertex[] vertices;
        VertexArray vertexArray; // TODO Move to a new container parent
        bool uploaded = false;

        public VertexBuffer( Vertex[] vertices )
        {
            numVertices = vertices.Length;
            this.vertices = vertices;
            vboId = GL.GenBuffer();
        }

        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboId);
            if ( !uploaded )
            {
                uploaded = true;
                GL.BufferData<Vertex>(BufferTarget.ArrayBuffer,
                                      (IntPtr)(Vertex.SizeInBytes * vertices.Length),
                                      vertices, BufferUsageHint.StaticDraw);

            }
            
        }

        public void AddVertexArray(VertexArray vertexArray)
        {
            this.vertexArray = vertexArray;
        }

        public void EnableAttributes(ref Shader shader)
        {
            vertexArray.Bind();
        }

        public int Size()
        {
            return numVertices;
        }

        public int Stride()
        {
            return Vertex.SizeInBytes;
        }
    }
}
