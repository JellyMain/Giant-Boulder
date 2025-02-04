//Copyright(c)2020 Procedural Worlds Pty Limited 
using System;
using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for creating Roads along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Roads", menuName = "Procedural Worlds/GeNa/Extensions/Roads", order = 3)]
    public class GeNaRoadExtension : GeNaSplineExtension
    {
        [SerializeField] protected GeNaRoadProfile m_roadProfile;
        [SerializeField] protected float m_width = 6.2f;
        [SerializeField] protected bool m_separateYScale = false;
        [SerializeField] protected float m_height = 1.0f; // <= 1.0 uses width
        [SerializeField] protected float m_intersectionSize = 1.0f;
        [SerializeField] protected float m_groundAttractDistance = 0.0f;
        [SerializeField] protected bool m_conformToGround = false;
        [SerializeField] protected bool m_addRoadCollider = true;
        [SerializeField] protected bool m_shadowsCast = false;
        [SerializeField] protected bool m_shadowsReceive = true;
        [SerializeField] protected bool m_raycastTerrainOnly = true;
        [SerializeField] protected bool m_useSlopedCrossection = false;
        [SerializeField] protected string m_tag = "Untagged";
        [SerializeField] protected int m_layer = -1;
        [SerializeField] protected bool m_splitAtTerrains = true;
        [SerializeField] protected bool m_postProcess = false;
        [SerializeField] protected List<GameObject> m_bakedMeshes = new List<GameObject>();
        [SerializeField] protected RoadCrossSectionOverride m_crossSectionOverrideComponent = null;
        [NonSerialized] protected GameObject m_roadMeshParent = null;
        public GeNaRoadProfile RoadProfile
        {
            get => m_roadProfile;
            set => m_roadProfile = value;
        }
        public RoadCrossSectionOverride CrossSectionOverride
        {
            get => m_crossSectionOverrideComponent;
            set => m_crossSectionOverrideComponent = value;
        }
        public float Width
        {
            get => m_width;
            set
            {
                if (!Mathf.Approximately(m_width, value))
                {
                    m_width = Mathf.Max(0.5f, value);
                    SyncCarveParameters();
                }
            }
        }
        public bool SeparateYScale
        {
            get => m_separateYScale;
            set
            {
                m_separateYScale = value;
            }
        }
        public float Height
        {
            get => m_height;
            set
            {
                if (!Mathf.Approximately(m_height, value))
                {
                    m_height = Mathf.Min(Mathf.Max(1.0f, value), 20.0f);
                }
            }
        }
        public float IntersectionSize
        {
            get => m_intersectionSize;
            set => m_intersectionSize = value;
        }
        public float GroundAttractDistance
        {
            get => m_groundAttractDistance;
            set => m_groundAttractDistance = Mathf.Clamp(value, 0.0f, 20.0f);
        }
        public bool ConformToGround
        {
            get => m_conformToGround;
            set => m_conformToGround = value;
        }
        public bool AddRoadCollider
        {
            get => m_addRoadCollider;
            set => m_addRoadCollider = value;
        }
        public bool ReceiveShadows
        {
            get => m_shadowsReceive;
            set => m_shadowsReceive = value;
        }
        public bool CastShadows
        {
            get => m_shadowsCast;
            set => m_shadowsCast = value;
        }
        public bool RaycastTerrainOnly
        {
            get => m_raycastTerrainOnly;
            set => m_raycastTerrainOnly = value;
        }
        public bool UseSlopedCrossSection
        {
            get => m_useSlopedCrossection;
            set => m_useSlopedCrossection = value;
        }
        public string Tag
        {
            get => m_tag;
            set => m_tag = value;
        }
        public int Layer
        {
            get => m_layer;
            set => m_layer = value;
        }
        public bool SplitAtTerrains
        {
            get => m_splitAtTerrains;
            set => m_splitAtTerrains = value;
        }
        public bool PostProcess
        {
            get => m_postProcess;
            set => m_postProcess = value;
        }
        /// <summary>
        /// GeNa Extension Methods
        /// </summary>
        #region GeNa Extension Methods
        public void Bake(bool postProcess)
        {
            m_postProcess = postProcess;
            Bake();
        }
        protected override GameObject OnBake(GeNaSpline spline)
        {
            if (m_postProcess)
            {
                PreExecute();
                Execute();
            }
            FindMeshesParent();

            if (m_bakedMeshes == null)
                m_bakedMeshes = new List<GameObject>();

            if (m_bakedMeshes.Count > 0)
            {
                for (int i = 0; i < m_bakedMeshes.Count; i++)
                {
                    if (m_bakedMeshes[i] != null)
                        GeNaEvents.Destroy(m_bakedMeshes[i]);
                }
            }
            m_bakedMeshes.Clear();
            spline.IsDirty = true;

            GameObject roadMeshes = GeNaEvents.BakeSpline(m_roadMeshParent, spline);

            if (m_postProcess && m_splitAtTerrains && roadMeshes != null)
            {
                List<Transform> meshTransforms = GeNaRoadsMesh.PostProcess(roadMeshes, BakedGroupName());
                Dictionary<Transform, List<Transform>> meshParentDict = new Dictionary<Transform, List<Transform>>();
                foreach (Transform meshXform in meshTransforms)
                {
                    if (!meshParentDict.ContainsKey(meshXform.parent))
                        meshParentDict.Add(meshXform.parent, new List<Transform>());
                    meshParentDict[meshXform.parent].Add(meshXform);
                }
                if (!GeNaEvents.HasTerrainsAsScenes())
                {
                    foreach (Transform parentTransform in meshParentDict.Keys)
                    {
                        m_bakedMeshes.Add(parentTransform.gameObject);
                    }
                }
            }
            else if (roadMeshes != null)
            {
                roadMeshes.name = BakedGroupName();
                if (m_postProcess)
                    m_bakedMeshes.Add(roadMeshes);

                GameObject bakedRoads = GameObject.Find("BakedUnsplit_RoadMeshes");
                if (bakedRoads == null)
                {
                    bakedRoads = new GameObject("BakedUnsplit_RoadMeshes");
                    bakedRoads.transform.position = Vector3.zero;
                }
                roadMeshes.transform.parent = bakedRoads.transform;
            }
            return roadMeshes;
        }
        protected override void OnAttach(GeNaSpline spline)
        {
            if (RoadProfile == null)
            {
                RoadProfile = Resources.Load<GeNaRoadProfile>("Road Profiles/RoadBitumenProfile");
            }
            if (m_layer < 0)
            {
                m_layer = LayerMask.NameToLayer("PW_Object_Large");
                if (m_layer < 0)
                    m_layer = 0;
            }
            SyncCarveParameters();

            ProcessSpline(spline);
            GameObjectEntity gameObjectEntity = ScriptableObject.CreateInstance<GameObjectEntity>();
            gameObjectEntity.m_gameObject = m_roadMeshParent;
            GeNaUndoRedo.RecordUndo(gameObjectEntity);

        }
        public override void Execute()
        {
            if (IsActive && Spline.Nodes.Count > 1)
            {
                ProcessSpline(Spline);
            }
        }
        public override void PreExecute()
        {
            SyncCarveParameters();
            DeleteRoadMeshGameobjects();
        }
        protected override void OnActivate()
        {
            if (Spline.Nodes.Count > 1)
                ProcessSpline(Spline);
        }
        protected override void OnDeactivate()
        {
            DeleteRoadMeshGameobjects();
        }
        protected override void OnDelete()
        {
            DeleteRoadMeshGameobjects();
            if (m_roadMeshParent != null && m_roadMeshParent.transform.parent == Spline.transform)
            {
                GeNaEvents.Destroy(m_roadMeshParent);
            }
            m_roadMeshParent = null;
        }
        #endregion End GeNa Extension Methods
        private void SyncCarveParameters()
        {
            //GeNaCarveExtension carve = Spline.GetExtension<GeNaCarveExtension>();
            FindMeshesParent();
        }

        void FindMeshesParent()
        {
            if (m_roadMeshParent != null && m_roadMeshParent.transform.parent != Spline.transform)
                m_roadMeshParent = null;
            if (m_roadMeshParent == null)
            {
                Transform parent = Spline.gameObject.transform.Find("Road Meshes");
                if (parent != null)
                    m_roadMeshParent = parent.gameObject;
            }
        }
        /// <summary>
        /// Delete all road mesh GameObjects
        /// </summary>
        private void DeleteRoadMeshGameobjects()
        {
            // Check to make sure they haven't move the road meshes in the hierarchy
            if (m_roadMeshParent != null && m_roadMeshParent.transform.parent != Spline.transform)
                m_roadMeshParent = null;
            if (m_roadMeshParent != null)
            {
                GeNaRoad[] genaRoads = m_roadMeshParent.GetComponentsInChildren<GeNaRoad>();
                foreach (GeNaRoad road in genaRoads)
                    GeNaEvents.Destroy(road.gameObject);
            }
        }
        private string BakedGroupName()
        {
            return $"Road Meshes ({this.Spline.GetInstanceID() % 997})";
        }
        public bool HasBakedRoads()
        {
            return m_bakedMeshes != null && m_bakedMeshes.Count > 0;
            /*
            string groupName = BakedGroupName();
            GameObject go = GameObject.Find(groupName);
            if (go != null)
                return true;
            return false;
            */
        }
        public void DeleteBakedRoad(bool reenableSpline = false)
        {
            if (!HasBakedRoads())
                return;
            int count = 0;
            foreach (GameObject go in m_bakedMeshes)
            {
                if (go != null)
                {
                    Transform parent = go.transform.parent;
                    GeNaEvents.Destroy(go);
                    count++;
                    if (parent != null && parent.childCount == 0 && (parent.name == "Baked_RoadMeshes" || parent.name == "BakedUnsplit_RoadMeshes"))
                    {
                        GeNaEvents.Destroy(parent.gameObject);
                    }
                }
            }
            if (reenableSpline)
                Spline.gameObject.SetActive(true);

            m_bakedMeshes.Clear();
            Spline.IsDirty = true;

            if (count > 0)
                Debug.Log($"{count} baked road mesh group(s) deleted.");
        }

        public void SmoothSplineForRoads()
        {
            Spline.Smooth();
            ProcessSpline(Spline);
        }

        public void LevelIntersectionTangents()
        {
            if (Spline == null)
                return;
            foreach (GeNaNode node in Spline.Nodes)
            {
                List<GeNaCurve> connected = Spline.GetConnectedCurves(node);
                if (connected.Count > 2)
                    FlattenIntersection(node, connected);
            }
        }

        void FlattenIntersection(GeNaNode node, List<GeNaCurve> curves)
        {
            foreach(GeNaCurve curve in curves)
            {
                if (curve.StartNode == node)
                    curve.StartTangent = new Vector3(curve.StartTangent.x, 0.0f, curve.StartTangent.z);
                if (curve.EndNode == node)
                    curve.EndTangent = new Vector3(curve.EndTangent.x, 0.0f, curve.EndTangent.z);
            }
        }
        /// <summary>
        /// Process the entire spline to create roads and intersections.
        /// </summary>
        /// <param name="spline"></param>
        private void ProcessSpline(GeNaSpline spline)
        {
            if (m_roadMeshParent == null)
            {
                m_roadMeshParent = new GameObject("Road Meshes")
                {
                    transform =
                    {
                        position = Vector3.zero,
                        parent = spline.gameObject.transform
                    }
                };
            }
            if (RoadProfile != null)
            {
                GeNaRoadsMesh geNaRoadsMesh = new GeNaRoadsMesh(RoadProfile.ApplyRoadProfile(), RoadProfile.ApplyIntersectionProfile(), m_roadMeshParent, m_tag, m_layer, m_shadowsCast, m_shadowsReceive)
                {
                    // Check for a user override of the Road Cross Section.
                    RoadCrossSection = null // default
                };

                RoadCrossSectionOverride xOverride = m_crossSectionOverrideComponent;
                if (xOverride != null && xOverride.enabled)
                {
                    RoadCrossSection crossSection = xOverride.GetRoadCrossSection();
                    if (crossSection != null)
                    {
                        if (crossSection.Points.Length > 1 && (crossSection.Points.Length & 1) == 0 && crossSection.Normals.Length == crossSection.Points.Length)
                        {
                            geNaRoadsMesh.RoadCrossSection = crossSection;
                        }
                        else
                        {
                            Debug.LogError($"The provided Road Cross Section Override on {Spline.Name} does not meet specifications.  The cross section must have an even number of points, and the same number of normals.");
                        }
                    }
                }

                // Generate the Road mesh(es).
                float heightScale = Height;
                if (SeparateYScale == false)
                    heightScale = 1.0f;
                geNaRoadsMesh.Process(spline, Width, m_intersectionSize, m_conformToGround, m_groundAttractDistance, m_addRoadCollider, m_raycastTerrainOnly, m_useSlopedCrossection, heightScale);
            }
        }
        protected override void OnDrawGizmosSelected()
        {
            if (Spline.Settings.Advanced.DebuggingEnabled == false)
                return;
            foreach (GeNaCurve curve in Spline.Curves)
                DrawCurveInfo(curve);
        }
        private void DrawCurveInfo(GeNaCurve geNaCurve)
        {
            // Draw arrows showing which direction a curve is facing (from StartNode to EndNode).
            GeNaSample geNaSample = geNaCurve.GetSample(0.45f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
            geNaSample = geNaCurve.GetSample(0.5f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
            geNaSample = geNaCurve.GetSample(0.55f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
        }
        private void DrawArrow(Vector3 position, Vector3 direction)
        {
            Gizmos.color = Color.red;
            direction.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Ray ray = new Ray(position + Vector3.up * 0.3f, (-direction + right) * 0.5f);
            Gizmos.DrawRay(ray);
            ray.direction = (-direction - right) * 0.5f;
            Gizmos.DrawRay(ray);
        }
    }
}