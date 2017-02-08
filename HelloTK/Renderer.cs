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
        private Shader shader;
        private Texture texture;
        
        // TODO Move to a node equiv. 
        private Matrix4 model = Matrix4.Identity;
        public Matrix4 Model { set { model = value; } get { return model; } }

        public Renderer(IGeometry geometry, Shader shader)
        {
            this.shader = shader;
            this.geometry = geometry;
            this.vertexBuffer = geometry.CreateVertexBuffer();
            this.indexBuffer = geometry.CreateIndexBuffer(); // returns null if there are no indices
        }

        public void Update( IGeometry geometry )
        {
            this.geometry = geometry;
            geometry.Upload(this.vertexBuffer, this.indexBuffer);
        }

        public void AddIndexBuffer(IndexBuffer indexBuffer)
        {
            this.indexBuffer = indexBuffer;
        }

        public void AddTexture(Texture texture)
        {
            this.texture = texture;
        }

        public void AddShader(Shader shader)
        {
            this.shader = shader;
        }

        public void Draw(Matrix4 view, Matrix4 projection, Vector3 lightPosition, Vector3 ambientColor)
        {
            if (shader != null)
            {
                shader.Use();

                if (texture != null)
                {
                    texture.Bind();
                    shader.SetSamplerUniform(0, 0);
                }

                // Set up uniforms:
                Matrix4 mv = this.model * view;
                Matrix3 mvIT = new Matrix3(mv);
                mvIT.Invert();
                mvIT.Transpose();
                shader.SetUniformMatrix3("mvIT", mvIT);
                shader.SetUniformMatrix4("modelView", mv);
                shader.SetUniformMatrix4("projection", projection);
                shader.SetUniformMatrix4("model", this.model);
                shader.SetUniformMatrix4("view", view);
                shader.SetUniformVector3("lightPosition", lightPosition);
                shader.SetUniformVector3("ambientColor", ambientColor);
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
