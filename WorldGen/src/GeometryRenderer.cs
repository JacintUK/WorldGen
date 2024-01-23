/*
 * Copyright 2019 David Ian Steele
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


using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.InteropServices;

namespace WorldGen
{
    public class TouchedEventArgs : EventArgs { public uint VertexIndex { get; set; } }
    public delegate void TouchedEvent(object sender, TouchedEventArgs e);

    abstract class IGeometryRenderer
    {
        public Renderer Renderer { get; set; }
        public bool Sensitive { get; set; } = false;
        public abstract bool HitTest(Vector3 origin, Vector3 direction);

        public abstract IGeometry GetGeometry();

        public event TouchedEvent touchedEvent;
        public virtual void RaiseTouchEvent(uint vertexIndex)
        {
            TouchedEventArgs e = new()
            {
                VertexIndex = vertexIndex
            };
            touchedEvent?.Invoke(this, e);
        }
   }

    class GeometryRenderer<TVertex> : IGeometryRenderer where TVertex : struct, IVertex
    {
        public Geometry<TVertex> geometry;
        private VertexBuffer<TVertex> vbo;
        private IndexBuffer ibo;
        public uint HitVertexIndex { get; private set; } = 0;

        public static GeometryRenderer<Vertex> NewQuad(Shader shader)
        {
            Vertex[] quad = new Vertex[4]
            {
                // Texture coords - top is 0, bottom is 1
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0,1), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),
                new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector2(1,1), new Vector4(1.0f, 0.0f, 1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f,  0.5f, 0.0f), new Vector2(0,0), new Vector4(1.0f, 1.0f, 1.0f, 1.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector2(1,0), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),
            };
            uint[] indices = new uint[6]
            {
                0, 1, 2, 2, 1, 3
            };
            var m = new Mesh<Vertex>(quad);
            var g = new Geometry<Vertex>(m, indices);
            var gr = new GeometryRenderer<Vertex>(g, shader);
            return gr;
        }

        public GeometryRenderer(Geometry<TVertex> geometry, Shader shader)
        {
            this.geometry = geometry;
            vbo = new VertexBuffer<TVertex>((geometry.Mesh as Mesh<TVertex>).vertices);
            if (geometry.Indices != null)
                ibo = new IndexBuffer((geometry.Indices as Indices).IndexArray);
            Renderer = new Renderer(vbo, ibo, shader);
            Renderer.PrimitiveType = geometry.PrimitiveType;
        }

        public GeometryRenderer(Renderer renderer)
        {
            this.geometry = null;
            Renderer = renderer;
            this.vbo = (VertexBuffer<TVertex>)renderer.vbo;
            this.ibo = renderer.ibo;
        }

        public void ChangeGeometry(Geometry<TVertex> geometry)
        {
            this.geometry = geometry;
            Update();
        }

        public void Update()
        {
            geometry?.Upload(vbo, ibo);
        }

        public void Update(Geometry<TVertex> geometry)
        {
            this.geometry = geometry;
            Update();
        }

        struct S { public uint primary; public Vector3 vec; public S(uint p, Vector3 v) { primary = p; vec = v; } };
            
        public override bool HitTest(Vector3 localOrigin, Vector3 localDirection)
        {
            if (Sensitive && Renderer.Visible)
            {
                var hits = new S[2];
                int hitIndex = 0;

                // TODO: Add octree to local mesh for fastest lookup!

                for (int i = 0; i<geometry.NumIndices / 3; ++i)
                {
                    Vector4 v0 = new Vector4(geometry.Mesh.GetPosition((int)geometry.Indices[i * 3]), 1);
                    Vector4 v1 = new Vector4(geometry.Mesh.GetPosition((int)geometry.Indices[i * 3 + 1]), 1);
                    Vector4 v2 = new Vector4(geometry.Mesh.GetPosition((int)geometry.Indices[i * 3 + 2]), 1);

                    // Intersect test triangle and ray
                    if (RayTriangleIntersection.RayTriangleIntersect(localOrigin, localDirection, v0.Xyz, v1.Xyz, v2.Xyz) != null)
                    {
                        if (hitIndex< 2)
                        {
                            hits[hitIndex++] = new S((uint)geometry.Mesh.GetPrimary((int)geometry.Indices[i * 3]), v0.Xyz);
                            if (hitIndex == 2) break; // Can stop looking after 2 hits
                        }
                    }
                }
                if(hitIndex > 0)
                {
                    Vector3 a = hits[0].vec - localOrigin;
                    Vector3 b = hits[1].vec - localOrigin;
                    HitVertexIndex = hits[0].primary;
                    if (hitIndex > 1 && a.LengthSquared > b.LengthSquared)
                    {
                        HitVertexIndex = hits[1].primary;
                    }
                    if (HitVertexIndex > geometry.NumVertices)
                    {
                        HitVertexIndex = 0;
                    }
                    RaiseTouchEvent(HitVertexIndex);
                    return true; // Stop further handlers responding
                }
            }
            return false;
        }

        public override Geometry<TVertex> GetGeometry()
        {
            return this.geometry;
        }

        public class RayTriangleIntersection
        {
            public static Vector3? RayTriangleIntersect(Vector3 rayOrigin, Vector3 rayDirection, Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
            {
                const float epsilon = 0.000001f; // A small value to avoid division by zero

                Vector3 edge1 = vertex1 - vertex0;
                Vector3 edge2 = vertex2 - vertex0;

                Vector3 h = Vector3.Cross(rayDirection, edge2);
                float a = Vector3.Dot(edge1, h);

                if (Math.Abs(a) < epsilon)
                    return null; // Ray and triangle are parallel

                float f = 1.0f / a;
                Vector3 s = rayOrigin - vertex0;
                float u = f * Vector3.Dot(s, h);

                if (u < 0.0f || u > 1.0f)
                    return null; // Intersection point is outside triangle

                Vector3 q = Vector3.Cross(s, edge1);
                float v = f * Vector3.Dot(rayDirection, q);

                if (v < 0.0f || u + v > 1.0f)
                    return null; // Intersection point is outside triangle

                float t = f * Vector3.Dot(edge2, q);

                if (t > epsilon)
                {
                    Vector3 intersectionPoint = rayOrigin + rayDirection * t;
                    return intersectionPoint; // Intersection point found
                }

                return null; // No intersection
            }
        }
    }
}

