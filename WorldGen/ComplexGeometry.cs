using OpenTK;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace WorldGenerator
{
    sealed class Indices : IIndices
    {
        private uint[] indices;
        public uint this[int i]{ get { return indices[i]; } set { indices[i] = value; } }
        public int Length {  get { return indices.Length; } }
        
        public Indices( uint[] indices )
        {
            this.indices = indices;
        }

        public Indices Clone()
        {
            uint[] newIndices = null;
            if (indices != null)
            {
               newIndices = (uint[])indices.Clone();
            }
            return new Indices(newIndices);
        }

        public IndexBuffer NewIndexBuffer()
        {
            return new IndexBuffer(indices);
        }

        public void Upload( IndexBuffer ibo )
        {
            ibo.Upload(indices);
        }
    }

    class ComplexGeometry<TVertex> : Geometry<TVertex>, IComplexGeometry where TVertex : struct, IVertex
    {
        public uint this[int i]
        {
            get
            {
                return indices[i];
            }
        }
        public Topology Topology { get; set; }


        public ComplexGeometry(Mesh<TVertex> mesh, uint[] indices)
            : base(mesh, indices)
        {
            this.Topology = new Topology(this);
        }
        public ComplexGeometry(Mesh<TVertex> mesh, Indices indices)
            : base(mesh, indices)
        {
            this.Topology = new Topology(this);
        }

        public ComplexGeometry(Mesh<TVertex> mesh)
            : base(mesh)
        {
            this.Topology = new Topology(this);
        }

        public IComplexGeometry Clone()
        {
            Mesh<TVertex> newMesh = new Mesh<TVertex>(mesh.vertices);
            Indices newIndices = indices.Clone();
            ComplexGeometry<TVertex> newGeom = new ComplexGeometry<TVertex>(newMesh, newIndices);
            return newGeom;
        }

        public IComplexGeometry ClonePosition<TVertex2>() where TVertex2 : struct, IVertex
        {
            TVertex2[] newVerts = new TVertex2[mesh.vertices.Length];
            int index = 0;
            foreach (var iter in mesh.vertices)
            {
                TVertex vertex = iter;
                Vector3 pos = MeshAttr.GetPosition(ref vertex);
                newVerts[index] = new TVertex2();
                MeshAttr.SetPosition(ref newVerts[index], ref pos);
                index++;
            }
            Mesh<TVertex2> newMesh = new Mesh<TVertex2>(newVerts);
            Indices newIndices = null;
            if (indices != null)
                newIndices = indices.Clone();
            ComplexGeometry<TVertex2> newGeom = new ComplexGeometry<TVertex2>(newMesh, newIndices);
            return newGeom;
        }

        // key is hash of vertex index of each vertex.
        // value is index of new midpoint vertex
        private Dictionary<Int64, uint> subDivideEdgeCache = new Dictionary<long, uint>();

        /// <summary>
        /// Subdivides mesh into smaller meshes; re-normalizes triangles.
        /// Keeps vertex count to a minimum.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int SubDivide(int level)
        {
            NeedsUpdate = true;
            // Assume initial mesh has minimised verts.
            int numVerts = 0;
            for (int j = 0; j < level; ++j)
            {
                // Retain all old vertices at the start of the list
                List<TVertex> newVerts = new List<TVertex>(mesh.vertices);
                List<uint> newIndices = new List<uint>();
                subDivideEdgeCache.Clear();
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
                indices = new Indices(newIndices.ToArray());
            }
            subDivideEdgeCache.Clear();
            return numVerts;
        }

        private uint GenerateVertex(ref List<TVertex> verts, int a, int b)
        {
            Int64 key1 = Topology.CreateEdgeKey(indices[a], indices[b]);
            uint middlePt;
            if (!subDivideEdgeCache.TryGetValue(key1, out middlePt))
            {
                Vector3 pos1 = MeshAttr.GetPosition(ref mesh.vertices[indices[a]]);
                Vector3 pos2 = MeshAttr.GetPosition(ref mesh.vertices[indices[b]]);
                Vector3 e1 = Vector3.Normalize(pos1 + (pos2 - pos1) * 0.5f);
                TVertex v1 = mesh.vertices[indices[a]];
                MeshAttr.SetPosition(ref v1, ref e1);
                middlePt = (uint)verts.Count;
                subDivideEdgeCache.Add(key1, middlePt);
                verts.Add(v1);
            }
            return middlePt;
        }

        public void TweakTriangles(float ratio, Random rand)
        {
            NeedsUpdate = true;

            // Assumptions: minimised mesh. Shared edges won't work without shared verts.
            Topology.GenerateEdges();

            // How many edges do we have? exchange some percentage of them...
            int numPerturbations = (int)((float)Topology.Edges.Count * ratio);
            int numTriangles = mesh.vertices.Length;
            List<Int64> keys = new List<Int64>(Topology.Edges.Keys);

            List<int> visitedTris = new List<int>();
            for (int i = 0; i < numPerturbations; ++i)
            {
                // Choose a random edge:
                int edgeIndex = rand.Next(Topology.Edges.Count);
                // Check out the tris around the edge.
                Int64 key = keys[edgeIndex];
                Topology.Edge edge = Topology.Edges[key];

                // TODO - add flag to triangle to avoid n2/2 lookup.
                bool r = false;
                foreach (var visited in visitedTris)
                {
                    if (edge.triangle1 == visited || edge.triangle2 == visited)
                    {
                        r = true;
                        break;
                    }
                }
                if (r) continue;

                visitedTris.Add(edge.triangle1);
                visitedTris.Add(edge.triangle2);

                if (IsObtuse(edge.triangle1) || IsObtuse(edge.triangle2))
                {
                    continue;
                }

                uint index1 = (uint)(key & 0xffffffff);
                uint index2 = (uint)((key >> 32) & 0xffffffff);
                uint index3;
                uint index4;
                uint a = index1;

                int cornerA = GetTriOffset(edge.triangle1, a);
                // get the next coord in the triangle (in ccw order)
                uint b = indices[edge.triangle1 * 3 + ((cornerA + 1) % 3)];
                uint c = indices[edge.triangle1 * 3 + ((cornerA + 2) % 3)];
                uint d = 0;
                if (b == index2)
                {
                    d = indices[edge.triangle2 * 3 + ((GetTriOffset(edge.triangle2, a) + 1) % 3)];
                    index3 = c;
                    index4 = d;
                }
                else
                {
                    d = indices[edge.triangle2 * 3 + ((GetTriOffset(edge.triangle2, a) + 2) % 3)];
                    index3 = b;
                    index4 = d;
                }

                if (Topology.trianglesPerVertex[index1] <= 5 || Topology.trianglesPerVertex[index2] <= 5 ||
                    Topology.trianglesPerVertex[index3] >= 7 || Topology.trianglesPerVertex[index4] >= 7)
                {
                    continue;
                }
                // Check edge lengths
                Vector3 pos1 = MeshAttr.GetPosition(ref mesh.vertices[index1]);
                Vector3 pos2 = MeshAttr.GetPosition(ref mesh.vertices[index2]);
                Vector3 pos3 = MeshAttr.GetPosition(ref mesh.vertices[index3]);
                Vector3 pos4 = MeshAttr.GetPosition(ref mesh.vertices[index4]);

                float oldLength = new Vector3(pos2 - pos1).Length;
                float newLength = new Vector3(pos4 - pos3).Length;
                if (oldLength / newLength >= 2 || oldLength / newLength <= 0.5f)
                {
                    continue;
                }

                Topology.trianglesPerVertex[index1]--;
                Topology.trianglesPerVertex[index2]--;
                Topology.trianglesPerVertex[index3]++;
                Topology.trianglesPerVertex[index4]++;

                // Need to keep tris in CCW order.
                if (b == index2)
                {
                    // order is a b c; c is the non-shared vertex.
                    // new tris are: ADC, DBC
                    // tri2 order is a d b
                    indices[edge.triangle1 * 3] = a;
                    indices[edge.triangle1 * 3 + 1] = d;
                    indices[edge.triangle1 * 3 + 2] = c;
                    indices[edge.triangle2 * 3] = d;
                    indices[edge.triangle2 * 3 + 1] = b;
                    indices[edge.triangle2 * 3 + 2] = c;
                }
                else
                {
                    // order is a b c; b is the non-shared value
                    // new tris are ACD, CBD
                    // tri2 order is a b d 
                    indices[edge.triangle1 * 3] = a;
                    indices[edge.triangle1 * 3 + 1] = b;
                    indices[edge.triangle1 * 3 + 2] = d;
                    indices[edge.triangle2 * 3] = c;
                    indices[edge.triangle2 * 3 + 1] = d;
                    indices[edge.triangle2 * 3 + 2] = b;
                }
            }

            Topology.Regenerate();
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
        public float RelaxTriangles1(float multiplier)
        {
            NeedsUpdate = true;

            double totalSurfaceArea = 4.0 * Math.PI;
            double idealFaceArea = totalSurfaceArea / (indices.Length / 3);
            double idealEdgeLength = Math.Sqrt(idealFaceArea * 4.0 / Math.Sqrt(3.0));
            double idealDistanceToCentroid = idealEdgeLength * Math.Sqrt(3) / 3.0 * 0.9;

            int numIndices = indices.Length;

            Vector3[] shiftPositions = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                shiftPositions[i] = new Vector3(Vector3.Zero);
            }

            for (int i = 0; i < numIndices; i += 3)
            {
                if (1 == 1)//TooThin(i))
                {
                    Vector3 centroid = Topology.CalculateCentroid(i / 3);
                    centroid.Normalize();

                    // Compare each corner to the centroid
                    // if too far off some ideal length ( the centroid of an equilateral triangle ),
                    // pull vertex closer to centroid, (without buggering up the other triangles using
                    // this point.)
                    Vector3[] oldPositions = new Vector3[3]
                    {
                        new Vector3( MeshAttr.GetPosition(ref mesh.vertices[indices[i]])),
                        new Vector3( MeshAttr.GetPosition(ref mesh.vertices[indices[i+1]])),
                        new Vector3( MeshAttr.GetPosition(ref mesh.vertices[indices[i+2]])),
                    };
                    Vector3[] newPositions = new Vector3[3];
                    for (int j = 0; j < 3; ++j)
                    {
                        TVertex v1 = mesh.vertices[indices[i + j]];
                        Vector3 v1Pos = MeshAttr.GetPosition(ref v1);
                        Vector3 e1 = centroid - v1Pos;

                        if (e1.Length > idealDistanceToCentroid * 1.1)
                        {
                            // Move vertex closer to centroid
                            float factor = 1.0f - (float)idealDistanceToCentroid / e1.Length;
                            float fraction = (multiplier * (1.0f - (float)idealDistanceToCentroid / e1.Length));
                            factor = factor * fraction;
                            v1Pos = Vector3.Normalize(v1Pos + e1 * factor);
                        }
                        else if (e1.Length < idealDistanceToCentroid * 0.9)
                        {
                            // Move vertex away from the centroid
                            float factor = 1 - e1.Length / (float)idealDistanceToCentroid;
                            float fraction = (multiplier * (e1.Length / (float)idealDistanceToCentroid));
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
                    if (phi < dot)
                    {
                        for (int j = 0; j < 3; ++j)
                        {
                            //SetPosition(ref mesh.vertices[indices[i + j]], ref newPositions[j]);
                            shiftPositions[indices[i + j]] = newPositions[j];
                        }
                    }

                    // If any surrounding triangles would be less parallel to the sphere than
                    // this is more parallel, don't do it.
                }
            }

            TVertex[] vertices = new TVertex[mesh.vertices.Length];
            float totalShift = 0;
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                Vector3 delta = shiftPositions[i] - MeshAttr.GetPosition(ref mesh.vertices[i]);
                MeshAttr.SetPosition(ref mesh.vertices[i], ref shiftPositions[i]);
                totalShift += delta.Length;
            }

            Topology.Regenerate();
            return totalShift;
        }

        public float RelaxTriangles(float multiplier)
        {
            NeedsUpdate = true;

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

            TVertex[] centroidVerts = new TVertex[numIndices / 3];
            TVertex centroidVertex = new TVertex();

            for (int i = 0; i < numIndices; i += 3)
            {
                Vector3 centroid = Topology.CalculateCentroid(i / 3);
                centroid.Normalize();
                MeshAttr.SetPosition(ref centroidVertex, ref centroid);

                Vector3[] oldPositions = new Vector3[3]
                {
                    new Vector3( MeshAttr.GetPosition(ref mesh.vertices[indices[i]])),
                    new Vector3( MeshAttr.GetPosition(ref mesh.vertices[indices[i+1]])),
                    new Vector3( MeshAttr.GetPosition(ref mesh.vertices[indices[i+2]])),
                };

                for (int j = 0; j < 3; ++j)
                {
                    Vector3 midLine = centroid - oldPositions[j];
                    float midLength = midLine.Length;
                    midLine *= (float)(multiplier * (midLength - idealDistanceToCentroid) / midLength);
                    shiftPositions[indices[i + j]] += midLine;
                }
            }

            var origin = Vector3.Zero;
            Plane p = new Plane(Vector3.UnitY, Vector3.Zero);
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                Vector3 vertexPosition = MeshAttr.GetPosition(ref mesh.vertices[i]);
                p.Redefine(vertexPosition, origin);
                shiftPositions[i] = vertexPosition + p.ProjectPoint(shiftPositions[i]);
                shiftPositions[i].Normalize();
            }

            // Stop poylgons rotating about their centroid.
            // Doesn't stop triangles flipping.
            float[] rotationSuppressions = new float[mesh.vertices.Length];
            rotationSuppressions.Initialize();

            Topology.GenerateEdges();
            float minEdgeLength = (float)idealEdgeLength * 0.8f;
            float maxEdgeLength = (float)idealEdgeLength * 1.2f;

            foreach (var iter in Topology.Edges)
            {
                Int64 key = iter.Key;
                int index1 = (int)(key & 0xffffffff);
                int index2 = (int)((key >> 32) & 0xffffffff);
                Vector3 oldPos1 = MeshAttr.GetPosition(ref mesh.vertices[index1]);
                Vector3 oldPos2 = MeshAttr.GetPosition(ref mesh.vertices[index2]);
                Vector3 newPos1 = shiftPositions[index1];
                Vector3 newPos2 = shiftPositions[index2];
                Vector3 oldEdge = oldPos2 - oldPos1;
                Vector3 newEdge = newPos2 - newPos1;
                if (newEdge.Length < minEdgeLength)
                {
                    // Move shift positions back out to ensure that the edge is never too small.
                    Vector3 midPt = newPos1 + 0.5f * newEdge;
                    newEdge.Normalize();
                    newEdge *= minEdgeLength * 0.5f;
                    shiftPositions[index1] = midPt - newEdge;
                    shiftPositions[index2] = midPt + newEdge;
                    newEdge = shiftPositions[index2] - shiftPositions[index1];
                }
                if (newEdge.Length > maxEdgeLength)
                {
                    // Move shift positions back in to ensure that the edge is never too large.
                    Vector3 midPt = newPos1 + 0.5f * newEdge;
                    newEdge.Normalize();
                    newEdge *= (maxEdgeLength * 0.5f);
                    shiftPositions[index1] = midPt - newEdge;
                    shiftPositions[index2] = midPt + newEdge;
                    newEdge = shiftPositions[index2] - shiftPositions[index1];
                }
                oldEdge.Normalize();
                newEdge.Normalize();
                float suppression = (1.0f - Vector3.Dot(oldEdge, newEdge)) * 0.5f;
                rotationSuppressions[index1] = Math.Max(suppression, rotationSuppressions[index1]);
                rotationSuppressions[index2] = Math.Max(suppression, rotationSuppressions[index2]);
            }

            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                Vector3 pos = MeshAttr.GetPosition(ref mesh.vertices[i]);
                Vector3 delta = pos;
                pos = Math2.Lerp(pos, shiftPositions[i], (float)(1.0f - Math.Sqrt(rotationSuppressions[i])));
                pos.Normalize();
                shiftPositions[i] = pos;
            }

            float totalShift = 0;
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                Vector3 delta = MeshAttr.GetPosition(ref mesh.vertices[i]);
                MeshAttr.SetPosition(ref mesh.vertices[i], ref shiftPositions[i]);
                delta -= shiftPositions[i];
                totalShift += delta.Length;
            }

            Topology.Regenerate();

            return totalShift;
        }

        public void ClearColor(Vector4 color)
        {
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                MeshAttr.SetColor(ref mesh.vertices[i], ref color);
            }
        }

        public Mesh<Vertex3D> GenerateCentroidPointMesh()
        {
            // Doesn't rely on topology
            int numTriangles = indices.Length / 3;
            Vertex3D[] centroidVerts = new Vertex3D[numTriangles];
            Vertex3D centroidVertex = new Vertex3D();
            Mesh<Vertex3D> centroidMesh = new Mesh<Vertex3D>(centroidVerts);
            int triangleIndex = 0;
            for (int i = 0; i < numTriangles; ++i)
            {
                Vector3 centroid = Topology.CalculateCentroid(i);
                IPositionVertex ipv = centroidVertex as IPositionVertex;
                if (ipv != null)
                {
                    ipv.SetPosition(centroid);
                }
                centroidMesh.vertices[triangleIndex++] = (Vertex3D)ipv;
            }
            return centroidMesh;
        }

        public Geometry<AltVertex> GenerateDualMesh<AltVertex>() where AltVertex : struct, IVertex
        {
            // For each edge, 
            //   get centroids of triangles each side
            //   make 2 new triangles from verts of edge + centroids

            List<AltVertex> newVerts = new List<AltVertex>(mesh.vertices.Length + Topology.Edges.Count * 2);
            // Initialize the first V verts
            for (int i = 0; i < mesh.vertices.Length; ++i)
            {
                newVerts.Add(new AltVertex());
            }

            List<uint> newIndices = new List<uint>();
            foreach (var iter in Topology.Edges)
            {
                Int64 key = iter.Key;
                int index1 = (int)(key & 0xffffffff);
                int index2 = (int)((key >> 32) & 0xffffffff);
                Topology.Edge e = iter.Value;

                Vector3 centroid1 = Topology.Centroids[e.triangle1].position;
                Vector3 centroid2 = Topology.Centroids[e.triangle2].position;

                // To find which order of vertices to use; 
                // if triangle1 contains index1 followed by index2, 
                // new tris are index1, c2, c1; index2, c1, c2
                // otherwise, the opposite order.

                bool edgeOrderIsAnticlockwise = false;
                for (int i = 0; i < 3; i++)
                {
                    if (indices[e.triangle1 * 3 + i] == index1)
                    {
                        if (indices[e.triangle1 * 3 + (i + 1) % 3] == index2)
                        {
                            edgeOrderIsAnticlockwise = true;
                        }
                        break;
                    }
                }
                if (edgeOrderIsAnticlockwise)
                {
                    AddTriangle2(ref newVerts, ref newIndices, index1, ref centroid2, ref centroid1);
                    AddTriangle2(ref newVerts, ref newIndices, index2, ref centroid1, ref centroid2);
                }
                else
                {
                    AddTriangle2(ref newVerts, ref newIndices, index1, ref centroid1, ref centroid2);
                    AddTriangle2(ref newVerts, ref newIndices, index2, ref centroid2, ref centroid1);
                }
            }

            var newMesh = new Mesh<AltVertex>(newVerts.ToArray());
            var newGeom = new Geometry<AltVertex>(newMesh, newIndices.ToArray());
            return newGeom;
        }

        public void AddTriangle<AltVertex>(ref List<AltVertex> newVerts, ref List<uint> newIndices, ref Vector3 v1, ref Vector3 v2, ref Vector3 v3, bool isAnticlockwise)
            where AltVertex : struct, IVertex
        {
            AltVertex[] triVerts = new AltVertex[3];
            triVerts.Initialize();
            MeshAttr.SetPosition(ref triVerts[0], ref v1);
            MeshAttr.SetPosition(ref triVerts[1], ref v2);
            MeshAttr.SetPosition(ref triVerts[2], ref v3);
            Vector3 v1N = v1;
            v1N.Normalize();
            for (int i = 0; i < 3; i++)
            {
                MeshAttr.SetNormal(ref triVerts[i], ref v1N);
            }

            Vector2 uv0 = new Vector2(0.5f, 1);
            Vector2 uv1 = new Vector2(0, 0);
            Vector2 uv2 = new Vector2(1, 0);

            MeshAttr.SetUV(ref triVerts[0], ref uv0);
            MeshAttr.SetUV(ref triVerts[1], ref uv1);
            MeshAttr.SetUV(ref triVerts[2], ref uv2);
            int index = newVerts.Count;
            for (int i = 0; i < 3; i++)
                newVerts.Add(triVerts[i]);

            if (isAnticlockwise)
            {
                newIndices.Add((uint)index);
                newIndices.Add((uint)index + 1);
                newIndices.Add((uint)index + 2);
            }
            else
            {
                newIndices.Add((uint)index);
                newIndices.Add((uint)index + 2);
                newIndices.Add((uint)index + 1);
            }
        }

        public void AddTriangle2<AltVertex>(ref List<AltVertex> newVerts, ref List<uint> newIndices, int v1index, ref Vector3 v2, ref Vector3 v3)
            where AltVertex : struct, IVertex
        {
            AltVertex[] triVerts = new AltVertex[3];
            triVerts.Initialize();

            Vector3 v1Pos = MeshAttr.GetPosition(ref mesh.vertices[v1index]);

            MeshAttr.SetPosition(ref triVerts[0], ref v1Pos);
            MeshAttr.SetPosition(ref triVerts[1], ref v2);
            MeshAttr.SetPosition(ref triVerts[2], ref v3);

            v1Pos.Normalize();
            for (int i = 0; i < 3; i++)
            {
                MeshAttr.SetNormal(ref triVerts[i], ref v1Pos);
            }

            Vector2 uv0 = new Vector2(0.5f, 1);
            Vector2 uv1 = new Vector2(0, 0);
            Vector2 uv2 = new Vector2(1, 0);

            MeshAttr.SetUV(ref triVerts[0], ref uv0);
            MeshAttr.SetUV(ref triVerts[1], ref uv1);
            MeshAttr.SetUV(ref triVerts[2], ref uv2);

            Vector4 c0 = MeshAttr.GetColor(ref mesh.vertices[v1index]);
            Vector4 c1 = MeshAttr.GetColor(ref mesh.vertices[v1index]);
            Vector4 c2 = MeshAttr.GetColor(ref mesh.vertices[v1index]);

            MeshAttr.SetColor(ref triVerts[0], ref c0);
            MeshAttr.SetColor(ref triVerts[1], ref c1);
            MeshAttr.SetColor(ref triVerts[2], ref c2);

            newVerts[v1index] = triVerts[0];
            int index = newVerts.Count;
            newVerts.Add(triVerts[1]);
            newVerts.Add(triVerts[2]);

            newIndices.Add((uint)v1index);
            newIndices.Add((uint)index++);
            newIndices.Add((uint)index++);
        }

        private int GetTriOffset(int triangle, uint corner)
        {
            int index = triangle * 3;
            if (indices[index++] == corner) return 0;
            if (indices[index++] == corner) return 1;
            if (indices[index] == corner) return 2;
            return -1;
        }

        private bool IsObtuse(int triangle)
        {
            int index = triangle * 3;

            TVertex v1 = mesh.vertices[indices[index++]];
            TVertex v2 = mesh.vertices[indices[index++]];
            TVertex v3 = mesh.vertices[indices[index]];
            Vector3 e1 = MeshAttr.GetPosition(ref v2) - MeshAttr.GetPosition(ref v1);
            Vector3 e2 = MeshAttr.GetPosition(ref v3) - MeshAttr.GetPosition(ref v1);
            Vector3 e3 = MeshAttr.GetPosition(ref v3) - MeshAttr.GetPosition(ref v2);
            float l1Sq = e1.LengthSquared;
            float l2Sq = e2.LengthSquared;
            float l3Sq = e3.LengthSquared;
            if (l1Sq + l2Sq < l3Sq || l1Sq + l3Sq < l2Sq || l2Sq + l3Sq < l1Sq)
            {
                return true;
            }
            return false;
        }

        private bool TooThin(int triangle)
        {
            int index = triangle * 3;
            TVertex v1 = mesh.vertices[indices[index++]];
            TVertex v2 = mesh.vertices[indices[index++]];
            TVertex v3 = mesh.vertices[indices[index]];
            Vector3 e1 = MeshAttr.GetPosition(ref v2) - MeshAttr.GetPosition(ref v1);
            Vector3 e2 = MeshAttr.GetPosition(ref v3) - MeshAttr.GetPosition(ref v1);
            Vector3 e3 = MeshAttr.GetPosition(ref v3) - MeshAttr.GetPosition(ref v2);

            float[] angles = new float[3];

            angles[0] = (float)Math.Acos((e2.LengthSquared + e3.LengthSquared - e1.LengthSquared) / (2.0f * e2.Length * e3.Length));
            angles[1] = (float)Math.Acos((e1.LengthSquared + e3.LengthSquared - e2.LengthSquared) / (2.0f * e1.Length * e3.Length));
            angles[2] = (float)Math.PI - (angles[0] + angles[1]);
            const float LOWER = (float)Math.PI * 35.0f / 180.0f;
            const float UPPER = (float)Math.PI * 80.0f / 180.0f;
            bool tooThin = false;
            for (int i = 0; i < 3; ++i)
            {
                if (angles[i] < LOWER || angles[i] > UPPER)
                {
                    tooThin = true;
                    break;
                }
            }
            return tooThin;
        }

        public void ConvertToVertexPerIndex()
        {
            List<TVertex> newVertices = new List<TVertex>();
            List<uint> newIndices = new List<uint>();
            uint newIndex = 0;
            for (int i = 0; i < indices.Length; ++i)
            {
                TVertex v = mesh.vertices[indices[i]];
                newVertices.Add(v);
                newIndices.Add(newIndex++);
            }
            mesh = new Mesh<TVertex>(newVertices.ToArray());
            indices = new Indices(newIndices.ToArray());
        }

        public void AddNormals()
        {
            // TODO Currently assuming that we are using triangles, and have
            // maximised the vertices.
            if (mesh.vertices[0].GetType().GetInterface("INormalVertex") != null)
            {
                for (int i = 0; i < indices.Length; i += 3)
                {
                    Vector3 pos1 = MeshAttr.GetPosition(ref mesh.vertices[i]);
                    Vector3 pos2 = MeshAttr.GetPosition(ref mesh.vertices[i + 1]);
                    Vector3 pos3 = MeshAttr.GetPosition(ref mesh.vertices[i + 2]);
                    Vector3 a = pos2 - pos1;
                    Vector3 b = pos3 - pos1;
                    Vector3 normal = Vector3.Cross(a, b);
                    normal.Normalize();

                    MeshAttr.SetNormal(ref mesh.vertices[i], ref normal);
                    MeshAttr.SetNormal(ref mesh.vertices[i + 1], ref normal);
                    MeshAttr.SetNormal(ref mesh.vertices[i + 2], ref normal);
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
                MeshAttr.SetUV(ref mesh.vertices[i], ref uv1);
                MeshAttr.SetUV(ref mesh.vertices[i + 1], ref uv2);
                MeshAttr.SetUV(ref mesh.vertices[i + 2], ref uv3);
            }
        }
    }
}
