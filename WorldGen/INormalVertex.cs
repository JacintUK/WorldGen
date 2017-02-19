using OpenTK;

namespace WorldGenerator
{
    internal interface INormalVertex
    {
        void SetNormal(Vector3 normal);
        Vector3 GetNormal();
    }
}