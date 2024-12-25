using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    public class GeNaBuilderDecorator : GeNaDecorator
    {
        public float CullingRange = 150f;
        public int RenderLimit = 10;
        public float LabelOffset = 0.5f;
        public bool RenderLabels = false;

        public List<Transform> Spawners
        {
            get { return m_spawners; }
            set
            {
                m_spawners = value;
            }
        }
        [SerializeField] private List<Transform> m_spawners = new List<Transform>();
        public Vector2 RandomizationValue = new Vector2(1f, 5f);

        private Transform m_selectedTransform;
        private Bounds m_selectedTransformBounds;
        private Transform m_rootTransform;
        private Bounds m_rootTransformBounds;

        /// <summary>
        /// Randomizes the spawners y transform rotation to randomize the spawns a bit.
        /// The value ranges within the Randomization Value and then randomizes the negative or positive input.
        /// </summary>
        public void RandomizeYRotation()
        {
            if (Spawners.Count > 0)
            {
                foreach (Transform spawner in Spawners)
                {
                    if (spawner != null)
                    {
                        Vector3 rotation = spawner.eulerAngles;
                        float yRandom = UnityEngine.Random.Range(RandomizationValue.x, RandomizationValue.y);
                        bool negativeValue = UnityEngine.Random.Range(0f, 100f) < 65f;

                        if (negativeValue)
                        {
                            rotation.y -= yRandom;
                        }
                        else
                        {
                            rotation.y += yRandom;
                        }

                        spawner.eulerAngles = rotation;
                    }
                }
            }
        }
        /// <summary>
        /// Determine if root transform is selected
        /// </summary>
        /// <returns>True if the root transform is selected</returns>
        public bool IsRootTransformSelected()
        {
            if (m_rootTransform == null)
                SetRootTransform();
            if (m_selectedTransform == null)
                SetSelectedTransform(transform);
            return (m_rootTransform.GetInstanceID() == m_selectedTransform.GetInstanceID());
        }
        /// <summary>
        /// Get current root transform
        /// </summary>
        /// <returns>Root transform</returns>
        public Transform GetRootTransform()
        {
            if (m_rootTransform == null)
                SetRootTransform();
            if (m_selectedTransform == null)
                SetSelectedTransform(transform);
            return m_rootTransform;
        }
        /// <summary>
        /// Set  the root tranform and update its bounds
        /// </summary>
        public void SetRootTransform()
        {
            m_rootTransform = transform;
            UpdateRootBounds();
        }
        /// <summary>
        /// Bounds of overall object and its children
        /// </summary>
        /// <returns>Bounds of object and its children</returns>
        public Bounds GetRootBounds()
        {
            if (m_rootTransform == null)
                SetRootTransform();
            if (GeNaUtility.ApproximatelyEqual(m_rootTransformBounds.size, Vector3.zero))
                UpdateRootBounds();
            return m_rootTransformBounds;
        }
        /// <summary>
        /// Update the root bounds when selection changes
        /// </summary>
        public void UpdateRootBounds()
        {
            m_rootTransformBounds = GeNaUtility.GetObjectBounds(gameObject, true, true, false, false);
        }
        /// <summary>
        /// Select the given transform
        /// </summary>
        /// <param name="newTransform">New selected transform</param>
        public void SetSelectedTransform(Transform newTransform)
        {
            m_selectedTransform = newTransform;
            UpdateSelectedBounds();
        }
        /// <summary>
        /// Get currently selected transform - any issues then returns the root transform
        /// </summary>
        /// <returns>Child or root</returns>
        public Transform GetSelectedTransform()
        {
            if (m_rootTransform == null)
                SetRootTransform();
            if (m_selectedTransform == null)
                SetSelectedTransform(transform);
            return m_selectedTransform;
        }
        /// <summary>
        /// Get currently selected objects bounds
        /// </summary>
        /// <returns>Currently selected objects bounds</returns>
        public Bounds GetSelectedBounds()
        {
            if (m_selectedTransform == null)
                SetSelectedTransform(transform);
            // if (GeNaUtility.ApproximatelyEqual(m_selectedTransformBounds.size, Vector3.zero))
            // {
            UpdateSelectedBounds();
            // }
            return m_selectedTransformBounds;
        }
        /// <summary>
        /// Update the selected bounds when selection changes
        /// </summary>
        public void UpdateSelectedBounds()
        {
            if (IsRootTransformSelected())
                m_selectedTransformBounds = GeNaUtility.GetObjectBounds(gameObject, true, true, false, false);
            else
                m_selectedTransformBounds = GeNaUtility.GetObjectBounds(m_selectedTransform.gameObject, true, true, false, false);
        }
        public void OnDrawGizmos()
        {
            // If not set then set up the relevant transforms and get bounds
            if (m_rootTransform == null || m_selectedTransform == null)
            {
                m_rootTransform = transform;
                if (m_selectedTransform == null)
                    m_selectedTransform = transform;
                UpdateSelectedBounds();
            }
            // Draw a north facing Gizmo on root transform
            Color oldColor = Gizmos.color;
            // Draw selected transform
            if (m_rootTransform.GetInstanceID() == m_selectedTransform.GetInstanceID())
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_selectedTransform.position, 0.15f);
            Bounds b = GetSelectedBounds();
            Gizmos.DrawWireCube(b.center, b.size);
            Gizmos.color = oldColor;
        }
    }
}