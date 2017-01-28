using OpenTK;
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

        // Is it minimal # verts? 
        // Does it need normals? (can minimise if not)
        // 

        public void ConvertToVertexPerIndex()
        {
            List<TVertex> newVertices = new List<TVertex>();
            List<uint> newIndices = new List<uint>();
            uint newIndex = 0;
            foreach (uint index in indices )
            {
                newVertices.Add(mesh.Vertices[index]);
                newIndices.Add(newIndex++);
            }
            mesh = new Mesh<TVertex>(newVertices.ToArray(), mesh.VertexFormat);
            indices = newIndices.ToArray();
        }

        public void AddNormals(string positionName, string normalName)
        {
            // TODO Currently assuming that we are using triangles, and have
            // maximised the vertices.
            int positionOffset = mesh.VertexFormat.FindOffset(positionName);
            int normalOffset = mesh.VertexFormat.FindOffset(normalName);
            if (positionOffset != -1 && normalOffset != -1)
            {
                for (int i = 0; i < indices.Length; i += 3)
                {
                    Vector3 pos1 = GetPosition(mesh.Vertices[i], positionName);
                    Vector3 pos2 = GetPosition(mesh.Vertices[i+1], positionName);
                    Vector3 pos3 = GetPosition(mesh.Vertices[i+2], positionName);
                    Vector3 a = pos2 - pos1;
                    Vector3 b = pos2 - pos3;
                    Vector3 normal = Vector3.Cross(a, b);
                    SetNormal(mesh.Vertices[i], normalName, normal);
                    SetNormal(mesh.Vertices[i+1], normalName, normal);
                    SetNormal(mesh.Vertices[i+2], normalName, normal);
                }
            }
        }

        private static Vector3 GetPosition(TVertex vertex, string positionName)
        {
            var posField = vertex.GetType().GetField(positionName);
            object pos = posField.GetValue(vertex);
            return (Vector3)pos;
        }

        private static void SetNormal(TVertex vertex, string normalName, Vector3 normal)
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
