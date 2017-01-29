using OpenTK;
using System;
using System.Collections.Generic;

namespace HelloTK
{
    class Geometry<TVertex> : IGeometry where TVertex : struct
    {
        Mesh<TVertex> mesh;
        uint[] indices;

        public Geometry( Mesh<TVertex> mesh, uint[] indices )
        {
            this.mesh = mesh;
            this.indices = indices;
        }
        public Geometry(Mesh<TVertex> mesh)
        {
            this.mesh = mesh;
            this.indices = null;
        }

        public void SubDivide(int level)
        {
            for (int j = 0; j < level; ++j)
            {
                List<TVertex> newVertices = new List<TVertex>();
                List<uint> newIndices = new List<uint>();
                int numIndices = indices.Length;
                for (int i = 0; i < numIndices; i += 3)
                {
                    TVertex v1 = mesh.vertices[indices[i]];
                    TVertex v2 = mesh.vertices[indices[i + 1]];
                    TVertex v3 = mesh.vertices[indices[i + 2]];
                    Vector3 pos1 = GetPosition(ref v1, "position");
                    Vector3 pos2 = GetPosition(ref v2, "position");
                    Vector3 pos3 = GetPosition(ref v3, "position");
                    Vector3 e1 = 2.0f * Vector3.Normalize(pos1 + (pos2 - pos1) * 0.5f);
                    Vector3 e2 = 2.0f * Vector3.Normalize(pos2 + (pos3 - pos2) * 0.5f);
                    Vector3 e3 = 2.0f * Vector3.Normalize(pos1 + (pos3 - pos1) * 0.5f);
                    TVertex e1v = SetPos(ref v1, ref e1);
                    TVertex e2v = SetPos(ref v2, ref e2);
                    TVertex e3v = SetPos(ref v3, ref e3);

                    AddTriangle(ref newVertices, ref newIndices, v1, e1v, e3v);
                    AddTriangle(ref newVertices, ref newIndices, v2, e2v, e1v);
                    AddTriangle(ref newVertices, ref newIndices, v3, e3v, e2v);
                    AddTriangle(ref newVertices, ref newIndices, e1v, e2v, e3v);
                }
                mesh = new Mesh<TVertex>(newVertices.ToArray(), mesh.VertexFormat);
                indices = newIndices.ToArray();
            }
        }

        private void AddTriangle( ref List<TVertex> verts, ref List<uint> indices, TVertex v1, TVertex v2, TVertex v3)
        {
            uint numVerts = (uint)verts.Count;
            verts.Add(v1);
            verts.Add(v2);
            verts.Add(v3);
            indices.Add(numVerts++);
            indices.Add(numVerts++);
            indices.Add(numVerts++);
        }

        private TVertex SetPos( ref TVertex vert, ref Vector3 pos )
        {
            IPositionVertex ipv = vert as IPositionVertex;
            if (ipv != null)
            {
                ipv.SetPosition(pos);
            }
            return (TVertex)ipv;
        }

        public void ConvertToVertexPerIndex()
        {
            List<TVertex> newVertices = new List<TVertex>();
            List<uint> newIndices = new List<uint>();
            uint newIndex = 0;
            foreach (uint index in indices )
            {
                newVertices.Add(mesh.vertices[index]);
                newIndices.Add(newIndex++);
            }
            mesh = new Mesh<TVertex>(newVertices.ToArray(), mesh.VertexFormat);
            indices = newIndices.ToArray();
        }

        public void AddNormals(string positionName, string normalName)
        {
            // TODO Currently assuming that we are using triangles, and have
            // maximised the vertices.
            if (mesh.vertices[0].GetType().GetInterface("INormalVertex") != null)
            {
                var posField = mesh.vertices[0].GetType().GetField(normalName);

                for (int i = 0; i < indices.Length; i += 3)
                {
                    Vector3 pos1 = GetPosition(ref mesh.vertices[i], positionName);
                    Vector3 pos2 = GetPosition(ref mesh.vertices[i + 1], positionName);
                    Vector3 pos3 = GetPosition(ref mesh.vertices[i + 2], positionName);
                    Vector3 a = pos2 - pos1;
                    Vector3 b = pos3 - pos1;
                    Vector3 normal = Vector3.Cross(a, b);
                    normal.Normalize();
                    INormalVertex inv = mesh.vertices[i] as INormalVertex;
                    if( inv != null) { 
                        inv.SetNormal(normal);
                    }
                    mesh.vertices[i] = (TVertex)inv;
                    inv = mesh.vertices[i+1] as INormalVertex;
                    if (inv != null)
                    {
                        inv.SetNormal(normal);
                    }
                    mesh.vertices[i+1] = (TVertex)inv;
                    inv = mesh.vertices[i+2] as INormalVertex;
                    if (inv != null)
                    {
                        inv.SetNormal(normal);
                    }
                    mesh.vertices[i+2] = (TVertex)inv;
                }
            }
        }

        private static Vector3 GetPosition(ref TVertex vertex, string positionName)
        {
            var posField = vertex.GetType().GetField(positionName);
            object pos = posField.GetValue(vertex);
            return (Vector3)pos;
        }

        private static void SetNormal(ref TVertex vertex, string normalName, Vector3 normal)
        {
            var posField = vertex.GetType().GetField(normalName);
            posField.SetValue(vertex, normal);
        }

        public IVertexBuffer CreateVertexBuffer()
        {
            return new VertexBuffer<TVertex>(mesh);
        }
        public IndexBuffer CreateIndexBuffer()
        {
            if (indices != null)
            {
                return new IndexBuffer(indices);
            }
            else
            {
                return null;
            }
        }
    }
}
