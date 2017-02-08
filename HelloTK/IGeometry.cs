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
        void RelaxTriangles(ref Random rand, int level);
        void Upload(IVertexBuffer vbo, IndexBuffer ibo);
    }
}