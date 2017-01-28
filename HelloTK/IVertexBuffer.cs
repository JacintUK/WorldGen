using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloTK
{
    interface IVertexBuffer
    {
        void Bind();
        void EnableAttributes(ref Shader shader);
        int Size { get; }
    }
}
