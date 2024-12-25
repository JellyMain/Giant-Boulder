//Copyright(c)2020 Procedural Worlds Pty Limited 
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for creating Rivers along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Rivers", menuName = "Procedural Worlds/GeNa/Extensions/Rivers", order = 4)]
    public class GeNaRiverExtension : GeNaSplineExtension
    {
        const string GENA_USER_DATA_PATH = "GeNa User Data/";
        const string BAKED_UNSPLIT_RIVER_PARENT_NAME = "BakedUnsplit_RiverMeshes";
        const string RIVER_MESHES_PARENT_NAME = "RiverMeshes";

        [SerializeField] protected GeNaRiverProfile m_riverProfile;
        [SerializeField] protected Material m_currentMaterial;
        [SerializeField] protected float m_seaLevel = 25f;
        [SerializeField] protected bool m_useGaiaSeaLevel = true;
        [SerializeField] protected float m_startFlow = 0.2f;
        [SerializeField] protected float m_vertexDistance = 3.0f;
        [SerializeField] protected float m_bankOverstep = 1.0f;
        [SerializeField] protected float m_splineSmoothing = 0.82f;
        [SerializeField] protected float m_riverWidth = 20f;
        [SerializeField] protected float m_capDistance = 15f;
        [SerializeField] protected float m_endCapDistance = 0.0f;
        [SerializeField] protected bool m_addCollider = false;
        [SerializeField] protected bool m_shadowsCast = false;
        [SerializeField] protected bool m_shadowsReceive = false;
        [SerializeField] protected bool m_raycastTerrainOnly = true;
        [SerializeField] protected bool m_useWorldspaceTextureWidth = false;
        [SerializeField] protected float m_worldspaceWidth = 15.0f;
        [SerializeField] protected string m_tag = "Untagged";
        [SerializeField] protected int m_layer = -1;
        [SerializeField] protected bool m_splitAtTerrains = true;
        [SerializeField] protected bool m_autoUpdateOnTerrainChange = false;
        [SerializeField] protected MeshRenderer m_meshRenderer;
        [NonSerialized] protected RenderTexture m_currentRenderTexture = null;
        [NonSerialized] protected GameObject m_riverMeshParent = null;
        [NonSerialized] protected bool m_postProcess = true;
        [SerializeField] protected List<GameObject> m_bakedMeshes = new List<GameObject>();

        public GeNaRiverProfile RiverProfile
        {
            get => m_riverProfile;
            set => m_riverProfile = value;
        }

        public bool UseGaiaSeaLevel
        {
            get => m_useGaiaSeaLevel;
            set => m_useGaiaSeaLevel = value;
        }

        public float SeaLevel
        {
            get => m_seaLevel;
            set => m_seaLevel = value;
        }

        public bool SyncToWeather
        {
            get => m_riverProfile.RiverParameters.m_syncToWeather;
            set => m_riverProfile.RiverParameters.m_syncToWeather = value;
        }

        public float StartFlow
        {
            get => m_startFlow;
            set => m_startFlow = Mathf.Clamp(value, 0.05f, Mathf.Infinity);
        }

        public float VertexDistance
        {
            get => m_vertexDistance;
            set => m_vertexDistance = value;
        }

        public float BankOverstep
        {
            get => m_bankOverstep;
            set => m_bankOverstep = Mathf.Clamp(value, 0.5f, 5.0f);
        }

        public float RiverWidth
        {
            get => m_riverWidth;
            set => m_riverWidth = Mathf.Max(value, 0.5f);
        }

        public float CapDistance
        {
            get => m_capDistance;
            set => m_capDistance = Mathf.Clamp(value, 0.1f, Mathf.Infinity);
        }

        public float EndCapDistance
        {
            get => m_endCapDistance;
            set => m_endCapDistance = Mathf.Clamp(value, 0.0f, 5000.0f);
        }

        public bool AddCollider
        {
            get => m_addCollider;
            set => m_addCollider = value;
        }

        public bool RaycastTerrainOnly
        {
            get => m_raycastTerrainOnly;
            set => m_raycastTerrainOnly = value;
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

        private void OnEnable()
        {
            if (m_autoUpdateOnTerrainChange)
            {
                GeNaEvents.onTerrainChanged -= TerrainChanged;
                GeNaEvents.onTerrainChanged += TerrainChanged;
            }
        }

        public bool UpdateOnTerrainChange
        {
            get => m_autoUpdateOnTerrainChange;
            set
            {
                if (m_autoUpdateOnTerrainChange != value)
                {
                    if (value)
                    {
                        GeNaEvents.onTerrainChanged -= TerrainChanged;
                        GeNaEvents.onTerrainChanged += TerrainChanged;
                    }
                    else
                    {
                        GeNaEvents.onTerrainChanged -= TerrainChanged;
                    }
                }

                m_autoUpdateOnTerrainChange = value;
            }
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

        public bool UseWorldspaceTextureWidth
        {
            get => m_useWorldspaceTextureWidth;
            set => m_useWorldspaceTextureWidth = value;
        }

        public float WorldspaceWidthRepeat
        {
            get => m_worldspaceWidth;
            set => m_worldspaceWidth = value;
        }

        public MeshRenderer MeshRenderer
        {
            get => m_meshRenderer;
            set => m_meshRenderer = value;
        }

        public GameObject Parent
        {
            get => m_riverMeshParent;
            set => m_riverMeshParent = value;
        }

        /// <summary>
        /// GeNa Extension Methods
        /// </summary>

        #region GeNaSpline Extension Methods

        protected override void OnAttach(GeNaSpline spline)
        {
            if (RiverProfile == null)
            {
                RiverProfile = GeNaUtility.LoadNewRiverProfile();
                if (m_layer < 0)
                {
                    m_layer = LayerMask.NameToLayer("PW_Object_Large");
                    if (m_layer < 0)
                        m_layer = 0;
                }
            }

            if (m_autoUpdateOnTerrainChange)
            {
                GeNaEvents.onTerrainChanged -= TerrainChanged;
                GeNaEvents.onTerrainChanged += TerrainChanged;
            }

            CreateRivers();
            GameObjectEntity gameObjectEntity = ScriptableObject.CreateInstance<GameObjectEntity>();
            gameObjectEntity.m_gameObject = m_riverMeshParent;
            GeNaUndoRedo.RecordUndo(gameObjectEntity);
        }

        List<Transform> affectedTerrains = new List<Transform>();

        private void TerrainChanged(Terrain terrain, TerrainChangedFlags flags)
        {
            if ((flags & TerrainChangedFlags.Heightmap) == TerrainChangedFlags.Heightmap || (flags & TerrainChangedFlags.FlushEverythingImmediately) == TerrainChangedFlags.FlushEverythingImmediately)
            {
                bool affected = false;
                Transform terrainTransform = terrain.transform;
                for (int i = 0; i < affectedTerrains.Count; i++)
                {
                    if (affectedTerrains[i] == terrainTransform)
                        affected = true;
                }

                if (!affected)
                    return;

                //Debug.Log($"Affected Terrain Changed ({Spline.Nodes.Count}).");
                PreExecute();
                Execute();
            }
        }

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
            else
            {
                CreateCurrentRenderTexture();
            }

            FindMeshesParent();

            if (m_autoUpdateOnTerrainChange)
                GeNaEvents.onTerrainChanged -= TerrainChanged;

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

            GameObject riverMeshes = GeNaEvents.BakeSpline(m_riverMeshParent, spline);
            if (riverMeshes == null)
                return null;
            Dictionary<Transform, List<Transform>> meshParentDict = null;
            if (m_postProcess && m_splitAtTerrains)
            {
                // PostProcess will split the meshes up into new meshes per terrain.
                List<Transform> meshTransforms = GeNaRiverMesh.PostProcess(riverMeshes, BakedGroupName());
                meshParentDict = ProcessRenderTargets(meshTransforms);
                if (!GeNaEvents.HasTerrainsAsScenes())
                {
                    foreach (Transform parentTransform in meshParentDict.Keys)
                    {
                        m_bakedMeshes.Add(parentTransform.gameObject);
                    }
                }
            }
            else
            {
                GameObject bakedRivers = GameObject.Find(BAKED_UNSPLIT_RIVER_PARENT_NAME);
                if (bakedRivers == null)
                {
                    bakedRivers = new GameObject(BAKED_UNSPLIT_RIVER_PARENT_NAME);
                }

                riverMeshes.transform.parent = bakedRivers.transform;
                riverMeshes.name = BakedGroupName();
                List<Transform> meshTransforms = new List<Transform>();
                Renderer[] renderers = riverMeshes.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                    meshTransforms.Add(renderers[i].transform);
                meshParentDict = ProcessRenderTargets(meshTransforms);
                if (m_postProcess)
                {
                    foreach (Transform parentTransform in meshParentDict.Keys)
                    {
                        m_bakedMeshes.Add(parentTransform.gameObject);
                    }
                }
            }

            return riverMeshes;
        }

        void FindMeshesParent()
        {
            if (m_riverMeshParent != null && m_riverMeshParent.transform.parent != Spline.transform)
                m_riverMeshParent = null;
            if (m_riverMeshParent == null)
            {
                Transform parent = Spline.gameObject.transform.Find(RIVER_MESHES_PARENT_NAME);
                if (parent != null)
                    m_riverMeshParent = parent.gameObject;
            }
        }

        Dictionary<Transform, List<Transform>> ProcessRenderTargets(List<Transform> meshTransforms)
        {
            Dictionary<Transform, List<Transform>> meshParentDict = new Dictionary<Transform, List<Transform>>();
            if (meshTransforms.Count < 1)
                return meshParentDict;

            foreach (Transform meshXform in meshTransforms)
            {
                if (!meshParentDict.ContainsKey(meshXform.parent))
                    meshParentDict.Add(meshXform.parent, new List<Transform>());
                meshParentDict[meshXform.parent].Add(meshXform);
            }

            // Get the Flow Render Target and Create a Texture2D from it,
            // to be assigned to the River Material.
            if (RiverProfile.RiverParameters.m_renderMode == Constants.ProfileRenderMode.RiverFlow)
            {
                Material material = meshTransforms[0].GetComponent<Renderer>().sharedMaterial;
                if (m_currentMaterial != null)
                    material = m_currentMaterial;

                // Create a Texture2D to replace the RenderTexture created in CaptureRiverFlow.
                Texture tex = material.GetTexture("_FlowMap");
                RenderTexture rtFlow = tex as RenderTexture;
                if (rtFlow != null)
                {
                    RenderTexture.active = rtFlow;
                    int size = rtFlow.width;
                    Texture2D tFlow = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
                    tFlow.anisoLevel = 0;
                    tFlow.ReadPixels(new Rect(0, 0, size, size), 0, 0, false);
                    tFlow.Apply();
                    RenderTexture.active = null;

                    material.SetTexture("_FlowMap", tFlow);

                    foreach (List<Transform> meshes in meshParentDict.Values)
                    {
                        Material newMat = new Material(material);
                        newMat.SetTexture("_FlowMap", tFlow);
                        foreach (Transform transform in meshes)
                        {
                            transform.GetComponent<Renderer>().sharedMaterial = newMat;
                        }
                    }
                }
            }


            return meshParentDict;
        }

        /// <summary>
        /// Returns a dictionary of mesh transforms key'ed by their parent transform.
        /// </summary>
        /// <typeparam name="Transform"></typeparam>
        /// <typeparam name="List"></typeparam>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        Dictionary<Transform, List<Transform>> ProcessRenderTargetsPerTerrain(List<Transform> meshTransforms)
        {
            Dictionary<Transform, List<Transform>> meshParentDict = new Dictionary<Transform, List<Transform>>();
            foreach (Transform meshXform in meshTransforms)
            {
                if (!meshParentDict.ContainsKey(meshXform.parent))
                    meshParentDict.Add(meshXform.parent, new List<Transform>());
                meshParentDict[meshXform.parent].Add(meshXform);
            }

            // If the current River Profile uses the RiverFlow shader, then 
            // we need to capture the flow texture for all of the meshes
            // that are parented to the same terrain, with the same resolution
            // texture for each.
            if (RiverProfile.RiverParameters.m_renderMode == Constants.ProfileRenderMode.RiverFlow)
            {
                //Debug.Log($"Creating a Texture2D for each RenderTexture for {terrainMeshDict.Count} terrain rivers.");

                GameObject oceanGO = GameObject.Find("Water Surface");
                Transform oceanMeshTransform = null;
                if (oceanGO != null)
                    oceanMeshTransform = oceanGO.transform;

                foreach (List<Transform> meshXForms in meshParentDict.Values)
                {
                    // Let's get the "Baked River Meshes" transform so that we capture the flow of all river meshes.
                    if (meshXForms[0].parent == null || meshXForms[0].parent.parent == null)
                    {
                        Debug.LogError("Missing River Mesh parent and/or grandparent.");
                        return meshParentDict;
                    }

                    Transform bakedRiverMeshes = meshXForms[0].parent.parent;
                    Terrain terrain = null;
                    if (bakedRiverMeshes != null && bakedRiverMeshes.parent != null)
                        terrain = bakedRiverMeshes.parent.GetComponent<Terrain>();

                    Material material = RiverProfile.ApplyProfile(SeaLevel, true, meshXForms[0].GetComponent<Renderer>().sharedMaterial);
                    foreach (Transform meshXForm in meshXForms)
                    {
                        meshXForm.GetComponent<Renderer>().sharedMaterial = material;
                    }

                    CaptureFlow captureFlow = new CaptureFlow();
                    CaptureFlow.RiverFlowResults results;
                    if (bakedRiverMeshes != null)
                        results = captureFlow.CaptureRiverFlow(bakedRiverMeshes, null, 4096, material, oceanMeshTransform, terrain);
                    else
                        results = captureFlow.CaptureRiverFlow(meshXForms, null, 4096, material, oceanMeshTransform, terrain);

                    material.SetVector("_BoundsMinimum", results.boundsCenter - results.boundsExtent);
                    material.SetVector("_BoundsMaximum", results.boundsCenter + results.boundsExtent);
                    material.SetVector("_Center", results.boundsCenter);
                    material.SetVector("_Extent", results.boundsExtent);

                    // Create a Texture2D to replace the RenderTexture created in CaptureRiverFlow.
                    RenderTexture.active = results.rtFlow;
                    int size = results.rtFlow.width;
                    Texture2D tFlow = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
                    tFlow.anisoLevel = 0;
                    tFlow.ReadPixels(new Rect(0, 0, size, size), 0, 0, false);
                    tFlow.Apply();
                    RenderTexture.active = null;

                    // Now, set all material properties of the river meshes that depend on the new Flow Texture.
                    // NOTE: for now, these operations will be done over and over, for each River Spline on a terrain.

                    if (bakedRiverMeshes != null)
                    {
                        Renderer[] allRenderers = bakedRiverMeshes.GetComponentsInChildren<Renderer>(false);
                        foreach (Renderer renderer in allRenderers)
                        {
                            Material thisMaterial = renderer.sharedMaterial;
                            if (thisMaterial.shader == material.shader)
                            {
                                thisMaterial.SetVector("_BoundsMinimum", results.boundsCenter - results.boundsExtent);
                                thisMaterial.SetVector("_BoundsMaximum", results.boundsCenter + results.boundsExtent);
                                thisMaterial.SetVector("_Center", results.boundsCenter);
                                thisMaterial.SetVector("_Extent", results.boundsExtent);
                                thisMaterial.SetTexture("_FlowMap", tFlow);
                            }
                        }
                    }
                }
            }

            return meshParentDict;
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
            //DeleteRiverMeshGameobjects(Spline);
        }

        protected override void OnActivate()
        {
            if (Spline.Nodes.Count > 1)
                ProcessSpline(Spline);
            if (m_autoUpdateOnTerrainChange)
            {
                GeNaEvents.onTerrainChanged -= TerrainChanged;
                GeNaEvents.onTerrainChanged += TerrainChanged;
            }
        }

        protected override void OnDeactivate()
        {
            DeleteRiverMeshGameobjects(Spline);
            if (m_autoUpdateOnTerrainChange)
                GeNaEvents.onTerrainChanged -= TerrainChanged;
        }

        protected override void OnDelete()
        {
            DeleteRiverMeshGameobjects(Spline);
            if (m_autoUpdateOnTerrainChange)
                GeNaEvents.onTerrainChanged -= TerrainChanged;
            if (m_riverMeshParent != null)
            {
                GeNaEvents.Destroy(m_riverMeshParent);
                m_riverMeshParent = null;
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            if (Spline.Settings.Advanced.DebuggingEnabled == false)
                return;
            foreach (GeNaCurve curve in Spline.Curves)
            {
                DrawCurveInfo(curve);
            }
        }

        private void DrawCurveInfo(GeNaCurve geNaCurve)
        {
            // Draw arrows showing which direction a curve is facing (from StartNode to EndNode).
            Gizmos.color = Color.red;
            GeNaSample geNaSample = geNaCurve.GetSample(0.45f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
            geNaSample = geNaCurve.GetSample(0.5f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
            geNaSample = geNaCurve.GetSample(0.55f);
            DrawArrow(geNaSample.Location, geNaSample.Forward);
        }

        private void DrawArrow(Vector3 position, Vector3 direction)
        {
            direction.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Ray ray = new Ray(position, (-direction + right) * 0.5f);
            Gizmos.DrawRay(ray);
            ray.direction = (-direction - right) * 0.5f;
            Gizmos.DrawRay(ray);
        }

        #endregion End GeNa Extension Methods

        private void DeleteRiverMeshGameobjects(GeNaSpline spline)
        {
            // Check to make sure they haven't move the road meshes in the hierarchy
            if (m_riverMeshParent != null && m_riverMeshParent.transform.parent != Spline.transform)
                m_riverMeshParent = null;
            if (m_riverMeshParent == null)
            {
                Transform splineTransform = Spline.gameObject.transform;
                // see if we can find it
                Transform riverMeshesTransform = splineTransform.Find(RIVER_MESHES_PARENT_NAME);
                if (riverMeshesTransform != null)
                    m_riverMeshParent = riverMeshesTransform.gameObject;
            }

            if (m_riverMeshParent != null)
            {
                List<Transform> children = new List<Transform>();
                foreach (Transform transform in m_riverMeshParent.transform)
                {
                    children.Add(transform);
                }

                for (int i = 0; i < children.Count; i++)
                {
                    GeNaEvents.Destroy(children[i].gameObject);
                }
            }
        }

        private void ProcessSpline(GeNaSpline spline)
        {
            CreateRivers();
        }

        public void UpdateMaterial()
        {
            if (RiverProfile == null)
            {
                return;
            }

            // recompute the river meshes and assign new material.
            ProcessSpline(this.Spline);
        }

        private string BakedGroupName()
        {
            return $"River Meshes ({this.Spline.GetInstanceID() % 9997})";
        }

        public bool HasBakedRivers()
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

        public void DeleteBakedRiver(bool reenableSpline = false)
        {
            //string groupName = BakedGroupName();
            //GameObject go = GameObject.Find(groupName);
            if (m_bakedMeshes == null || m_bakedMeshes.Count < 1)
                return;

            int count = 0;
            //while (go != null)
            foreach (GameObject go in m_bakedMeshes)
            {
                if (go != null)
                {
                    Transform parent = go.transform.parent;
                    GeNaEvents.Destroy(go);
                    count++;
                    if (parent != null && parent.childCount == 0 && (parent.name == "Baked_RiverMeshes" || parent.name == BAKED_UNSPLIT_RIVER_PARENT_NAME))
                    {
                        GeNaEvents.Destroy(parent.gameObject);
                    }
                }
                //go = GameObject.Find(groupName);
            }

            if (reenableSpline)
                Spline.gameObject.SetActive(true);

            m_bakedMeshes.Clear();
            Spline.IsDirty = true;

            if (count > 0)
                Debug.Log($"{count} baked river mesh group(s) deleted.");
        }

        public void SetSplineToDownhill()
        {
            if (Spline == null)
                return;
            _SetSplineToDownhill();
        }

        private void _SetSplineToDownhill()
        {
            Dictionary<int, List<GeNaCurve>> trees = Spline.GetTrees();
            foreach (List<GeNaCurve> curCurves in trees.Values)
            {
                (float min, float max) minMax = _GetCurvesMinMax(curCurves);
                List<GeNaCurve> curves = new List<GeNaCurve>(curCurves);
                if (minMax.min > minMax.max)
                {
                    curves.Reverse();
                    (minMax.min, minMax.max) = (minMax.max, minMax.min);
                }

                float curHeight = minMax.max;
                for (int i = 0; i < curves.Count; i++)
                {
                    if (curves[i].EndNode.Position.y >= curHeight)
                    {
                        Vector3 pos = curves[i].EndNode.Position;
                        pos = new Vector3(pos.x, curHeight - 0.001f, pos.z);
                        curves[i].EndNode.Position = pos;
                    }

                    curHeight = curves[i].EndNode.Position.y;
                }
            }

            Spline.Smooth();
        }

        private (float min, float max) _GetCurvesMinMax(List<GeNaCurve> curves)
        {
            return (curves[curves.Count - 1].P3.y, curves[0].P0.y);
        }

        private void CreateRivers()
        {
            if (Spline == null || Spline.Nodes.Count < 2)
                return;

            // See if we already have a material
            Material material = null;
            if (m_riverMeshParent != null)
            {
                Renderer child = m_riverMeshParent.GetComponentInChildren<Renderer>();
                if (child != null)
                {
                    material = child.sharedMaterial;
                    if (!material.HasProperty("_FlowMap"))
                    {
                        material = m_currentMaterial;
                    }
                }
            }

            DeleteRiverMeshGameobjects(Spline);
            if (m_riverMeshParent == null)
            {
                Transform splineTransform = Spline.gameObject.transform;
                // see if we can find it
                Transform riverMeshesTransform = splineTransform.Find(RIVER_MESHES_PARENT_NAME);
                if (riverMeshesTransform != null)
                    m_riverMeshParent = riverMeshesTransform.gameObject;
                if (m_riverMeshParent == null)
                {
                    m_riverMeshParent = new GameObject(RIVER_MESHES_PARENT_NAME);
                    m_riverMeshParent.transform.position = Vector3.zero;
                    m_riverMeshParent.transform.parent = splineTransform;
                }
            }

            if (RiverProfile != null)
            {
                if (GeNaUtility.Gaia2Present)
                {
                    if (UseGaiaSeaLevel)
                    {
                        SeaLevel = GeNaEvents.GetSeaLevel(SeaLevel);
                    }
                }

                m_currentMaterial = RiverProfile.ApplyProfile(SeaLevel, true, material);
                GeNaRiverMesh geNaRiverMesh = new GeNaRiverMesh(Spline, m_startFlow, m_vertexDistance, m_bankOverstep, m_currentMaterial, m_riverWidth, SyncToWeather, RiverProfile, m_shadowsCast, m_shadowsReceive);
                if (m_autoUpdateOnTerrainChange)
                    geNaRiverMesh.m_affectedTerrains = affectedTerrains;
                else
                    geNaRiverMesh.m_affectedTerrains = null;
                geNaRiverMesh.CreateMeshes(m_riverMeshParent.transform, m_addCollider, m_raycastTerrainOnly, m_tag, m_layer, SeaLevel, CapDistance, UseWorldspaceTextureWidth, WorldspaceWidthRepeat, EndCapDistance);

                RenderTexture newRTex = CaptureRiverFlowTexture();
                if (m_currentRenderTexture != null && m_currentRenderTexture != newRTex)
                {
                    GeNaEvents.Destroy(m_currentRenderTexture);
                    //Debug.Log("Render Texture for River Material destroyed.");
                }

                m_currentRenderTexture = newRTex;
            }
        }

        void CreateCurrentRenderTexture()
        {
            FindMeshesParent();
            RenderTexture newRTex = CaptureRiverFlowTexture();
            if (m_currentRenderTexture != null && m_currentRenderTexture != newRTex)
            {
                GeNaEvents.Destroy(m_currentRenderTexture);
                //Debug.Log("Render Texture for River Material destroyed.");
            }

            m_currentRenderTexture = newRTex;
        }

        public RenderTexture CaptureRiverFlowTexture(bool saveFlowTexture = false)
        {
            if (m_riverMeshParent == null)
                return m_currentRenderTexture;

            // Do we need to create a river flow texture?
            GeNaRiverParameters riverParameters = RiverProfile.RiverParameters;
            if (riverParameters != null)
            {
                if (riverParameters.m_renderMode == Constants.ProfileRenderMode.RiverFlow)
                {
                    GameObject oceanGO = GameObject.Find("Water Surface");
                    Transform oceanMeshTransform = null;
                    if (oceanGO != null)
                        oceanMeshTransform = oceanGO.transform;

                    int textureSize = 1024;
                    string filename = string.Empty;
                    if (saveFlowTexture)
                    {
                        filename = System.IO.Path.Combine(Application.dataPath, GENA_USER_DATA_PATH, $"FlowMap_{Spline.GetInstanceID()}.png");
                        Debug.Log($"Flow Map filename = {filename}");
                    }
                    else
                    {
                        textureSize = 2048;
                    }

                    CaptureFlow captureFlow = new CaptureFlow();

                    CaptureFlow.RiverFlowResults results = captureFlow.CaptureRiverFlow(m_riverMeshParent.transform, filename, textureSize, m_currentMaterial, oceanMeshTransform);

                    m_currentMaterial.SetVector("_BoundsMinimum", results.boundsCenter - results.boundsExtent);
                    m_currentMaterial.SetVector("_BoundsMaximum", results.boundsCenter + results.boundsExtent);
                    m_currentMaterial.SetVector("_Center", results.boundsCenter);
                    m_currentMaterial.SetVector("_Extent", results.boundsExtent);

                    return results.rtFlow;
                }
            }

            return null;
        }
    }
}