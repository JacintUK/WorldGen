using OpenTK;

namespace WorldGenerator
{
    internal interface ITangentVertex
    {
        Vector3 GetTangent();
        void SetTangent(Vector3 tangent);
    }
}