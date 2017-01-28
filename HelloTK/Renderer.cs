using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace HelloTK
{
    class Renderer
    {
        private IVertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private IGeometry geometry;
        public Shader shader;

        private Matrix4 model = Matrix4.Identity;
        public Matrix4 Model { set { model = value; } get { return model; } }

        public Renderer(IGeometry geometry, Shader shader)
        {
            this.shader = shader;
            this.geometry = geometry;
            this.vertexBuffer = geometry.CreateVertexBuffer();
            this.indexBuffer = geometry.CreateIndexBuffer(); // returns null if there are no indices
        }

        public void AddIndexBuffer(IndexBuffer indexBuffer)
        {
            this.indexBuffer = indexBuffer;
        }

        //public void AddTexture(Texture texture)
        //{
        //}

        public void AddShader(Shader shader)
        {
            this.shader = shader;
        }

        public void Draw(Matrix4 view, Matrix4 projection)
        {
            if (shader != null)
            {
                shader.Use();

                // Set up uniforms:
                shader.SetUniformMatrix4("modelView", this.model*view);
                shader.SetUniformMatrix4("projection", projection);
            }
            vertexBuffer.Bind(shader);

            if (indexBuffer != null)
            {
                indexBuffer.Bind();
                GL.DrawElements(PrimitiveType.Triangles, indexBuffer.Size(), DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertexBuffer.Size);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
