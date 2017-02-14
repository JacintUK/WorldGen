using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace HelloTK
{
    class VertexArray<TVertex> where TVertex : struct, IVertex
    {
        private int handle;

        public VertexArray(VertexBuffer<TVertex> vertexBuffer, Shader shader, VertexFormat vertexFormat)
        {
            // Construct array of VertexAttribute from vertex format:
            List < VertexAttribute > attrs = new List<VertexAttribute>();
            foreach (var attr in vertexFormat.Attributes)
            {
                attrs.Add(new VertexAttribute(attr.Name, 
                    VertexFormat.NumberOfFloatsInType(attr.Type),
                    VertexAttribPointerType.Float, vertexFormat.size, attr.Offset));
            }
            Initialize(vertexBuffer, shader, attrs.ToArray());
        }

        public VertexArray(VertexBuffer<TVertex> vertexBuffer, Shader shader,
            params VertexAttribute[] attributes)
        {
            Initialize(vertexBuffer, shader, attributes);
        }

        private void Initialize(VertexBuffer<TVertex> vertexBuffer, Shader shader,
            params VertexAttribute[] attributes)
        {
            // create new vertex array object
            GL.GenVertexArrays(1, out this.handle);

            // bind the object so we can modify it
            this.Bind();

            // bind the vertex buffer object
            vertexBuffer.Bind(shader);

            // set all attributes
            foreach (var attribute in attributes)
                attribute.Set(shader);

            // unbind objects to reset state
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Bind()
        {
            // bind for usage (modification or rendering)
            GL.BindVertexArray(this.handle);
        }
    }
}
