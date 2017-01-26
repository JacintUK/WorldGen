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
        public List<VertexBuffer> vbos = new List<VertexBuffer>();
        public IBO ibo;
        public Shader shader;
        private Matrix4 modelView=Matrix4.Identity;
        public Matrix4 ModelView { set { modelView = value;} get { return modelView; } }

        public Renderer()
        {
        }

        public void AddVBO(VertexBuffer vbo)
        {
            if (vbos != null)
            {
                vbos.Add(vbo);
            }
        }

        public void AddIBO(IBO ibo)
        {
            this.ibo = ibo;
        }

        //public void AddTexture(Texture texture)
        //{
        //}

        public void AddShader(Shader shader)
        {
            this.shader = shader;
        }

        public void Draw(Matrix4 modelView, Matrix4 projection)
        {
            if (shader != null)
            {
                shader.Use();

                // Set up uniforms:
                shader.SetUniformMatrix4("modelView", this.modelView);
                shader.SetUniformMatrix4("projection", projection);
            }
            foreach (VertexBuffer vbo in vbos)
            {
                vbo.Bind();
                vbo.EnableAttributes(ref shader);
            }

            if (ibo != null)
            {
                ibo.Bind();
                GL.DrawElements(PrimitiveType.Triangles, ibo.Size(), DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, vbos[0].Size());
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
    }
}
