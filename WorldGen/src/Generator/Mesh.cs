﻿/*
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

using OpenTK.Mathematics;

namespace WorldGen
{
    internal class Mesh<TVertex> : IMesh where TVertex : struct, IVertex
    {
        public TVertex[] vertices;
        VertexFormat vertexFormat;
        public int Length { get { return vertices.Length; } }
        public VertexFormat VertexFormat { get { return vertexFormat; } }

        public Mesh(TVertex[] vertices)
        {
            this.vertexFormat = vertices[0].GetVertexFormat();
            this.vertices = vertices;
        }

        public void SetColor(int index, ref Vector4 color)
        {
            MeshAttr.SetColor(ref vertices[index], ref color);
        }

        public Vector4 GetColor(int index)
        {
            return MeshAttr.GetColor(ref vertices[index]);
        }

        public void SetPosition(int index, ref Vector3 position)
        {
            MeshAttr.SetPosition(ref vertices[index], ref position);
        }

        public Vector3 GetPosition(int index)
        {
            return MeshAttr.GetPosition(ref vertices[index]);
        }

        public void SetNormal(int index, ref Vector3 normal)
        {
            MeshAttr.SetNormal(ref vertices[index], ref normal);
        }

        public Vector3 GetNormal(int index)
        {
            return MeshAttr.GetNormal(ref vertices[index]);
        }

        public float GetPrimary(int index)
        {
            return MeshAttr.GetPrimary(ref vertices[index]);
        }
    }

    internal class MeshAttr
    { 
        public static Vector3 GetPosition<TVertex2>(ref TVertex2 vertex) where TVertex2 : struct, IVertex
        {
            //if (vertex is IPositionVertex)
            //    return ((IPositionVertex)vertex).GetPosition();
            IPositionVertex ipv = vertex as IPositionVertex;
            if (ipv != null)
            {
                return ipv.GetPosition();
            }
            return Vector3.Zero;
        }

        public static void SetPosition<TVertex2>(ref TVertex2 vert, ref Vector3 pos) where TVertex2 : struct, IVertex
        {
            IPositionVertex ipv = vert as IPositionVertex;
            if (ipv != null)
            {
                ipv.SetPosition(pos);
                vert = (TVertex2)ipv;
            }
        }

        public static Vector3 GetNormal<TVertex2>(ref TVertex2 vertex) where TVertex2 : struct, IVertex
        {
            INormalVertex ipv = vertex as INormalVertex;
            if (ipv != null)
            {
                return ipv.GetNormal();
            }
            return Vector3.Zero;
        }

        public static void SetNormal<TVertex2>(ref TVertex2 vertex, ref Vector3 normal) where TVertex2 : struct, IVertex
        {
            INormalVertex inv = vertex as INormalVertex;
            if (inv != null)
            {
                inv.SetNormal(normal);
                vertex = (TVertex2)inv;
            }
        }

        public static void SetUV<TVertex2>(ref TVertex2 vertex, ref Vector2 uv) where TVertex2 : struct, IVertex
        {
            ITextureCoordinateVertex inv = vertex as ITextureCoordinateVertex;
            if (inv != null)
            {
                inv.SetTextureCoordinates(uv);
                vertex = (TVertex2)inv;
            }
        }
        public static void SetColor<TVertex2>(ref TVertex2 vertex, ref Vector4 color) where TVertex2 : struct, IVertex
        {
            IColorVertex inv = vertex as IColorVertex;
            if (inv != null)
            {
                inv.SetColor(color);
                vertex = (TVertex2)inv;
            }
        }

        public static Vector4 GetColor<TVertex2>(ref TVertex2 vertex) where TVertex2 : struct, IVertex
        {
            IColorVertex ipv = vertex as IColorVertex;
            if (ipv != null)
            {
                return ipv.GetColor();
            }
            return Vector4.Zero;
        }

        public static void SetPrimary<TVertex2>(ref TVertex2 vertex, float primary) where TVertex2 : struct, IVertex
        {
            IPrimary ip = vertex as IPrimary;
            if (ip != null)
            {
                ip.SetPrimary(primary);
                vertex = (TVertex2)ip;
            }
        }

        public static float GetPrimary<TVertex2>(ref TVertex2 vertex) where TVertex2: struct, IVertex
        {
            IPrimary ip = vertex as IPrimary;
            if (ip != null)
            {
                return ip.GetPrimary();
            }
            return 0;
        }
    }
}
