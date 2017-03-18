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
    struct Vertex3DTangent : IVertex, IPositionVertex, ITangentVertex
    {
        public Vector3 position;
        public Vector3 tangent;
        private static VertexFormat format = new VertexFormat(new List<Attribute> {
                new Attribute() { Name = "aPosition", Type = Attribute.AType.VECTOR3 },
                new Attribute() { Name = "aTangent", Type = Attribute.AType.VECTOR3 },
                });

        public VertexFormat GetVertexFormat()
        {
            return format;
        }
        public static int SizeInBytes { get { return Vector3.SizeInBytes; } }

        public Vertex3DTangent(Vector3 position )
        {
            this.position = position;
            this.tangent = Vector3.UnitZ;
        }
        public Vector3 GetPosition()
        {
            return this.position;
        }
        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }
        public Vector3 GetTangent()
        {
            return tangent;
        }
        public void SetTangent(Vector3 tangent)
        {
            this.tangent = tangent;
        }
    }
}
