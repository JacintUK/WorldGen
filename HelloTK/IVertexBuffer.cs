using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldGenerator
{
    interface IVertexBuffer
    {
        void Bind(Shader shader);
        int Size { get; }
        void Upload(IMesh mesh);
    }
}
