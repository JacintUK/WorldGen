using OpenTK;

namespace HelloTK
{
    internal class Plane
    {
        Vector3 normal;
        Vector3 origin;

        public Plane(Vector3 normal, Vector3 origin)
        {
            this.normal = normal;
            this.normal.Normalize();
            this.origin = origin;
        }
        public void Redefine(Vector3 normal, Vector3 origin)
        {
            this.normal = normal;
            this.normal.Normalize();
            this.origin = origin;
        }
        public Vector3 ProjectPoint(Vector3 point)
        {
            Vector3 toOrig = point - origin;
            Vector3 projectedPoint = point - Vector3.Dot(toOrig, normal) * normal;
            return projectedPoint;
        }
    }
}