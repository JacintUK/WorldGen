using OpenTK;
using System;
using System.Collections.Generic;

namespace HelloTK
{
    class Geometry<TVertex> : IGeometry where TVertex : struct
    {
        Mesh<TVertex> mesh;
        uint[] indices;

        struct Edge { public int triangle1; public int triangle2; }

        public Geometry(Mesh<TVertex> mesh, uint[] indices)
        {
            this.mesh = mesh;
            this.indices = indices;
        }
        public Geometry(Mesh<TVertex> mesh)
        {
            this.mesh = mesh;
            this.indices = null;
        }


        private Dictionary<Int64, uint> edgeCache = new Dictionary<long, uint>();

        /// <summary>
        /// Subdivides mesh into smaller meshes; re-normalizes triangles.
        /// Keeps vertex count to a minimum.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int SubDivide(int level)
        {
            // Assume initial mesh has minimised verts.
            int numVerts = 0;
            for (int j = 0; j < level; ++j)
            {
                // Retain all old vertices at the start of the list
                List<TVertex> newVerts = new List<TVertex>(mesh.vertices);
                List<uint> newIndices = new List<uint>();
                edgeCache.Clear();
                int numIndices = indices.Length;
                numVerts = newVerts.Count;
                for (int i = 0; i < numIndices; i += 3)
                {
                    uint pt1 = GenerateVertex(ref newVerts, i, i + 1);
                    uint pt2 = GenerateVertex(ref newVerts, i + 1, i + 2);
                    uint pt3 = GenerateVertex(ref newVerts, i, i + 2);
                    newIndices.Add(indices[i]); newIndices.Add(pt1); newIndices.Add(pt3);
                    newIndices.Add(indices[i + 1]); newIndices.Add(pt2); newIndices.Add(pt1);
                    newIndices.Add(indices[i + 2]); newIndices.Add(pt3); newIndices.Add(pt2);
                    newIndices.Add(pt1); newIndices.Add(pt2); newIndices.Add(pt3);
                }
                mesh.vertices = newVerts.ToArray();
                indices = newIndices.ToArray();
            }
            edgeCache.Clear();
            return numVerts;
        }

        uint GenerateVertex(ref List<TVertex> verts, int a, int b)
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

        public void TweakTriangles(float ratio)
        {
            // Assumptions: minimised mesh. Shared edges won't work without shared verts.
            FindEdges();
            // How many edges do we have? exchange some percentage of them...
            int numPeturbations = (int)((float)edgeCache2.Count * ratio);
            int numTriangles = mesh.vertices.Length;
            Random rand = new Random();
            List<Int64> keys = new List<Int64>(edgeCache2.Keys);

            for (int i = 0; i < numPeturbations; ++i)
            {
                // Choose a random edge:
                int edgeIndex = rand.Next(edgeCache2.Count);

                // Check out the tris around the edge.
                Int64 key = keys[edgeIndex];
                Edge edge = edgeCache2[key];

                if (IsObtuse(edge.triangle1) || IsObtuse(edge.triangle2))
                {
                    i--;
                    continue;
                }

                // Need to keep tris in CCW order.
                uint a = (uint)(key & 0xffffffff);
                int cornerA = GetTriOffset(edge.triangle1, a);
                // get the next coord in the triangle (in ccw order)
                uint b = indices[edge.triangle1 + ((cornerA + 1) % 3)];
                uint c = indices[edge.triangle1 + ((cornerA + 2) % 3)];

                // if it's the other edge corner; 
                if( b == (uint)((key>>32)& 0xffffffff ))
                {
                    // order is a b c; c is the non-shared vertex.
                    // new tris are: ADC, DBC
                    // tri2 order is a d b
                    uint d = indices[edge.triangle2 + ((GetTriOffset(edge.triangle2, a) + 1) % 3)];
                    indices[edge.triangle1]   = a;
                    indices[edge.triangle1+1] = d;
                    indices[edge.triangle1+2] = c;
                    indices[edge.triangle2]   = d;
                    indices[edge.triangle2+1] = b;
                    indices[edge.triangle2+2] = c;
                }
                else
                {
                    // order is a b c; b is the non-shared value
                    // new tris are ACD, CBD
                    // tri2 order is a b d 
                    uint d = indices[edge.triangle2 + ((GetTriOffset(edge.triangle2, a) + 2) % 3)];
                    indices[edge.triangle1]   = a;
                    indices[edge.triangle1+1] = b;
                    indices[edge.triangle1+2] = d;
                    indices[edge.triangle2]   = c;
                    indices[edge.triangle2+1] = d;
                    indices[edge.triangle2+2] = b;
                }
            }
        }

        private int GetTriOffset(int triangle, uint corner)
        {
            if (indices[triangle] == corner) return 0;
            if (indices[triangle+1] == corner) return 1;
            if (indices[triangle+2] == corner) return 2;
            return -1;
        }

        private bool IsObtuse( int triangle )
        {
            TVertex v1 = mesh.vertices[indices[triangle]];
            TVertex v2 = mesh.vertices[indices[triangle+1]];
            TVertex v3 = mesh.vertices[indices[triangle+2]];
            Vector3 e1 = GetPosition(ref v2) - GetPosition(ref v1);
            Vector3 e2 = GetPosition(ref v3) - GetPosition(ref v1);
            Vector3 e3 = GetPosition(ref v3) - GetPosition(ref v2);
            float l1Sq = e1.LengthSquared;
            float l2Sq = e2.LengthSquared;
            float l3Sq = e3.LengthSquared;
            if( l1Sq + l2Sq < l3Sq || l1Sq + l3Sq < l2Sq || l2Sq + l3Sq < l1Sq )
            {
                return true;
            }
            return false;
        }


        private Dictionary<Int64, Edge> edgeCache2 = new Dictionary<long, Edge>();
        public void FindEdges()
        {
            int numVerts = 0;
            edgeCache2.Clear();
            int numIndices = indices.Length;
            for (int i = 0; i < numIndices; i += 3)
            {
                RegisterEdge(i, i + 1);
                RegisterEdge(i + 1, i + 2);
                RegisterEdge(i, i + 2);
            }
        }

        void RegisterEdge(int a, int b)
        {
            Int64 key1 = CreateKey(indices[a], indices[b]);
            Edge edge;
            if ( ! edgeCache2.TryGetValue(key1, out edge) )
            {
                edge.triangle1 = a-(a%3); // First vertex of the triangle
                edgeCache2.Add(key1, edge);
            }
            else
            {
                edge.triangle2 = a-(a%3);
                edgeCache2[key1] = edge;
            }
        }

        // Do we need some adjacency graphs?



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

        public void AddNormals()
        {
            // TODO Currently assuming that we are using triangles, and have
            // maximised the vertices.
            if (mesh.vertices[0].GetType().GetInterface("INormalVertex") != null)
            {
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
