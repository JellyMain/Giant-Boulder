using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Decorations
{
    public class DecorationStructureRoot : MonoBehaviour
    {
        [SerializeField] private List<Transform> childObjects;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private string ignoreSnapTag;


#if UNITY_EDITOR


        [Button]
        private void FindChildren()
        {
            childObjects = GetComponentsInChildren<Transform>().ToList();
        }
        
        
        [Button]
        private void SnapToGroundAllObjects()
        {
            foreach (Transform child in childObjects)
            {
                if (!child.CompareTag(ignoreSnapTag))
                {
                    Vector3 rayStart = child.position + Vector3.up * 1000;

                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
                    {
                        float currentYRotation = child.eulerAngles.y;

                        Quaternion groundAlignment = Quaternion.FromToRotation(Vector3.up, hit.normal);

                        child.rotation = groundAlignment * Quaternion.Euler(0, currentYRotation, 0);

                        child.position = hit.point;
                    }
                }
            }
        }


#endif
    }
}
