// .NET
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using PWCommon5;
// Unity
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    [InitializeOnLoad]
    public class GeNaSpawnerPrefabs
    {
        static GeNaSpawnerPrefabs()
        {
            PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdated;
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdated;
        }
        static void OnPrefabInstanceUpdated(GameObject gameObject)
        {
            GeNaSpawner spawner = gameObject.GetComponent<GeNaSpawner>();
            if (spawner == null)
                return;
            if (!GeNaEditorUtility.IsPrefab(gameObject))
                return;
            GameObject asset = GeNaEditorUtility.GetPrefabAsset(gameObject);
            //GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAsset);
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            GeNaSpawnerData mainFile = default(GeNaSpawnerData);
            foreach (Object @object in objects)
            {
                mainFile = @object as GeNaSpawnerData;
                if (mainFile == null)
                    continue;
                if (mainFile.name.Contains("Main"))
                    break;
            }
            if (mainFile == null)
            {
                spawner.SpawnerData = ScriptableObject.Instantiate(spawner.SpawnerData);
                spawner.SpawnerData.name = "Spawner Data (Main)";
                AssetDatabase.AddObjectToAsset(spawner.SpawnerData, asset);
                // This needs to be in a delay call to avoid this
                // https://issuetracker.unity3d.com/issues/assertion-failed-on-expression-gforcereimports-empty-is-thrown-when-a-new-universal-rp-project-is-created?page=5#comments
                EditorApplication.delayCall += () =>
                {
                    mainFile = default;
                    objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (Object @object in objects)
                    {
                        mainFile = @object as GeNaSpawnerData;
                        if (mainFile == null)
                            continue;
                        if (mainFile.name.Contains("Main"))
                            break;
                    }
                    if (mainFile != null)
                    {
                        GameObject go = PrefabUtility.LoadPrefabContents(assetPath);
                        GeNaSpawner prefabSpawner = go.GetComponent<GeNaSpawner>();
                        prefabSpawner.SpawnerData = mainFile;
                        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
                        PrefabUtility.UnloadPrefabContents(go);
                    }
                };
            }
        }
    }
    /// <summary>
    /// Editor for GeNa spawning system
    /// </summary>
    [CustomEditor(typeof(Spawner))]
    public class SpawnerEditor : GeNaEditor
    {
        #region Variables
        // Settings
        private static bool m_yesToAllValidations = false;
        private static bool m_noToAllValidations = false;
        // Panel Toggles
        private static bool m_showDetailedHelp = false;
        private static bool m_showQuickStart = true;
        private static bool m_showOverview = true;
        private static bool m_showPlacementCriteria = false;
        private static bool m_showSpawnCriteria = false;
        private static bool m_showPrototypes = true;
        private static bool m_showAdvancedSettings = false;
        // Settings
        private GeNaSpawner m_spawner;
        private SerializableDictionary<int, bool> m_showSpawnCriteriaOverrides = new SerializableDictionary<int, bool>();
        private SerializableDictionary<int, bool> m_showPrototypeAdvanced = new SerializableDictionary<int, bool>();
        private SerializableDictionary<int, bool> m_showPrototypeInEditor = new SerializableDictionary<int, bool>();
        private SerializableDictionary<int, bool> m_showResourceInEditor = new SerializableDictionary<int, bool>();
        private SerializableDictionary<int, bool> m_showChildResourceInEditor = new SerializableDictionary<int, bool>();
        // Switch to drop custom ground level for ingestion
        private Vector2 m_scrollPosition = Vector2.zero;
        // Helpers
        private bool m_hasPrefabs;
        private bool m_hasTrees;
        private bool m_hasTextures;
        private Vector2 m_lastMousePos = Vector2.zero;
        private Vector3 m_lastSpawnPosition = Vector3.zero;
        private Color m_separatorColor = Color.black;
        private int m_instanceTopLimit;
        private static bool m_showVisualizer = true;
        // GUI
        private Texture2D m_overridesIco;
        private Texture2D m_ChildSpawnerIco;
        public AabbTest[,] m_fitnessArray = new AabbTest[1, 1];
        private bool m_isPartOfPrefab;
        private bool m_isPrefabMode;
        #endregion
        #region Properties
        private GeNaSpawnerData SpawnerData => m_spawner.SpawnerData;
        private PlacementCriteria PlacementCriteria => SpawnerData.PlacementCriteria;
        private SpawnCriteria SpawnCriteria => SpawnerData.SpawnCriteria;
        private SpawnerSettings Settings => SpawnerData.Settings;
        #endregion
        #region Methods
        #region Unity
        private void SaveChanges(GeNaSpawner spawner)
        {
            spawner.SpawnerData = GeNaEditorUtility.ApplyToMain(spawner);
            spawner.SpawnerData.IsDirty = false;
            // spawner.SerializeToFile(dataFile);
            //PrefabUtility.ApplyPrefabInstance(spawner.gameObject, InteractionMode.AutomatedAction);
            // EditorUtility.SetDirty(spawner);
            // PrefabUtility.RecordPrefabInstancePropertyModifications(spawner);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            spawner.Load();
        }
        private void DiscardChanges(GeNaSpawner spawner)
        {
            GeNaSpawnerData data = GeNaEditorUtility.RevertToMain(spawner);
            spawner.SpawnerData = Instantiate(data);
            spawner.SpawnerData.name = "Spawner Data (Main)";
            // spawner.DeserializeFromFile(dataFile);
            PrefabUtility.RecordPrefabInstancePropertyModifications(spawner);
            //EditorUtility.SetDirty(spawner);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            spawner.Load();
        }
        private bool m_isDestroying = false;
        private void OnSpawnerDestroyed(GeNaSpawnerData geNaSpawnerData)
        {
            if (geNaSpawnerData.GetInstanceID() == SpawnerData.GetInstanceID())
            {
                m_isDestroying = true;
            }
        }
        protected override void OnDestroy()
        {
            if (m_isDestroying)
                GeNaEditorUtility.RemoveDataBufferScriptable(m_spawner);
            GeNaEvents.onBeforeSpawn -= OnBeforeSpawn;
            GeNaEvents.onSpawnFinished -= OnSpawnFinished;
            GeNaEvents.onSpawnerDestroyed -= OnSpawnerDestroyed;
        }
        private void OnBeforeSpawn(GeNaSpawnerData spawnerData)
        {
            Repaint();
        }
        private void OnSpawnFinished()
        {
            m_spawner.UpdateVisualization();
            Repaint();
        }
        protected void OnEnable()
        {
            m_showVisualizer = true;
            GameObject tempObject = GeNaSpawnerInternal.TempGameObject;
            tempObject.SetActive(false);
            string prefString = "PWShowNews" + PWApp.CONF.NameSpace;
            EditorPrefs.SetBool(prefString, true);
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, null, null);
            // Temp
            DeleteEditorPrefsKeys();
            #region Spawner Setup
            m_spawner = target as GeNaSpawner;
            m_isPartOfPrefab = PrefabUtility.IsPartOfAnyPrefab(m_spawner.gameObject) && !AssetDatabase.Contains(m_spawner.gameObject);
            GeNaReferenceLoader.visitedSpawners.Clear();
            m_spawner.Load();
            // Setup defaults
            m_spawner.SetDefaults(GeNaEditorUtility.Defaults);
            m_instanceTopLimit = m_spawner.GetInstancesTopLimit();
            if (!SpawnerData.DefaultsSet)
            {
                // Default Spawn To Target
                SpawnerData.SpawnToTarget = Preferences.DefaultSpawnToTarget;
            }
            if (SpawnerData.GetSeaLevel)
            {
                GeNaEvents.SetSeaLevel(SpawnerData);
            }
            m_noToAllValidations = false;
            m_yesToAllValidations = false;
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                ValidateSpawnerPrototypes(m_spawner, SpawnerData, terrain);
            }
            #endregion
            #region Load Textures
            // If Unity Pro
            if (EditorGUIUtility.isProSkin)
            {
                if (m_overridesIco == null)
                    m_overridesIco = Resources.Load("fschklicop") as Texture2D;
                if (m_ChildSpawnerIco == null)
                    m_ChildSpawnerIco = Resources.Load("protoparentp") as Texture2D;
                m_separatorColor = new Color(0.34f, 0.34f, 0.34f);
            }
            // or Unity Personal
            else
            {
                if (m_overridesIco == null)
                    m_overridesIco = Resources.Load("fschklico") as Texture2D;
                if (m_ChildSpawnerIco == null)
                    m_ChildSpawnerIco = Resources.Load("protoparent") as Texture2D;
            }
            #endregion
            SpawnerData.VisualizationFixed = false;
            GeNaEvents.onSpawnFinished -= OnSpawnFinished;
            GeNaEvents.onSpawnFinished += OnSpawnFinished;
            GeNaEvents.onBeforeSpawn -= OnBeforeSpawn;
            GeNaEvents.onBeforeSpawn += OnBeforeSpawn;
            GeNaEvents.onSpawnerDestroyed -= OnSpawnerDestroyed;
            GeNaEvents.onSpawnerDestroyed += OnSpawnerDestroyed;
            GeNaEditorEvents.onBeforeAssemblyReloads -= Dispose;
            GeNaEditorEvents.onBeforeAssemblyReloads += Dispose;
            Tools.hidden = true;
            m_isPrefabMode = GeNaEditorUtility.IsPrefab(m_spawner.gameObject) && AssetDatabase.Contains(m_spawner.gameObject);
        }
        protected void OnDisable()
        {
            Tools.hidden = false;
            m_showVisualizer = true;
            GeNaEvents.onBeforeSpawn -= OnBeforeSpawn;
            GeNaEvents.onSpawnFinished -= OnSpawnFinished;
            GeNaEvents.onSpawnerDestroyed -= OnSpawnerDestroyed;
            GeNaEditorEvents.onBeforeAssemblyReloads -= Dispose;
            Dispose();
        }
        public override void OnSceneGUI()
        {
            // Exit if no spawner
            if (SpawnerData == null)
                return;
            GeNaSpawnerEditor(m_spawner, Repaint);
            if (!m_isPrefabMode)
            {
                GeNaManager geNaManager = GeNaManager.GetInstance();
                if (geNaManager != null)
                {
                    Queue<SpawnerEntry> spawnEntries = geNaManager.SpawnEntryQueue;
                    foreach (SpawnerEntry spawnEntry in spawnEntries)
                    {
                        Handles.color = Color.green;
                        foreach (SpawnCall spawnCall in spawnEntry.SpawnCalls)
                        {
                            if (!spawnCall.CanSpawn)
                                continue;
                            Handles.DrawLine(spawnCall.Location, spawnCall.Location + spawnCall.Normal * 1.5f);
                            Handles.DrawWireArc(spawnCall.Location, spawnCall.Normal, Vector3.forward, 360f, 1f);
                        }
                    }
                }
            }
        }
        public void Dispose()
        {
            GameObject tempObject = GeNaSpawnerInternal.TempGameObject;
            if (tempObject != null)
                GeNaEvents.Destroy(tempObject);
            m_spawner.ForEachProtoResource(resource => resource.ClearCache());
        }
        /// <summary>
        /// Returns true if the Spawner needs to be Serialized
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="onRepaint"></param>
        /// <returns></returns>
        public static void GeNaSpawnerEditor(GeNaSpawner spawner, Action onRepaint = null)
        {
            if (spawner == null)
                return;
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            // Exit if event does not have current value
            if (e == null)
                return;
            GeNaSpawnerData spawnerData = spawner.SpawnerData;
            // Repaint for Visualization
            SceneView.RepaintAll();
            SpawnerSettings settings = spawnerData.Settings;
            PlacementCriteria placementCriteria = spawnerData.PlacementCriteria;
            SpawnCriteria spawnCriteria = spawnerData.SpawnCriteria;
            bool raycastHit = GeNaEditorUtility.ShowSpawnRange(spawnerData, out RaycastHit hitInfo, m_showVisualizer);
            // If SHIFT is not held down, visualization will be of
            settings.VisualizationActive = false;
            #region Keyboard Handling
            GameObject lastSpawnedObject = spawnerData.LastSpawnedObject;
            //Keyboard handling
            if (lastSpawnedObject != null)
            {
                Transform lastSpawnedObjectTransform = lastSpawnedObject.transform;
                //Ctrl Left
                if (e.Equals(GeNaEditorUtility.Defaults.KeyLeftEvent(false, true)))
                {
                    GUIUtility.hotControl = controlID;
                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.left;
                    lastSpawnedObjectTransform.position += (movement * 0.05f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Shift Ctrl Left
                if (e.Equals(GeNaEditorUtility.Defaults.KeyLeftEvent(true, true)))
                {
                    GUIUtility.hotControl = controlID;
                    lastSpawnedObjectTransform.Rotate(Vector3.up, -1f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Ctrl right
                if (e.Equals(GeNaEditorUtility.Defaults.KeyRightEvent(false, true)))
                {
                    GUIUtility.hotControl = controlID;
                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.right;
                    lastSpawnedObjectTransform.position += (movement * 0.05f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Shift Ctrl Right
                if (e.Equals(GeNaEditorUtility.Defaults.KeyRightEvent(true, true)))
                {
                    GUIUtility.hotControl = controlID;
                    lastSpawnedObjectTransform.Rotate(Vector3.up, 1f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Ctrl Forward
                if (e.Equals(GeNaEditorUtility.Defaults.KeyForwardEvent(false, true)))
                {
                    GUIUtility.hotControl = controlID;
                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.forward;
                    lastSpawnedObjectTransform.position += (movement * 0.05f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Shift Ctrl Forward
                if (e.Equals(GeNaEditorUtility.Defaults.KeyForwardEvent(true, true)))
                {
                    GUIUtility.hotControl = controlID;
                    lastSpawnedObjectTransform.Translate(Vector3.up * 0.1f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Ctrl Backward
                if (e.Equals(GeNaEditorUtility.Defaults.KeyBackwardEvent(false, true)))
                {
                    GUIUtility.hotControl = controlID;
                    Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.back;
                    lastSpawnedObjectTransform.position += (movement * 0.05f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }

                //Shift Ctrl Backward
                if (e.Equals(GeNaEditorUtility.Defaults.KeyBackwardEvent(true, true)))
                {
                    GUIUtility.hotControl = controlID;
                    lastSpawnedObjectTransform.Translate(Vector3.down * 0.1f);
                    e.Use();
                    GUIUtility.hotControl = 0;
                    return;
                }
            }
            #endregion
            #region Hot Key Actions
            if (e.control && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.R)
                {
                    spawnerData.RandomGenerator.Reset();
                }
            }
            // Ctrl Delete Backspace
            if (e.Equals(GeNaEditorUtility.Defaults.KeyDeleteEvent(false, true)))
            {
                GUIUtility.hotControl = controlID;
                if (EditorUtility.DisplayDialog("WARNING!",
                        "Are you sure you want to delete all instances of prefabs referred to by this spawner from your scene?\n\n" +
                        "NOTE: This will also clear the Undo History.",
                        "OK", "Cancel"))
                {
                    GeNaSpawnerInternal.DespawnAllPrefabs(spawnerData);
                }
                e.Use();
                GUIUtility.hotControl = 0;
                GUIUtility.ExitGUI();
                return;
            }
            // Scroll wheel
            if (e.type == EventType.ScrollWheel)
            {
                if (e.control)
                {
                    Serialize(spawner);
                    int offset = (int)e.delta.y;
                    if (offset > 0)
                    {
                        spawnerData.MinInstances -= offset;
                        spawnerData.MaxInstances -= offset;
                    }
                    else
                    {
                        spawnerData.MaxInstances -= offset;
                        spawnerData.MinInstances -= offset;
                    }
                    e.Use();
                    //Settings changed, let's update ranges - probably no need to update child spawners, since their settings did not change.
                    GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, false);
                    onRepaint?.Invoke();
                    return;
                }
                if (e.shift)
                {
                    Serialize(spawner);
                    float scroll = e.delta.y;
                    float spawnRange = spawnerData.SpawnRange;
                    if (spawnerData.UseLargeRanges)
                        spawnRange -= scroll;
                    else
                        spawnRange = Mathf.Clamp(spawnRange - scroll, 0, 200);
                    spawnerData.SpawnRange = spawnRange;
                    //Settings changed, let's update ranges - probably no need to update child spawners, since their settings did not change.
                    SpawnCall spawnCall = new SpawnCall();
                    spawnCall.Normal = hitInfo.normal;
                    spawnCall.Location = hitInfo.point;
                    spawnCall.Target = hitInfo.transform;
                    GeNaSpawnerInternal.SetSpawnOrigin(spawnerData, spawnCall);
                    //Let's update ranges - including child spawners.
                    GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, false);
                    GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                    onRepaint?.Invoke();
                    e.Use();
                    return;
                }
            }

            // Check for the shift + ctrl + left mouse button event - spawn entire terrain
            if (e.shift && e.control && e.isMouse)
            {
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    if (raycastHit)
                    {
                        GUIUtility.hotControl = controlID;
                        GlobalSpawnDialog window = EditorWindow.GetWindow<GlobalSpawnDialog>(true, "GeNa Global Spawn", true);
                        window.Init(spawner, hitInfo);
                        e.Use();
                    }
                    return;
                }
                return;
            }
            #endregion
            bool mouseUp = false;
            // Check for the CTRL + LEFT Mouse Button event - Spawn
            if (e.control && e.isMouse)
            {
                // Left button
                if (e.button == 0)
                {
                    switch (e.type)
                    {
                        case EventType.MouseDown:
                            // Spawn Logic
                            GUIUtility.hotControl = controlID;
                            if (raycastHit)
                            {
                                // Generate a new spawn call on hit point
                                spawnerData.SpawnCalls.Clear();
                                SpawnCall spawnCall = GeNaSpawnerInternal.GenerateSpawnCall(spawnerData, hitInfo);
                                bool singleSpawnMode = spawnerData.SpawnMode != Constants.SpawnMode.Paint;
                                SpawnerEntry entry = new SpawnerEntry(spawner)
                                {
                                    Initialize = true,
                                    Title = "GeNa Spawner",
                                    Info = spawnerData.Name + " is Spawning...",
                                    RecordUndo = singleSpawnMode,
                                    RootSpawnCall = spawnCall
                                };
                                spawnCall.Target = hitInfo.transform;
                                spawnCall.Ground = hitInfo.transform;
                                GeNaSpawnerInternal.SetSpawnOrigin(spawnerData, spawnCall, true);
                                //Let's update ranges - including child spawners.
                                GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, hitInfo, true);
                                GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                                entry.AddSpawnCall(spawnCall);
                                GeNaEditorUtility.ScheduleSpawn(entry);
                                spawnerData.LastSpawnPosition = hitInfo.point;
                            }
                            onRepaint?.Invoke();
                            // Use Event
                            e.Use();
                            break;
                        case EventType.MouseDrag:
                            GUIUtility.hotControl = controlID;
                            if (raycastHit)
                            {
                                Vector3 direction = hitInfo.point - spawnerData.LastSpawnPosition;
                                if (direction.magnitude > spawnerData.FlowRate)
                                {
                                    // Paint Mode
                                    if (spawnerData.SpawnMode == Constants.SpawnMode.Paint)
                                    {
                                        SpawnCall spawnCall = GeNaSpawnerInternal.GenerateSpawnCall(spawnerData, hitInfo);
                                        SpawnerEntry entry = new SpawnerEntry(spawner)
                                        {
                                            Initialize = false,
                                            RecordUndo = false,
                                            RootSpawnCall = spawnCall
                                        };
                                        entry.AddSpawnCall(spawnCall);
                                        // Perform Spawn
                                        GeNaEditorUtility.ScheduleSpawn(entry);
                                        spawnerData.LastSpawnPosition = hitInfo.point;
                                    }
                                }
                            }
                            e.Use();
                            break;
                        case EventType.MouseUp:
                            mouseUp = true;
                            break;
                    }
                }
            }

            // Check Raw Events
            switch (e.rawType)
            {
                case EventType.MouseUp:
                    mouseUp = true;
                    spawnerData.VisualizationFixed = false;
                    break;
            }
            if (mouseUp)
            {
                bool paintedSpawn = spawnerData.SpawnMode == Constants.SpawnMode.Paint;
                if (paintedSpawn)
                {
                    GeNaEditorUtility.ScheduleSpawn(new SpawnerEntry(spawner)
                    {
                        Initialize = false,
                        RecordUndo = true,
                        Description = "Painted Spawn"
                    });
                    SceneView.RepaintAll();
                }
                mouseUp = false;
            }
            bool cancel = true;
            GeNaManager geNaManager = GeNaGlobalReferences.GeNaManagerInstance;
            if (geNaManager != null)
                cancel = geNaManager.Cancel;
            // Check for the CTRL + SHIFT + I - Iterate
            if (e.control &&
                e.shift &&
                e.type == EventType.KeyDown &&
                e.keyCode == KeyCode.I &&
                cancel)
            {
                // Disable Visualizer
                m_showVisualizer = false;
                spawner.Iterate();
                e.Use();
                return;
            }
            if (e.type == EventType.Repaint)
                GeNaSpawnerInternal.DrawVisualization(spawnerData);
            spawnerData.VisualizationActive = false;
            bool refreshVisualization = false;
            List<Prototype> spawnPrototypes = spawnerData.SpawnPrototypes;
            if (spawnPrototypes.Count == 1)
            {
                Prototype spawnPrototype = spawnPrototypes[0];
                if (spawnPrototype.Resources.Count == 1)
                {
                    if (geNaManager != null)
                    {
                        TerrainTools terrainTools = geNaManager.TerrainTools;
                        foreach (Prototype prototype in spawnPrototypes)
                        {
                            foreach (Resource resource in prototype.Resources)
                            {
                                foreach (IDecorator decorator in resource.Decorators)
                                {
                                    if (decorator is GeNaTerrainDecorator terrainDecorator)
                                    {
                                        TerrainModifier terrainModifier = terrainDecorator.TerrainModifier;
                                        terrainModifier.Position = spawnerData.SpawnOriginLocation;
                                        terrainModifier.RotationY = spawnerData.PlacementCriteria.MinRotationY;
                                        TerrainEntity terrainEntity = terrainModifier.GenerateTerrainEntity();
                                        if (terrainEntity != null)
                                        {
                                            terrainTools.Visualize(terrainEntity);
                                            terrainEntity.Dispose();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // Check for the SHIFT (show/move visualization) and SHIFT + left mouse events (update ranges + move visualisation - drag rotation)
            if (e.shift)
            {
                if (m_showVisualizer)
                    // SHIFT is down -> visualization is active
                    spawnerData.VisualizationActive = true;
                // If SHIFT and CONTROL both down, update the location of the visualization
                if (e.control && raycastHit)
                {
                    Serialize(spawner);
                    SpawnCall spawnCall = new SpawnCall();
                    spawnCall.Location = hitInfo.point;
                    spawnCall.Normal = hitInfo.normal;
                    spawnCall.Target = hitInfo.transform;
                    GeNaSpawnerInternal.SetSpawnOrigin(spawnerData, spawnCall);
                    //Let's update ranges - including child spawners.
                    GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, false);
                    GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                    refreshVisualization = true;
                    onRepaint?.Invoke();
                }
                if (e.isMouse)
                {
                    // Left button
                    if (e.button == 0)
                    {
                        switch (e.type)
                        {
                            case EventType.MouseDown:
                            {
                                GUIUtility.hotControl = controlID;
                                if (raycastHit)
                                {
                                    Serialize(spawner);
                                    SpawnCall spawnCall = new SpawnCall();
                                    spawnCall.Location = hitInfo.point;
                                    spawnCall.Normal = hitInfo.normal;
                                    spawnCall.Target = hitInfo.transform;
                                    spawnCall.Ground = hitInfo.transform;
                                    GeNaSpawnerInternal.SetSpawnOrigin(spawnerData, spawnCall, true);
                                    //Let's update ranges - including child spawners.
                                    GeNaSpawnerInternal.UpdateTargetSpawnerRanges(spawnerData, hitInfo, true);
                                    GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                                    onRepaint?.Invoke();
                                    refreshVisualization = true;
                                }
                                break;
                            }
                            case EventType.MouseDrag when GUIUtility.hotControl == controlID &&
                                                          placementCriteria.RotationAlgorithm == Constants.RotationAlgorithm.Fixed &&
                                                          placementCriteria.EnableRotationDragUpdate && raycastHit:
                            {
                                Serialize(spawner);
                                spawnerData.VisualizationFixed = true;
                                Quaternion rot = Quaternion.LookRotation(hitInfo.point - spawnerData.SpawnOriginLocation);
                                placementCriteria.MinRotationY = placementCriteria.MinRotationY = placementCriteria.MaxRotationY = rot.eulerAngles.y;
                                GeNaSpawnerInternal.UpdateVisualization(spawnerData);
                                onRepaint?.Invoke();
                                refreshVisualization = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (refreshVisualization)
            {
                GeNaSpawnerInternal.UpdateVisualization(spawnerData);
            }
            if (!e.shift || !e.control)
            {
                m_showVisualizer = true;
            }
        }
        protected void ExampleDragDropGUI(Rect dropArea)
        {
            // Cache References:
            Event currentEvent = Event.current;
            EventType currentEventType = currentEvent.type;

            // The DragExited event does not have the same mouse position data as the other events,
            // so it must be checked now:
            if (currentEventType == EventType.DragExited) DragAndDrop.PrepareStartDrag(); // Clear generic data when user pressed escape. (Unfortunately, DragExited is also called when the mouse leaves the drag area)
            if (!dropArea.Contains(currentEvent.mousePosition)) return;
            switch (currentEventType)
            {
                case EventType.MouseDown:
                    DragAndDrop.PrepareStartDrag(); // reset data
                    // CustomDragData dragData = new CustomDragData();
                    // dragData.originalIndex = somethingYouGotFromYourProperty;
                    // dragData.originalList = this.targetList;
                    // DragAndDrop.SetGenericData(dragDropIdentifier, dragData);
                    // Object[] objectReferences = new Object[1] {property.objectReferenceValue}; // Careful, null values cause exceptions in existing editor code.
                    // DragAndDrop.objectReferences = objectReferences; // Note: this object won't be 'get'-able until the next GUI event.
                    // currentEvent.Use();
                    break;
                case EventType.MouseDrag:
                    // // If drag was started here:
                    // CustomDragData existingDragData = DragAndDrop.GetGenericData(dragDropIdentifier) as CustomDragData;
                    // if (existingDragData != null)
                    // {
                    //     DragAndDrop.StartDrag("Dragging List ELement");
                    //     currentEvent.Use();
                    // }
                    break;
                case EventType.DragUpdated:
                    // if (IsDragTargetValid()) DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    // else DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    // currentEvent.Use();
                    break;
                case EventType.Repaint:
                    // if (
                    //     DragAndDrop.visualMode == DragAndDropVisualMode.None ||
                    //     DragAndDrop.visualMode == DragAndDropVisualMode.Rejected) break;
                    // EditorGUI.DrawRect(dropArea, Color.grey);
                    break;
                case EventType.DragPerform:
                    // DragAndDrop.AcceptDrag();
                    // CustomDragData receivedDragData = DragAndDrop.GetGenericData(dragDropIdentifier) as CustomDragData;
                    // if (receivedDragData != null receivedDragData.originalList == this.targetList) ReorderObject();
                    // else AddDraggedObjectsToList();
                    currentEvent.Use();
                    break;
                case EventType.MouseUp:
                    // // Clean up, in case MouseDrag never occurred:
                    // DragAndDrop.PrepareStartDrag();
                    break;
            }
        }
        private Color m_baseColor = Color.white;
        private Color m_backgroundColor = new Color(1f, 0.01f, 0f, 0.24f); 
        private Color m_proBackgroundColor = new Color(1f, 0.01f, 0f, 0.31f);
        private Color m_saveColor = new Color(0.47f, 0.72f, 0f);
        private Color m_discardColor = new Color(0.72f, 0.06f, 0f);
        private Color m_disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            #region Header
            m_editorUtils.GUIHeader();
            // Set the targetPreset
            m_spawner = (GeNaSpawner)target;
            GeNaEditorUtility.DisplayWarnings();
            m_editorUtils.GUINewsHeader();
            #endregion
            m_hasPrefabs = HasPrefabs(SpawnerData.SpawnPrototypes);
            m_hasTrees = HasTrees(SpawnerData.SpawnPrototypes);
            m_hasTextures = HasTextures(SpawnerData.SpawnPrototypes);
            if (!GeNaEditorUtility.ValidateComputeShader())
            {
                Color guiColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                EditorGUILayout.BeginVertical(Styles.box);
                m_editorUtils.Text("NoComputeShaderHelp");
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = guiColor;
                GUI.enabled = false;
            }
            #region Scroll
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, Styles.panel);
            {
                //Monitor for changes
                EditorGUI.BeginChangeCheck();
                {
                    if (GeNaEditorUtility.ValidateComputeShader())
                    {
                        GUI.enabled = !SpawnerData.IsProcessing;
                    }
                    #region Panels
                    m_showQuickStart = m_editorUtils.Panel("Quick Start", QuickStartPanel, m_showQuickStart);

                    Color backgroundColor = EditorGUIUtility.isProSkin ? m_proBackgroundColor : m_backgroundColor;
                    bool isPrefabAndTemp = m_isPartOfPrefab && SpawnerData.IsTemp; 
                    Color oldBackColor = GUI.backgroundColor;
                    GUI.backgroundColor = isPrefabAndTemp ? backgroundColor : oldBackColor;
                    // Overview Panel
                    GUIStyle overviewLabelStyle = Styles.panelLabel;
                    string overviewText = string.Format("{0} : {1}", m_editorUtils.GetTextValue("Overview Panel"), SpawnerData.Name);
                    GUIContent overviewPanelLabel = new GUIContent(overviewText, m_editorUtils.GetTooltip("Overview Panel"));
                    m_showOverview = m_editorUtils.Panel(overviewPanelLabel, OverviewPanel, overviewLabelStyle, m_showOverview);
                    if (m_spawner.Palette != null)
                    {
                        // Placement Criteria Panel
                        m_showPlacementCriteria = m_editorUtils.Panel("Placement Criteria Panel Label", PlacementCritPanel, m_showPlacementCriteria);
                        // Spawn Criteria Panel
                        m_showSpawnCriteria = m_editorUtils.Panel("Spawn Criteria Panel Label", SpawnCritPanel, m_showSpawnCriteria);
                        // Prototypes Panel
                        GUIContent protoPanelLabel = new GUIContent(string.Format("{0} ({1}) [{2}]", m_editorUtils.GetTextValue("Spawn Prototypes"), SpawnerData.SpawnPrototypes.Count, SpawnerData.InstancesSpawned), m_editorUtils.GetTooltip("Spawn Prototypes"));
                        m_showPrototypes = m_editorUtils.Panel(protoPanelLabel, PrototypesPanel, m_showPrototypes);
                        // Advanced Panel
                        m_showAdvancedSettings = m_editorUtils.Panel("Advanced Panel Label", AdvancedPanel, m_showAdvancedSettings);
                        // Add Panel
                        AddPrototypesPanel();
                    } 
                    GUI.backgroundColor = oldBackColor;
                    #endregion
                    if (GeNaEditorUtility.ValidateComputeShader())
                    {
                        GUI.enabled = true;
                    }
                }
                #region Change Check
                //Check for changes, make undo record, make changes and let editor know we are dirty
                if (EditorGUI.EndChangeCheck())
                {
                    Serialize();
                    // m_spawner.UpdateSpawnCritOverrides();
                    if (!GeNaEditorUtility.IsPrefab(m_spawner.gameObject))
                        m_spawner.UpdateGoName();
                    m_spawner.UpdateVisualization();
                    // Random Generator
                    m_spawner.UpdateRandom(SpawnerData.RandomSeed);
                    // Spawn Algorithm
                    if (PlacementCriteria.SpawnAlgorithm == Constants.LocationAlgorithm.Every && SpawnerData.ThrowDistance < 0.5f)
                        SpawnerData.ThrowDistance = 0.5f;
                    // Rotation Algorithm
                    switch (PlacementCriteria.RotationAlgorithm)
                    {
                        case Constants.RotationAlgorithm.Fixed:
                            PlacementCriteria.MaxRotationY = PlacementCriteria.MinRotationY;
                            break;
                        case Constants.RotationAlgorithm.LastSpawnCenter:
                        case Constants.RotationAlgorithm.LastSpawnClosest:
                            PlacementCriteria.MinRotationY = PlacementCriteria.MaxRotationY = 0f;
                            break;
                    }
                    // Min Rotation / Max Rotation
                    PlacementCriteria.MinRotationY = Mathf.Min(PlacementCriteria.MinRotationY, PlacementCriteria.MaxRotationY);
                    PlacementCriteria.MaxRotationY = Mathf.Max(PlacementCriteria.MinRotationY, PlacementCriteria.MaxRotationY);
                    // Spawn Height
                    if (SpawnCriteria.MaxSpawnHeight < SpawnCriteria.MinSpawnHeight)
                        SpawnCriteria.MaxSpawnHeight = SpawnCriteria.MinSpawnHeight;
                    if (SpawnCriteria.MaxSpawnHeight < SpawnCriteria.MinSpawnHeight)
                        SpawnCriteria.MinSpawnHeight = SpawnCriteria.MaxSpawnHeight;
                    // Mask Image
                    Vector3 minScale = PlacementCriteria.MinScale;
                    Vector3 maxScale = PlacementCriteria.MaxScale;
                    // Max could be pushing down min
                    minScale.x = Mathf.Min(minScale.x, maxScale.x);
                    // Min could be pushing up max
                    maxScale.x = Mathf.Max(minScale.x, maxScale.x);
                    // Max could be pushing down min
                    minScale.y = Mathf.Min(minScale.y, maxScale.y);
                    // Min could be pushing up max
                    maxScale.y = Mathf.Max(minScale.y, maxScale.y);
                    // Max could be pushing down min
                    minScale.z = Mathf.Min(minScale.z, maxScale.z);
                    // Min could be pushing up max
                    maxScale.z = Mathf.Max(minScale.z, maxScale.z);
                    PlacementCriteria.MinScale = minScale;
                    PlacementCriteria.MaxScale = maxScale;
                    // Handle sorting
                    if (SpawnerData.SortPrototypes)
                    {
                        EditorUtility.DisplayProgressBar("GeNa", "Sorting prototypes...", 0.5f);
                        m_spawner.SortPrototypesAZ();
                        EditorUtility.ClearProgressBar();
                    }
                    // Set name based on the first thing added
                    if (SpawnerData.SpawnPrototypes.Count == 0 && SpawnerData.SpawnPrototypes.Count > 0)
                    {
                        SpawnerData.Name = SpawnerData.SpawnPrototypes[0].Name;
                        m_spawner.UpdateGoName();
                    }
                    // Update their ID's
                    GeNaEditorUtility.UpdateResourceIDs(SpawnerData);
                    // Settings changed, let's update ranges - probably no need to update child spawners, since their settings did not change.
                    m_spawner.UpdateTargetSpawnerRanges(false);
                }
                if (m_spawner.IsDirty)
                {
                    Serialize(m_spawner);
                    m_spawner.IsDirty = false;
                }
                if (Event.current.type != EventType.Layout)
                    GeNaEditorUtility.ForceUpdate();
                #endregion
                GUILayout.Space(5);
            }
            EditorGUILayout.EndScrollView();
            #endregion
            m_editorUtils.GUINewsFooter(false);
        }
        public static void CreateTempSpawnerData(GeNaSpawner spawner)
        {
            GeNaSpawnerData spawnerData = spawner.SpawnerData;
            if (spawnerData.name.Contains("Main"))
            {
                spawner.SpawnerData = Instantiate(spawner.SpawnerData);
                spawner.SpawnerData.name = "Spawner Data (Temp)";
                spawner.Load();
            }
        }
        public void Serialize() => Serialize(m_spawner);
        public static void Serialize(GeNaSpawner spawner)
        {
            if (spawner == null)
                return;
            // If the spawner is part of a prefab AND the user has selected the Scene Instance.
            if (GeNaEditorUtility.IsPrefab(spawner.gameObject) && !AssetDatabase.Contains(spawner.gameObject))
            {
                CreateTempSpawnerData(spawner);
                PrefabUtility.RecordPrefabInstancePropertyModifications(spawner);
            }
            else
            {
                GeNaSpawnerData spawnerData = spawner.SpawnerData;
                EditorUtility.SetDirty(spawnerData);
                if (!Application.isPlaying)
                    EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene);
            }
            spawner.Load();
        }
        #endregion
        #region Core
        /// <summary>
        /// Ingest a resource tree or a single resource.
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="spawner">The spawner the resource tree belongs to.</param>
        /// <param name="parentResource">Null if a top level resource. Used by the method recursively to build the resource tree.</param>
        /// <param name="go">The object to be ingested as resource.</param>
        /// <param name="names">Reference to the nameset that's used to ensure unique resource names inside a prototype (May not be needed for resource trees).</param>
        /// <param name="protoBounds">Reference to Bounds for the whole prototype.</param>
        /// <param name="treeContainsPrefab">Does the tree contain a prefab?</param>
        /// <param name="structureIngestion"></param>
        /// <param name="currentResources"></param>
        /// <param name="maxResources"></param>
        /// <returns>Returns the top level resource of the tree.</returns>
        private void IngestResource(Prototype proto,
            GeNaSpawnerData spawner,
            Resource parentResource,
            GameObject go,
            ref HashSet<string> names,
            ref Bounds protoBounds,
            ref bool treeContainsPrefab,
            bool structureIngestion,
            int currentResources,
            int maxResources)
        {
            // Warn the user if it has more components than just the Transform since it's not a prefab.
            IDecorator[] decorators = go.GetComponents<IDecorator>();
            bool destroyUnpackedObject = false;
            bool unpackPrefab = false;
            if (decorators.Length > 0)
            {
                unpackPrefab = decorators.Any(item => item.UnpackPrefab) || decorators.Any(item => item is GeNaPrefabUnpackerDecorator);
                treeContainsPrefab = decorators.Any(item => item is GeNaSubSpawnerDecorator);
            }
            if (unpackPrefab)
            {
                EditorUtility.DisplayProgressBar("Ingesting Resources", $"Unpacking Prefab '{go.name}'", (float)currentResources / (float)maxResources);
                go = PrefabUnpackerUtility.ExecuteUnpackMasterGameObject(go);
                destroyUnpackedObject = true;
            }
            Resource res = new Resource(spawner);
            res.SetParent(parentResource);
            proto.AddResource(res);
            res.Name = GetUniqueName(go.name, ref names);
            EditorUtility.DisplayProgressBar("Ingesting Resources", $"Ingesting Resource '{res.Name}'", (float)currentResources / (float)maxResources);
            currentResources++;

            // Get bounds
            Bounds localBounds = GeNaUtility.GetInstantiatedBounds(go);
            localBounds.size = Vector3.Max(localBounds.size, Vector3.one);
            Bounds localColliderBounds = GeNaUtility.GetLocalObjectBounds(go);
            // If first time then set bounds up
            if (protoBounds.size == Vector3.zero)
                protoBounds = new Bounds(localBounds.center, localBounds.size);
            // Otherwise expand on it
            else
                protoBounds.Encapsulate(localBounds);
            // Get colliders
            res.HasRootCollider = GeNaUtility.HasRootCollider(go);
            res.HasColliders = GeNaUtility.HasColliders(go);
            res.HasMeshes = GeNaUtility.HasMeshes(go);
            res.HasRigidbody = GeNaUtility.HasRigidBody(go);
            Vector3 localPosition = go.transform.localPosition;
            Vector3 localEulerAngles = go.transform.localEulerAngles;
            // If top level resource
            if (parentResource == null)
            {
                res.BasePosition = Vector3.zero;
                res.BaseRotation = Vector3.zero;
                // Top level is not static by default, but descendants are
                res.SetStatic(proto, Constants.ResourceStatic.Dynamic);
                Vector3 basePosition = res.BasePosition;
                if (structureIngestion)
                {
                    //Offsets
                    //Using global positions for x and z because the offsets for structure ingestion will be 
                    //calculated by global bounds center.
                    basePosition.x = basePosition.x = localPosition.x;
                    basePosition.y = basePosition.y = localPosition.y;
                    basePosition.z = basePosition.z = localPosition.z;
                }
                else
                {
                    //Offsets
                    //If importing a single proto, it gets no offset
                    basePosition = Vector3.zero;
                }
                res.BasePosition = basePosition;
            }
            else
            {
                res.BasePosition = localPosition;
                res.BaseRotation = localEulerAngles;
            }
            if (GeNaUtility.ApproximatelyEqual(go.transform.localScale.x, go.transform.localScale.y, 0.000001f) &&
                GeNaUtility.ApproximatelyEqual(go.transform.localScale.x, go.transform.localScale.z, 0.000001f))
                res.SameScale = true;
            else
                res.SameScale = false;
            Vector3 localScale = go.transform.localScale;
            res.MinScale = res.MaxScale = localScale;
            res.BaseScale = localScale;
            res.BaseSize = localBounds.size;
            res.BaseColliderCenter = localColliderBounds.center;
            res.BaseColliderScale = localColliderBounds.size;
            res.BoundsCenter = localBounds.center;
            if (GeNaEditorUtility.IsPrefab(go))
            {
                GameObject prefabAsset = GeNaEditorUtility.GetPrefabAsset(go);
                if (prefabAsset == null)
                    prefabAsset = go;
                res.AddPrefab(prefabAsset, m_spawner.Palette);
            }
            // We can only determine if it is a prefab in the editor
            if (GeNaEditorUtility.IsPrefab(go))
            {
                if (res.Prefab != null)
                {
                    //We got a prefab here
                    treeContainsPrefab = true;
                    //Get its asset ID
                    string path = AssetDatabase.GetAssetPath(res.Prefab);
                    if (!string.IsNullOrEmpty(path))
                    {
                        res.AssetID = AssetDatabase.AssetPathToGUID(path);
                        res.AssetName = GeNaEditorUtility.GetAssetName(path);
                    }

                    //Get flags
                    SpawnFlags spawnFlags = res.SpawnFlags;
                    StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(res.Prefab);
                    spawnFlags.FlagBatchingStatic = (flags & StaticEditorFlags.BatchingStatic) == StaticEditorFlags.BatchingStatic;
                    spawnFlags.FlagLightmapStatic = (flags & StaticEditorFlags.ContributeGI) == StaticEditorFlags.ContributeGI;
                    spawnFlags.FlagOccludeeStatic = (flags & StaticEditorFlags.OccludeeStatic) == StaticEditorFlags.OccludeeStatic;
                    spawnFlags.FlagOccluderStatic = (flags & StaticEditorFlags.OccluderStatic) == StaticEditorFlags.OccluderStatic;
                    spawnFlags.FlagReflectionProbeStatic = (flags & StaticEditorFlags.ReflectionProbeStatic) == StaticEditorFlags.ReflectionProbeStatic;
      
#if !UNITY_2022_2_OR_NEWER
                    spawnFlags.FlagNavigationStatic = (flags & StaticEditorFlags.NavigationStatic) == StaticEditorFlags.NavigationStatic;
                    spawnFlags.FlagOffMeshLinkGeneration = (flags & StaticEditorFlags.OffMeshLinkGeneration) == StaticEditorFlags.OffMeshLinkGeneration;
#endif
                }
                else
                    GeNaDebug.LogErrorFormat("Unable to get prefab for '{0}'", res.Name);
                if (go.transform.childCount < res.Prefab.transform.childCount)
                    GeNaDebug.LogErrorFormat("What's going on here? The Prefab Instance seems to have less childs than the Prefab Asset: {0} < {1}", go.transform.childCount, res.Prefab.transform.childCount);
                else
                {
                    foreach (Transform child in go.transform)
                    {
                        if (PrefabUtility.IsPartOfAnyPrefab(child.gameObject))
                            continue;
                        // This GO or Prefab is not part of the Prefab that's being ingested. Let's process it.
                        IngestResource(proto,
                            spawner,
                            res,
                            child.gameObject,
                            ref names,
                            ref protoBounds,
                            ref treeContainsPrefab,
                            structureIngestion,
                            currentResources,
                            maxResources);
                    }
                }
            }
            // Else this is just a GO (container in the tree) not a prefab: Keep traversing the tree.
            else
            {
                res.ContainerOnly = true;
                // Keep traversing the tree.
                if (go.transform.childCount > 0)
                    foreach (Transform child in go.transform)
                        IngestResource(proto,
                            spawner,
                            res,
                            child.gameObject,
                            ref names,
                            ref protoBounds,
                            ref treeContainsPrefab,
                            structureIngestion,
                            currentResources,
                            maxResources);
            }

            // This needs to be here so prefabs can get added
            foreach (IDecorator decorator in decorators)
            {
                decorator.OnIngest(res);
            }
            foreach (IDecorator decorator in decorators)
                res.AddDecoratorEntry(decorator);
            res.DeserializeDecorators();
            if (destroyUnpackedObject)
            {
                GeNaEvents.Destroy(go);
            }
        }
        /// <summary>
        /// Draws a separator between secitons
        /// </summary>
        private void Separator(Rect widthRect)
        {
            GUILayout.Space(5f);
            Rect r = GUILayoutUtility.GetLastRect();
            Handles.BeginGUI();
            Color oldColor = Handles.color;
            Handles.color = m_separatorColor;
            Handles.DrawLine(new Vector3(widthRect.xMin, r.yMax), new Vector3(widthRect.xMax, r.yMax));
            Handles.color = oldColor;
            Handles.EndGUI();
        }
        /// <summary>
        /// Return the bounds of the supplied game object
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private Bounds GetBounds(GameObject go)
        {
            Bounds bounds = new Bounds(go.transform.position, Vector3.zero);
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
                bounds.Encapsulate(r.bounds);
            foreach (Collider c in go.GetComponentsInChildren<Collider>())
                bounds.Encapsulate(c.bounds);
            return bounds;
        }
        public void AddGameObject(GeNaSpawnerData spawner, GameObject gameObject) => AddGameObjects(spawner, new List<GameObject>() { gameObject });
        public void RefreshIngestion()
        {
            m_spawner.RemoveAllPrototypes();
            List<GameObject> prototypeRootPrefabs = m_spawner.PrototypeRootPrefabs;
            foreach (GameObject prototypeRootPrefab in prototypeRootPrefabs)
            {
                AddGameObjects(SpawnerData, new List<GameObject>() { prototypeRootPrefab }, false);
            }
            // Mark the Spawner as Dirty
            m_spawner.IsDirty = true;
            EditorUtility.SetDirty(m_spawner.Palette);
        }
        /// <summary>
        /// Add the game object from a list of prefabs instantiated as game objects.
        /// </summary>
        /// <param name="spawner">Spawner to add GameObjects to</param>
        /// <param name="ingestList">The list of resources, resource trees to generate the prototypes from.</param>
        /// <param name="updateCriteria"></param>
        public void AddGameObjects(GeNaSpawnerData spawner, List<GameObject> ingestList, bool updateCriteria = true)
        {
            if (ingestList == null || ingestList.Count < 1)
            {
                GeNaDebug.LogWarning("Can't add null or empty resource list");
                return;
            }
            if (spawner == null)
            {
                GeNaDebug.LogWarning("Can't add resources because spawner is missing");
                return;
            }
            bool ingestStructure = false;
            if (ingestList.Count > 1)
            {
                // Ask the user if they want to import a structure or just multiple individual items
                if (EditorUtility.DisplayDialog("GeNa Ingestion",
                        m_editorUtils.GetTextValue("Structure Ingestion Dialog Text"),
                        "Yes",
                        "No"))
                {
                    ingestStructure = true;
                    // Automatically set type to structured
                    spawner.SpawnType = Constants.SpawnerType.Random;
                    PlacementCriteria.RotationAlgorithm = Constants.RotationAlgorithm.Fixed;
                    PlacementCriteria.MaxRotationY = PlacementCriteria.MinRotationY = 0f;
                }
            }
            Bounds globalBounds = new Bounds();
            // Ingest prototypes
            foreach (GameObject go in ingestList)
            {
                if (go == null)
                    continue;
                // Used to track unique names in a prototype
                HashSet<string> names = new HashSet<string>();
                // Now add in the resource tree
                Bounds protoBounds = new Bounds();
                bool treeContainsPrefab = false;
                // Create and add the prototype
                Prototype proto = new Prototype(spawner)
                {
                    Name = go.name,
                    Size = protoBounds.size,
                    Extents = protoBounds.size * .5f,
                    ForwardRotation = 0f
                };
                proto.SetSpawner(m_spawner.SpawnerData);
                proto.SetPalette(m_spawner.Palette);

                // var palette = m_spawner.Palette;
                if (go != null)
                {
                    // if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(go, out string guid, out long localID))
                    // {
                    //     int id = palette.Add(go, guid);
                    //     if (palette.IsValidID(id))
                    //     {
                    if (!m_spawner.PrototypeRootPrefabs.Contains(go))
                        m_spawner.PrototypeRootPrefabs.Add(go);
                    //     }
                    // }
                }
                int currentResources = 0;
                int maxResources = 0;
                Transform[] children = go.GetComponentsInChildren<Transform>();
                maxResources = children.Length;

                // Ingest Resource
                IngestResource(proto, spawner, null, go, ref names, ref protoBounds, ref treeContainsPrefab, ingestStructure, currentResources, maxResources);
                EditorUtility.ClearProgressBar();

                // Add proto
                m_spawner.AddProto(proto);
                // If ingested several things (a structure), we want to set their m_position offset relative to the center of their collective bounds
                if (ingestStructure)
                {
                    Resource res = proto.Resources[0];
                    // Proto bounds are world origin based, let's adjust
                    protoBounds.center = new Vector3(protoBounds.center.x + res.MinOffset.x,
                        protoBounds.center.y + res.MinOffset.y,
                        protoBounds.center.z + res.MinOffset.z);
                    // If first time then set bounds up
                    if (globalBounds.size == Vector3.zero)
                        globalBounds = new Bounds(protoBounds.center, protoBounds.size);
                    // Otherwise expand on it
                    else
                        globalBounds.Encapsulate(protoBounds);
                }

                // If first one, then update some settings to be more prefab friendly
                if (spawner.SpawnPrototypes.Count == 1 && updateCriteria)
                {
                    // Activate bounds checking
                    SpawnCriteria.CheckCollisionType = Constants.VirginCheckType.Bounds;
                    PlacementCriteria.ScaleToNearestInt = false;
                    //m_spawner.ThrowDistance = Mathf.Min(proto.Size.x, proto.Size.z) * 2f;
                }
            }
            // If ingested several things, we want to set their m_position offset relative to the center of their collective bounds
            if (ingestStructure)
            {
                // Then process each resource
                foreach (Resource res in spawner.SpawnPrototypes.Select(proto => proto.Resources[0]))
                {
                    Vector3 basePosition = res.BasePosition;
                    basePosition.x = basePosition.x - globalBounds.center.x;
                    basePosition.z = basePosition.z - globalBounds.center.z;
                    res.BasePosition = basePosition;
                }
            }
            // Mark the Spawner as Dirty
            m_spawner.IsDirty = true;
            EditorUtility.SetDirty(m_spawner.Palette);
        }
        #endregion
        #region DropDown Menues
        private void ConformMenu()
        {
            int selection = 0;
            string[] optionsKeys = new[] { "Conform Dropdown", "Conform All", "Conform None" };
            selection = m_editorUtils.Popup(selection, optionsKeys, GUILayout.Width(70f));
            if (selection != 0)
            {
                switch (selection)
                {
                    case 1:
                        m_spawner.ForEachProtoResource(resource => resource.ConformToSlope = true);
                        break;
                    case 2:
                        m_spawner.ForEachProtoResource(resource => resource.ConformToSlope = false);
                        break;
                    default:
                        throw new NotImplementedException("[GeNa] No idea what was selected here: " + selection);
                }
                GUI.changed = true;
            }
        }
        private void SnapToGroundMenu()
        {
            int selection = 0;
            string[] optionsKeys = new[] { "Snap Dropdown", "Snap All", "Snap None" };
            selection = m_editorUtils.Popup(selection, optionsKeys);
            if (selection != 0)
            {
                switch (selection)
                {
                    case 1:
                        m_spawner.ForEachProtoResource(resource => resource.SnapToGround = true);
                        break;
                    case 2:
                        m_spawner.ForEachProtoResource(resource => resource.SnapToGround = false);
                        break;
                    default:
                        throw new NotImplementedException("[GeNa] No idea what was selected here: " + selection);
                }
                GUI.changed = true;
            }
        }
        #endregion
        #region Panels
        private void QuickStartPanel(bool helpEnabled)
        {
            if (m_showDetailedHelp)
            {
                m_editorUtils.Label("Visualise Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Visualise Cursor Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Rotation Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Range Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Instances Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Move Last Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Height&Rot Last Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Single Spawn Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Global Spawn Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Iterate Help", m_editorUtils.Styles.help);
                m_editorUtils.Label("Undo Help", m_editorUtils.Styles.help);
            }
            else
                EditorGUILayout.LabelField("Visualise: Shift + Left click.\nSingle Spawn: Ctrl + Left click.\nGlobal Spawn: Ctrl + Shift + Left click.", Styles.wrappedText);
            if (m_editorUtils.Button("View Tutorials Btn"))
                Application.OpenURL(PWApp.CONF.TutorialsLink);
        }
        /// <summary>
        /// Overview Panel
        /// </summary>
        private void OverviewPanel(bool helpEnabled)
        {
            float spawnRange = SpawnerData.SpawnRange;
            m_editorUtils.InlineHelp("Overview Panel", helpEnabled);
            bool canSaveAndDiscard = SpawnerData.IsTemp && GeNaEditorUtility.IsPrefab(m_spawner.gameObject) &&
                                     !AssetDatabase.Contains(m_spawner.gameObject); 
            GUI.enabled = canSaveAndDiscard;
            {
                EditorGUILayout.BeginHorizontal();
                {
                    Color oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = canSaveAndDiscard ? m_saveColor : m_disabledColor;
                    GUIContent saveToPrefabContent = new GUIContent("Save Changes");
                    if (GUILayout.Button(saveToPrefabContent, Styles.cancelBtn, GUILayout.MaxHeight(25f)))
                    {
                        SaveChanges(m_spawner);
                        GUI.changed = false;
                    }
                    GUI.backgroundColor = oldColor;
                    GUIContent revertToPrefab = new GUIContent("Discard Changes");
                    oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = canSaveAndDiscard ? m_discardColor : m_disabledColor;
                    if (GUILayout.Button(revertToPrefab, Styles.cancelBtn, GUILayout.MaxHeight(25f)))
                    {
                        DiscardChanges(m_spawner);
                        GUI.changed = false;
                    }
                    GUI.backgroundColor = oldColor;
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("Spawner Changes", helpEnabled);
            }
            
            EditorGUILayout.Space(1f);
            
            GUI.enabled = true;
            bool cancel = true;
            GeNaManager geNaManager = GeNaGlobalReferences.GeNaManagerInstance;
            if (geNaManager != null)
                cancel = geNaManager.Cancel;
            if (!cancel)
            {
                if (GeNaEditorUtility.ValidateComputeShader())
                {
                    GUI.enabled = true;
                }
                GUIContent cancelContent = new GUIContent("Cancel \u00D7");
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = m_discardColor;
                if (GUILayout.Button(cancelContent, Styles.cancelBtn, GUILayout.MaxHeight(25f)))
                {
                    if (geNaManager != null)
                        geNaManager.Cancel = true;
                    //GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = oldColor;
                GUI.enabled = false;
            }
            EditorGUI.BeginChangeCheck();
            {
                Constants.SpawnMode spawnMode = (Constants.SpawnMode)SpawnerData.SpawnMode;
                // Controls
                if (Settings.Advanced.DebugEnabled)
                {
                    EditorGUILayout.LabelField($"Version: {m_spawner.Version}");
                }
                EditorGUILayout.BeginHorizontal();
                m_spawner.Palette = (Palette)m_editorUtils.ObjectField("Palette", m_spawner.Palette, typeof(Palette), false);
                if (m_editorUtils.Button("NewPalette", GUILayout.MaxWidth(40f)))
                {
                    m_spawner.Palette = CreatePalette();
                    EditorUtility.SetDirty(m_spawner);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("Palette", helpEnabled);
                if (m_spawner.Palette != null)
                {
                    //m_spawner.SpawnerData = (GeNaSpawnerData) EditorGUILayout.ObjectField("Spawner Data", m_spawner.SpawnerData, typeof(GeNaSpawnerData), false);
                    SpawnerData.Name = m_editorUtils.TextField("Spawner Name", SpawnerData.Name, helpEnabled);
                    SpawnerData.SpawnType = (Constants.SpawnerType)m_editorUtils.EnumPopup("Spawner Type", SpawnerData.SpawnType, helpEnabled);
                    //m_editorUtils.PropertyField("Physics Type", m_physicsModeProperty, helpEnabled);
                    if (SpawnerData.HasActivePhysicsProtosRecursive())
                    {
                        SpawnerData.PhysicsType = (Constants.PhysicsType)m_editorUtils.EnumPopup("Physics Type", SpawnerData.PhysicsType, helpEnabled);
                        if (SpawnerData.PhysicsType == Constants.PhysicsType.Spawner)
                        {
                            EditorGUI.indentLevel++;
                            PhysicsSimulatorSettings physicsSettings = SpawnerData.PhysicsSettings;
                            physicsSettings.Iterations = m_editorUtils.IntField("Iterations", physicsSettings.Iterations, helpEnabled);
                            physicsSettings.Speed = m_editorUtils.FloatField("Speed", physicsSettings.Speed, helpEnabled);
                            physicsSettings.EmbedOffsetY = m_editorUtils.Slider("Embed Offset Y", physicsSettings.EmbedOffsetY, -5f, 5f, helpEnabled);
                            physicsSettings.MinHeightY = m_editorUtils.FloatField("Min Height Y", physicsSettings.MinHeightY, helpEnabled);
                            EditorGUI.indentLevel--;
                        }
                    }
                    SpawnerData.SpawnMode = (Constants.SpawnMode)m_editorUtils.EnumPopup("Spawn Mode", SpawnerData.SpawnMode, helpEnabled);
                    if (!SpawnerData.UseLargeRanges)
                    {
                        if ((Constants.SpawnMode)SpawnerData.SpawnMode > Constants.SpawnMode.Single)
                        {
                            EditorGUI.indentLevel += 1;
                            SpawnerData.FlowRate = m_editorUtils.Slider("Flow Rate", SpawnerData.FlowRate, 0.01f * spawnRange, 5f * spawnRange, helpEnabled);
                            EditorGUI.indentLevel -= 1;
                        }
                        SpawnerData.SpawnRangeShape = (Constants.SpawnRangeShape)m_editorUtils.EnumPopup("Spawn Shape", SpawnerData.SpawnRangeShape, helpEnabled);
                        SpawnerData.RandomSeed = m_editorUtils.IntField("Spawn Seed", SpawnerData.RandomSeed, helpEnabled);
                        spawnRange = m_editorUtils.Slider("Spawn Range", spawnRange, 0f, 200f, helpEnabled);
                        SpawnerData.ThrowDistance = m_editorUtils.Slider("Throw Distance", SpawnerData.ThrowDistance, 0f, spawnRange, helpEnabled);
                        int instanceLimit = m_instanceTopLimit;
                        float minInstances = (float)SpawnerData.MinInstances;
                        float maxInstances = (float)SpawnerData.MaxInstances;
                        m_editorUtils.MinMaxSliderWithFields("Instances", ref minInstances, ref maxInstances, 1, instanceLimit, helpEnabled);
                        int minInstancesInt = Mathf.RoundToInt(minInstances);
                        int maxInstancesInt = Mathf.RoundToInt(maxInstances);
                        // Min Instances Changed
                        if (SpawnerData.MinInstances != minInstancesInt)
                        {
                            if (minInstances > SpawnerData.MaxInstances)
                                SpawnerData.MaxInstances = minInstancesInt;
                            SpawnerData.MinInstances = Mathf.RoundToInt(minInstances);
                        }
                        // Max Instances Changed
                        if (SpawnerData.MaxInstances != maxInstancesInt)
                        {
                            if (maxInstances < SpawnerData.MinInstances)
                                SpawnerData.MinInstances = maxInstancesInt;
                            SpawnerData.MaxInstances = Mathf.RoundToInt(maxInstances);
                        }
                    }
                    else
                    {
                        if (spawnMode > Constants.SpawnMode.Single)
                        {
                            EditorGUI.indentLevel += 1;
                            SpawnerData.FlowRate = m_editorUtils.FloatField("Flow Rate", SpawnerData.FlowRate, helpEnabled);
                            EditorGUI.indentLevel -= 1;
                        }
                        SpawnerData.SpawnRangeShape = (Constants.SpawnRangeShape)m_editorUtils.EnumPopup("Spawn Shape", SpawnerData.SpawnRangeShape, helpEnabled);
                        SpawnerData.RandomSeed = m_editorUtils.IntField("Spawn Seed", SpawnerData.RandomSeed, helpEnabled);
                        spawnRange = m_editorUtils.FloatField("Spawn Range", spawnRange, helpEnabled);
                        SpawnerData.ThrowDistance = m_editorUtils.FloatField("Throw Distance", SpawnerData.ThrowDistance, helpEnabled);
                        SpawnerData.MinInstances = m_editorUtils.LongField("Min Instances", SpawnerData.MinInstances, helpEnabled);
                        SpawnerData.MaxInstances = m_editorUtils.LongField("Max Instances", SpawnerData.MaxInstances, helpEnabled);
                    }
                    SpawnerData.SpawnToGround = m_editorUtils.Toggle("Spawn to Ground", SpawnerData.SpawnToGround, helpEnabled);
                    if (m_hasPrefabs)
                    {
                        // spawnerData.MergeSpawns = m_editorUtils.Toggle("Merge Instances", spawnerData.MergeSpawns, helpEnabled);
                        SpawnerData.SpawnToTarget = m_editorUtils.Toggle("Spawn to Target", SpawnerData.SpawnToTarget, helpEnabled);
                    }
                    SpawnerData.SpawnTimeInterval = m_editorUtils.Slider("Spawn Time Interval", SpawnerData.SpawnTimeInterval, 0f, 1000f, helpEnabled);
                    SpawnerData.UseLargeRanges = m_editorUtils.Toggle("Use Large Ranges", SpawnerData.UseLargeRanges, helpEnabled);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Serialize();
                SpawnerData.SpawnRange = spawnRange;
                m_spawner.IsDirty = true;
            }
        }
        private void PlacementCritPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                PlacementCriteria placementCriteria = SpawnerData.PlacementCriteria;
                Constants.LocationAlgorithm spawnAlgorithm = placementCriteria.SpawnAlgorithm;
                int maxFailCount = placementCriteria.MaxFailCount;
                AnimationCurve scaleFalloff = placementCriteria.ScaleFalloff;
                float seedThrowJitter = placementCriteria.SeedThrowJitter;
                Constants.RotationAlgorithm rotationAlgorithm = placementCriteria.RotationAlgorithm;
                float minRotationY = placementCriteria.MinRotationY;
                float maxRotationY = placementCriteria.MaxRotationY;
                bool enableRotationDragUpdate = placementCriteria.EnableRotationDragUpdate;
                m_editorUtils.Label("Control how and where we can spawn.", Styles.wrappedText);
                spawnAlgorithm = (Constants.LocationAlgorithm)m_editorUtils.EnumPopup("Spawn Type", spawnAlgorithm, helpEnabled);
                EditorGUI.indentLevel++;
                switch (spawnAlgorithm)
                {
                    case Constants.LocationAlgorithm.Every:
                    {
                        seedThrowJitter = m_editorUtils.Slider("Jitter Strength", seedThrowJitter, 0f, 1f, helpEnabled);
                        break;
                    }
                    case Constants.LocationAlgorithm.Organic:
                    {
                        maxFailCount = m_editorUtils.IntField("Max Fail Count", maxFailCount, helpEnabled);
                        scaleFalloff = m_editorUtils.CurveField("Scale Falloff", scaleFalloff, helpEnabled);
                        break;
                    }
                }
                EditorGUI.indentLevel--;
                if (m_hasPrefabs || m_hasTrees || (SpawnCriteria.CheckMask && SpawnCriteria.CheckMaskType == Constants.MaskType.Image))
                {
                    rotationAlgorithm = (Constants.RotationAlgorithm)m_editorUtils.EnumPopup("Rotation Type", rotationAlgorithm, helpEnabled);
                    switch (rotationAlgorithm)
                    {
                        case Constants.RotationAlgorithm.Ranged:
                            EditorGUI.indentLevel++;
                            m_editorUtils.MinMaxSliderWithFields("Rotation", ref minRotationY, ref maxRotationY, 0f, 360f, helpEnabled);
                            EditorGUI.indentLevel--;
                            break;
                        case Constants.RotationAlgorithm.Fixed:
                            EditorGUI.indentLevel++;
                            minRotationY = m_editorUtils.Slider("Fixed Rotation", minRotationY, 0f, 360f, helpEnabled);
                            enableRotationDragUpdate = m_editorUtils.Toggle("Draggable Rotation", enableRotationDragUpdate, helpEnabled);
                            EditorGUI.indentLevel--;
                            break;
                        case Constants.RotationAlgorithm.LastSpawnCenter:
                            break;
                        case Constants.RotationAlgorithm.LastSpawnClosest:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    PlacementScale(PlacementCriteria, helpEnabled);
                }
                else if (m_hasTextures)
                    PlacementScale(PlacementCriteria, helpEnabled);
                placementCriteria.SpawnAlgorithm = spawnAlgorithm;
                placementCriteria.MaxFailCount = maxFailCount;
                placementCriteria.ScaleFalloff = scaleFalloff;
                placementCriteria.SeedThrowJitter = seedThrowJitter;
                placementCriteria.RotationAlgorithm = rotationAlgorithm;
                placementCriteria.MinRotationY = minRotationY;
                placementCriteria.MaxRotationY = maxRotationY;
                placementCriteria.EnableRotationDragUpdate = enableRotationDragUpdate;
            }
            if (EditorGUI.EndChangeCheck())
            {
                Serialize();
                m_spawner.IsDirty = true;
            }
        }
        private void PlacementScale(PlacementCriteria placementCriteria, bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                bool sameScale = placementCriteria.SameScale;
                bool scaleToNearestInt = placementCriteria.ScaleToNearestInt;
                Vector3 minScale = placementCriteria.MinScale;
                Vector3 maxScale = placementCriteria.MaxScale;
                sameScale = m_editorUtils.Toggle("Same Scale XYZ", sameScale, helpEnabled);
                EditorGUI.indentLevel++;
                if (sameScale)
                {
                    if (scaleToNearestInt)
                    {
                        int min = (int)minScale.x;
                        int max = (int)maxScale.x;
                        m_editorUtils.MinMaxSliderWithFields("Scale", ref min, ref max, 1, 100, helpEnabled);
                        minScale.x = min;
                        maxScale.x = max;
                    }
                    else
                    {
                        m_editorUtils.MinMaxSliderWithFields("Scale", ref minScale.x, ref maxScale.x, 0.1f, 100f, helpEnabled);
                    }
                }
                else
                {
                    if (scaleToNearestInt)
                    {
                        minScale.x = m_editorUtils.IntSlider("Min Scale X", (int)minScale.x, 1, 1000, helpEnabled);
                        maxScale.x = m_editorUtils.IntSlider("Max Scale X", (int)maxScale.x, 1, 1000, helpEnabled);
                        minScale.y = m_editorUtils.IntSlider("Min Scale Y", (int)minScale.y, 1, 1000, helpEnabled);
                        maxScale.y = m_editorUtils.IntSlider("Max Scale Y", (int)maxScale.y, 1, 1000, helpEnabled);
                        minScale.z = m_editorUtils.IntSlider("Min Scale Z", (int)minScale.z, 1, 1000, helpEnabled);
                        maxScale.z = m_editorUtils.IntSlider("Max Scale Z", (int)maxScale.z, 1, 1000, helpEnabled);
                    }
                    else
                    {
                        minScale.x = m_editorUtils.Slider("Min Scale X", minScale.x, 0.1f, 1000f, helpEnabled);
                        maxScale.x = m_editorUtils.Slider("Max Scale X", maxScale.x, 0.1f, 1000f, helpEnabled);
                        minScale.y = m_editorUtils.Slider("Min Scale Y", minScale.y, 0.1f, 1000f, helpEnabled);
                        maxScale.y = m_editorUtils.Slider("Max Scale Y", maxScale.y, 0.1f, 1000f, helpEnabled);
                        minScale.z = m_editorUtils.Slider("Min Scale Z", minScale.z, 0.1f, 1000f, helpEnabled);
                        maxScale.z = m_editorUtils.Slider("Max Scale Z", maxScale.z, 0.1f, 1000f, helpEnabled);
                    }
                }
                placementCriteria.SameScale = sameScale;
                placementCriteria.ScaleToNearestInt = scaleToNearestInt;
                placementCriteria.MinScale = minScale;
                placementCriteria.MaxScale = maxScale;
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck())
            {
                Serialize();
                m_spawner.IsDirty = true;
            }
        }
        private void SpawnCritPanel(bool helpEnabled)
        {
            SpawnCriteria spawnCriteria = SpawnerData.SpawnCriteria;
            bool forceSpawn = spawnCriteria.ForceSpawn;
            LayerMask groundLayer = SpawnerData.GroundLayer;
            LayerMask spawnCollisionLayers = spawnCriteria.SpawnCollisionLayers;
            Constants.VirginCheckType virginCheckType = spawnCriteria.CheckCollisionType;
            float blendAmount = spawnCriteria.BlendAmount;
            float boundsExtents = spawnCriteria.BoundsExtents;
            Constants.CriteriaRangeType checkHeightType = spawnCriteria.CheckHeightType;
            bool getSeaLevel = SpawnerData.GetSeaLevel;
            float seaLevel = spawnCriteria.SeaLevel;
            float minSpawnHeight = spawnCriteria.MinSpawnHeight;
            float maxSpawnHeight = spawnCriteria.MaxSpawnHeight;
            float minHeight = spawnCriteria.MinHeight;
            float maxHeight = spawnCriteria.MaxHeight;
            float heightVariance = spawnCriteria.HeightRange;
            float bottomBoundary = spawnCriteria.BottomBoundary;
            float topBoundary = spawnCriteria.TopBoundary;
            // Check Slopes
            Constants.CriteriaRangeType checkSlopeTypeProperty = spawnCriteria.CheckSlopeType;
            float minSpawnSlopeProperty = spawnCriteria.MinSpawnSlope;
            float maxSpawnSlopeProperty = spawnCriteria.MaxSpawnSlope;
            float minSlopeProperty = spawnCriteria.MinSlope;
            float maxSlopeProperty = spawnCriteria.MaxSlope;
            float slopeRangeProperty = spawnCriteria.SlopeRange;
            // Check Textures
            bool checkTextures = spawnCriteria.CheckTextures;
            int selectedTextureIdxProperty = spawnCriteria.SelectedTextureIdx;
            string selectedTextureNameProperty = spawnCriteria.SelectedTextureName;
            float textureStrengthProperty = spawnCriteria.TextureStrength;
            float textureVarianceProperty = spawnCriteria.TextureRange;
            // Check Mask
            bool checkMask = spawnCriteria.CheckMask;
            Constants.MaskType maskType = spawnCriteria.CheckMaskType;
            // Mask Fractal
            Fractal maskFractal = spawnCriteria.MaskFractal;
            float midMaskFractal = spawnCriteria.MidMaskFractal;
            float maskFractalRange = spawnCriteria.MaskFractalRange;
            bool maskInvert = spawnCriteria.MaskInvert;
            Texture2D maskImage = spawnCriteria.MaskImage;
            Color imageFilterColor = spawnCriteria.ImageFilterColor;
            float imageFilterFuzzyMatch = spawnCriteria.ImageFilterFuzzyMatch;
            bool constrainWithinMaskedBounds = spawnCriteria.ConstrainWithinMaskedBounds;
            bool invertMaskedAlpha = spawnCriteria.InvertMaskedAlpha;
            bool successOnMaskedAlpha = spawnCriteria.SuccessOnMaskedAlpha;
            bool scaleOnMaskedAlpha = spawnCriteria.ScaleOnMaskedAlpha;
            float minScaleOnMaskedAlpha = spawnCriteria.MinScaleOnMaskedAlpha;
            float maxScaleOnMaskedAlpha = spawnCriteria.MaxScaleOnMaskedAlpha;
            m_editorUtils.Label("Control when we can spawn.", Styles.wrappedText);
            EditorGUI.BeginChangeCheck();
            {
                forceSpawn = m_editorUtils.Toggle("Force Spawn", forceSpawn, helpEnabled);
                if (GeNaEditorUtility.ValidateComputeShader())
                {
                    if (!SpawnerData.IsProcessing)
                        GUI.enabled = !forceSpawn;
                }
                #region Check Collisions
                groundLayer = m_editorUtils.LayerMaskField("Ground Layer", groundLayer, helpEnabled);
                virginCheckType = (Constants.VirginCheckType)m_editorUtils.EnumPopup("Check Collisions", virginCheckType, helpEnabled);
                if (virginCheckType != Constants.VirginCheckType.None)
                {
                    EditorGUI.indentLevel++;
                    spawnCollisionLayers = m_editorUtils.LayerMaskField("Collision Layers", spawnCollisionLayers, helpEnabled);
                    EditorGUI.indentLevel--;
                    if (virginCheckType == Constants.VirginCheckType.Bounds)
                    {
                        EditorGUI.indentLevel++;
                        blendAmount = m_editorUtils.Slider("Blend Amount", blendAmount, 0.001f, 10f, helpEnabled);
                        boundsExtents = m_editorUtils.Slider("Bounds Extents", boundsExtents, 0.0f, 100f, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    m_editorUtils.InlineHelp("Collision Layers", helpEnabled);
                }
                #endregion
                #region Check Height Type
                // Height
                checkHeightType = (Constants.CriteriaRangeType)m_editorUtils.EnumPopup("Check Height Type", checkHeightType, helpEnabled);
                if (checkHeightType != Constants.CriteriaRangeType.None)
                {
                    EditorGUI.indentLevel++;
                    getSeaLevel = m_editorUtils.Toggle("SeaLevel", getSeaLevel, helpEnabled);
                    if (getSeaLevel)
                    {
                        EditorGUILayout.HelpBox("'Get Sea Level' is enabled this will get the sea level from Gaia and add it to the spawn min height. You can change the sea level from the gaia water object in the scene.", MessageType.Info);
                        // EditorGUI.indentLevel++;
                        // seaLevel = m_editorUtils.FloatField("ExtraSeaLevel", seaLevel);
                        // EditorGUI.indentLevel--;
                    }
                    if (SpawnerData.UseLargeRanges)
                    {
                        switch (checkHeightType)
                        {
                            case Constants.CriteriaRangeType.Range:
                            case Constants.CriteriaRangeType.MinMax:
                                if (GeNaUtility.Gaia2Present)
                                {
                                    minSpawnHeight = m_editorUtils.FloatField("Min Height", minSpawnHeight, helpEnabled);
                                }
                                else
                                    minSpawnHeight = m_editorUtils.FloatField("Min Height", minSpawnHeight, helpEnabled);
                                maxSpawnHeight = m_editorUtils.FloatField("Max Height", maxSpawnHeight, helpEnabled);
                                break;
                            default:
                                m_editorUtils.LabelField("Min Height", new GUIContent(minSpawnHeight.ToString(CultureInfo.InvariantCulture)), helpEnabled);
                                m_editorUtils.LabelField("Max Height", new GUIContent(maxSpawnHeight.ToString(CultureInfo.InvariantCulture)), helpEnabled);
                                break;
                        }
                        switch (checkHeightType)
                        {
                            case Constants.CriteriaRangeType.Range:
                            case Constants.CriteriaRangeType.Mixed:
                                heightVariance = m_editorUtils.FloatField("Height Range", heightVariance, helpEnabled);
                                break;
                        }
                    }
                    else
                    {
                        bool oldEnabled = GUI.enabled;
                        float minValue = minHeight;
                        float maxValue = maxHeight;
                        float minSpawnValue = minSpawnHeight;
                        float maxSpawnValue = maxSpawnHeight;
                        float minLimit = bottomBoundary;
                        float maxLimit = topBoundary;
                        if (!SpawnerData.IsProcessing)
                            if (GeNaEditorUtility.ValidateComputeShader())
                            {
                                GUI.enabled = !forceSpawn && checkHeightType >= Constants.CriteriaRangeType.MinMax;
                            }
                        if (checkHeightType >= Constants.CriteriaRangeType.MinMax)
                        {
                            GUI.enabled = checkHeightType != Constants.CriteriaRangeType.Mixed;
                            m_editorUtils.MinMaxSliderWithFields("Min Max Spawn Height", ref minSpawnValue, ref maxSpawnValue, minLimit, maxLimit, helpEnabled);
                            GUI.enabled = oldEnabled;
                        }
                        if (checkHeightType != Constants.CriteriaRangeType.MinMax)
                        {
                            GUI.enabled = false;
                            Color oldColor = GUI.color;
                            if (checkHeightType == Constants.CriteriaRangeType.Mixed)
                            {
                                if (maxSpawnValue <= minValue || minSpawnValue >= maxValue)
                                    GUI.color = Color.red;
                                m_editorUtils.MinMaxSliderWithFields("Min Max Height", ref minValue, ref maxValue, minLimit, maxLimit, helpEnabled);
                            }
                            else
                                m_editorUtils.MinMaxSliderWithFields("Min Max Spawn Height", ref minSpawnValue, ref maxSpawnValue, minLimit, maxLimit, helpEnabled);
                            GUI.color = oldColor;
                        }
                        if (!SpawnerData.IsProcessing)
                            if (GeNaEditorUtility.ValidateComputeShader())
                            {
                                GUI.enabled = !forceSpawn;
                            }
                        switch (checkHeightType)
                        {
                            case Constants.CriteriaRangeType.Range:
                            case Constants.CriteriaRangeType.Mixed:
                                heightVariance = m_editorUtils.Slider("Height Range", heightVariance, 0.1f, 200f, helpEnabled);
                                break;
                        }
                        minHeight = minValue;
                        maxHeight = maxValue;
                        minSpawnHeight = minSpawnValue;
                        maxSpawnHeight = maxSpawnValue;
                        bottomBoundary = minLimit;
                        topBoundary = maxLimit;
                    }
                    Settings.ShowCritMinSpawnHeight = Settings.ShowCritMaxSpawnHeight = m_editorUtils.Toggle("Visualize", Settings.ShowCritMinSpawnHeight, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                #endregion
                #region Check Slope
                // Check Slope
                checkSlopeTypeProperty = (Constants.CriteriaRangeType)m_editorUtils.EnumPopup("Check Slope Type", checkSlopeTypeProperty, helpEnabled);
                if (checkSlopeTypeProperty != Constants.CriteriaRangeType.None)
                {
                    EditorGUI.indentLevel++;
                    float minValue = minSlopeProperty;
                    float maxValue = maxSlopeProperty;
                    float minSpawnValue = minSpawnSlopeProperty;
                    float maxSpawnValue = maxSpawnSlopeProperty;
                    float minLimit = 0f;
                    float maxLimit = 90f;
                    if (!SpawnerData.IsProcessing)
                        if (GeNaEditorUtility.ValidateComputeShader())
                        {
                            GUI.enabled = !forceSpawn && checkSlopeTypeProperty >= Constants.CriteriaRangeType.MinMax;
                        }
                    if (checkSlopeTypeProperty >= Constants.CriteriaRangeType.MinMax)
                        m_editorUtils.MinMaxSliderWithFields("Min Max Slope", ref minValue, ref maxValue, minLimit, maxLimit, helpEnabled);
                    if (checkSlopeTypeProperty != Constants.CriteriaRangeType.MinMax)
                    {
                        GUI.enabled = false;
                        Color oldColor = GUI.color;
                        if (checkSlopeTypeProperty == Constants.CriteriaRangeType.Mixed)
                            if (maxSpawnValue <= minValue || minSpawnValue >= maxValue)
                                GUI.color = Color.red;
                        m_editorUtils.MinMaxSliderWithFields("Min Max Spawn Slope", ref minSpawnValue, ref maxSpawnValue, minLimit, maxLimit, helpEnabled);
                        GUI.color = oldColor;
                    }
                    if (!SpawnerData.IsProcessing)
                        if (GeNaEditorUtility.ValidateComputeShader())
                        {
                            GUI.enabled = !forceSpawn;
                        }
                    minSlopeProperty = minValue;
                    maxSlopeProperty = maxValue;
                    minSpawnSlopeProperty = minSpawnValue;
                    maxSpawnSlopeProperty = maxSpawnValue;
                    switch (checkSlopeTypeProperty)
                    {
                        case Constants.CriteriaRangeType.Range:
                        case Constants.CriteriaRangeType.Mixed:
                            slopeRangeProperty = m_editorUtils.Slider("Slope Range", slopeRangeProperty, 0.1f, 90f, helpEnabled);
                            break;
                    }
                    EditorGUI.indentLevel--;
                    minSlopeProperty = minValue;
                    maxSlopeProperty = maxValue;
                    minSpawnSlopeProperty = minSpawnValue;
                    maxSpawnSlopeProperty = maxSpawnValue;
                }
                #endregion
                if (SpawnerData.SpawnOriginIsTerrain)
                {
                    bool oldEnabled = GUI.enabled;
                    Constants.Processor currentProcessor = Settings.Advanced.CurrentProcessor;
                    if (currentProcessor == Constants.Processor.CPU || !SystemInfo.supportsComputeShaders)
                    {
                        EditorGUILayout.HelpBox("Warning: The following settings are unavailable when using CPU, please consider using a machine with a GPU.", MessageType.Warning);
                        GUI.enabled = false;
                    }
                    #region Check Textures
                    checkTextures = m_editorUtils.Toggle("Check Textures", checkTextures, helpEnabled);
                    if (checkTextures)
                    {
                        EditorGUI.indentLevel++;
                        Terrain terrain = Terrain.activeTerrain;
                        if (terrain != null)
                        {
                            TerrainData terrainData = terrain.terrainData;
                            if (terrainData != null)
                            {
                                int alphamapLayers = terrainData.alphamapLayers;
                                TerrainLayer[] terrainLayers = terrainData.terrainLayers;
                                if (terrainLayers != null)
                                {
                                    GUIContent[] assetChoices = new GUIContent[alphamapLayers];
                                    for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                                    {
                                        string name = "Error";
                                        TerrainLayer terrainLayer = terrainLayers[assetIdx];
                                        if (terrainLayer != null)
                                        {
                                            Texture2D diffuseTexture = terrainLayer.diffuseTexture;
                                            if (diffuseTexture != null)
                                            {
                                                name = diffuseTexture.name;
                                            }
                                            else
                                            {
                                                name = "Diffuse texture is Missing";
                                            }
                                        }
                                        else
                                        {
                                            name = "Terrain Layer is Missing";
                                        }
                                        assetChoices[assetIdx] = new GUIContent(name);
                                    }
                                    EditorGUI.BeginChangeCheck();
                                    selectedTextureIdxProperty = m_editorUtils.Popup("Texture", selectedTextureIdxProperty, assetChoices, helpEnabled);
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        Serialize();
                                        TerrainLayer terrainLayer = terrainLayers[selectedTextureIdxProperty];
                                        if (terrainLayer != null)
                                        {
                                            Texture2D diffuseTexture = terrainLayer.diffuseTexture;
                                            if (diffuseTexture != null)
                                            {
                                                string name = terrainLayer.diffuseTexture.name;
                                                selectedTextureNameProperty = name;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        textureStrengthProperty = m_editorUtils.Slider("Texture Strength", textureStrengthProperty, 0f, 1f, helpEnabled);
                        textureVarianceProperty = m_editorUtils.Slider("Texture Range", textureVarianceProperty, 0f, 1f, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    #endregion
                    #region Check Mask
                    // Check Mask
                    checkMask = m_editorUtils.Toggle("Check Mask", checkMask, helpEnabled);
                    if (checkMask)
                    {
                        EditorGUI.indentLevel++;
                        maskType = (Constants.MaskType)m_editorUtils.EnumPopup("Mask Type", maskType, helpEnabled);
                        if (maskType != Constants.MaskType.Image)
                        {
                            // maskFractalSeed = m_editorUtils.Slider("Seed", maskFractalSeed, 0f, 65000f, helpEnabled);
                            // maskFractalOctaves = m_editorUtils.IntSlider("Octaves", maskFractalOctaves, 1, 12, helpEnabled);
                            // maskFractalFrequency = m_editorUtils.Slider("Frequency", maskFractalFrequency, 0f, spawnerData.UseLargeRanges ? 1f : 0.3f, helpEnabled);
                            // maskFractalPersistence = m_editorUtils.Slider("Persistence", maskFractalPersistence, 0f, 1f, helpEnabled);
                            // maskFractalLacunarity = m_editorUtils.Slider("Lacunarity", maskFractalLacunarity, 1.5f, 3.5f, helpEnabled);
                            // m_editorUtils.Fractal(maskFractal, helpEnabled);
                            maskFractal.Enabled = true;
                            maskFractal.NoiseFalloff = m_editorUtils.CurveField("Noise Falloff", maskFractal.NoiseFalloff, helpEnabled);
                            maskFractal.NoiseType = (Constants.NoiseType)m_editorUtils.EnumPopup("Mask Type", maskFractal.NoiseType, helpEnabled);
                            maskFractal.Strength = m_editorUtils.Slider("Strength", maskFractal.Strength, -2f, 2f, helpEnabled);
                            maskFractal.Seed = m_editorUtils.Slider("Seed", maskFractal.Seed, 1f, 65000f, helpEnabled);
                            maskFractal.Octaves = m_editorUtils.IntSlider("Octaves", maskFractal.Octaves, 1, 15, helpEnabled);
                            maskFractal.Frequency = m_editorUtils.Slider("Frequency", maskFractal.Frequency, 0.0001f, 1f, helpEnabled);
                            maskFractal.Persistence = m_editorUtils.Slider("Persistence", maskFractal.Persistence, 0f, 1f, helpEnabled);
                            maskFractal.Lacunarity = m_editorUtils.Slider("Lacunarity", maskFractal.Lacunarity, 1.5f, 3f, helpEnabled);
                            maskFractal.Amplitude = m_editorUtils.Slider("Amplitude", maskFractal.Amplitude, 0.00001f, 1f, helpEnabled);
                            switch (maskFractal.NoiseType)
                            {
                                case Constants.NoiseType.Perlin:
                                case Constants.NoiseType.Billow:
                                    break;
                                case Constants.NoiseType.Ridged:
                                    maskFractal.RidgedOffset = m_editorUtils.Slider("Ridge Offset", maskFractal.RidgedOffset, 0f, 3f, helpEnabled);
                                    break;
                                case Constants.NoiseType.IQ:
                                    break;
                                case Constants.NoiseType.Swiss:
                                    maskFractal.RidgedOffset = m_editorUtils.Slider("Ridge Offset", maskFractal.RidgedOffset, 0f, 3f, helpEnabled);
                                    maskFractal.Warp = m_editorUtils.Slider("Warp", maskFractal.Warp, 0f, 5f, helpEnabled);
                                    break;
                                case Constants.NoiseType.Jordan:
                                    maskFractal.Warp = m_editorUtils.Slider("Warp", maskFractal.Warp, 0f, 5f, helpEnabled);
                                    maskFractal.Warp0 = m_editorUtils.Slider("Warp0", maskFractal.Warp0, 0f, 15f, helpEnabled);
                                    maskFractal.Damp = m_editorUtils.Slider("Damp", maskFractal.Damp, 0f, 5f, helpEnabled);
                                    maskFractal.Damp0 = m_editorUtils.Slider("Damp0", maskFractal.Damp0, 0f, 5f, helpEnabled);
                                    maskFractal.DampScale = m_editorUtils.Slider("Damp Scale", maskFractal.DampScale, -5, 20f, helpEnabled);
                                    break;
                                /*
                                 *
                                 * CELL NOISE
                                    noiseCellType = EditorGUILayout.IntSlider(GetLabel("Cell Type"), noiseCellType, 1, 9);
                                    noiseCellDistanceFunction = EditorGUILayout.IntSlider(GetLabel("Cell Dist"), noiseCellDistanceFunction, 1, 7);
                                 */
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            Vector2 offset = maskFractal.Offset;
                            offset.x = m_editorUtils.FloatField("Offset X", offset.x);
                            offset.y = m_editorUtils.FloatField("Offset Y", offset.y);
                            maskFractal.Offset = offset;
                            midMaskFractal = m_editorUtils.Slider("Midpoint", midMaskFractal, 0f, 1f, helpEnabled);
                            maskFractalRange = m_editorUtils.Slider("Range", maskFractalRange, 0f, 1f, helpEnabled);
                            maskInvert = m_editorUtils.Toggle("Invert Mask", maskInvert, helpEnabled);
                        }
                        else
                        {
                            maskFractal.Enabled = false;
                            maskImage = (Texture2D)m_editorUtils.ObjectField("Image Mask", maskImage, typeof(Texture2D), helpEnabled);
                            imageFilterColor = m_editorUtils.ColorField("Selection Color", imageFilterColor, helpEnabled);
                            imageFilterFuzzyMatch = m_editorUtils.Slider("Selection Accuracy", imageFilterFuzzyMatch, 0f, 1f, helpEnabled);
                            constrainWithinMaskedBounds = m_editorUtils.Toggle("Fit Within Mask", constrainWithinMaskedBounds, helpEnabled);
                            invertMaskedAlpha = m_editorUtils.Toggle("Invert Alpha", invertMaskedAlpha, helpEnabled);
                            successOnMaskedAlpha = m_editorUtils.Toggle("Success By Alpha", successOnMaskedAlpha, helpEnabled);
                            scaleOnMaskedAlpha = m_editorUtils.Toggle("Scale By Alpha", scaleOnMaskedAlpha, helpEnabled);
                            if (SpawnCriteria.ScaleOnMaskedAlpha)
                            {
                                EditorGUI.indentLevel++;
                                minScaleOnMaskedAlpha = m_editorUtils.Slider("Mask Alpha Min Scale", minScaleOnMaskedAlpha, 0f, 10f, helpEnabled);
                                maxScaleOnMaskedAlpha = m_editorUtils.Slider("Mask Alpha Max Scale", maxScaleOnMaskedAlpha, 0f, 10f, helpEnabled);
                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    #endregion
                    GUI.enabled = oldEnabled;
                }
                if (!SpawnerData.IsProcessing)
                {
                    if (GeNaEditorUtility.ValidateComputeShader())
                    {
                        GUI.enabled = true;
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Serialize();
                spawnCriteria.ForceSpawn = forceSpawn;
                SpawnerData.GroundLayer = groundLayer;
                spawnCriteria.SpawnCollisionLayers = spawnCollisionLayers;
                spawnCriteria.CheckCollisionType = virginCheckType;
                spawnCriteria.BlendAmount = blendAmount;
                spawnCriteria.BoundsExtents = boundsExtents;
                spawnCriteria.CheckHeightType = checkHeightType;
                SpawnerData.GetSeaLevel = getSeaLevel;
                if (!Mathf.Approximately(seaLevel, spawnCriteria.SeaLevel))
                {
                    spawnCriteria.SeaLevel = seaLevel;
                    GeNaEvents.SetSeaLevel(SpawnerData);
                }
                spawnCriteria.MinSpawnHeight = minSpawnHeight;
                spawnCriteria.MaxSpawnHeight = maxSpawnHeight;
                spawnCriteria.MinHeight = minHeight;
                spawnCriteria.MaxHeight = maxHeight;
                spawnCriteria.HeightRange = heightVariance;
                spawnCriteria.BottomBoundary = bottomBoundary;
                spawnCriteria.TopBoundary = topBoundary;
                spawnCriteria.CheckSlopeType = checkSlopeTypeProperty;
                spawnCriteria.MinSpawnSlope = minSpawnSlopeProperty;
                spawnCriteria.MaxSpawnSlope = maxSpawnSlopeProperty;
                spawnCriteria.MinSlope = minSlopeProperty;
                spawnCriteria.MaxSlope = maxSlopeProperty;
                spawnCriteria.SlopeRange = slopeRangeProperty;
                spawnCriteria.CheckTextures = checkTextures;
                spawnCriteria.SelectedTextureIdx = selectedTextureIdxProperty;
                spawnCriteria.SelectedTextureName = selectedTextureNameProperty;
                spawnCriteria.TextureStrength = textureStrengthProperty;
                spawnCriteria.TextureRange = textureVarianceProperty;
                spawnCriteria.CheckMask = checkMask;
                spawnCriteria.CheckMaskType = maskType;
                spawnCriteria.MaskFractal = maskFractal;
                // maskFractal.Seed = maskFractalSeed;
                // maskFractal.Octaves = maskFractalOctaves;
                // maskFractal.Frequency = maskFractalFrequency;
                // maskFractal.Persistence = maskFractalPersistence;
                // maskFractal.Lacunarity = maskFractalLacunarity;
                spawnCriteria.MidMaskFractal = midMaskFractal;
                spawnCriteria.MaskFractalRange = maskFractalRange;
                spawnCriteria.MaskInvert = maskInvert;
                spawnCriteria.MaskImage = maskImage;
                spawnCriteria.ImageFilterColor = imageFilterColor;
                spawnCriteria.ImageFilterFuzzyMatch = imageFilterFuzzyMatch;
                spawnCriteria.ConstrainWithinMaskedBounds = constrainWithinMaskedBounds;
                spawnCriteria.InvertMaskedAlpha = invertMaskedAlpha;
                spawnCriteria.SuccessOnMaskedAlpha = successOnMaskedAlpha;
                spawnCriteria.ScaleOnMaskedAlpha = scaleOnMaskedAlpha;
                spawnCriteria.MinScaleOnMaskedAlpha = minScaleOnMaskedAlpha;
                spawnCriteria.MaxScaleOnMaskedAlpha = maxScaleOnMaskedAlpha;
                m_spawner.IsDirty = true;
            }
        }
        private void PrototypesPanel(bool helpEnabled)
        {
            m_editorUtils.InlineHelp("Spawn Prototypes", helpEnabled);
            GUILayout.BeginHorizontal();
            {
                m_editorUtils.LabelField("Spawn Proto Panel Intro");
                if (m_spawner.PrototypeRootPrefabs.Count > 0)
                {
                    if (GUILayout.Button("Refresh Ingestion"))
                    {
                        RefreshIngestion();
                    }
                }
                GUILayout.FlexibleSpace();
                // ConformMenu();
                // SnapToGroundMenu();
                bool sortPrototypes = SpawnerData.SortPrototypes;
                m_editorUtils.ToggleButtonNonLocalized(" A-Z↓", ref sortPrototypes, GUILayout.Height(18f));
                m_editorUtils.Styles.deleteButton.fixedHeight = 18f;
                m_editorUtils.Styles.deleteButton.fixedWidth = 21f;
                m_editorUtils.Styles.deleteButton.alignment = TextAnchor.LowerCenter;
                if (m_editorUtils.DeleteButton())
                {
                    if (EditorUtility.DisplayDialog("WARNING!",
                            "Are you sure you want to delete ALL of the Prototypes?",
                            "OK",
                            "Cancel"))
                    {
                        m_spawner.RemoveAllPrototypes();
                        m_spawner.RemoveAllPrototypeRoots();
                    }
                    m_spawner.IsDirty = true;
                    GUIUtility.ExitGUI();
                }
                SpawnerData.SortPrototypes = sortPrototypes;
            }
            GUILayout.EndHorizontal();
            m_editorUtils.InlineHelp("Conform Dropdown", helpEnabled);
            m_editorUtils.InlineHelp("Snap Dropdown", helpEnabled);
            ProtoPanel(SpawnerData.SpawnPrototypes, helpEnabled);
        }
        private void ProtoPanel(List<Prototype> prototypes, bool helpEnabled)
        {
            Rect protoPanelWidthRect = GUILayoutUtility.GetLastRect();
            foreach (Prototype prototype in prototypes)
            {
                GUILayout.BeginVertical(Styles.gpanel);
                {
                    string protoName = " <b>" + prototype.Name;
                    if (prototype.HasType(Constants.ResourceType.Prefab))
                    {
                        string typeCode = " (P)";
                        if (prototype.Resources[0] != null && prototype.Resources[0].ContainerOnly)
                            typeCode = " (G)";
                        protoName += typeCode;
                        if (prototype.GetTopLevelResources().Count == 1)
                        {
                            if (prototype.Resources[0].ConformToSlope)
                                protoName += " *C*";
                        }
                    }
                    if (prototype.HasType(Constants.ResourceType.TerrainTree))
                    {
                        protoName += " (T)";
                    }
                    if (prototype.HasType(Constants.ResourceType.TerrainGrass))
                    {
                        protoName += " (G)";
                    }
                    if (prototype.HasType(Constants.ResourceType.TerrainTexture))
                    {
                        protoName += " (Tx)";
                    }
                    if (prototype.IsActive != true)
                        protoName += " [inactive]";
                    else
                        protoName += string.Format(" {0:0}% [{1}]", prototype.GetSuccessChance() * 100f, prototype.InstancesSpawned);
                    GUIStyle protoLabelStyle = Styles.richLabel;
                    protoName += "</b>";
                    // Let's check we are just changing the active state now
                    bool active = prototype.IsActive;
                    GUILayout.BeginHorizontal();
                    {
                        prototype.IsActive = EditorGUILayout.Toggle(prototype.IsActive, GUILayout.Width(10f));
                        if (GUILayout.Button(protoName, protoLabelStyle))
                            prototype.IsActive = !prototype.IsActive;
                        //Prep the rect if any ico needs to be drawn
                        Rect r = GUILayoutUtility.GetLastRect();
                        r = new Rect(r.xMax - 2f, r.yMin + 2f, 18f, 18f);
                        IReadOnlyList<Resource> resources = prototype.Resources;
                        if (resources.Count > 0)
                        {
                            if (resources[0].SpawnCriteria.OverrideApplies)
                            {
                                if (m_overridesIco != null)
                                {
                                    //GUI.DrawTexture(r, m_overridesIco);
                                    GUI.Label(r, "*");
                                    //Add to rect in case another ico needs drawing after this
                                    r.x += 20f;
                                }
                                else
                                    GeNaDebug.LogWarningFormat("Missing overrides icon.");
                            }
                        }
                        GUILayout.FlexibleSpace();
                        if (!m_showPrototypeAdvanced.ContainsKey(prototype.Id))
                            m_showPrototypeAdvanced.Add(prototype.Id, false);
                        bool showAdvanced = m_showPrototypeAdvanced[prototype.Id];
                        m_editorUtils.ToggleButton("Advanced Toggle", ref showAdvanced, Styles.advancedToggle, Styles.advancedToggleDown);
                        m_showPrototypeAdvanced[prototype.Id] = showAdvanced;
                        // foreach (var prototype in prototypes)
                        // {
                        //     prototype.ShowAdvancedOptions = showAdvanced;
                        // }
                        //GUILayout.Space(10f);
                        if (m_editorUtils.DeleteButton())
                        {
                            if (EditorUtility.DisplayDialog("WARNING!", string.Format("Are you sure you want to delete the prototype [{0}]?", prototype.Name), "OK", "Cancel"))
                            {
                                m_spawner.RemoveProto(prototype);
                            }
                            m_spawner.IsDirty = true;
                            GUIUtility.ExitGUI();
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2f);
                    // In here we can process different things if the active state changed.
                    if (active != prototype.IsActive)
                    {
                        m_spawner.UpdatePrototypes();
                    }

                    // Display the the proto properties if active
                    if (prototype.IsActive)
                    {
                        EditorGUI.indentLevel++;
                        // Only show help for static switches if they are actually drawn
                        bool staticSwitchesDrawn = false;
                        if (!m_showPrototypeInEditor.ContainsKey(prototype.Id))
                            m_showPrototypeInEditor.Add(prototype.Id, false);
                        if (!m_showPrototypeAdvanced.ContainsKey(prototype.Id))
                            m_showPrototypeAdvanced.Add(prototype.Id, false);
                        bool showPrototypeInEditor = m_showPrototypeInEditor[prototype.Id];
                        bool showPrototypeAdvanced = m_showPrototypeAdvanced[prototype.Id];
                        GUILayout.BeginHorizontal((prototype.Resources.Count < 1 || prototype.Resources[0].Static != Constants.ResourceStatic.Dynamic) ? Styles.staticResHeader : Styles.dynamicResHeader);
                        {
                            showPrototypeInEditor = m_editorUtils.Foldout(showPrototypeInEditor, "Details Foldout");
                            GUILayout.FlexibleSpace();
                            // Add the static switch if not POI
                            if (prototype.GetTopLevelResources().Count == 1)
                                staticSwitchesDrawn = StaticSwitch(prototype, prototype.Resources[0]);
                        }
                        m_showPrototypeInEditor[prototype.Id] = showPrototypeInEditor;
                        GUILayout.EndHorizontal();
                        // Adding help here because it should not be in the horizontal area. This way we also have the help only once to avoid cluttering the GUI.
                        if (staticSwitchesDrawn)
                            m_editorUtils.InlineHelp(Enum.GetNames(typeof(Constants.ResourceStatic)), helpEnabled);
                        GUILayout.Space(3f);
                        if (showPrototypeInEditor)
                        {
                            if (prototype.HasType(Constants.ResourceType.Prefab))
                                prototype.ForwardRotation = m_editorUtils.Slider("Forward Rotation", prototype.ForwardRotation, -360f, 360f, helpEnabled);
                            if (prototype.GetTopLevelResources().Count == 1)
                            {
                                Separator(protoPanelWidthRect);
                                EditResource(prototype, prototype.Resources[0], false, helpEnabled, showPrototypeAdvanced);
                            }
                            else
                            {
                                prototype.Name = m_editorUtils.TextField("Proto Name", prototype.Name, helpEnabled);
                                m_editorUtils.LabelField("Proto Size", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", prototype.Size.x, prototype.Size.y, prototype.Size.z)), helpEnabled);
                                Separator(protoPanelWidthRect);
                                foreach (Resource res in prototype.Resources)
                                {
                                    string resName = res.Name;
                                    if (res.ConformToSlope)
                                        resName += " *C*";
                                    if (!m_showResourceInEditor.ContainsKey(res.Id))
                                        m_showResourceInEditor.Add(res.Id, false);
                                    bool displayedInEditor = m_showResourceInEditor[res.Id];
                                    GUILayout.BeginHorizontal((res.Static == Constants.ResourceStatic.Dynamic) ? Styles.dynamicResHeader : Styles.staticResHeader);
                                    {
                                        displayedInEditor = EditorGUILayout.Foldout(displayedInEditor, resName, true);
                                        GUILayout.FlexibleSpace();
                                        StaticSwitch(prototype, res);
                                    }
                                    m_showResourceInEditor[res.Id] = displayedInEditor;
                                    GUILayout.EndHorizontal();
                                    if (displayedInEditor)
                                        EditResource(prototype, res, true, helpEnabled, showPrototypeAdvanced);
                                }
                            }
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(20);
                            GUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                    GUILayout.Space(3);
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }
        private void AdvancedPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                SpawnerSettings.AdvancedSettings advancedSettings = Settings.Advanced;
                advancedSettings.CurrentProcessor = (Constants.Processor)m_editorUtils.EnumPopup("Current Processor", advancedSettings.CurrentProcessor, helpEnabled);
                // spawnerData.MergeSpawns = m_editorUtils.Toggle("Merge Instances", spawnerData.MergeSpawns, helpEnabled);
                // Placement
                PlacementCriteria.ScaleToNearestInt = m_editorUtils.Toggle("Scale Nearest Int", PlacementCriteria.ScaleToNearestInt, helpEnabled);
                PlacementCriteria.GlobalSpawnJitterPct = m_editorUtils.Slider("Global Spawn Jitter", PlacementCriteria.GlobalSpawnJitterPct * 100f, 0f, 100f, helpEnabled) * 0.01f;
                //m_spawner.m_advAddColliderToSpawnedPrefabs = m_editorUtils.Toggle("Add Collider To POI", m_spawner.m_advAddColliderToSpawnedPrefabs, helpEnabled);
                Settings.MaxSubSpawnerDepth = m_editorUtils.IntField("Max Sub Spawner Depth", Settings.MaxSubSpawnerDepth, helpEnabled);
                Settings.AutoProbe = m_editorUtils.Toggle("Add Light Probes", Settings.AutoProbe, helpEnabled);
                if (Settings.AutoProbe)
                {
                    Settings.MinProbeGroupDistance = m_editorUtils.Slider("Min PG Dist", Settings.MinProbeGroupDistance, 10f, 200f, helpEnabled);
                    Settings.MinProbeDistance = m_editorUtils.Slider("Min Probe Dist", Settings.MinProbeDistance, 5f, 50f, helpEnabled);
                }
                Settings.AutoOptimise = m_editorUtils.Toggle("Spawn Optimizer", Settings.AutoOptimise, helpEnabled);
                if (Settings.AutoOptimise)
                {
                    Settings.MaxSizeToOptimise = m_editorUtils.Slider("Smaller Than (m)", Settings.MaxSizeToOptimise, 5f, 50f, helpEnabled);
                }
                Settings.MaxVisualizationDimensions = m_editorUtils.IntSlider("Visualiser Resolution", Settings.MaxVisualizationDimensions, 1, 512, helpEnabled);
                // Advanced
                advancedSettings.PerformUndoAtRuntime = m_editorUtils.Toggle("Perform Undo At Runtime", advancedSettings.PerformUndoAtRuntime, helpEnabled);
                advancedSettings.SpawnCheckOffset = m_editorUtils.FloatField("Collision Test Offset", advancedSettings.SpawnCheckOffset, helpEnabled);
                advancedSettings.BoundsOffset = m_editorUtils.FloatField("Bounds Offset", advancedSettings.BoundsOffset, helpEnabled);
                advancedSettings.DebugEnabled = m_editorUtils.Toggle("Debug Enabled", advancedSettings.DebugEnabled, helpEnabled);
                if (advancedSettings.DebugEnabled)
                {
                    if (!SpawnerData.IsProcessing)
                        GUI.enabled = false;
                    ToggleNonLocalized("Affects Height", SpawnerData.AffectsHeight);
                    ToggleNonLocalized("Affects Trees", SpawnerData.AffectsTrees);
                    ToggleNonLocalized("Affects Grass", SpawnerData.AffectsGrass);
                    ToggleNonLocalized("Affects Texture", SpawnerData.AffectsTexture);
                    ToggleNonLocalized("Has GameObject Protos", SpawnerData.HasGameObjectProtos);
                    ToggleNonLocalized("Has Tree Protos", SpawnerData.HasTerrainTrees);
                    ToggleNonLocalized("Has Grass Protos", SpawnerData.HasTerrainGrass);
                    ToggleNonLocalized("Has Texture Protos", SpawnerData.HasTerrainTextures);
                    ToggleNonLocalized("Has Heights Protos", SpawnerData.HasTerrainHeights);
                    ToggleNonLocalized("Has Active Physics Protos", SpawnerData.HasActivePhysicsProtos());
                    ToggleNonLocalized("Has Active Tree Protos", SpawnerData.HasActiveTerrainTrees);
                    ToggleNonLocalized("Has Active Grass Protos", SpawnerData.HasActiveTerrainGrass);
                    ToggleNonLocalized("Has Active Texture Protos", SpawnerData.HasActiveTerrainTextures);
                    ToggleNonLocalized("Has Active Heights Protos", SpawnerData.HasActiveTerrainHeights);
                    ToggleNonLocalized("Has Active SubSpawner Protos", SpawnerData.HasActiveSubSpawnerProtos());
                    if (!SpawnerData.IsProcessing)
                    {
                        if (GeNaEditorUtility.ValidateComputeShader())
                        {
                            GUI.enabled = true;
                        }
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_spawner.IsDirty = true;
            }
        }
        private void AddTerrainPrototype(EffectType effectType)
        {
            string name = effectType.ToString();
            GameObject prototype = new GameObject(name);
            GeNaTerrainDecorator decorator = prototype.AddComponent<GeNaTerrainDecorator>();
            TerrainModifier terrainModifier = decorator.TerrainModifier;
            terrainModifier.EffectType = effectType;
            int selected = 0;
            LoadUnityBrushIcons(terrainModifier.AddBrushTexture, ref selected);
            AddGameObject(SpawnerData, prototype);
            DestroyImmediate(prototype);
        }
        private void AddPrototypesPanel()
        {
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginVertical(m_editorUtils.Styles.panelFrame);
                {
                    //Add prototypes
                    GUILayout.BeginHorizontal();
                    {
                        if (DrawPrefabGUI())
                            GUI.changed = true;
                        if (m_editorUtils.Button("Add Tree", Styles.addBtn, GUILayout.Width(50), GUILayout.Height(49)))
                        {
                            m_spawner.AddTreeProto();
                            GUI.changed = true;
                            GUIUtility.ExitGUI();
                        }
                        if (m_editorUtils.Button("Add Grass", Styles.addBtn, GUILayout.Width(50), GUILayout.Height(49)))
                        {
                            //Add and init the brushsets for it
                            m_spawner.AddGrassProto();
                            // AddTerrainPrototype(EffectType.Detail);
                            GUI.changed = true;
                            GUIUtility.ExitGUI();
                        }
                        if (m_editorUtils.Button("Add Tx", Styles.addBtn, GUILayout.Width(50), GUILayout.Height(49)))
                        {
                            //Add and init the brushsets for it
                            // m_spawner.AddTextureProto();
                            AddTerrainPrototype(EffectType.Texture);
                            GUI.changed = true;
                            GUIUtility.ExitGUI();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_spawner.IsDirty = true;
            }
        }
        private void LoadUnityBrushIcons(Action<Texture2D> addMethod, ref int selected)
        {
            int num1 = 1;
            Texture2D texture2D;
            do
            {
                texture2D = Resources.Load("pwub_builtin_" + num1.ToString() + PWConst.VERSION_IN_FILENAMES) as Texture2D;
                if ((bool)(Object)texture2D)
                {
                    addMethod(texture2D);
                    ++num1;
                }
                else
                    break;
            } while ((bool)(Object)texture2D);
            int num2 = 0;
            Texture2D texture;
            do
            {
                texture = EditorGUIUtility.FindTexture("brush_" + num2.ToString() + ".png");
                if ((bool)(Object)texture)
                {
                    addMethod(texture);
                    ++num2;
                }
                else
                    break;
            } while ((bool)(Object)texture);
            if (selected < 0)
                selected = 0;
            GUI.changed = true;
        }
        #region Decorators
        [NonSerialized] protected readonly Dictionary<string, GeNaDecoratorEditor> m_decoratorEditors = new Dictionary<string, GeNaDecoratorEditor>();
        public GeNaDecoratorEditor GetDecoratorEditor(string hashCode, IDecorator decorator)
        {
            if (m_decoratorEditors.ContainsKey(hashCode))
            {
                GeNaDecoratorEditor geNaDecoratorEditor = m_decoratorEditors[hashCode];
                if (geNaDecoratorEditor != null)
                {
                    geNaDecoratorEditor.m_useStatic = false;
                    return geNaDecoratorEditor;
                }
                m_decoratorEditors.Remove(hashCode);
            }
            GeNaDecoratorEditor editor = CreateEditor(decorator as Component) as GeNaDecoratorEditor;
            if (editor is GeNaDecoratorEditor decoratorEditor)
            {
                decoratorEditor.SetSpawner(m_spawner);
                decoratorEditor.m_showCommonPanelLocal = false;
                decoratorEditor.m_showSettingsPanelLocal = false;
            }
            m_decoratorEditors.Add(hashCode, editor);
            return editor;
        }
        private void EditDecorators(Resource res)
        {
            List<IDecorator> decorators = res.Decorators;
            if (decorators.Count > 0)
            {
                GUILayout.Label("Decorators:");
            }
            bool decoratorChanged = false;
            for (int i = 0; i < decorators.Count; i++)
            {
                IDecorator decorator = decorators[i];
                string hashCode = $"{res.Id}{i}";
                GeNaDecoratorEditor decoratorEditor = GetDecoratorEditor(hashCode, decorator);
                if (decoratorEditor == null)
                    continue;
                if (decoratorEditor.HideInSpawner)
                    continue;
                EditorGUI.BeginChangeCheck();
                {
                    bool showDecorator = true;
                    if (decorator is GeNaPhysicsDecorator physicsDecorator)
                    {
                        var spawnerData = m_spawner.SpawnerData;
                        if (spawnerData.PhysicsType == Constants.PhysicsType.Spawner)
                        {
                            decoratorEditor.RenderTitle();
                            EditorGUILayout.LabelField("Physics Type set to 'Spawner' in Overview Panel. Local Physics Disabled.");
                            showDecorator = false;
                        }
                    }
                    if (showDecorator) 
                        decoratorEditor.OnInspectorGUI();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    decoratorChanged = true;
                }
            }
            if (decoratorChanged)
                res.SerializeDecorators();
        }
        public void EditOneChildOf(Prototype proto, Resource res, bool helpEnabled)
        {
            if (res.ResourceType != Constants.ResourceType.Prefab)
                return;
            if (!res.OneChildOf)
                return;
            res.OneChildOf = m_editorUtils.Toggle("Child Of Toggle", res.OneChildOf, helpEnabled);
            if (res.OneChildOf)
            {
                List<Resource> children = proto.GetChildren(res);
                if (children != null && children.Count > 0)
                {
                    EditorGUI.indentLevel++;
                    foreach (Resource decoratorChild in children)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            string title = decoratorChild.Name;
                            EditorGUILayout.LabelField(title, Styles.body, GUILayout.Width(EditorGUIUtility.labelWidth));
                            decoratorChild.OneChildOfWeight = EditorGUILayout.Slider(decoratorChild.OneChildOfWeight, 0f, 1f);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }
        #endregion
        /// <summary>
        /// Create a new palette scriptable object
        /// </summary>
        /// <returns></returns>
        private PaletteData CreatePalette()
        {
            string fileName = "New GeNa Palette";
            string filePath = $"Assets/{fileName}.asset";
            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            PaletteData asset = CreateInstance<PaletteData>();
            AssetDatabase.CreateAsset(asset, filePath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            return asset;
        }
        /// <summary>
        /// Edit the selected resource
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="res"></param>
        /// <param name="child"></param>
        /// <param name="helpEnabled"></param>
        /// <param name="advanced"></param>
        private void EditResource(Prototype proto, Resource res, bool child, bool helpEnabled, bool advanced)
        {
            if (!m_showSpawnCriteriaOverrides.ContainsKey(res.Id))
                m_showSpawnCriteriaOverrides.Add(res.Id, false);
            bool showSpawnCriteriaOverrides = m_showSpawnCriteriaOverrides[res.Id];
            bool isChanged = false;
            if (advanced)
            {
                GUILayout.BeginVertical(Styles.gpanel);
                {
                    if (res.SpawnCriteria.OverrideApplies)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            showSpawnCriteriaOverrides = m_editorUtils.Foldout(showSpawnCriteriaOverrides, "Proto SpCrit Overrides Label");
                            if (m_overridesIco != null)
                            {
                                Rect r = GUILayoutUtility.GetLastRect();
                                // EditorGUI.LabelField(new Rect(r.x + EditorStyles.foldout.CalcSize(m_editorUtils.GetContent("Proto SpCrit Overrides Label")).x + 3f, r.y - 1f, 18f, 18f), "*");
                                GUI.Label(new Rect(r.x + EditorStyles.foldout.CalcSize(m_editorUtils.GetContent("Proto SpCrit Overrides Label")).x + 3f, r.y + 2f, 18f, 18f), "*");
                            }
                            else
                                GeNaDebug.LogWarningFormat("Missing overrides icon.");
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        showSpawnCriteriaOverrides = m_editorUtils.Foldout(showSpawnCriteriaOverrides, "Proto SpCrit Overrides Label");
                    }
                    if (showSpawnCriteriaOverrides)
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            res.SpawnCriteria = m_editorUtils.SpawnCriteriaOverrides(res.SpawnCriteria, SpawnerData.SpawnCriteria, helpEnabled);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            isChanged = true;
                        }
                    }
                    GUILayout.Space(3);
                }
                GUILayout.EndVertical();
            }
            m_showSpawnCriteriaOverrides[res.Id] = showSpawnCriteriaOverrides;
            GUILayout.Space(3f);
            if (child)
                EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            {
                res.Name = m_editorUtils.TextField("Resource Name", res.Name, helpEnabled);
                if (res.Static > Constants.ResourceStatic.Static)
                    res.SuccessRate = 0.01f * m_editorUtils.Slider("Res Success", res.SuccessRate * 100f, 0f, 100f, helpEnabled);
            }
            if (EditorGUI.EndChangeCheck())
            {
                isChanged = true;
            }
            bool snapToGround = res.SnapToGround;
            bool conformToSlope = res.ConformToSlope;
            Vector3 basePosition = res.BasePosition;
            Vector3 baseRotation = res.BaseRotation;
            Vector3 baseScale = res.BaseScale;
            Vector3 baseSize = res.BaseSize;
            Vector3 minRotation = res.MinRotation;
            Vector3 maxRotation = res.MaxRotation;
            Vector3 minOffset = res.MinOffset;
            Vector3 maxOffset = res.MaxOffset;
            Vector3 minScale = res.MinScale;
            Vector3 maxScale = res.MaxScale;
            EditorGUI.BeginChangeCheck();
            {
                switch (res.ResourceType)
                {
                    case Constants.ResourceType.Prefab:
                        PrefabField(res, advanced, helpEnabled);
                        if (res.Static > Constants.ResourceStatic.Static)
                        {
                            snapToGround = m_editorUtils.Toggle("Snap To Ground", snapToGround, helpEnabled);
                            conformToSlope = m_editorUtils.Toggle("Conform Slope", conformToSlope, helpEnabled);
                            if (advanced)
                            {
                                minOffset = m_editorUtils.Vector3Field("Min Position Offset", minOffset, helpEnabled);
                                maxOffset = m_editorUtils.Vector3Field("Max Position Offset", maxOffset, helpEnabled);
                            }
                            else
                                m_editorUtils.MinMaxSliderWithFields("Position Modifier Y", ref minOffset.y, ref maxOffset.y, -10f, 10f, helpEnabled);
                        }
                        else
                        {
                            m_editorUtils.LabelField("Static Position Offset", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", basePosition.x, basePosition.y, basePosition.z)));
                            m_editorUtils.LabelField("Static Rotation Offset", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseRotation.x, baseRotation.y, baseRotation.z)));
                            m_editorUtils.LabelField("Static Scale", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseScale.x, baseScale.y, baseScale.z)));
                        }
                        if (advanced)
                        {
                            if (res.Static > Constants.ResourceStatic.Static)
                            {
                                minRotation = m_editorUtils.Vector3Field("Min Rotation Offset", minRotation, helpEnabled);
                                maxRotation = m_editorUtils.Vector3Field("Max Rotation Offset", maxRotation, helpEnabled);
                                res.SameScale = m_editorUtils.Toggle("Same O Scale", res.SameScale, helpEnabled);
                                if (res.SameScale)
                                    m_editorUtils.MinMaxSliderWithFields("Res Scale", ref minScale.x, ref maxScale.x, 0.1f, 100f, helpEnabled);
                                else
                                {
                                    minScale = m_editorUtils.Vector3Field("Res Min Scale", minScale, helpEnabled);
                                    maxScale = m_editorUtils.Vector3Field("Res Max Scale", maxScale, helpEnabled);
                                }
                            }
                            res.BaseColliderUseConstScale = m_editorUtils.Toggle("Same C Scale", res.BaseColliderUseConstScale, helpEnabled);
                            if (res.BaseColliderUseConstScale)
                                res.BaseColliderConstScaleAmount = m_editorUtils.Slider("Collider Scale", res.BaseColliderConstScaleAmount, 0.25f, 2f, helpEnabled);
                            else
                                res.BaseColliderScale = m_editorUtils.Vector3Field("Collider Scale", res.BaseColliderScale, helpEnabled);
                            bool canUseColliders = res.HasColliders && res.Static == Constants.ResourceStatic.Dynamic;
                            m_editorUtils.SpawnFlags(res.SpawnFlags, canUseColliders, helpEnabled);
                        }
                        // if not advanced mode and not static, Height Offset only if not 
                        else if (res.Static > Constants.ResourceStatic.Static)
                            m_editorUtils.MinMaxSliderWithFields("Y Rotation Offset", ref minRotation.y, ref maxRotation.y, -180f, 180f, helpEnabled);
                        m_editorUtils.LabelField("Base Position", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", basePosition.x, basePosition.y, basePosition.z)), helpEnabled);
                        m_editorUtils.LabelField("Base Rotation", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseRotation.x, baseRotation.y, baseRotation.z)), helpEnabled);
                        if (res.Static >= Constants.ResourceStatic.Dynamic)
                            m_editorUtils.LabelField("Base Scale", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseScale.x, baseScale.y, baseScale.z)), helpEnabled);
                        m_editorUtils.LabelField("Base Size", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseSize.x, baseSize.y, baseSize.z)), helpEnabled);
                        m_editorUtils.LabelField("Res Spawned", new GUIContent(string.Format("{0}", res.InstancesSpawned)), helpEnabled);
                        break;
                    case Constants.ResourceType.TerrainGrass:
                        Terrain terrain = Terrain.activeTerrain;
                        if (terrain != null)
                        {
                            GUIContent[] assetChoices = new GUIContent[terrain.terrainData.detailPrototypes.Length];
                            DetailPrototype detailProto;
                            for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                            {
                                detailProto = terrain.terrainData.detailPrototypes[assetIdx];
                                if (detailProto.prototypeTexture != null)
                                    assetChoices[assetIdx] = new GUIContent(detailProto.prototypeTexture.name);
                                else if (detailProto.prototype != null)
                                    assetChoices[assetIdx] = new GUIContent(detailProto.prototype.name);
                                else
                                    assetChoices[assetIdx] = new GUIContent("Unknown asset");
                            }
                            int oldIdx = res.TerrainProtoIdx;
                            res.TerrainProtoIdx = m_editorUtils.Popup("Grass", res.TerrainProtoIdx, assetChoices, helpEnabled);
                            res.SameScale = true;
                            m_editorUtils.MinMaxSliderWithFields("Grass Strength", ref minScale.x, ref maxScale.x, 0f, 1f, helpEnabled);
                            m_editorUtils.MinMaxSliderWithFields("Position Modifier X", ref minOffset.x, ref maxOffset.x, -10f, 10f, helpEnabled);
                            m_editorUtils.MinMaxSliderWithFields("Position Modifier Z", ref minOffset.z, ref maxOffset.z, -10f, 10f, helpEnabled);
                            if (res.TerrainProtoIdx != oldIdx)
                            {
                                detailProto = terrain.terrainData.detailPrototypes[res.TerrainProtoIdx];
                                if (detailProto.prototypeTexture != null)
                                {
                                    res.AddDetailPrototype(detailProto.prototypeTexture, m_spawner.Palette);
                                    res.Name = detailProto.prototypeTexture.name;
                                    proto.Name = res.Name;
                                }
                                else if (detailProto.prototype != null)
                                {
                                    res.AddDetailPrototype(detailProto.prototype, m_spawner.Palette);
                                    res.Name = detailProto.prototype.name;
                                    proto.Name = res.Name;
                                }
                                else
                                {
                                    res.Name = "Unknown asset";
                                    proto.Name = res.Name;
                                }
                                res.BaseSize = new Vector3(detailProto.minWidth, detailProto.minHeight, detailProto.minWidth);
                                proto.Size = res.BaseSize;
                                proto.Extents = res.BaseSize * 0.5f;
                            }
                            m_editorUtils.LabelField("Base Size", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", res.BaseSize.x, res.BaseSize.y, res.BaseSize.z)), helpEnabled);
                        }
                        break;
                    case Constants.ResourceType.TerrainTree:
                        terrain = Terrain.activeTerrain;
                        if (terrain != null)
                        {
                            GUIContent[] assetChoices = new GUIContent[terrain.terrainData.treePrototypes.Length];
                            TreePrototype treeProto;
                            for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                            {
                                treeProto = terrain.terrainData.treePrototypes[assetIdx];
                                if (treeProto.prefab != null)
                                    assetChoices[assetIdx] = new GUIContent(treeProto.prefab.name);
                                else
                                    assetChoices[assetIdx] = new GUIContent("Unknown asset");
                            }
                            int oldIdx = res.TerrainProtoIdx;
                            res.TerrainProtoIdx = m_editorUtils.Popup("Tree", res.TerrainProtoIdx, assetChoices, helpEnabled);
                            if (res.TerrainProtoIdx != oldIdx)
                            {
                                treeProto = terrain.terrainData.treePrototypes[res.TerrainProtoIdx];
                                if (treeProto.prefab != null)
                                {
                                    res.AddPrefab(treeProto.prefab, m_spawner.Palette);
                                    res.Name = treeProto.prefab.name;
                                    proto.Name = res.Name;
                                    res.BaseSize = GeNaUtility.GetInstantiatedBounds(treeProto.prefab).size;
                                    res.BaseScale = treeProto.prefab.transform.localScale;
                                }
                                else
                                {
                                    res.Name = "Unknown asset";
                                    proto.Name = res.Name;
                                }
                                res.MinScale = res.BaseScale;
                                res.MinScale = res.BaseScale;
                                proto.Size = res.BaseSize;
                                proto.Extents = res.BaseSize * 0.5f;
                            }
                            m_editorUtils.MinMaxSliderWithFields("Position Modifier X", ref minOffset.x, ref maxOffset.x, -100f, 100f, helpEnabled);
                            m_editorUtils.MinMaxSliderWithFields("Position Modifier Z", ref minOffset.z, ref maxOffset.z, -100f, 100f, helpEnabled);
                            m_editorUtils.MinMaxSliderWithFields("Y Rotation Offset", ref minRotation.y, ref maxRotation.y, -180f, 180f, helpEnabled);
                            if (advanced)
                            {
                                res.SameScale = m_editorUtils.Toggle("Same O Scale", res.SameScale, helpEnabled);
                                if (res.SameScale)
                                    m_editorUtils.MinMaxSliderWithFields("Res Scale", ref minScale.x, ref maxScale.x, 0.1f, 100f, helpEnabled);
                                else
                                {
                                    minScale = m_editorUtils.Vector3Field("Res Min Scale", minScale, helpEnabled);
                                    maxScale = m_editorUtils.Vector3Field("Res Max Scale", maxScale, helpEnabled);
                                }
                            }
                            m_editorUtils.LabelField("Base Size", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseSize.x, baseSize.y, baseSize.z)), helpEnabled);
                            m_editorUtils.LabelField("Base Scale", new GUIContent(string.Format("{0:0.00}, {1:0.00}, {2:0.00}", baseScale.x, baseScale.y, baseScale.z)), helpEnabled);
                        }
                        break;
                    case Constants.ResourceType.TerrainTexture:
                        terrain = Terrain.activeTerrain;
                        if (terrain != null)
                        {
                            GUIContent[] assetChoices = new GUIContent[terrain.terrainData.alphamapLayers];
                            for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                            {
#if UNITY_2018_3_OR_NEWER
                                assetChoices[assetIdx] = new GUIContent(terrain.terrainData.terrainLayers[assetIdx].diffuseTexture.name);
#else
                            assetChoices[assetIdx] = new GUIContent(terrain.terrainData.splatPrototypes[assetIdx].texture.name);
#endif
                            }
                            int oldIdx = res.TerrainProtoIdx;
                            res.TerrainProtoIdx = m_editorUtils.Popup("Texture", res.TerrainProtoIdx, assetChoices, helpEnabled);
                            if (res.TerrainProtoIdx != oldIdx)
                            {
                                //res.TexturePrototypeData = GeNaSpawner.UpdateTexturePrototypeData(terrain.terrainData.terrainLayers[res.TerrainProtoIdx]);
                                TerrainLayer terrainLayer = terrain.terrainData.terrainLayers[res.TerrainProtoIdx];
                                res.AddTerrainLayerAsset(terrainLayer.diffuseTexture, m_spawner.Palette);
                                //res.AssetID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(res.TexturePrototypeData.terrainLayerAsset.diffuseTexture));
#if UNITY_2018_3_OR_NEWER
                                res.Name = terrain.terrainData.terrainLayers[res.TerrainProtoIdx].diffuseTexture.name;
#else
                            res.Name = terrain.terrainData.splatPrototypes[res.TerrainProtoIdx].texture.name;
#endif
                                proto.Name = res.Name;
                            }

                            // Shape
                            res.BrushTXIndex = m_editorUtils.BrushSelectionGrid("TxBrush Shape",
                                res.BrushTXIndex,
                                out _,
                                res.BrushTextures.ToArray(),
                                res.AddBrushTexture,
                                res.RemoveBrushTexture,
                                res.ClearBrushTextures,
                                helpEnabled);
                            res.SameScale = true;
                            // Calculate the minimum and a maximum 100 final scale
                            TerrainData terrainData = terrain.terrainData;
                            float splatPixelSize = terrainData.size.x / terrainData.alphamapResolution;
                            int lowerScaleLimit = Mathf.CeilToInt((Constants.MIN_TX_BRUSH_SIZE_IN_PIX * splatPixelSize) / (PlacementCriteria.SameScale ? PlacementCriteria.MinScale.x : 0.5f * (PlacementCriteria.MinScale.x + PlacementCriteria.MinScale.z)));
                            int higherScaleLimit = Mathf.FloorToInt((100f * splatPixelSize) / (PlacementCriteria.SameScale ? PlacementCriteria.MaxScale.x : 0.5f * (PlacementCriteria.MaxScale.x + PlacementCriteria.MaxScale.z)));
                            int minScaleInt = (int)res.MinScale.x;
                            int maxScaleInt = (int)res.MaxScale.x;
                            m_editorUtils.MinMaxSliderWithFields("Texture Size", ref minScaleInt, ref maxScaleInt, lowerScaleLimit, higherScaleLimit, helpEnabled);
                            minScale.x = minScaleInt;
                            maxScale.x = maxScaleInt;
                            res.Opacity = 0.01f * m_editorUtils.Slider("Opacity", res.Opacity * 100f, 0, 100f, helpEnabled);
                            res.TargetStrength = m_editorUtils.Slider("Target Strength", res.TargetStrength, 0, 1f, helpEnabled);
                        }
                        break;
                    default:
                        throw new NotImplementedException("Not sure what to do with ResourceType '" + res.ResourceType + "'");
                }
                EditOneChildOf(proto, res, helpEnabled);
                EditDecorators(res);
            }
            if (EditorGUI.EndChangeCheck())
            {
                isChanged = true;
            }
            // Keep traversing down the tree
            ChildResources(proto, res, helpEnabled, advanced);
            res.SnapToGround = snapToGround;
            res.ConformToSlope = conformToSlope;
            res.BasePosition = basePosition;
            res.BaseRotation = baseRotation;
            res.BaseScale = baseScale;
            res.BaseSize = baseSize;
            res.MinRotation = minRotation;
            res.MaxRotation = maxRotation;
            res.MinOffset = minOffset;
            res.MaxOffset = maxOffset;
            res.MinScale = minScale;
            res.MaxScale = maxScale;
            if (child)
                EditorGUI.indentLevel--;
            if (isChanged)
            {
                SpawnerData.IsDirty = true;
            }
        }
        /// <summary>
        /// Displays the Children of the Resource in a Resource Tree, if there are any.
        /// </summary>
        private void ChildResources(Prototype proto, Resource res, bool helpEnabled, bool advanced)
        {
            List<Resource> children = proto.GetChildren(res);
            if (children == null)
                return;
            // Child Resources (if Resource tree)
            foreach (Resource child in children)
            {
                string childName = child.Name;
                switch (child.ResourceType)
                {
                    case Constants.ResourceType.Prefab:
                        childName += child.ContainerOnly ? " (G)" : " (P)";
                        if (child.ConformToSlope)
                            childName += " *C*";
                        break;
                    case Constants.ResourceType.TerrainTree:
                        childName += " (T)";
                        break;
                    case Constants.ResourceType.TerrainGrass:
                        childName += " (G)";
                        break;
                    case Constants.ResourceType.TerrainTexture:
                        childName += " (Tx)";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                childName += string.Format(" {0:0}%", child.SuccessRate * 100f);
                GUILayout.BeginVertical(Styles.gpanel);
                {
                    if (!m_showChildResourceInEditor.ContainsKey(child.Id))
                        m_showChildResourceInEditor.Add(child.Id, false);
                    bool showChildResourceInEditor = m_showChildResourceInEditor[child.Id];
                    GUILayout.BeginHorizontal((child.Static == Constants.ResourceStatic.Dynamic) ? Styles.dynamicResHeader : Styles.staticResHeader);
                    {
                        showChildResourceInEditor = EditorGUILayout.Foldout(showChildResourceInEditor, childName, true, Styles.resTreeFoldout);
                        GUILayout.FlexibleSpace();
                        StaticSwitch(proto, child);
                    }
                    GUILayout.EndHorizontal();
                    m_showChildResourceInEditor[child.Id] = showChildResourceInEditor;
                    if (showChildResourceInEditor)
                    {
                        // Proto won't be changed since we can't mix terrain resources into the mix.
                        // If we do later we can avoid changes being made to the proto by m_children of trees.
                        EditResource(proto, child, true, helpEnabled, advanced);
                    }
                }
                GUILayout.EndVertical();
            }
        }
        public bool ToggleNonLocalized(string label, bool value)
        {
            value = GUILayout.Toggle(value, label);
            return value;
        }
        /// <summary>
        /// Draw a prefab field that handles prefab replacement
        /// </summary>
        /// <param name="res"></param>
        /// <param name="advanced"></param>
        /// <param name="helpEnabled"></param>
        private void PrefabField(Resource res, bool advanced, bool helpEnabled)
        {
            GameObject prefab = res.Prefab;
            if (prefab == null && !advanced)
                return;
            prefab = (GameObject)m_editorUtils.ObjectField("Prefab", prefab, typeof(GameObject), true, helpEnabled);
            if (prefab != res.Prefab)
            {
                if (prefab != null)
                {
                    ReplaceResourcePrefab(res, prefab);
                    m_spawner.IsDirty = true;
                }
                else
                    GeNaDebug.LogWarningFormat("Prefab was set to null for Resource [{0}]. This is an invalid operation and was ignored. You can delete the Resource or replace it with a blank GameObject if you wish.", res.Name);
            }
        }
        /// <summary>
        /// Draw the static switch for a res
        /// </summary>
        private bool StaticSwitch(Prototype proto, Resource res)
        {
            // We only use this for prefab resources
            if (res.ResourceType == Constants.ResourceType.Prefab)
            {
                Constants.ResourceStatic val = res.Static;
                val = (Constants.ResourceStatic)m_editorUtils.Toolbar((int)res.Static, Enum.GetNames(typeof(Constants.ResourceStatic)), GUILayout.ExpandWidth(false));
                if (val != res.Static)
                    res.SetStatic(proto, val);
                return true;
            }
            return false;
        }
        private GameObject tempSpawnerObject = null;
        /// <summary>
        /// Handle drop area for new objects
        /// </summary>
        public bool DrawPrefabGUI()
        {
            // Ok - set up for drag and drop
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            string dropMsg = m_editorUtils.GetTextValue("Add proto drop box msg");
            GUI.Box(dropArea, dropMsg, Styles.gpanel);
            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (!dropArea.Contains(evt.mousePosition))
                    return false;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    List<GameObject> resources = new List<GameObject>();
                    // Handle game objects / prefabs
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            GameObject geNaSpawnerPrefab = GeNaEditorUtility.GetPrefabAsset(go);
                            GeNaSpawner geNaSpawner = geNaSpawnerPrefab != null ? geNaSpawnerPrefab.GetComponent<GeNaSpawner>() : null;
                            if (geNaSpawnerPrefab != null && geNaSpawner != null && geNaSpawner != m_spawner)
                            {
                                // We dont want to spawn spawners
                                tempSpawnerObject = new GameObject(geNaSpawner.name);
                                GeNaSubSpawnerDecorator subSpawnerDecorator = tempSpawnerObject.AddComponent<GeNaSubSpawnerDecorator>();
                                subSpawnerDecorator.SubSpawner = geNaSpawner;
                                resources.Add(tempSpawnerObject);
                            }
                            else if (m_spawner != null)
                                resources.Add(go);
                        }
                    }
                    // Handle speedtrees
                    foreach (string path in DragAndDrop.paths)
                    {
                        // Update in case unity has messed with it 
                        if (path.StartsWith("Assets"))
                        {
                            // Check file type and process as we can
                            string fileType = Path.GetExtension(path).ToLower();
                            // Check for speed trees - and add them
                            if (fileType == ".spm")
                            {
                                GameObject speedTree = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                                if (speedTree != null)
                                    resources.Add(speedTree);
                                else
                                    GeNaDebug.LogWarning("Unable to load " + path);
                            }
                        }
                    }

                    // Start managing them
                    AddGameObjects(SpawnerData, resources);
                    if (tempSpawnerObject != null)
                    {
                        GeNaEvents.Destroy(tempSpawnerObject);
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region Utilities
        /// <summary>
        /// Deletes the editor prefs key
        /// </summary>
        public void DeleteEditorPrefsKeys()
        {
            EditorPrefs.DeleteKey("GeNa Performance Rating");
            EditorPrefs.DeleteKey("GeNa Performance Rating Time");
        }
        /// <summary>
        /// Checks to see if the asset is null
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        private static bool CheckForAsset(GameObject asset)
        {
            if (asset == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="fileName">File name to search for</param>
        /// <returns></returns>
        public static string GetAssetPath(string fileName)
        {
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string[] assets = AssetDatabase.FindAssets(fName, null);
            foreach (string asset in assets)
            {
                string path = AssetDatabase.GUIDToAssetPath(asset);
                if (Path.GetFileName(path) == fileName)
                {
                    return path;
                }
            }
            return "";
        }
        /// <summary>
        /// Get a unique name
        /// </summary>
        /// <param name="name">The original name</param>
        /// <param name="names">The names dictionary</param>
        /// <returns>The new unique name</returns>
        private static string GetUniqueName(string name, ref HashSet<string> names)
        {
            int idx = 0;
            string newName = name;
            while (names.Contains(newName))
            {
                newName = name + " " + idx.ToString();
                idx++;
            }
            names.Add(newName);
            return newName;
        }
        /// <summary>
        /// Returns the formatted time since the UndoRecord was recorded.
        /// </summary>
        /// <returns></returns>
        public static string GetTimeDelta()
        {
            TimeSpan delta = TimeSpan.FromSeconds(PWCommon5.Utils.GetFrapoch()); // - record.Time);
            return string.Format("{0}{1}m", (int)delta.TotalHours > 0 ? (int)delta.TotalHours + "h " : "", delta.Minutes);
        }
        /// <summary>
        /// Return true if the resource list provided has prefabs
        /// </summary>
        /// <param name="sourcePrototypes"></param>
        /// <returns></returns>
        public static bool HasPrefabs(IEnumerable<Prototype> sourcePrototypes)
        {
            return sourcePrototypes.Any(srcProto => srcProto.HasType(Constants.ResourceType.Prefab));
        }
        /// <summary>
        /// Return true if the resource list provided has trees
        /// </summary>
        /// <param name="sourcePrototypes"></param>
        /// <returns></returns>
        public static bool HasTrees(IEnumerable<Prototype> sourcePrototypes)
        {
            return sourcePrototypes.Any(srcProto => srcProto.HasType(Constants.ResourceType.TerrainTree));
        }
        /// <summary>
        /// Return true if the resource list provided has textures
        /// </summary>
        /// <param name="sourcePrototypes"></param>
        /// <returns></returns>
        public static bool HasTextures(IEnumerable<Prototype> sourcePrototypes)
        {
            return sourcePrototypes.Any(srcProto => srcProto.HasType(Constants.ResourceType.TerrainTexture));
        }
        public static void MakeTextureUncompressed(Texture2D texture)
        {
            if (texture == null)
                return;
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null && tImporter.textureCompression != TextureImporterCompression.Uncompressed)
            {
                tImporter.textureCompression = TextureImporterCompression.Uncompressed;
                tImporter.SaveAndReimport();
                AssetDatabase.Refresh();
            }
        }
        public static Texture2D DuplicateTexture(Texture2D source)
        {
            byte[] pix = source.GetRawTextureData();
            Texture2D readableText = new Texture2D(source.width, source.height, source.format, source.mipmapCount, true);
            readableText.LoadRawTextureData(pix);
            readableText.Apply();
            return readableText;
        }
        public static Texture2D MakeTextureReadable(Texture2D texture)
        {
            if (texture == null)
                return null;
            if (texture.isReadable)
                return texture;
            return DuplicateTexture(texture);

            //byte[] tmp = texture.GetRawTextureData();
            //Texture2D tmpTexture = new Texture2D(texture.width, texture.height);
            //tmpTexture.LoadRawTextureData(tmp);
            //return tmpTexture;
            //string assetPath = AssetDatabase.GetAssetPath(texture);
            //var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            //if (tImporter != null && tImporter.isReadable != true)
            //{
            //    tImporter.isReadable = true;
            //    tImporter.SaveAndReimport();
            //    AssetDatabase.Refresh();
            //}
        }
        /// <summary>
        /// Process prefab replacement
        /// </summary>
        private void ReplaceResourcePrefab(Resource resource, GameObject go)
        {
            resource.Name = go.name;
            GameObject prefabAsset = GeNaEditorUtility.GetPrefabAsset(go);
            if (prefabAsset == null)
                prefabAsset = go;
            resource.AddPrefab(prefabAsset, m_spawner.Palette);
            // Get bounds
            Bounds localColliderBounds = GeNaUtility.GetLocalObjectBounds(go);
            // Get colliders
            resource.HasRootCollider = GeNaUtility.HasRootCollider(go);
            resource.HasColliders = GeNaUtility.HasColliders(go);
            // Get meshes
            resource.HasMeshes = GeNaUtility.HasMeshes(go);
            // Get rigid body
            resource.HasRigidbody = GeNaUtility.HasRigidBody(go);
            // If top level resource
            resource.BasePosition = resource.ParentID == -1 ? Vector3.zero : go.transform.localPosition;
            resource.BaseRotation = go.transform.localEulerAngles;
            resource.BaseScale = go.transform.localScale;
            resource.BaseColliderCenter = localColliderBounds.center;
            resource.BaseColliderScale = localColliderBounds.size;
            if (GeNaUtility.ApproximatelyEqual(go.transform.localScale.x, go.transform.localScale.y, 0.000001f) &&
                GeNaUtility.ApproximatelyEqual(go.transform.localScale.x, go.transform.localScale.z, 0.000001f))
                resource.SameScale = true;
            else
                resource.SameScale = false;
            // We can only determine if it is a prefab in the editor
            if (GeNaEditorUtility.IsPrefab(go))
            {
#if UNITY_2018_3_OR_NEWER
                resource.Prefab = GeNaEditorUtility.GetPrefabAsset(go);
                if (resource.Prefab == null)
                    resource.Prefab = go;
#else
                if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
                {
                    resource.Prefab = GetPrefabAsset(go);
                }
                else
                {
                    resource.Prefab = go;
                }
#endif
                if (resource.Prefab != null)
                {
                    //Get its asset ID
                    string path = AssetDatabase.GetAssetPath(resource.Prefab);
                    if (!string.IsNullOrEmpty(path))
                    {
                        resource.AssetID = AssetDatabase.AssetPathToGUID(path);
                        resource.AssetName = GeNaEditorUtility.GetAssetName(path);
                    }

                    // Get flags
                    SpawnFlags spawnFlags = resource.SpawnFlags;
                    StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(resource.Prefab);
                    spawnFlags.FlagBatchingStatic = (flags & StaticEditorFlags.BatchingStatic) == StaticEditorFlags.BatchingStatic;
                    spawnFlags.FlagLightmapStatic = (flags & StaticEditorFlags.ContributeGI) == StaticEditorFlags.ContributeGI;
                    spawnFlags.FlagOccludeeStatic = (flags & StaticEditorFlags.OccludeeStatic) == StaticEditorFlags.OccludeeStatic;
                    spawnFlags.FlagOccluderStatic = (flags & StaticEditorFlags.OccluderStatic) == StaticEditorFlags.OccluderStatic;
                    spawnFlags.FlagReflectionProbeStatic = (flags & StaticEditorFlags.ReflectionProbeStatic) == StaticEditorFlags.ReflectionProbeStatic;
             
#if !UNITY_2022_2_OR_NEWER
                    spawnFlags.FlagNavigationStatic = (flags & StaticEditorFlags.NavigationStatic) == StaticEditorFlags.NavigationStatic;
                    spawnFlags.FlagOffMeshLinkGeneration = (flags & StaticEditorFlags.OffMeshLinkGeneration) == StaticEditorFlags.OffMeshLinkGeneration;
#endif
                }
                else
                    GeNaDebug.LogErrorFormat("Unable to get prefab for '{0}'", resource.Name);
            }
            //Else this is just a GO (container in the tree) not a prefab.
            else
            {
                resource.ContainerOnly = true;
                // Warn the user if it has more components than just the Transform since it's not a prefab.
                Component[] components = go.GetComponents<Component>();
                if (components != null && components.Length > 1)
                {
                    GeNaDebug.LogWarningFormat("Gameobject '{0}' has Components but it's not a Prefab Instance. Make it into a Prefab if you wish to keep its Components information for spawning.",
                        go.name);
                }
            }
            resource.RecalculateBounds();
            EditorUtility.SetDirty(m_spawner.Palette);
        }
        #endregion
        #region Validation
        /// <summary>
        /// Validates all the aseets in the spawner
        /// </summary>
        /// <param name="genaSpawner"></param>
        /// <param name="spawner"></param>
        /// <param name="terrain"></param>
        /// <param name="overrideSceneLoaded"></param>
        public static void ValidateSpawnerPrototypes(GeNaSpawner genaSpawner, GeNaSpawnerData spawner, Terrain terrain, bool overrideSceneLoaded = false)
        {
            if (genaSpawner == null)
            {
                return;
            }
            if (!genaSpawner.gameObject.scene.isLoaded && overrideSceneLoaded == false)
            {
                return;
            }
            if (spawner == null)
                return;
            // ValidatePrefabPrototypes(spawner);
            ValidateTerrainPrototypes(spawner, terrain);
            ValidateTerrainGrassPrototypes(spawner, terrain);
            ValidateTerrainTexturePrototypes(spawner, terrain);
            List<GeNaSpawnerData> subSpawners = new List<GeNaSpawnerData>();
            foreach (Prototype prototype in spawner.SpawnPrototypes)
            {
                foreach (Resource resource in prototype.Resources)
                {
                    if (resource.SubSpawnerData != null)
                    {
                        subSpawners.Add(resource.SubSpawnerData);
                    }
                }
            }
            ValidateSubSpawners(subSpawners, terrain);
        }
        /// <summary>
        /// Validate all terrain trees in the spawner
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="terrain"></param>
        private static void ValidateTerrainPrototypes(GeNaSpawnerData spawner, Terrain terrain)
        {
            if (spawner == null)
            {
                return;
            }
            if (terrain != null)
            {
                List<Prototype> prototypes = spawner.SpawnPrototypes;
                foreach (Prototype prototype in prototypes)
                {
                    IReadOnlyList<Resource> resources = prototype.Resources;
                    foreach (Resource resource in resources)
                    {
                        if (!m_noToAllValidations)
                        {
                            if (resource.ResourceType == Constants.ResourceType.TerrainTree)
                            {
                                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                                if (prefab == null)
                                {
                                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPath(resource.AssetName + "Prefab"));
                                }
                                if (!GeNaEditorUtility.IsTreeOnTerrain(prefab, terrain))
                                {
                                    if (!m_yesToAllValidations)
                                    {
                                        if (EditorUtility.DisplayDialog("Add Resources", "The terrain trees in this spawner are not found on the terrain. The spawner may not function correctly if the trees are not on the terrain, we recommend that you add them. Would you like to add them?", "Yes", "No"))
                                        {
                                            m_yesToAllValidations = true;
                                        }
                                        else
                                        {
                                            m_noToAllValidations = true;
                                        }
                                    }
                                    if (m_yesToAllValidations)
                                    {
                                        resource.TerrainProtoIdx = GeNaEditorUtility.AddTreeResourceToTerrain(prefab, terrain);
                                    }
                                }
                                else
                                {
                                    resource.TerrainProtoIdx = GeNaEditorUtility.GetTreeID(terrain.terrainData.treePrototypes, prefab);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Validate all prefabs in the spawner
        /// </summary>
        /// <param name="spawner"></param>
        private static void ValidatePrefabPrototypes(GeNaSpawnerData spawner)
        {
            if (spawner == null)
            {
                return;
            }
            List<Prototype> prototypes = spawner.SpawnPrototypes;
            foreach (Prototype prototype in prototypes)
            {
                IReadOnlyList<Resource> resources = prototype.Resources;
                foreach (Resource resource in resources)
                {
                    if (resource.ResourceType == Constants.ResourceType.Prefab)
                    {
                        if (!CheckForAsset(resource.Prefab))
                        {
                            resource.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                            if (resource.Prefab == null)
                            {
                                resource.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPath(resource.AssetName + "prefab"));
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Validate sub spawners
        /// </summary>
        /// <param name="subSpawners"></param>
        /// <param name="terrain"></param>
        private static void ValidateSubSpawners(List<GeNaSpawnerData> subSpawners, Terrain terrain)
        {
            if (subSpawners.Count < 1)
            {
                return;
            }
            foreach (GeNaSpawnerData subspawner in subSpawners)
            {
                //ValidatePrefabPrototypes(subspawner);
                ValidateTerrainPrototypes(subspawner, terrain);
                ValidateTerrainGrassPrototypes(subspawner, terrain);
                ValidateTerrainTexturePrototypes(subspawner, terrain);
            }
        }
        /// <summary>
        /// Validates all terrains grass in the spawner
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="terrain"></param>
        private static void ValidateTerrainGrassPrototypes(GeNaSpawnerData spawner, Terrain terrain)
        {
            if (spawner == null)
            {
                return;
            }
            if (terrain != null)
            {
                List<Prototype> prototypes = spawner.SpawnPrototypes;
                foreach (Prototype prototype in prototypes)
                {
                    IReadOnlyList<Resource> resources = prototype.Resources;
                    foreach (Resource resource in resources)
                    {
                        if (!m_noToAllValidations)
                        {
                            if (resource.ResourceType == Constants.ResourceType.TerrainGrass)
                            {
                                Texture2D grassTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                                if (grassTexture == null)
                                {
                                    grassTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetAssetPath(resource.Name));
                                }

                                if (grassTexture != null)
                                {
                                  
                                    if (!GeNaEditorUtility.IsGrassOnTerrain(grassTexture, terrain))
                                    {
                                        if (!m_yesToAllValidations)
                                        {
                                            if (EditorUtility.DisplayDialog("Add Resources", "The terrain grasses in this spawner are not found on the terrain. The spawner may not function correctly if the grasses are not on the terrain, we recommend that you add them. Would you like to add them?", "Yes", "No"))
                                            {
                                                m_yesToAllValidations = true;
                                            }
                                            else
                                            {
                                                m_noToAllValidations = true;
                                            }
                                        }
                                        if (m_yesToAllValidations)
                                        {
                                            resource.TerrainProtoIdx = GeNaEditorUtility.AddGrassResourceToTerrain(resource.DetailPrototypeData, terrain);
                                        }
                                    }
                                    else
                                    {
                                        resource.TerrainProtoIdx = GeNaEditorUtility.GetGrassID(terrain.terrainData.detailPrototypes, grassTexture);
                                    }  
                                }
                                else
                                {
                                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                                    if (prefab == null)
                                    {
                                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPath(resource.AssetName + "Prefab"));
                                    }
                                    if (!GeNaEditorUtility.IsGrassOnTerrain(prefab, terrain))
                                    {
                                        if (!m_yesToAllValidations)
                                        {
                                            if (EditorUtility.DisplayDialog("Add Resources", "The terrain grass in this spawner are not found on the terrain. The spawner may not function correctly if the grass are not on the terrain, we recommend that you add them. Would you like to add them?", "Yes", "No"))
                                            {
                                                m_yesToAllValidations = true;
                                            }
                                            else
                                            {
                                                m_noToAllValidations = true;
                                            }
                                        }
                                        if (m_yesToAllValidations)
                                        {
                                            resource.TerrainProtoIdx = GeNaEditorUtility.AddTreeResourceToTerrain(prefab, terrain);
                                        }
                                    }
                                    else
                                    {
                                        resource.TerrainProtoIdx = GeNaEditorUtility.GetGrassID(terrain.terrainData.detailPrototypes, prefab);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Validates all terrain textures in the spawner
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="terrain"></param>
        private static void ValidateTerrainTexturePrototypes(GeNaSpawnerData spawner, Terrain terrain)
        {
            if (spawner == null)
            {
                return;
            }
            if (terrain != null)
            {
                List<Prototype> prototypes = spawner.SpawnPrototypes;
                foreach (Prototype prototype in prototypes)
                {
                    IReadOnlyList<Resource> resources = prototype.Resources;
                    foreach (Resource resource in resources)
                    {
                        if (!m_noToAllValidations)
                        {
                            if (resource.ResourceType == Constants.ResourceType.TerrainTexture)
                            {
                                Texture2D terrainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(resource.AssetID));
                                if (terrainTexture == null)
                                {
                                    terrainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetAssetPath(resource.Name));
                                }
                                if (!GeNaEditorUtility.IsTextureOnTerrain(terrainTexture, terrain))
                                {
                                    if (!m_yesToAllValidations)
                                    {
                                        if (EditorUtility.DisplayDialog("Add Resources", "The terrain textures in this spawner are not found on the terrain. The spawner may not function correctly if the textures are not on the terrain, we recommend that you add them. Would you like to add them?", "Yes", "No"))
                                        {
                                            m_yesToAllValidations = true;
                                        }
                                        else
                                        {
                                            m_noToAllValidations = true;
                                        }
                                    }
                                    if (m_yesToAllValidations)
                                    {
                                        resource.TerrainProtoIdx = GeNaEditorUtility.AddTextureResourceToTerrain(resource.TexturePrototypeData, terrain);
                                    }
                                }
                                else
                                {
                                    resource.TerrainProtoIdx = GeNaEditorUtility.GetTextureID(terrain.terrainData.terrainLayers, terrainTexture);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}