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
using OpenTK.Mathematics;

namespace WorldGen
{
    public struct Edge { public int triangle1; public int triangle2; }

    internal interface ITopology
    {
        /// <summary>
        /// Edges stores the edges of triangles in the primary mesh. 
        /// </summary>
        Dictionary<Int64, Edge> Edges { get; }

        /// <summary>
        /// Centroids is an array of centroids of the triangles in the primary mesh.
        /// These are the vertices in the dual mesh.
        /// We also refer to them as "corners" of the tiles in the primary mesh.
        /// </summary>
        List<Centroid> Centroids { get; }

        /// <summary>
        /// VertexNeighbours stores adjacent vertices in the primary mesh
        /// </summary>
        VertexNeighbours VertexNeighbours { get; }
        int[] TrianglesPerVertex { get; set; }

        void Regenerate();
        void GenerateEdges();
        Vector3 CalculateCentroid(int triangle);

        /// <summary>
        /// Get the centroid indices of the shared edge between the given faces.
        /// Note, the first corner index can be in either face.
        /// The centroid index is equivalent to a vertex index in the dual mesh
        /// </summary>
        /// <param name="v1Index">VertexIndex in the primary mesh of the first face</param>
        /// <param name="v2Index">VertexIndex in the primary mesh of the second face</param>
        /// <param name="c1Index">Index of one corner triangle</param>
        /// <param name="c2Index">Index of the other corner triangle</param>
        void GetCorners(int v1Index, int v2Index, out int c1Index, out int c2Index);
    }

    // Topology of a (triangular face) geometry
    // A geometry comprises a primary Mesh of vertices connected by edges forming triangles.
    // Each triangle has a centroid. The centroid is used to compute the dual mesh.
    // (Both vertices and centroids are used for generating a rendering geometry)
    // 
    // Triangle edges are stored in the edge cache. EdgeCache keys are computed from the vertex indexes.
    // Each vertex in the primary mesh has N neighbouring vertices which comprise all triangles that
    // the vertex is in.
    //
    // In usage, we may use both primary mesh and dual mesh terms - vertices and faces/tiles - to mean
    // the same thing. Very confusing. Must fix!
    // Similarly, we use the dual mesh term "Corner" to mean the corners between faces; i.e. the centroids,
    // so as not to confuse it with vertex, which is the face index.
    partial class Geometry
    {
        public class Topology : ITopology
        {
            private Dictionary<Int64, Edge> edgeCache;
            private VertexNeighbours vertexNeighbours;
            private List<Centroid> centroids; // indexed by triangle.

            /// <summary>
            /// Edges stores the edges of triangles in the primary mesh. 
            /// </summary>
            public Dictionary<Int64, Edge> Edges { get { GenerateTopology(); return edgeCache; } }

            /// <summary>
            /// Centroids is an array of centroids of the triangles in the primary mesh.
            /// These are the vertices in the dual mesh.
            /// We also refer to them as "corners" of the tiles in the primary mesh.
            /// </summary>
            public List<Centroid> Centroids { get { GenerateTopology(); return centroids; } }

            /// <summary>
            /// VertexNeighbours stores adjacent vertices in the primary mesh
            /// </summary>
            public VertexNeighbours VertexNeighbours { get { GenerateTopology(); return vertexNeighbours; } } // Face neighbours


            public int[] TrianglesPerVertex { get; set; }

            private bool regenerateTopology = true;
            private readonly IGeometry geometry;

            public Topology(IGeometry geometry)
            {
                this.geometry = geometry; // Warning: circular reference will keep geom/topo alive.
                edgeCache = new Dictionary<long, Edge>();
                centroids = new List<Centroid>();
                vertexNeighbours = null;
            }

            public void Regenerate()
            {
                regenerateTopology = true;
            }

            public void GenerateTopology()
            {
                if (regenerateTopology)
                {
                    GenerateEdges();
                    GenerateCentroids();

                    foreach (var iter in edgeCache)
                    {
                        Int64 key = iter.Key;
                        int index1 = (int)(key & 0xffffffff);
                        int index2 = (int)((key >> 32) & 0xffffffff);
                        Edge e = iter.Value;

                        centroids[e.triangle1].AddNeighbour(e.triangle2);
                        centroids[e.triangle2].AddNeighbour(e.triangle1);
                    }

                    GenerateNeighbours();

                    regenerateTopology = false;
                }
            }

            public void GenerateEdges()
            {
                edgeCache = new Dictionary<long, Edge>();
                TrianglesPerVertex = new int[geometry.Mesh.Length];
                int numIndices = geometry.NumIndices;
                for (int i = 0; i < numIndices; i += 3)
                {
                    RegisterEdge(i, i + 1);
                    RegisterEdge(i + 1, i + 2);
                    RegisterEdge(i, i + 2);
                    TrianglesPerVertex[geometry.Indices[i]]++;
                    TrianglesPerVertex[geometry.Indices[i + 1]]++;
                    TrianglesPerVertex[geometry.Indices[i + 2]]++;
                }
            }

            private void RegisterEdge(int a, int b)
            {
                Int64 key = CreateEdgeKey(geometry.Indices[a], geometry.Indices[b]);
                Edge edge;
                if (!edgeCache.TryGetValue(key, out edge))
                {
                    edge.triangle1 = a / 3;
                    edgeCache.Add(key, edge);
                }
                else
                {
                    edge.triangle2 = a / 3;
                    edgeCache[key] = edge;
                }
            }

            public void GenerateNeighbours()
            {
                vertexNeighbours = new VertexNeighbours(geometry.NumVertices);

                for (int i = 0; i < geometry.NumIndices; i += 3)
                {
                    int v0 = (int)geometry.Indices[i];
                    int v1 = (int)geometry.Indices[i + 1];
                    int v2 = (int)geometry.Indices[i + 2];
                    vertexNeighbours.AddTriangle(v0, v1, v2);
                }
            }

            public void GenerateCentroids()
            {
                int numTriangles = geometry.NumIndices / 3;

                centroids = new List<Centroid>(numTriangles);
                for (int i = 0; i < numTriangles; ++i)
                {
                    Vector3 centroidPos = CalculateCentroid(i);
                    centroidPos.Normalize();

                    Centroid centroid = new Centroid(centroidPos);
                    centroid.AddFace((int)geometry.Indices[i * 3]);
                    centroid.AddFace((int)geometry.Indices[i * 3 + 1]);
                    centroid.AddFace((int)geometry.Indices[i * 3 + 2]);
                    centroids.Add(centroid); // Index into list is triangle index
                }
            }

            public void GetCorners(int v1Index, int v2Index, out int c1Index, out int c2Index)
            {
                Int64 edgeKey = CreateEdgeKey((uint)v1Index, (uint)v2Index);
                Edge edge;

                if (edgeCache.TryGetValue(edgeKey, out edge))
                {
                    c1Index = edge.triangle1;
                    c2Index = edge.triangle2;
                }
                else
                {
                    c1Index = -1;
                    c2Index = -1;
                }
            }

            public Vector3 CalculateCentroid(int triangle)
            {
                Vector3 v1 = geometry.Mesh.GetPosition((int)geometry.Indices[triangle * 3]);
                Vector3 v2 = geometry.Mesh.GetPosition((int)geometry.Indices[triangle * 3 + 1]);
                Vector3 v3 = geometry.Mesh.GetPosition((int)geometry.Indices[triangle * 3 + 2]);
                return Math2.GetCentroid(v1, v2, v3);
            }

            /// <summary>
            /// Key is generated from the vertex index in primary mesh of adjacent tiles
            /// </summary>
            /// <param name="a">vertex index of first tile</param>
            /// <param name="b">vertex index of second tile</param>
            /// <returns></returns>
            public static Int64 CreateEdgeKey(uint a, uint b)
            {
                Int64 min = a < b ? a : b;
                Int64 max = a >= b ? a : b;
                Int64 key = min << 32 | max;
                return key;
            }
        }
    }
}
