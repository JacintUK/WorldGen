/*
 * Copyright 2018 David Ian Steele
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;

namespace WorldGen
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

        public static Geometry<Vertex3DColor> GenerateCircle(Vector3 origin, Vector3 axis, float radius, Vector4 color)
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
        
        /// <summary>
        /// Create a unit icosphere (radius 1) from an icosahedron with the given number
        /// of subdivisions.
        /// </summary>
        /// <param name="subDivisions"></param>
        /// <returns>icosphere geometry.</returns>
        static public IGeometry CreateIcosphere(int subDivisions)
        {
            Vertex3DColor[] verts = new Vertex3DColor[12];
            Vector4 color = new Vector4(0.2f, 0.2f, 1.0f, 1.0f);
            Vector4 color2 = new Vector4(1.0f, 0.2f, 1.0f, 1.0f);
            Vector4 color3 = new Vector4(0.2f, 1.0f, 1.0f, 1.0f);

            const float t = 1.61803398875f;// approximation of golden ratio

            verts[0] = new Vertex3DColor(Vector3.Normalize(new Vector3(-1, t, 0)), color2); // Subdivision generates Sierpinski triangle at pole
            verts[1] = new Vertex3DColor(Vector3.Normalize(new Vector3(1, t, 0)), color);
            verts[2] = new Vertex3DColor(Vector3.Normalize(new Vector3(-1, -t, 0)), color);
            verts[3] = new Vertex3DColor(Vector3.Normalize(new Vector3(1, -t, 0)), color3); // Subdivision generates Sierpinski triangle at other pole

            verts[4] = new Vertex3DColor(Vector3.Normalize(new Vector3(0, -1, t)), color);
            verts[5] = new Vertex3DColor(Vector3.Normalize(new Vector3(0, 1, t)), color);
            verts[6] = new Vertex3DColor(Vector3.Normalize(new Vector3(0, -1, -t)), color);
            verts[7] = new Vertex3DColor(Vector3.Normalize(new Vector3(0, 1, -t)), color);

            verts[8] = new Vertex3DColor(Vector3.Normalize(new Vector3(t, 0, -1)), color);
            verts[9] = new Vertex3DColor(Vector3.Normalize(new Vector3(t, 0, 1)), color);
            verts[10] = new Vertex3DColor(Vector3.Normalize(new Vector3(-t, 0, -1)), color);
            verts[11] = new Vertex3DColor(Vector3.Normalize(new Vector3(-t, 0, 1)), color);
            var mesh = new Mesh<Vertex3DColor>(verts);
            var indices = new List<uint>();

            AddIndices(ref indices, 0, 1, 7);
            AddIndices(ref indices, 0, 5, 1);
            AddIndices(ref indices, 0, 11, 5);
            AddIndices(ref indices, 0, 10, 11);
            AddIndices(ref indices, 0, 7, 10);

            AddIndices(ref indices, 1, 5, 9);
            AddIndices(ref indices, 5, 4, 9);
            AddIndices(ref indices, 5, 11, 4);
            AddIndices(ref indices, 11, 2, 4);
            AddIndices(ref indices, 11, 10, 2);

            AddIndices(ref indices, 10, 6, 2);
            AddIndices(ref indices, 10, 7, 6);
            AddIndices(ref indices, 7, 8, 6);
            AddIndices(ref indices, 7, 1, 8);
            AddIndices(ref indices, 1, 9, 8);

            AddIndices(ref indices, 3, 4, 2);
            AddIndices(ref indices, 3, 2, 6);
            AddIndices(ref indices, 3, 6, 8);
            AddIndices(ref indices, 3, 8, 9);
            AddIndices(ref indices, 3, 9, 4);

            var geometry = new Geometry<Vertex3DColor>(mesh, indices.ToArray());
            int vertCount = geometry.SubDivide(subDivisions);

            return geometry;
        }


        private static void AddIndices(ref List<uint> list, uint one, uint two, uint three)
        {
            list.Add(one); list.Add(two); list.Add(three);
        }
    }
}
