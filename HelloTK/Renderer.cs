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
        public List<IVertexBuffer> vertexBuffers = new List<IVertexBuffer>();
        public IndexBuffer indexBuffer;
        public Shader shader;

        private Matrix4 model = Matrix4.Identity;
        public Matrix4 Model { set { model = value; } get { return model; } }

        public Renderer()
        {
        }

        public void AddVertexBuffer(IVertexBuffer vertexBuffer)
        {
            if (vertexBuffers != null)
            {
                vertexBuffers.Add(vertexBuffer);
            }
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
                shader.SetUniformMatrix4("modelView", view * this.model);
                shader.SetUniformMatrix4("projection", projection);
            }
            foreach (IVertexBuffer vertexBuffer in vertexBuffers)
            {
                vertexBuffer.Bind();
                vertexBuffer.EnableAttributes(ref shader);
            }

            if (indexBuffer != null)
            {
                indexBuffer.Bind();
                GL.DrawElements(PrimitiveType.Triangles, indexBuffer.Size(), DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertexBuffers[0].Size);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
