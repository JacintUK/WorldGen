using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.Generic;

namespace WorldGenerator
{
    using Borders = Dictionary<Int64, Border>;

    internal interface IGeometry
    {
        PrimitiveType PrimitiveType { get; set; }
        bool NeedsUpdate { set; get; }
        IMesh Mesh { get; }
        Neighbours Neighbours { get; }
        List<Centroid> Centroids { get; }

        IGeometry Clone();
        IGeometry ClonePosition<TVertex2>() where TVertex2 : struct, IVertex;

        IVertexBuffer CreateVertexBuffer();
        IndexBuffer CreateIndexBuffer();
        void Upload(IVertexBuffer vbo, IndexBuffer ibo);
        
        void ConvertToVertexPerIndex();
        void AddNormals();
        void AddUVs();
        void TweakTriangles(float ratio, Random rand);
        float RelaxTriangles( float multiplier );
        void ClearColor(Vector4 color);
        Mesh<Vertex3D> GenerateCentroidPointMesh();
        Geometry<AltVertex> GenerateDualMesh<AltVertex>() where AltVertex : struct, IVertex;
        Geometry<AltVertex> GenerateBorderGeometry<AltVertex>(Borders borders) where AltVertex : struct, IVertex;

        void GetCorners(int v1Index, int v2Index, out int c1Index, out int c2Index);
    }
}