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

        public void SubDivideBad(int level)
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
                    Vector3 pos1 = GetPosition(ref v1);
                    Vector3 pos2 = GetPosition(ref v2);
                    Vector3 pos3 = GetPosition(ref v3);
                    Vector3 e1 = 2.0f * Vector3.Normalize(pos1 + (pos2 - pos1) * 0.5f);
                    Vector3 e2 = 2.0f * Vector3.Normalize(pos2 + (pos3 - pos2) * 0.5f);
                    Vector3 e3 = 2.0f * Vector3.Normalize(pos1 + (pos3 - pos1) * 0.5f);
                    TVertex e1v = SetPosition(ref v1, ref e1);
                    TVertex e2v = SetPosition(ref v2, ref e2);
                    TVertex e3v = SetPosition(ref v3, ref e3);

                    AddTriangle(ref newVertices, ref newIndices, v1, e1v, e3v);
                    AddTriangle(ref newVertices, ref newIndices, v2, e2v, e1v);
                    AddTriangle(ref newVertices, ref newIndices, v3, e3v, e2v);
                    AddTriangle(ref newVertices, ref newIndices, e1v, e2v, e3v);
                }
                mesh = new Mesh<TVertex>(newVertices.ToArray(), mesh.VertexFormat);
                indices = newIndices.ToArray();
            }
        }

        private void AddTriangle(ref List<TVertex> verts, ref List<uint> indices, TVertex v1, TVertex v2, TVertex v3)
        {
            uint numVerts = (uint)verts.Count;
            verts.Add(v1);
            verts.Add(v2);
            verts.Add(v3);
            indices.Add(numVerts++);
            indices.Add(numVerts++);
            indices.Add(numVerts++);
        }

        // Eventually holds all old unique edges as keys, with first new midpoint index
        private Dictionary<Int64, uint> edgeCache = new Dictionary<long, uint>(); 
        public int SubDivide(int level)
        {
            // Assume initial mesh has minimised verts.
            int numVerts=0;
            for( int j=0; j<level; ++j)
            {
                // Retain all old vertices at the start of the list
                // On the last iteration, these are the centroids.
                List<TVertex> newVerts = new List<TVertex>(mesh.vertices);
                List<uint> newIndices = new List<uint>();
                edgeCache.Clear();
                int numIndices = indices.Length;
                numVerts = newVerts.Count; 
                for (int i = 0; i < numIndices; i += 3)
                {
                    uint pt1 = GenerateVertex(ref newVerts, i, i+1);
                    uint pt2 = GenerateVertex(ref newVerts, i+1, i+2);
                    uint pt3 = GenerateVertex(ref newVerts, i, i+2);
                    newIndices.Add(indices[i]);   newIndices.Add(pt1); newIndices.Add(pt3);
                    newIndices.Add(indices[i+1]); newIndices.Add(pt2); newIndices.Add(pt1);
                    newIndices.Add(indices[i+2]); newIndices.Add(pt3); newIndices.Add(pt2);
                    newIndices.Add(pt1); newIndices.Add(pt2); newIndices.Add(pt3);
                }
                mesh.vertices = newVerts.ToArray();
                indices = newIndices.ToArray();
            }
            return numVerts;
        }

        uint GenerateVertex( ref List<TVertex> verts, int a, int b)
        {
            Int64 key1 = CreateKey(indices[a], indices[b]);
            uint middlePt;
            if (!edgeCache.TryGetValue(key1, out middlePt))
            {
                TVertex v1 = mesh.vertices[indices[a]];
                TVertex v2 = mesh.vertices[indices[b]];
                Vector3 pos1 = GetPosition(ref v1);
                Vector3 pos2 = GetPosition(ref v2);
                Vector3 e1 = 2.0f * Vector3.Normalize(pos1 + (pos2 - pos1) * 0.5f);
                TVertex e1v = SetPosition(ref v1, ref e1);
                middlePt = (uint)verts.Count;
                edgeCache.Add(key1, middlePt);
                verts.Add(e1v);
            }
            return middlePt;
        }

        static Int64 CreateKey(uint a, uint b)
        {
            Int64 min = a<b ? a : b;
            Int64 max = a>=b ? a : b;
            Int64 key = min << 32 | max;
            return key;
        }


        public void ConvertToVertexPerIndex()
        {
            List<TVertex> newVertices = new List<TVertex>();
            List<uint> newIndices = new List<uint>();
            uint newIndex = 0;
            foreach (uint index in indices )
            {
                newVertices.Add(mesh.vertices[(int)index]);
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
                    Vector3 pos1 = GetPosition(ref mesh.vertices[i]);
                    Vector3 pos2 = GetPosition(ref mesh.vertices[i + 1]);
                    Vector3 pos3 = GetPosition(ref mesh.vertices[i + 2]);
                    Vector3 a = pos2 - pos1;
                    Vector3 b = pos3 - pos1;
                    Vector3 normal = Vector3.Cross(a, b);
                    normal.Normalize();

                    mesh.vertices[i]   = SetNormal(ref mesh.vertices[i], normal);
                    mesh.vertices[i+1] = SetNormal(ref mesh.vertices[i+1], normal);
                    mesh.vertices[i+2] = SetNormal(ref mesh.vertices[i+2], normal);
                }
            }
        }

        public void AddUVs()
        {
            Vector2 uv1 = new Vector2(0, 1);
            Vector2 uv2 = new Vector2(0.5f, 0.1328f);
            Vector2 uv3 = new Vector2(1, 1);
            for (int i = 0; i < indices.Length; i += 3)
            {
                mesh.vertices[i]   = SetUV(ref mesh.vertices[i],   uv1);
                mesh.vertices[i+1] = SetUV(ref mesh.vertices[i+1], uv2);
                mesh.vertices[i+2] = SetUV(ref mesh.vertices[i+2], uv3);
            }
        }

        private static Vector3 GetPosition(ref TVertex vertex)
        {
            IPositionVertex ipv = vertex as IPositionVertex;
            if (ipv != null)
            {
                return ipv.GetPosition();
            }
            return Vector3.Zero;
        }

        private static TVertex SetPosition(ref TVertex vert, ref Vector3 pos)
        {
            IPositionVertex ipv = vert as IPositionVertex;
            if (ipv != null)
            {
                ipv.SetPosition(pos);
            }
            return (TVertex)ipv;
        }

        private static TVertex SetNormal(ref TVertex vertex, Vector3 normal)
        {
            INormalVertex inv = vertex as INormalVertex;
            if (inv != null)
            {
                inv.SetNormal(normal);
            }
            return (TVertex)inv;
        }
        private static TVertex SetUV(ref TVertex vertex, Vector2 uv)
        {
            ITextureCoordinateVertex inv = vertex as ITextureCoordinateVertex;
            if (inv != null)
            {
                inv.SetTextureCoordinates(uv);
            }
            return (TVertex)inv;
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
