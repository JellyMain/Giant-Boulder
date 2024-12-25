using System;
using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for Clearing Terrain Trees along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Clear Colliders", menuName = "Procedural Worlds/GeNa/Extensions/Clear Colliders", order = 0)]
    public class GeNaClearCollidersExtension : GeNaSplineExtension
    {
        #region Variables
        [SerializeField] protected float m_width = 10f;
        [SerializeField] protected LayerMask m_layerMask = -1;
        [SerializeField] protected List<ColliderEntry> m_ignoredColliders = new List<ColliderEntry>();
        #endregion
        #region Properties
        public float Width
        {
            get => m_width;
            set => m_width = Mathf.Max(1f, value);
        }
        public LayerMask LayerMask
        {
            get => m_layerMask;
            set => m_layerMask = value;
        }
        public List<ColliderEntry> IgnoredColliders => m_ignoredColliders;
        #endregion
        protected override void OnSelect()
        {
            m_isSelected = true;
        }
        protected override void OnDeselect()
        {
            m_isSelected = false;
        }
        public override void Execute()
        {
        }
        public void Clear()
        {
            UndoRecord undoRecord = CreateInstance<UndoRecord>();
            List<Collider> colliders = DetectColliders();
            foreach (var collider in colliders)
            {
                GameObjectEntity gameObjectEntity = CreateInstance<GameObjectEntity>();
                gameObjectEntity.m_gameObject = collider.gameObject;
                gameObjectEntity.m_destroy = true;
                undoRecord.Record(gameObjectEntity);
            }
            GeNaUndoRedo.RecordUndo(undoRecord);
        }
        protected override void OnDrawGizmos()
        {
            if (!m_isSelected)
                return;
            List<Collider> result = new List<Collider>();
            float distance = 0.0f;
            float spread = m_width * 2.0f;
            float radius = m_width * 0.5f;
            while (distance < Spline.Length)
            {
                float nextDistance = Mathf.Min(distance + m_width, Spline.Length);
                GeNaSample sample = Spline.GetSampleAtDistance(distance);
                if (sample == null)
                    return;
                GeNaSample nextSample = Spline.GetSampleAtDistance(nextDistance);
                if (nextSample == null)
                    return;
                if (nextSample != null && sample != null)
                {
                    Vector3 origin = sample.Location;
                    Vector3 direction = (nextSample.Location - sample.Location).normalized;
                    Ray ray = new Ray(origin, direction);
                    float maxDistance = Mathf.Abs(nextDistance - distance);
                    RaycastHit[] hits = Physics.SphereCastAll(ray, radius, maxDistance, m_layerMask);
                    foreach (RaycastHit hit in hits)
                    {
                        Collider collider = hit.collider;
                        if (result.Contains(collider))
                            continue;
                        if (m_ignoredColliders.Exists(item => item.IsActive && item.Collider == collider))
                            continue;
                        // Skip Terrains
                        Terrain terrain = collider.GetComponent<Terrain>();
                        if (terrain != null)
                            continue;
                        result.Add(collider);
                        Transform transform = collider.transform;
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(transform.position, 1.0f);
                    }
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(origin, radius);
                }
                distance += m_width;
            }
        }
        protected List<Collider> DetectColliders()
        {
            List<Collider> result = new List<Collider>();
            float distance = 0.0f;
            float spread = m_width * 2.0f;
            float radius = m_width * 0.5f;
            while (distance < Spline.Length)
            {
                GeNaSample sample = Spline.GetSampleAtDistance(distance);
                if (sample == null)
                    return result;
                float nextDistance = Mathf.Min(distance + m_width, Spline.Length);
                GeNaSample nextSample = Spline.GetSampleAtDistance(nextDistance);
                if (nextSample == null)
                    return result;
                Vector3 origin = sample.Location;
                Vector3 direction = (nextSample.Location - sample.Location).normalized;
                Ray ray = new Ray(origin, direction);
                float maxDistance = Mathf.Abs(nextDistance - distance);
                RaycastHit[] hits = Physics.SphereCastAll(ray, radius, maxDistance, m_layerMask);
                foreach (RaycastHit hit in hits)
                {
                    Collider collider = hit.collider;
                    if (result.Contains(collider))
                        continue;
                    if (m_ignoredColliders.Exists(item => item.IsActive && item.Collider == collider))
                        continue;
                    // Skip Terrains
                    Terrain terrain = collider.GetComponent<Terrain>();
                    if (terrain != null)
                        continue;
                    result.Add(collider);
                }
                distance += m_width;
            }
            return result;
        }
    }
}