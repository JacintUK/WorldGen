using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace HelloTK
{
    class IBO
    {
        uint[] indices;
        int iboId;
        int numIndices;
        bool uploaded=false;

        public IBO(uint[] indices)
        {
            numIndices = indices.Length;
            this.indices = indices;
            iboId = GL.GenBuffer();
        }

        public void Bind()
        {
            GL.EnableClientState(ArrayCap.IndexArray);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboId);
            if( !uploaded )
            {
                uploaded = true;
                GL.BufferData<uint>(BufferTarget.ElementArrayBuffer,
                       (IntPtr)(sizeof(uint) * indices.Length),
                       indices, BufferUsageHint.StaticDraw);
            }
        }

        public int Size()
        {
            return numIndices;
        }
    }
}
