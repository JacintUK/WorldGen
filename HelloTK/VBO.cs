using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace HelloTK
{
    class VBO
    {
        int vboId;
        int numVertices;
        Vertex[] vertices;
        bool uploaded = false;

        public VBO( Vertex[] vertices )
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
            Vertex.SetOffsets();
        }

        public int Size()
        {
            return numVertices;
        }
    }
}
