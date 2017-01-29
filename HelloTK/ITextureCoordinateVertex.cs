using OpenTK;

namespace HelloTK
{
    internal interface ITextureCoordinateVertex
    {
        Vector2 GetTextureCoordinates();
        void SetTextureCoordinates(Vector2 position);
    }
}