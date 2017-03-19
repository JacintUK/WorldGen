using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;

namespace WorldGenerator
{
    class Math2
    {
        public static Vector3 ProjectOnVector( Vector3 lhs, Vector3 rhs )
        {
            Vector3 rhsN = rhs;
            rhsN.Normalize();
            float length = Vector3.Dot(lhs, rhsN);
            return rhsN * length;
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return (1.0f - t) * a + t * b;
        }

        public static Vector3 HSV2RGB(Vector3 hsv)
        {
            Vector4 K = new Vector4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
            Vector3 p = Abs(Fract(Fill(hsv.X) + K.Xyz) * 6.0f - Fill(K.W));
            return hsv.Z * Mix(Fill(K.X), Vector3.Clamp(p - Fill(K.X), Fill(0.0f), Fill(1.0f)), hsv.Y);
        }
        public static Vector3 Mix(Vector3 a, Vector3 b, float t)
        {
            Vector3 c = a + t * (b - a);
            return c;
        }
        public static float Fract(float a)
        {
            return (float)(a - Math.Floor(a));
        }
        public static Vector3 Fract(Vector3 a)
        {
            Vector3 b;
            b.X = Fract(a.X);
            b.Y = Fract(a.Y);
            b.Z = Fract(a.Z);
            return b;
        }
        public static Vector3 Abs(Vector3 a)
        {
            Vector3 b;
            b.X = Math.Abs(a.X);
            b.Y = Math.Abs(a.Y);
            b.Z = Math.Abs(a.Z);
            return b;
        }
        public static Vector3 Fill(float a)
        {
            return new Vector3(a, a, a);
        }
        public static float Clamp( float value, float minimum, float maximum )
        {
            return Math.Min(Math.Max(value, minimum), maximum);
        }
        public static Quaternion FromEuler(float pitch, float yaw, float roll)
        {
            yaw *= 0.5f * (float)Math.PI / 180.0f;
            pitch *= 0.5f * (float)Math.PI / 180.0f;
            roll *= 0.5f * (float)Math.PI / 180.0f;

            float c1 = (float)Math.Cos(yaw);
            float c2 = (float)Math.Cos(pitch);
            float c3 = (float)Math.Cos(roll);
            float s1 = (float)Math.Sin(yaw);
            float s2 = (float)Math.Sin(pitch);
            float s3 = (float)Math.Sin(roll);

            float w = c1 * c2 * c3 - s1 * s2 * s3;
            Vector3 xyz = new Vector3();
            xyz.X = s1 * s2 * c3 + c1 * c2 * s3;
            xyz.Y = s1 * c2 * c3 + c1 * s2 * s3;
            xyz.Z = c1 * s2 * c3 - s1 * c2 * s3;
            return new Quaternion(xyz, w);
        }

        public static Vector3 ToVec3(Color value)
        {
            return new Vector3(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
        }

        // Given a triangle A, G1, G2
        // Projects a line parallel to the base (G1-G2) of the triangle a distance h upwards,
        // and returns a point A' which is the intersection of this line and edge A-G1.
        public static Vector3 BaseProjection(Vector3 A, Vector3 G1, Vector3 G2, float h)
        {
            // Want 1 line parallel to G2-G1 that are x pixels away and lie on tri A,G1,G2
            // a' = h / cos(90-theta)
            // a' lies on A-G1
            // cos(theta) = (G1-A).(G1-G2)
            // cos(90-theta)= cos(90)cos(theta)+sin(90)sin(theta) = sin(theta) = sqrt(1-cos^2(theta))
            // a' = h / sqrt(1-cos^2(theta))
            Vector3 AG1 = (G1 - A);
            Vector3 AG1N = AG1;
            AG1N.Normalize();
            Vector3 b = (G1 - G2);
            b.Normalize();
            float cosTheta = Vector3.Dot(AG1N, b);
            float aPrime = h / (float)Math.Sqrt(1.0f - cosTheta * cosTheta);
            Vector3 APrime = A + AG1 * (1.0f - aPrime);
            return APrime;
        }

        public static Vector3 GetCentroid( Vector3 v1, Vector3 v2, Vector3 v3 )
        {
            Vector3 e1 = v2 - v1;
            Vector3 midPt = v1 + e1 * 0.5f;
            Vector3 centroid = midPt + (v3 - midPt) / 3.0f;
            return centroid;
        }

    }
}

