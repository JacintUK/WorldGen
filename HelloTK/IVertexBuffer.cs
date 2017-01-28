using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloTK
{
    interface IVertexBuffer
    {
        void Bind(Shader shader);
        int Size { get; }
    }
}
