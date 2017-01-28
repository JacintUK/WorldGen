namespace HelloTK
{
    internal interface IGeometry
    {
        IVertexBuffer CreateVertexBuffer();
        IndexBuffer CreateIndexBuffer();
    }
}