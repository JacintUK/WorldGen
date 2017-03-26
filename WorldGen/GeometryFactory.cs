using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WorldGenerator
{
    class GeometryFactory
    {
        public static void AddArc(IGeometry geometry, Vector3 pos1, Vector3 pos2, float radius, Vector4 color)
        {
            List<Vertex3DColor> verts = new List<Vertex3DColor>();
            AddArcVerts(verts, pos1, pos2, radius, color);

            Mesh<Vertex3DColor> mesh = new Mesh<Vertex3DColor>(verts.ToArray());
            geometry.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Lines;

        }

        public static IGeometry GenerateShortestArc(Vector3 pos1, Vector3 pos2, float radius, Vector4 color)
        {
            List<Vertex3DColor> verts = new List<Vertex3DColor>();
            AddArcVerts(verts, pos1, pos2, radius, color);
            Mesh<Vertex3DColor> mesh = new Mesh<Vertex3DColor>(verts.ToArray());
            Geometry<Vertex3DColor> geometry = new Geometry<Vertex3DColor>(mesh);
            geometry.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip;
            return geometry;
        }

        public static IGeometry GenerateCircle(Vector3 origin, Vector3 axis, float radius, Vector4 color)
        {
            List<Vertex3DColor> verts = new List<Vertex3DColor>();
            // iterate along circular arc, generating points each 0.2 degrees
            axis.Normalize();

            // find a vector perp to axis:
            // set x, y to 1;  axis.x + axis.y + z*axis.z = 0
            // z = (-axis.x-axis.y)/axis.z
            Vector3 perp1 = Vector3.UnitZ;
            if (axis.Z != 0)
                perp1 = new Vector3(1.0f, 1.0f, (-axis.X - axis.Y) / axis.Z);
            // else if axis.Z is zero, UnitZ is perpendicular to axis.
            Vector3 perp2 = Vector3.Cross(axis, perp1);

            AddArcVerts(verts, origin + perp1, origin + perp2, radius, color);
            AddArcVerts(verts, origin + perp2, origin - perp1, radius, color);
            AddArcVerts(verts, origin - perp1, origin - perp2, radius, color);
            AddArcVerts(verts, origin - perp2, origin + perp1, radius, color);

            Mesh<Vertex3DColor> mesh = new Mesh<Vertex3DColor>(verts.ToArray());
            Geometry<Vertex3DColor> geometry = new Geometry<Vertex3DColor>(mesh);
            geometry.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.LineStrip;
            return geometry;
        }

        public static void AddArcVerts(List<Vertex3DColor> verts, Vector3 pos1, Vector3 pos2, float radius, Vector4 color)
        {
            // Project points onto sphere of given radius
            pos1.Normalize();
            pos2.Normalize();

            // Determine arc degree:
            double dot = Vector3.Dot(pos1, pos2);
            if (dot != 1)
            {
                double theta = Math.Acos(dot);
                pos2 *= radius;
                pos2 *= radius;
                double SineTheta = Math.Sin(theta);
                // Slerp = Sine(theta-angle)*pos1/Sine(theta) + Sine(angle)*pos2/Sine(theta)
                // angle: 0 -> theta
                // iterate along arc, generating points each 0.2 degrees
                for (double angle = 0.0f; angle <= theta; angle += Math.PI / 1800.0f)
                {
                    Vector3 pAngle = (float)(Math.Sin(theta - angle) / SineTheta) * pos1 + (float)(Math.Sin(angle) / SineTheta) * pos2;
                    Vertex3DColor vertex = new Vertex3DColor(pAngle, color);
                    verts.Add(vertex);
                }
            }
        }
    }
}
