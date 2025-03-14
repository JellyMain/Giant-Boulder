using UnityEngine;


namespace Utils
{
    public static class RuntimeMeshUtility
    {
        public static Vector3 GetMeshCenter(Transform transform, Collider collider)
        {
            Vector3 localCenter = Vector3.zero;

            switch (collider)
            {
                case MeshCollider meshCollider:
                {
                    localCenter = meshCollider.sharedMesh.bounds.center;
                    break;
                }
                case SphereCollider sphereCollider:
                {
                    localCenter = sphereCollider.center;
                    break;
                }
                case BoxCollider boxCollider:
                {
                    localCenter = boxCollider.center;
                    break;
                }
            }

            return transform.TransformPoint(localCenter);
        }
    }
}
