using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace HelloTK
{
    internal interface IColorVertex
    {
        void SetColor(Vector4 color);
        Vector4 GetColor();
    }
}
