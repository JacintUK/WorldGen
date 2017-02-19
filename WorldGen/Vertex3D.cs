using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WorldGenerator
{
    struct Vertex3D : IVertex, IPositionVertex
    {
        public Vector3 position;
        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3 }
                });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }
        public static int SizeInBytes { get { return Vector3.SizeInBytes; } }

        public Vertex3D(Vector3 position )
        {
            this.position = position;
        }
        public Vector3 GetPosition()
        {
            return this.position;
        }
        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }
    }
}
