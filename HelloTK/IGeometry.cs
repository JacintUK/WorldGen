using System;

namespace HelloTK
{
    internal interface IGeometry
    {
        IGeometry Clone();
        IVertexBuffer CreateVertexBuffer();
        IndexBuffer CreateIndexBuffer();
        void ConvertToVertexPerIndex();
        void AddNormals();
        void AddUVs();
        float RelaxTriangles( float multiplier );
        void Upload(IVertexBuffer vbo, IndexBuffer ibo);
        void ClearColor();
    }
}