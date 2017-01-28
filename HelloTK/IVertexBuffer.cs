using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloTK
{
    abstract class IVertexBuffer
    {
        public abstract void Bind();
        public abstract void EnableAttributes(ref Shader shader);
        public abstract int Size { get; }
    }
}
