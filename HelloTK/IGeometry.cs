using System;
using OpenTK.Graphics.OpenGL;

namespace HelloTK
{
    internal interface IGeometry
    {
        PrimitiveType PrimitiveType { get; set; }
        bool NeedsUpdate { set; get; }

        IGeometry Clone();
        IVertexBuffer CreateVertexBuffer();
        IndexBuffer CreateIndexBuffer();
        void Upload(IVertexBuffer vbo, IndexBuffer ibo);
        
        void ConvertToVertexPerIndex();
        void AddNormals();
        void AddUVs();
        void TweakTriangles(float ratio, ref Random rand);
        float RelaxTriangles( float multiplier );
        void ClearColor();
        Mesh<Vertex3D> GenerateCentroidMesh();
    }
}