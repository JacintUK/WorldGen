using OpenTK;

namespace WorldGenerator
{
    internal interface ITextureCoordinateVertex
    {
        Vector2 GetTextureCoordinates();
        void SetTextureCoordinates(Vector2 position);
    }
}