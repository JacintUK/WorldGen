using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace HelloTK
{
    class Renderer
    {
        public List<VBO> vbos = new List<VBO>();
        public IBO ibo;

        public Renderer()
        {
            //vbos = new List<VBO>();
        }

        public void AddVBO(VBO vbo)
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

        public void Draw()
        {
            foreach (VBO vbo in vbos)
            {
                vbo.Bind();
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
        }

    }
}
