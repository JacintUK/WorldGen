using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WorldGenerator
{
    interface IMesh
    {
        int Length { get; }

        void SetColor(int index, ref Vector4 color);
        Vector4 GetColor(int index);
        void SetPosition(int index, ref Vector3 position);
        Vector3 GetPosition(int index);
    }
}
