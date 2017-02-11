using OpenTK;
using System;
using System.Collections.Generic;

namespace HelloTK
{
    class Geometry<TVertex> : IGeometry where TVertex : struct
    {
        Mesh<TVertex> mesh;
        uint[] indices;
        float idealDistanceToCentroid=1;
        float IdealDistanceToCentroid { get { return idealDistanceToCentroid; } }
        struct Edge { public int triangle1; public int triangle2; }
        Mesh<TVertex> Mesh { get { return mesh; } }

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
        public IGeometry Clone()
        {
            Mesh<TVertex> newMesh = new Mesh<TVertex>(mesh.vertices, mesh.VertexFormat);
            uint[] newIndices = (uint[])indices.Clone();
            Geometry<TVertex> newGeom = new Geometry<TVertex>(newMesh, newIndices);
            return newGeom;
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
                Vector3 pos1 = GetPosition(ref mesh.vertices[indices[a]]);
                Vector3 pos2 = GetPosition(ref mesh.vertices[indices[b]]);
                Vector3 e1 = Vector3.Normalize(pos1 + (pos2 - pos1) * 0.5f);
                TVertex v1 = mesh.vertices[indices[a]];
                SetPosition(ref v1, ref e1);
                middlePt = (uint)verts.Count;
                edgeCache.Add(key1, middlePt);
                verts.Add(v1);
            }
            return middlePt;
        }

        public void TweakTriangles(float ratio, ref Random rand)
        {
            // Assumptions: minimised mesh. Shared edges won't work without shared verts.
            FindEdges();
            // How many edges do we have? exchange some percentage of them...
            int numPeturbations = (int)((float)edgeCache2.Count * ratio);
            int numTriangles = mesh.vertices.Length;
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

                // TODO Don't tweak tris of a pentagon.

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

        // Alternative algorithm
        // for each triangle
        //   find normal
        //      find centroid radius
        //        Tilt triangle towards centroid radius
        //        moving edges towards symmetry.
        //        generate new point for each tri vert
        // for each vert, calc average of all new verts, normalise and apply



        // Algorithm is nearly there, but on some triangles, there is a potential that converging on
        // centroid tilts through random moves away from centroidal radius, and they become thin / start overlapping.
        public void RelaxTriangles1(ref Random rand, int levels)
        {
            int numIndices = indices.Length;
            for (int l = 0; l < levels; ++l)
            {
                for (int i = 0; i < numIndices; i += 3)
                {
                    if (1==1)//TooThin(i))
                    {
                        Vector3 centroid = GetCentroid(i);
                        centroid.Normalize();

                        // Compare each corner to the centroid
                        // if too far off some ideal length ( the centroid of an equilateral triangle ),
                        // pull vertex closer to centroid, (without buggering up the other triangles using
                        // this point.)
                        Vector3[] oldPositions = new Vector3[3]
                        {
                            new Vector3( GetPosition(ref mesh.vertices[indices[i]])),
                            new Vector3( GetPosition(ref mesh.vertices[indices[i+1]])),
                            new Vector3( GetPosition(ref mesh.vertices[indices[i+2]])),
                        };
                        Vector3[] newPositions = new Vector3[3];
                        for (int j = 0; j < 3; ++j)
                        {
                            TVertex v1 = mesh.vertices[indices[i + j]];
                            Vector3 v1Pos = GetPosition(ref v1);
                            Vector3 e1 = centroid - v1Pos;

                            if (e1.Length > idealDistanceToCentroid * 1.1)
                            {
                                // Move vertex closer to centroid
                                float factor = 1 - idealDistanceToCentroid / e1.Length;
                                //int fraction = 1 + rand.Next((int)(90.0*(1.0-idealDistanceToCentroid/ e1.Length)));
                                //factor = factor * (float)fraction / 100.0f;
                                float fraction = (0.5f + 0.05f * rand.Next(5)) * (1.0f - idealDistanceToCentroid / e1.Length);
                                factor = factor * fraction;
                                v1Pos = Vector3.Normalize(v1Pos + e1 * factor);
                            }
                            else if (e1.Length < idealDistanceToCentroid * 0.9)
                            {
                                // Move vertex away from the centroid
                                float factor = 1 - e1.Length / idealDistanceToCentroid;
                                //int fraction = 1 + rand.Next((int)(90.0*(e1.Length/idealDistanceToCentroid)));
                                //factor = factor * (float)fraction / 100.0f;
                                float fraction = (0.5f + 0.05f * rand.Next(5)) * (e1.Length / idealDistanceToCentroid);
                                factor = factor * fraction;
                                v1Pos = Vector3.Normalize(v1Pos - e1 * factor);
                            }
                            newPositions[j] = v1Pos;
                        }

                        // If this makes the triangle less parallel to the sphere, don't do it.
                        Vector3 a = newPositions[1] - newPositions[0];
                        Vector3 b = newPositions[2] - newPositions[1];
                        Vector3 normal = Vector3.Cross(a, b);
                        normal.Normalize();
                        float dot = Vector3.Dot(centroid, normal);
                        double phi = Math.Acos(dot);

                        a = oldPositions[1] - oldPositions[0];
                        b = oldPositions[2] - oldPositions[1];
                        normal = Vector3.Cross(a, b);
                        normal.Normalize();
                        dot = Vector3.Dot(centroid, normal);
                        double theta = Math.Acos(dot);
                        if(phi < dot)
                        {
                            for (int j = 0; j < 3; ++j)
                            {
                                SetPosition(ref mesh.vertices[indices[i + j]], ref newPositions[j]);
                            }
                        }

                        // If any surrounding triangles would be less parallax to the sphere than
                        // this is more parallax, don't do it.
                    }
                }
            }
        }

        public float RelaxTriangles(float multiplier)
        {
            double totalSurfaceArea = 4.0 * Math.PI; 
            double idealFaceArea = totalSurfaceArea / (indices.Length / 3);
            double idealEdgeLength = Math.Sqrt(idealFaceArea * 4.0 / Math.Sqrt(3.0));
            double idealDistanceToCentroid = idealEdgeLength * Math.Sqrt(3) / 3.0 * 0.9;

            Vector3[] shiftPositions = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                shiftPositions[i] = new Vector3(Vector3.Zero);
            }
            int numIndices = indices.Length;

            for (int i = 0; i < numIndices; i += 3)
            {
                Vector3 centroid = GetCentroid(i);
                centroid.Normalize();

                Vector3[] oldPositions = new Vector3[3]
                {
                    new Vector3( GetPosition(ref mesh.vertices[indices[i]])),
                    new Vector3( GetPosition(ref mesh.vertices[indices[i+1]])),
                    new Vector3( GetPosition(ref mesh.vertices[indices[i+2]])),
                };
                float[] edgeLengths = new float[3]
                {
                    new Vector3(oldPositions[1] - oldPositions[0]).Length,
                    new Vector3(oldPositions[2] - oldPositions[1]).Length,
                    new Vector3(oldPositions[2] - oldPositions[0]).Length
                };
                for( int j=0; j<3; ++j)
                {
                    Vector3 midLine = centroid - oldPositions[j];
                    float midLength = midLine.Length;
                    midLine *= (float)(multiplier * (midLength - idealDistanceToCentroid) / midLength);
                    shiftPositions[indices[i+j]] += midLine;
                }
            }
            var origin = Vector3.Zero;
            Plane p = new Plane(Vector3.UnitY, Vector3.Zero);
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                Vector3 vertexPosition = GetPosition(ref mesh.vertices[i]);
                p.Redefine(vertexPosition, origin);
                shiftPositions[i] = vertexPosition + p.ProjectPoint(shiftPositions[i]);
                shiftPositions[i].Normalize();
            }
            float[] rotationSuppressions= new float[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                rotationSuppressions[i] = 0;
            }
            FindEdges();
            foreach( var iter in edgeCache2)
            {
                Int64 key = iter.Key;
                int index1 = (int)(key & 0xffffffff);
                int index2 = (int)((key >> 32) & 0xffffffff);
                Vector3 oldPos1 = GetPosition(ref mesh.vertices[index1]);
                Vector3 oldPos2 = GetPosition(ref mesh.vertices[index2]);
                Vector3 newPos1 = shiftPositions[index1];
                Vector3 newPos2 = shiftPositions[index2];
                Vector3 oldEdge = oldPos2 - oldPos1;
                oldEdge.Normalize();
                Vector3 newEdge = newPos2 - newPos1;
                newEdge.Normalize();
                float suppression = (1.0f - Vector3.Dot(oldEdge, newEdge)) * 0.5f;
                rotationSuppressions[index1] = Math.Max(suppression, rotationSuppressions[index1]);
                rotationSuppressions[index2] = Math.Max(suppression, rotationSuppressions[index2]);
            }

            float totalShift = 0;
            for( int i=0; i<mesh.vertices.Length; ++i )
            {
                Vector3 pos = GetPosition(ref mesh.vertices[i]);
                Vector3 delta = pos;
                pos = Lerp(pos, shiftPositions[i], (float)(1.0f - Math.Sqrt(rotationSuppressions[i])));
                pos.Normalize();
                SetPosition(ref mesh.vertices[i], ref pos);
                delta -= pos;
                totalShift += delta.Length;
            }
            return totalShift;
        }

        static private Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return (1.0f - t) * a + t * b;
        }

        public void ClearColor()
        {
            for(int i=0; i<mesh.vertices.Length; ++i)
            {
                SetColor(ref mesh.vertices[i], new Vector4(0.2f, 0.2f, 1.0f, 1.0f));
            }
        }
        public float CalculateIdealDistanceToCentroid()
        {
            TVertex v1 = mesh.vertices[indices[0]];
            TVertex v2 = mesh.vertices[indices[1]];
            TVertex v3 = mesh.vertices[indices[2]];
            Vector3 e1 = GetPosition(ref v2) - GetPosition(ref v1);
            Vector3 e2 = GetPosition(ref v3) - GetPosition(ref v1);
            Vector3 e3 = GetPosition(ref v3) - GetPosition(ref v2);
            float averageLength = (e1.Length + e2.Length + e3.Length) / 3.0f;

            idealDistanceToCentroid = averageLength * (float)Math.Sqrt(3) / 3.0f;
            return idealDistanceToCentroid;
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

        private bool TooThin( int triangle )
        {
            TVertex v1 = mesh.vertices[indices[triangle]];
            TVertex v2 = mesh.vertices[indices[triangle + 1]];
            TVertex v3 = mesh.vertices[indices[triangle + 2]];
            Vector3 e1 = GetPosition(ref v2) - GetPosition(ref v1);
            Vector3 e2 = GetPosition(ref v3) - GetPosition(ref v1);
            Vector3 e3 = GetPosition(ref v3) - GetPosition(ref v2);

            float[] angles = new float[3];
            
            angles[0] = (float)Math.Acos((e2.LengthSquared + e3.LengthSquared - e1.LengthSquared) / (2.0f * e2.Length * e3.Length));
            angles[1] = (float)Math.Acos((e1.LengthSquared + e3.LengthSquared - e2.LengthSquared) / (2.0f * e1.Length * e3.Length));
            angles[2] = (float)Math.PI - (angles[0] + angles[1]);
            const float LOWER = (float)Math.PI * 35.0f / 180.0f;
            const float UPPER = (float)Math.PI * 80.0f / 180.0f;
            bool tooThin = false;
            for (int i=0; i<3; ++i)
            {
                if( angles[i] < LOWER || angles[i] > UPPER )
                {
                    tooThin = true;
                    break;
                }
            }
            return tooThin;
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

        static Int64 CreateKey(uint a, uint b)
        {
            Int64 min = a<b ? a : b;
            Int64 max = a>=b ? a : b;
            Int64 key = min << 32 | max;
            return key;
        }

        private Vector3 GetCentroid(int triangle)
        {
            TVertex v1 = mesh.vertices[indices[triangle]];
            TVertex v2 = mesh.vertices[indices[triangle + 1]];
            TVertex v3 = mesh.vertices[indices[triangle + 2]];
            Vector3 v1Pos = GetPosition(ref v1);
            Vector3 v2Pos = GetPosition(ref v2);
            Vector3 v3Pos = GetPosition(ref v3);
            Vector3 e1 = v2Pos - v1Pos;
            Vector3 midPt = v1Pos + e1 * 0.5f;
            Vector3 centroid = midPt + (v3Pos - midPt) / 3.0f;
            return centroid;
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

                    SetNormal(ref mesh.vertices[i], normal);
                    SetNormal(ref mesh.vertices[i+1], normal);
                    SetNormal(ref mesh.vertices[i+2], normal);
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
                SetUV(ref mesh.vertices[i],   uv1);
                SetUV(ref mesh.vertices[i+1], uv2);
                SetUV(ref mesh.vertices[i+2], uv3);
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

        private static void SetPosition(ref TVertex vert, ref Vector3 pos)
        {
            IPositionVertex ipv = vert as IPositionVertex;
            if (ipv != null)
            {
                ipv.SetPosition(pos);
            }
            vert = (TVertex)ipv;
        }

        private static void SetNormal(ref TVertex vertex, Vector3 normal)
        {
            INormalVertex inv = vertex as INormalVertex;
            if (inv != null)
            {
                inv.SetNormal(normal);
            }
            vertex = (TVertex)inv;
        }
        private static void SetUV(ref TVertex vertex, Vector2 uv)
        {
            ITextureCoordinateVertex inv = vertex as ITextureCoordinateVertex;
            if (inv != null)
            {
                inv.SetTextureCoordinates(uv);
            }
            vertex = (TVertex)inv;
        }
        private static void SetColor(ref TVertex vertex, Vector4 color)
        {
            IColorVertex inv = vertex as IColorVertex;
            if (inv != null)
            {
                inv.SetColor(color);
            }
            vertex = (TVertex)inv;
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
        public void Upload( IVertexBuffer vbo, IndexBuffer ibo)
        {
            vbo.Upload(mesh);
            ibo.Upload(indices);
        }
    }
}
