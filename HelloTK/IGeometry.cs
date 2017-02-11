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
        void TweakTriangles(float ratio, ref Random rand);
        float RelaxTriangles( float multiplier );
        void Upload(IVertexBuffer vbo, IndexBuffer ibo);
        void ClearColor();
    }
}