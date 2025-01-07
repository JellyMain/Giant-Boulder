using UnityEngine;

namespace Utils
{
    public static class GeometryUtility
    {
        private static Vector3 GetCircumcenter(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            float dA = pointA.x * pointA.x + pointA.z * pointA.z;
            float dB = pointB.x * pointB.x + pointB.z * pointB.z;
            float dC = pointC.x * pointC.x + pointC.z * pointC.z;

            float determinant = 2 * (pointA.x * (pointB.z - pointC.z) +
                                     pointB.x * (pointC.z - pointA.z) +
                                     pointC.x * (pointA.z - pointB.z));

            if (Mathf.Approximately(determinant, 0))
                return Vector3.positiveInfinity;

            float centerX = (dA * (pointB.z - pointC.z) +
                             dB * (pointC.z - pointA.z) +
                             dC * (pointA.z - pointB.z)) / determinant;

            float centerZ = (dA * (pointC.x - pointB.x) +
                             dB * (pointA.x - pointC.x) +
                             dC * (pointB.x - pointA.x)) / determinant;

            return new Vector3(centerX, 0, centerZ);
        }

        public static bool IsInCircumcircle(Vector3 point, Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 circumcenter = GetCircumcenter(pointA, pointB, pointC);
            if (circumcenter == Vector3.positiveInfinity) return false;

            float radius = Vector3.Distance(circumcenter, pointA);
            return Vector3.Distance(circumcenter, point) <= radius;
        }

        public static bool AreTrianglePointsCollinear(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            float area = Mathf.Abs(pointA.x * (pointB.z - pointC.z) +
                                   pointB.x * (pointC.z - pointA.z) +
                                   pointC.x * (pointA.z - pointB.z)) / 2;

            return Mathf.Approximately(area, 0);
        }
    }
}