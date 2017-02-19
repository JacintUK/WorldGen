using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace HelloTK
{
    interface IMesh
    {
        void SetColor(int index, ref Vector4 color);
        Vector4 GetColor(int index);
    }
}
