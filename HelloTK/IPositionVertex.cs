using OpenTK;

namespace WorldGenerator
{
    internal interface IPositionVertex
    {
        Vector3 GetPosition();
        void SetPosition(Vector3 position);
    }
}