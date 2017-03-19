using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace WorldGenerator
{
    // Topology of a geometry
    class Topology
    {
        // Topology
        public struct Edge { public int triangle1; public int triangle2; }

        private Dictionary<Int64, Edge> edgeCache;
        private Neighbours neighbours;
        private List<Centroid> centroids; // indexed by triangle.
        public int[] trianglesPerVertex;

        public Neighbours Neighbours { get { GenerateTopology(); return neighbours; } } // Face neighbours
        public List<Centroid> Centroids { get { GenerateTopology(); return centroids; } }
        public Dictionary<Int64, Edge> Edges { get { GenerateTopology();  return edgeCache; } }

        private bool regenerateTopology = true;
        private readonly IGeometry geometry;

        public Topology(IGeometry geometry) 
        {
            this.geometry = geometry;
            edgeCache = new Dictionary<long, Edge>();
            centroids = new List<Centroid>();
            neighbours = null;
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
            trianglesPerVertex = new int[geometry.Mesh.Length];
            int numIndices = geometry.NumIndices;
            for (int i = 0; i < numIndices; i += 3)
            {
                RegisterEdge(i, i + 1);
                RegisterEdge(i + 1, i + 2);
                RegisterEdge(i, i + 2);
                trianglesPerVertex[geometry.Indices[i]]++;
                trianglesPerVertex[geometry.Indices[i + 1]]++;
                trianglesPerVertex[geometry.Indices[i + 2]]++;
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

        public static Int64 CreateEdgeKey(uint a, uint b)
        {
            Int64 min = a < b ? a : b;
            Int64 max = a >= b ? a : b;
            Int64 key = min << 32 | max;
            return key;
        }

        public void GenerateNeighbours()
        {
            neighbours = new Neighbours(geometry.NumVertices);

            for (int i = 0; i < geometry.NumIndices; i += 3)
            {
                int v0 = (int)geometry.Indices[i];
                int v1 = (int)geometry.Indices[i + 1];
                int v2 = (int)geometry.Indices[i + 2];
                neighbours.AddTriangle(v0, v1, v2);
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
            Vector3 v2 = geometry.Mesh.GetPosition((int)geometry.Indices[triangle * 3+1]);
            Vector3 v3 = geometry.Mesh.GetPosition((int)geometry.Indices[triangle * 3+2]);
            return Math2.GetCentroid(v1, v2, v3);
        }

        /// <summary>
        /// Key is generated from the vertex index of adjacent tiles
        /// </summary>
        /// <param name="a">vertex index of first tile</param>
        /// <param name="b">vertex index of second tile</param>
        /// <returns></returns>
        public static Int64 CreateBorderKey(uint a, uint b)
        {
            Int64 min = a < b ? a : b;
            Int64 max = a >= b ? a : b;
            Int64 key = min << 32 | max;
            return key;
        }
    }
}
