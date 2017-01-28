using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace HelloTK
{
    class VertexBuffer<TVertex> : IVertexBuffer where TVertex : struct 
    {
        private TVertex[] vertices;
        private VertexArray<TVertex> vertexArray; // TODO Move to a new container parent
        private int vertexSize;
        private int numVertices;
        private int handle;
        private bool uploaded = false;

        public override int Size
        {
            get { return numVertices; }
        }

        public int Stride
        {
            get { return vertexSize; }
        }

        public VertexBuffer( TVertex[] vertices, int vertexSize )
        {
            numVertices = vertices.Length;
            this.vertices = vertices;
            this.vertexSize = vertexSize;
            handle = GL.GenBuffer();
        }

        public override void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
            if ( !uploaded )
            {
                uploaded = true;
                GL.BufferData<TVertex>(BufferTarget.ArrayBuffer,
                                      (IntPtr)(vertexSize * vertices.Length),
                                      vertices, BufferUsageHint.StaticDraw);
            }
        }

        public void AddVertexArray(VertexArray<TVertex> vertexArray)
        {
            this.vertexArray = vertexArray;
        }

        public override void EnableAttributes(ref Shader shader)
        {
            vertexArray.Bind();
        }
    }
}
