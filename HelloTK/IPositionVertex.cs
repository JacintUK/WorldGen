using OpenTK;

namespace HelloTK
{
    internal interface IPositionVertex
    {
        Vector3 GetPosition();
        void SetPosition(Vector3 position);
    }
}