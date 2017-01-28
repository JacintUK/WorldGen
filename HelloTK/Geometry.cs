using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloTK
{
    class Geometry<TVertex> where TVertex : struct
    {
        Mesh<TVertex> mesh;
        uint[] indices;

        public Geometry( Mesh<TVertex> mesh, uint[] indices )
        {
            this.mesh = mesh;
            this.indices = indices;
        }

        // Is it minimal # verts? 
        // Does it need normals? (can minimise if not)
        // 

        public void ConvertToVertexPerIndex(string positionName)
        {
        }

        public void AddNormals(string positionName, string normalName)
        {
        }
    }
}
