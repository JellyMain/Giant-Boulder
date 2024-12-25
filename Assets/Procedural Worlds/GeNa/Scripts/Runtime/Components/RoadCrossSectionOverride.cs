using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeNa.Core
{
    /// <summary>
    /// Place a derrived instance of this class on a GeNa Spline
    /// object to override the cross section used to create Road Meshes.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class RoadCrossSectionOverride : MonoBehaviour
    {
        private void OnEnable()
        {
            NotifySpline();
        }
        private void OnDisable()
        {
            NotifySpline();
        }
        private void NotifySpline()
        {
            GeNaSpline spline = GetComponent<GeNaSpline>();
            if (spline != null && spline.enabled && spline.gameObject.activeInHierarchy)
            {
                GeNaRoadExtension road = spline.GetExtension<GeNaRoadExtension>();
                if (road != null && road.IsActive)
                {
                    road.PreExecute();
                    road.Execute();
                }
            }
        }
        /// <summary>
        /// Return a RoadCrossSection instance for overriding
        /// the cross section used by the GeNa Road Extension
        /// to generate the road meshe(s).
        /// Note that the number of points and normals must be even, and the same size.
        /// </summary>
        /// <returns></returns>
        public abstract RoadCrossSection GetRoadCrossSection();
    }
}