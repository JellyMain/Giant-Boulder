using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using PWCommon5;
using UnityEditor.Graphs;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Directory = System.IO.Directory;
using Object = UnityEngine.Object;
#if !UNITY_2021_2_OR_NEWER
using UnityEngine.Experimental.TerrainAPI;
#endif
namespace GeNa.Core
{
    public static class GeNaEditorUtility
    {
        #region Variables
        private static GenaDefaults m_defaults;
        public static bool DisplayProgress = false;
        public static double RefreshRate = .25d;
        public static double StartTime = 0d;
        public static double DisplayTimer = 0d;
        #endregion
        #region Properties
        public static GenaDefaults Defaults
        {
            get
            {
                // Doesnt exist yet?
                if (m_defaults == null)
                {
                    // Try Finding it in Assets
                    string[] guids = AssetDatabase.FindAssets("GenaDefaults");
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (path.Contains("GenaDefaults.asset"))
                            m_defaults = AssetDatabase.LoadAssetAtPath<GenaDefaults>(path);
                    }

                    // Still doesnt exist?
                    if (m_defaults == null)
                    {
                        // Create a new one
                        m_defaults = ScriptableObject.CreateInstance<GenaDefaults>();
                        //TODO : Manny : This should not be hard coded
                        string geNaFolder = "Assets/Procedural Worlds/GeNa";
                        if (!Directory.Exists(geNaFolder))
                            Directory.CreateDirectory(geNaFolder);
                        AssetDatabase.CreateAsset(m_defaults, $"{geNaFolder}/GenaDefaults.asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
                return m_defaults;
            }
        }
        #endregion
        #region Internal Methods
        public static Material GetVisulizationMaterial(Material mat, string shaderName, Constants.RenderPipeline pipeline, bool forceUpdate = false)
        {
            if (mat == null)
            {
                return null;
            }
            if (!forceUpdate)
            {
                if (mat.shader == null || mat.shader.name != shaderName || Defaults.RenderPipeline != pipeline)
                {
                    //Apply shader
                    Shader visulizationShader = Shader.Find(shaderName);
                    if (visulizationShader != null)
                    {
                        mat.shader = ProcessShaderSetup(visulizationShader, pipeline);
                        Defaults.RenderPipeline = pipeline;
                        if (mat.shader != null)
                        {
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mat.shader), ImportAssetOptions.ForceUpdate);
                        }
                    }
                }
                else
                {
                    return mat;
                }
            }
            else
            {
                //Apply shader
                Shader visulizationShader = Shader.Find(shaderName);
                if (visulizationShader != null)
                {
                    mat.shader = ProcessShaderSetup(visulizationShader, pipeline);
                    Defaults.RenderPipeline = pipeline;
                    if (mat.shader != null)
                    {
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mat.shader), ImportAssetOptions.ForceUpdate);
                    }
                }
            }
            return mat;
        }
        public static Shader ProcessShaderSetup(Shader currentShader, Constants.RenderPipeline pipeline)
        {
            EditorUtility.DisplayProgressBar("Setting Up Shader!", "We are setting up the shader for " + pipeline + " please wait...", 0.5f);
            Shader updatedShader = currentShader;
            switch (pipeline)
            {
                case Constants.RenderPipeline.BuiltIn:
                case Constants.RenderPipeline.Universal:
                {
                    string path = AssetDatabase.GetAssetPath(currentShader);
                    string BuiltInTextFile = AssetUtils.GetAssetPath(GeNaShaderID.m_visulizationShaderFileBuiltIn);
                    if (!string.IsNullOrEmpty(BuiltInTextFile))
                    {
                        string newContent = File.ReadAllText(BuiltInTextFile);
                        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(newContent))
                        {
                            return currentShader;
                        }
                        path = ConvertFile(path, Constants.FileConvertType.TextFile);
                        string absolutePath = LocalToAbsolutePath(path);
                        File.WriteAllText(absolutePath, newContent);
                        ConvertFile(path, Constants.FileConvertType.ShaderFile);
                        updatedShader = Shader.Find(GeNaShaderID.m_visulizationShader);
                    }
                    break;
                }
                // case Constants.RenderPipeline.Universal:
                // {
                //     string URPSRCFile = AssetUtils.GetAssetPath(GeNaShaderID.m_visulizationShaderFileURPSRC);
                //     AssetDatabase.ImportAsset(URPSRCFile);
                //     string path = AssetDatabase.GetAssetPath(currentShader);
                //     string URPTextFile = AssetUtils.GetAssetPath(GeNaShaderID.m_visulizationShaderFileURP);
                //     if (!string.IsNullOrEmpty(URPTextFile))
                //     {
                //         string newContent = File.ReadAllText(URPTextFile);
                //         if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(newContent))
                //         {
                //             return currentShader;
                //         }
                //         path = ConvertFile(path, Constants.FileConvertType.TextFile);
                //         string absolutePath = LocalToAbsolutePath(path);
                //         File.WriteAllText(absolutePath, newContent);
                //         ConvertFile(path, Constants.FileConvertType.ShaderFile);
                //         updatedShader = Shader.Find(GeNaShaderID.m_visulizationShader);
                //     }
                //     break;
                // }
                case Constants.RenderPipeline.HighDefinition:
                {
                    string HDRPSRCFile = AssetUtils.GetAssetPath(GeNaShaderID.m_visulizationShaderFileHDRPSRC);
                    AssetDatabase.ImportAsset(HDRPSRCFile);
                    string path = AssetDatabase.GetAssetPath(currentShader);
                    string HDRPTextFile = AssetUtils.GetAssetPath(GeNaShaderID.m_visulizationShaderFileHDRP);
                    if (!string.IsNullOrEmpty(HDRPTextFile))
                    {
                        string newContent = File.ReadAllText(HDRPTextFile);
                        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(newContent))
                        {
                            return currentShader;
                        }
                        path = ConvertFile(path, Constants.FileConvertType.TextFile);
                        string absolutePath = LocalToAbsolutePath(path);
                        File.WriteAllText(absolutePath, newContent);
                        ConvertFile(path, Constants.FileConvertType.ShaderFile);
                        updatedShader = Shader.Find(GeNaShaderID.m_visulizationShader);
                    }
                    break;
                }
            }
            EditorUtility.ClearProgressBar();
            return updatedShader;
        }
        public static string ConvertFile(string path, Constants.FileConvertType type)
        {
            string filePath = path;
            string newPath = string.Empty;
            if (!string.IsNullOrEmpty(filePath))
            {
                switch (type)
                {
                    case Constants.FileConvertType.TextFile:
                    {
                        string newFilePath = Path.ChangeExtension(filePath, "txt");
                        AssetDatabase.MoveAsset(filePath, newFilePath);
                        newPath = newFilePath;
                        AssetDatabase.SaveAssets();
                        break;
                    }
                    case Constants.FileConvertType.ShaderFile:
                    {
                        string newFilePath = Path.ChangeExtension(filePath, "shader");
                        AssetDatabase.MoveAsset(filePath, newFilePath);
                        newPath = newFilePath;
                        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(newFilePath);
                        if (shader != null)
                        {
                            EditorUtility.SetDirty(shader);
                        }
                        AssetDatabase.SaveAssets();
                        break;
                    }
                }
            }
            return newPath;
        }
        public static string LocalToAbsolutePath(string absolutepath)
        {
            if (absolutepath.StartsWith("Assets/"))
            {
                return absolutepath.Replace("Assets/", Application.dataPath + "/");
            }
            return absolutepath;
        }
        public static Material ProcessVisulizationMaterial(bool forceUpdate = false)
        {
            //Load material if it's null
            Material quadMaterial = QuadManager.m_quadMaterial;
            if (quadMaterial == null)
            {
                quadMaterial = GeNaUtility.GetResourceMaterial("GeNa_QuadMaterial");
            }

            //Gets and setup the material based on the pipeline
            quadMaterial = GetVisulizationMaterial(quadMaterial, GeNaShaderID.m_visulizationShader, GeNaUtility.GetActivePipeline(), forceUpdate);
            QuadManager.m_quadMaterial = quadMaterial;
            return QuadManager.m_quadMaterial;
        }
        public static bool FinalizeAll(GeNaSpline spline, GameObject parentTo)
        {
            if (spline == null)
            {
                Debug.LogWarning("The Spline was empty, finalize all will not finish executing.");
                return false;
            }

            //Execute all extensions
            for (int i = 0; i < spline.Extensions.Count; i++)
            {
                GeNaSplineExtension extension = spline.Extensions[i].Extension;
                if (extension != null)
                {
                    EditorUtility.DisplayProgressBar("Baking " + extension.name, "Baking the " + extension.name + " this could take a while.", (float)i / spline.Extensions.Count);
                    GameObject newObject = extension.Bake();
                    if (newObject != null)
                    {
                        if (parentTo != null)
                        {
                            newObject.transform.SetParent(parentTo.transform);
                        }
                    }
                }
                else
                {
                    Debug.LogError("A extension was empty, please insure all extensions are not null before performing this action.");
                    return false;
                }
            }
            EditorUtility.ClearProgressBar();
            return true;
        }
        public static void BakeSpline(GameObject splineParent, GameObject splineObject, GeNaSpline spline)
        {
            if (splineParent == null || splineObject == null || spline == null)
            {
                return;
            }
            if (EditorUtility.DisplayDialog("Baking Spline",
                    "You are about to bake your spline. This will remove the spline object from your scene and bake the results. You can undo this using Edit/Undo or Ctrl + Z. Are you sure you want to proceed?",
                    "Yes", "No"))
            {
                Undo.SetTransformParent(splineParent.transform, null, "Spline Baked");
                splineParent.transform.SetParent(null);
                string[] names = GeNaUtility.GetBakeSplineNames(splineParent);
                Undo.RecordObject(splineParent, "Spline Baked");
                splineParent.name = ObjectNames.GetUniqueName(names, splineParent.name);
                GeNaUtility.AddBakedSplineName(splineParent.name);
                Undo.RecordObject(spline, "Spline Baked");
                Undo.DestroyObjectImmediate(splineObject);
                Undo.FlushUndoRecordObjects();
            }
        }
        public static void ForceUpdate()
        {
            // If the Editor isn't playing
            if (!Application.isPlaying)
            {
                // Force Update to run in the Editor
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
        }
        public static void SubscribeEvents()
        {
            GeNaUndoRedo.GetCurrentGroup = GetCurrentGroup;
            GeNaEvents.ErrorDialogue = ErrorDialogue;
            GeNaEvents.Simulate = Simulate;
            GeNaEvents.Instantiate = Instantiate;
            GeNaEvents.StartCoroutine = StartCoroutine;
            GeNaEvents.Destroy = Destroy;
            GeNaEvents.MarkSceneDirty = MarkSceneDirty;
            GeNaEvents.RepaintAll = RepaintAll;
            GeNaEvents.TryGetGUID = TryGetGUID;
            GeNaEvents.SetNavigationStatic = SetNavigationFlags;
            GeNaEvents.OptimiseGameObject = OptimiseGameObject;
            GeNaEvents.UnOptimiseGameObject = UnOptimiseGameObject;
            GeNaEvents.GetQuadMaterial = GetVisulizationMaterial;
            TerrainCallbacks.textureChanged -= OnTerrainTextureChanged;
            TerrainCallbacks.textureChanged += OnTerrainTextureChanged;
        }
        public static void OnTerrainTextureChanged(Terrain terrain, string textureName, RectInt texelRegion, bool synched)
        {
            GeNaEvents.textureChanged?.Invoke(terrain, textureName, texelRegion, synched);
        }
        public static int GetCurrentGroup() => Undo.GetCurrentGroup();
        public static void MarkSceneDirty(Scene scene)
        {
            if (Application.isPlaying)
                return;
            EditorSceneManager.MarkSceneDirty(scene);
        }
        public static void RepaintAll()
        {
            if (Application.isPlaying)
                return;
            SceneView.RepaintAll();
        }
        public static void ScheduleSpawn(SpawnerEntry entry)
        {
            GeNaUtility.ScheduleSpawn(entry);
            ForceUpdate();
        }
        public static void ScheduleIterate(SpawnerEntry entry)
        {
            GeNaUtility.ScheduleIterate(entry);
            ForceUpdate();
        }
        public static GameObject Instantiate(GameObject prefab)
        {
            GameObject asset = GetPrefabAsset(prefab);
            if (asset != null)
                return PrefabUtility.InstantiatePrefab(asset) as GameObject;
            return Object.Instantiate(prefab);
        }
        public static void Destroy(Object @object)
        {
            Object.DestroyImmediate(@object);
        }
        #region Events
        public static bool ErrorDialogue(string title, string message, string ok)
        {
            return EditorUtility.DisplayDialog(title, message, ok);
        }
        public static void Error(string message)
        {
            Debug.LogError(message);
        }
        public static void SetNavigationFlags(GameObject go, int flags, bool setChildren = false)
        {
            // Handle rubbish
            if (go == null)
                return;
            // Set on this game object
            GameObjectUtility.SetStaticEditorFlags(go, (StaticEditorFlags)flags);
            if (setChildren)
            {
                Transform transform = go.transform;
                // Do same for all child game objects
                for (int childIdx = 0; childIdx < transform.childCount; childIdx++)
                    SetNavigationFlags(transform.GetChild(childIdx).gameObject, flags, setChildren);
            }
        }
        public static void UnOptimiseGameObject(Resource resource, GameObject go)
        {
            // Non optimised - set the flags as specified in its settings
            StaticEditorFlags flag = 0;
            SpawnFlags spawnFlags = resource.SpawnFlags;
            flag |= spawnFlags.FlagBatchingStatic ? StaticEditorFlags.BatchingStatic : flag;
            flag |= spawnFlags.FlagOccludeeStatic ? StaticEditorFlags.OccludeeStatic : flag;
            flag |= spawnFlags.FlagOccluderStatic ? StaticEditorFlags.OccluderStatic : flag;
            flag |= spawnFlags.FlagLightmapStatic ? StaticEditorFlags.ContributeGI : flag;
            flag |= spawnFlags.FlagReflectionProbeStatic ? StaticEditorFlags.ReflectionProbeStatic : flag;
            
#if !UNITY_2022_2_OR_NEWER
            flag |= spawnFlags.FlagNavigationStatic ? StaticEditorFlags.NavigationStatic : flag;
            flag |= spawnFlags.FlagOffMeshLinkGeneration ? StaticEditorFlags.OffMeshLinkGeneration : flag;
#endif

            bool setChildren = spawnFlags.ApplyToChildren;

            // And do same to game object m_children
            SetNavigationFlags(go, (int)flag, setChildren);
        }
        public static void OptimiseGameObject(Resource resource, GameObject go)
        {
            SpawnFlags spawnFlags = resource.SpawnFlags;
            // If there is are any m_mesh renderers, then set blend probes on, lightprobes need them to work
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in renderers)
            {
                rend.lightProbeUsage = LightProbeUsage.BlendProbes;
                rend.reflectionProbeUsage = spawnFlags.FlagIsOutdoorObject ? ReflectionProbeUsage.BlendProbesAndSkybox : ReflectionProbeUsage.BlendProbes;
            }

            // Then see if we can optimise the editor flags
            StaticEditorFlags flag = 0;
            // Mark everything as static as possible if not moving
            if (!spawnFlags.FlagMovingObject)
            {
                flag |= StaticEditorFlags.BatchingStatic;
                flag |= StaticEditorFlags.OccludeeStatic;
                flag |= StaticEditorFlags.OccluderStatic;
                flag |= StaticEditorFlags.ReflectionProbeStatic;
   
#if !UNITY_2022_2_OR_NEWER
                flag |= StaticEditorFlags.NavigationStatic;
                flag |= StaticEditorFlags.OffMeshLinkGeneration;
#endif
            }
            bool setChildren = spawnFlags.ApplyToChildren;

            // And do it to the game object and all its m_children
            SetNavigationFlags(go, (int)flag, setChildren);
        }
        public static Material GetQuadMaterial() => GetVisulizationMaterial();
        public static Material GetVisulizationMaterial(bool forceUpdate = false) => ProcessVisulizationMaterial(forceUpdate);
        #endregion
        #region Prefabs
        public static T GetPrefabAsset<T>(T @object) where T : Object
        {
            T prefab;
#if UNITY_2018_2_OR_NEWER
            if (PrefabUtility.GetPrefabAssetType(@object) == PrefabAssetType.Variant)
            {
                prefab = PrefabUtility.GetCorrespondingObjectFromSourceAtPath(@object, AssetDatabase.GetAssetPath(@object));
                if (prefab == null)
                    prefab = PrefabUtility.GetCorrespondingObjectFromSource(@object);
            }
            else
                prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(@object);
#else
            prefab = PrefabUtility.GetPrefabParent(@object) as T;
#endif
            return prefab;
        }
        public static void DespawnEmptyParents(GeNaSpawnerData spawner)
        {
            // List<GameObject> newList = new List<GameObject>();
            // foreach (GameObject parent in spawner.ParentsUndoList.Where(parent => parent != null))
            // {
            //     if (parent.transform.childCount < 1)
            //     {
            //         Object.DestroyImmediate(parent);
            //         continue;
            //     }
            //     newList.Add(parent);
            // }
            // spawner.ParentsUndoList = newList;
        }
        public static void DespawnGameObjects(GeNaSpawnerData spawner, int protoIdx)
        {
            if (protoIdx < 0)
                return;
            Prototype proto = spawner.SpawnPrototypes[protoIdx];
            // Make sure we are in right place
            if (!proto.HasType(Constants.ResourceType.Prefab))
                return;
            // Clear the undo redo stacks
            // UndoObjectInternal.ClearUndoStack(spawner);
            // List<GameObject> newList = new List<GameObject>();
            // foreach (Resource resource in proto.GetResources().Where(resource => resource.ResourceType == Constants.ResourceType.Prefab))
            // {
            //     foreach (GameObject undo in spawner.PrefabUndoList)
            //     {
            //         // In case it's corrupted data
            //         if (undo == null)
            //             continue;
            //         bool prefabDeleted = false;
            //         GameObject prefabParent = GetPrefabAsset(undo);
            //         if (prefabParent != null)
            //         {
            //             if (prefabParent.GetInstanceID() == resource.Prefab.GetInstanceID())
            //             {
            //                 Object.DestroyImmediate(undo);
            //                 proto.InstancesSpawned -= resource.InstancesSpawned;
            //                 spawner.InstancesSpawned -= resource.InstancesSpawned;
            //                 prefabDeleted = true;
            //             }
            //         }
            //         // This still could be a container
            //         else
            //         {
            //             if (undo.name.StartsWith("_Sp_" + resource.Name))
            //             {
            //                 Object.DestroyImmediate(undo);
            //                 proto.InstancesSpawned -= resource.InstancesSpawned;
            //                 spawner.InstancesSpawned -= resource.InstancesSpawned;
            //                 prefabDeleted = true;
            //             }
            //         }
            //         if (!prefabDeleted)
            //             newList.Add(undo);
            //     }
            //     resource.ResetInstancesSpawned();
            // }
            // spawner.PrefabUndoList = newList;
            GeNaSpawnerInternal.DespawnEmptyParents(spawner);
        }
        public static bool IsPrefab(GameObject go)
        {
#if UNITY_2018_3_OR_NEWER
            // if (PrefabUtility.IsPartOfPrefabInstance(go))
            //     return false;
            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(go);
            return (prefabType == PrefabAssetType.Regular ||
                    prefabType == PrefabAssetType.Model ||
                    prefabType == PrefabAssetType.Variant);
#else
            PrefabType prefabType = PrefabUtility.GetPrefabType(go);
            return (prefabType == PrefabType.Prefab ||
                prefabType == PrefabType.ModelPrefab ||
                prefabType == PrefabType.PrefabInstance ||
                prefabType == PrefabType.ModelPrefabInstance);
#endif
        }
        public static string GetAssetName(string path)
        {
            if (string.IsNullOrEmpty(path) == false)
            {
                string[] filename = System.IO.Path.GetFileName(path).Split('.');
                if (filename.Length == 2)
                    return filename[0];
            }
            Debug.LogErrorFormat("Unable to determine prefab filename for path '{0}'", path);
            return null;
        }
        #endregion
        #region Tools
        public static void DrawSpawnRange(GeNaSpawnerData spawner, Vector3 origin, float range, Color? outerColor = null, Color? innerColor = null)
        {
            if (spawner.VisualizationFixed)
            {
                origin = spawner.SpawnOriginLocation;
            }
            Vector3 aboveOrigin = origin;
            // aboveOrigin.y += 5000f;
            Vector3[] outerNodes = null;
            Vector3[] innerNodes = null;
            Vector3 pivot = origin;
            float maxRotationY = spawner.PlacementCriteria.MaxRotationY;
            Quaternion angle = Quaternion.Euler(0f, maxRotationY, 0f);
            switch (spawner.SpawnRangeShape)
            {
                case Constants.SpawnRangeShape.Circle:
                    int res = Mathf.CeilToInt(24 + range * 0.1f);
                    outerNodes = new Vector3[res];
                    innerNodes = new Vector3[res];
                    float step = 360f / (res - 1);
                    float radius = range * 0.5f;
                    float innerRadius = radius * 0.99f;
                    for (int i = 0; i < outerNodes.Length; i++)
                    {
                        outerNodes[i] = new Vector3(Mathf.Sin(step * i * Mathf.Deg2Rad), 0f, Mathf.Cos(step * i * Mathf.Deg2Rad)) * radius + aboveOrigin;
                        innerNodes[i] = new Vector3(Mathf.Sin(step * i * Mathf.Deg2Rad), 0f, Mathf.Cos(step * i * Mathf.Deg2Rad)) * innerRadius + aboveOrigin;
                        float height = 0.0f;
                        GeNaSpawnerInternal.GetEdgeHeight(spawner, outerNodes[i], origin.y, out height);
                        outerNodes[i].y = innerNodes[i].y = height;
                    }
                    break;
                case Constants.SpawnRangeShape.Square:
                    res = 6 + Mathf.CeilToInt(range * 0.1f);
                    outerNodes = new Vector3[res * 4 + 1];
                    innerNodes = new Vector3[res * 4 + 1];
                    step = range / res;
                    radius = range * 0.5f;
                    innerRadius = radius * 0.99f;
                    for (int i = 0; i < res; i++)
                    {
                        float height = 0f;
                        int index = i;
                        outerNodes[index] = new Vector3(-radius, 0f, -radius + i * step) + aboveOrigin;
                        outerNodes[index] = GeNaUtility.RotatePointAroundPivot(outerNodes[index], pivot, angle);
                        innerNodes[index] = new Vector3(-innerRadius, 0f, -innerRadius + i * step) + aboveOrigin;
                        innerNodes[index] = GeNaUtility.RotatePointAroundPivot(innerNodes[index], pivot, angle);
                        GeNaSpawnerInternal.GetEdgeHeight(spawner, outerNodes[index], origin.y, out height);
                        outerNodes[index].y = innerNodes[index].y = height;
                        index += res;
                        outerNodes[index] = new Vector3(-radius + i * step, 0f, radius) + aboveOrigin;
                        outerNodes[index] = GeNaUtility.RotatePointAroundPivot(outerNodes[index], pivot, angle);
                        innerNodes[index] = new Vector3(-innerRadius + i * step, 0f, innerRadius) + aboveOrigin;
                        innerNodes[index] = GeNaUtility.RotatePointAroundPivot(innerNodes[index], pivot, angle);
                        GeNaSpawnerInternal.GetEdgeHeight(spawner, outerNodes[index], origin.y, out height);
                        outerNodes[index].y = innerNodes[index].y = height;
                        index += res;
                        outerNodes[index] = new Vector3(radius, 0f, radius - i * step) + aboveOrigin;
                        outerNodes[index] = GeNaUtility.RotatePointAroundPivot(outerNodes[index], pivot, angle);
                        innerNodes[index] = new Vector3(innerRadius, 0f, innerRadius - i * step) + aboveOrigin;
                        innerNodes[index] = GeNaUtility.RotatePointAroundPivot(innerNodes[index], pivot, angle);
                        GeNaSpawnerInternal.GetEdgeHeight(spawner, outerNodes[index], origin.y, out height);
                        outerNodes[index].y = innerNodes[index].y = height;
                        index += res;
                        outerNodes[index] = new Vector3(radius - i * step, 0f, -radius) + aboveOrigin;
                        outerNodes[index] = GeNaUtility.RotatePointAroundPivot(outerNodes[index], pivot, angle);
                        innerNodes[index] = new Vector3(innerRadius - i * step, 0f, -innerRadius) + aboveOrigin;
                        innerNodes[index] = GeNaUtility.RotatePointAroundPivot(innerNodes[index], pivot, angle);
                        GeNaSpawnerInternal.GetEdgeHeight(spawner, outerNodes[index], origin.y, out height);
                        outerNodes[index].y = innerNodes[index].y = height;
                    }

                    // And close the "circle"
                    outerNodes[outerNodes.Length - 1] = outerNodes[0];
                    innerNodes[innerNodes.Length - 1] = innerNodes[0];
                    break;
            }
            if (outerNodes != null)
            {
                Handles.color = outerColor ?? Color.blue;
                Handles.DrawAAPolyLine(6f, outerNodes);
            }
            if (innerNodes != null)
            {
                Handles.color = innerColor ?? Color.white;
                Handles.DrawAAPolyLine(1f, innerNodes);
            }
            // // We only got here if the mouse is over the sceneview - also only update if there was more than tiny movement of the mouse
            // if ((m_lastMousePos - mousePos).sqrMagnitude > 4f)
            // {
            //     m_lastMousePos = mousePos;
            // SceneView.lastActiveSceneView.Repaint();
            // }
        }
        public static void RenderTerrainModifier(Transform transform, TerrainModifier modifier)
        {
            TerrainModifier terrainModifier = modifier;
            float radius = terrainModifier.AreaOfEffect * .5f;
            Vector3 center = transform.position;
            Vector3 size = new Vector3(radius, 0f, radius);
            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Handles.DrawWireCube(Vector3.zero, size);
            Handles.matrix = oldMatrix;
        }
        public static void RenderSpawnRange(GeNaSpawnerData spawner, Vector3 position, float rotationY = 0f)
        {
            float radius = spawner.SpawnRange;
            SpawnerSettings settings = spawner.Settings;
            SpawnerSettings.AdvancedSettings advanced = settings.Advanced;
            float boundsOffset = advanced.BoundsOffset;
            float range = radius + boundsOffset;
            DrawSpawnRange(spawner, position, radius);
            DrawSpawnRange(spawner, position, range, Color.green);
            PlacementCriteria placementCriteria = spawner.PlacementCriteria;
            // Visualise it
            if (placementCriteria.RotationAlgorithm <= Constants.RotationAlgorithm.Fixed)
            {
                Handles.color = new Color(0f, 0f, 255f, 0.85f);
                if (GeNaUtility.ApproximatelyEqual(placementCriteria.MinRotationY, placementCriteria.MaxRotationY))
                {
                    Quaternion rotation = Quaternion.Euler(0f, placementCriteria.MinRotationY + rotationY, 0f);
                    float size = Mathf.Clamp(spawner.SpawnRange * 0.2f, 0.25f, 40f);
                    Handles.ArrowHandleCap(0, position, rotation, size, EventType.Repaint);
                }
                // else
                // {
                //     Quaternion rotation = Quaternion.AngleAxis(placementCriteria.MinRotationY, Vector3.up);
                //     Handles.DrawSolidArc(position, Vector3.up, rotation * Vector3.forward, angle, radius);
                // }
            }
        }
        public static bool ShowSpawnRange(GeNaSpawnerData spawner, out RaycastHit hitInfo, bool renderSpawnRange = true)
        {
            // Stop if not over the SceneView
            if (!MouseOverSceneView(out Vector2 mousePos))
            {
                hitInfo = new RaycastHit();
                return false;
            }

            // bool inside = MouseOverSceneView(out mousePos);
            // Handles.Label(new Vector3(-395f, 60f, -316f), string.Format("{0} is {1}in {2}", mousePos, inside ? "" : "not ", SceneView.lastActiveSceneView.m_position.ToString()));
            // Let's do the raycast first
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            if (GeNaSpawnerInternal.Sample(spawner, ray, out hitInfo))
            {
                if (renderSpawnRange)
                    RenderSpawnRange(spawner, hitInfo.point);
                return true;
            }
            hitInfo = default;
            return false;
        }
        public static void UpdateResourceIDs(GeNaSpawnerData spawner)
        {
            if (spawner == null)
                return;
            Terrain terrain = Terrain.activeTerrain;
            foreach (Resource resource in spawner.SpawnPrototypes.SelectMany(proto => proto.Resources))
            {
                string path;
                switch (resource.ResourceType)
                {
                    case Constants.ResourceType.Prefab:
                        if (string.IsNullOrEmpty(resource.AssetID) || string.IsNullOrEmpty(resource.AssetName))
                        {
                            path = AssetDatabase.GetAssetPath(resource.Prefab);
                            if (!string.IsNullOrEmpty(path))
                            {
                                resource.AssetID = AssetDatabase.AssetPathToGUID(path);
                                resource.AssetName = GeNaEditorUtility.GetAssetName(path);
                            }
                        }
                        break;
                    case Constants.ResourceType.TerrainTree:
                        if (string.IsNullOrEmpty(resource.AssetID) && terrain != null)
                        {
                            TreePrototype[] treePrototypes = terrain.terrainData.treePrototypes;
                            if (resource.TerrainProtoIdx < treePrototypes.Length)
                            {
                                path = AssetDatabase.GetAssetPath(treePrototypes[resource.TerrainProtoIdx].prefab);
                                if (!string.IsNullOrEmpty(path))
                                    resource.AssetID = AssetDatabase.AssetPathToGUID(path);
                            }
                        }
                        break;
                    case Constants.ResourceType.TerrainGrass:
                        if (string.IsNullOrEmpty(resource.AssetID) && terrain != null)
                        {
                            DetailPrototype[] dtlPrototypes = terrain.terrainData.detailPrototypes;
                            if (resource.TerrainProtoIdx < dtlPrototypes.Length)
                            {
                                path = AssetDatabase.GetAssetPath(dtlPrototypes[resource.TerrainProtoIdx].prototype);
                                if (!string.IsNullOrEmpty(path))
                                    resource.AssetID = AssetDatabase.AssetPathToGUID(path);
                                else
                                {
                                    path = AssetDatabase.GetAssetPath(dtlPrototypes[resource.TerrainProtoIdx].prototypeTexture);
                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        resource.AssetID = AssetDatabase.AssetPathToGUID(path);
                                    }
                                    resource.DetailPrototypeData = GeNaSpawner.UpdateDetailPrototypeData(dtlPrototypes[resource.TerrainProtoIdx]);
                                    resource.AssetID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(dtlPrototypes[resource.TerrainProtoIdx].prototypeTexture));
                                }
                            }
                        }
                        break;
                    case Constants.ResourceType.TerrainTexture:
                        if (string.IsNullOrEmpty(resource.AssetID) && terrain != null)
                        {
                            TerrainLayer[] terrainTextures = terrain.terrainData.terrainLayers;
                            if (resource.TerrainProtoIdx < terrainTextures.Length)
                            {
                                path = AssetDatabase.GetAssetPath(terrainTextures[resource.TerrainProtoIdx].diffuseTexture);
                                if (!string.IsNullOrEmpty(path))
                                {
                                    resource.AssetID = AssetDatabase.AssetPathToGUID(path);
                                }
                                Texture2D texture2D = terrainTextures[resource.TerrainProtoIdx].diffuseTexture;
                                resource.TexturePrototypeData = GeNaSpawner.UpdateTexturePrototypeData(texture2D);
                                resource.AssetID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(texture2D));
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public static bool LookForMissingAsset(GeNaSpawnerData geNaSpawner, bool allOk, Prototype proto, Resource res)
        {
            // If the prefab is missing
            if (res.Prefab == null && res.ContainerOnly == false)
            {
                // Attempt to find by GUID - This won't be needed, since if we have the Asset with the GUID, then we have the Asset. Leaving here for good measure.
                string path = AssetDatabase.GUIDToAssetPath(res.AssetID);
                if (string.IsNullOrEmpty(path) == false)
                {
                    res.Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (res.Prefab != null)
                    {
                        Debug.LogWarningFormat("[GeNa]: Identified missing asset for '{0}' by ID at path '{1}'.", res.Name, path);
                        UpdateResourceIDs(geNaSpawner);
                    }
                }

                // If still missing
                if (res.Prefab == null)
                {
                    // Attempt to find by name
                    if (string.IsNullOrEmpty(res.AssetName) == false)
                    {
                        res.Prefab = AssetUtils.GetAssetPrefab(res.AssetName);
                        if (res.Prefab != null)
                        {
                            Debug.LogWarningFormat("[GeNa]: Identified missing asset for '{0}' by filename at path '{1}'.", res.Name, AssetDatabase.GetAssetPath(res.Prefab));
                            UpdateResourceIDs(geNaSpawner);
                        }
                    }
                    if (res.Prefab == null)
                    {
                        Debug.LogErrorFormat("Spawn aborted. Could not find the prefab for {0}", res.Name);
                        allOk = false;
                    }
                }
            }
            List<Resource> children = proto.GetChildren(res);
            if (children != null)
            {
                foreach (Resource child in children)
                    allOk = LookForMissingAsset(geNaSpawner, allOk, proto, child);
            }
            return allOk;
        }
        public static bool MouseOverSceneView(out Vector2 mousePos)
        {
            mousePos = Event.current.mousePosition;
            if (mousePos.x < 0f || mousePos.y < 0f)
                return false;
            Rect swPos = SceneView.lastActiveSceneView.position;
            if (mousePos.x > swPos.width || mousePos.y > swPos.height)
                return false;
            return true;
        }
        #endregion
        #region Menu Item
        /// <summary>
        /// Create GeNa River Profile asset
        /// </summary>
        [MenuItem("Assets/Create/Procedural Worlds/GeNa/River Profile")]
        public static void CreateRiverProfile()
        {
            RiverProfile asset = ScriptableObject.CreateInstance<RiverProfile>();
            AssetDatabase.CreateAsset(asset, "Assets/GeNa River Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        /// <summary>
        /// Create GeNa River Profile asset
        /// </summary>
        [MenuItem("Assets/Create/Procedural Worlds/GeNa/Road Profile")]
        public static void CreateRoadProfile()
        {
            RoadProfile asset = ScriptableObject.CreateInstance<RoadProfile>();
            AssetDatabase.CreateAsset(asset, "Assets/GeNa Road Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        /// <summary>
        /// Adds a Spline
        /// </summary>
        [MenuItem("GameObject/GeNa/Add Spline", false, 15)]
        public static void AddSpline()
        {
            //Create the spawner
            GameObject genaGo = new GameObject("Spline");
            // Add Spline Component
            genaGo.AddComponent<Spline>();
            // GeNa Spawner
            GameObject parent = GeNaUtility.GeNaSplinesTransform.gameObject;
            // Reparent it
            GameObjectUtility.SetParentAndAlign(genaGo, parent);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(genaGo, $"[{PWApp.CONF.Name}] Created '{genaGo.name}'");
            // Select the newly created Spline
            Selection.activeGameObject = genaGo;
        }
        /// <summary>
        /// Adds a Spawner
        /// </summary>
        [MenuItem("GameObject/GeNa/Add Spawner", false, 14)]
        public static void AddSpawner()
        {
            //Create the spawner
            GameObject genaGo = new GameObject("Spawner");
            Spawner spawner = genaGo.AddComponent<Spawner>();
            spawner.SetDefaults(Defaults);
            spawner.Version = 2;
            // GeNa Root
            // GeNa Spawner
            Transform parent = GeNaUtility.GeNaSpawnersTransform;
            // Reparent it
            GameObjectUtility.SetParentAndAlign(genaGo, parent.gameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(genaGo, string.Format("[{0}] Created '{1}'", PWApp.CONF.Name, genaGo.name));
            //Make it active
            Selection.activeObject = genaGo;
        }
        #endregion
        #region Editor Extensions

        public static Vector2 CalculateTextSize(string text, GUIStyle guiStyle = null)
        {
            if (guiStyle == null)
                guiStyle = EditorStyles.numberField;
            
            return guiStyle.CalcSize(new GUIContent(text));
        }
        public static Rect GetGUILabelWorldRect(string text, Vector3 worldPos, Vector2 screenOffset = default, GUIStyle guiStyle = null)
        {
            // Convert world position to GUI position
            Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos);
            guiPos += screenOffset;

            Vector2 size = CalculateTextSize(text, guiStyle);
            
            // Set up the rectangle with corrected positions. The y-axis is inverted in GUI space, so we subtract size.y
            return new Rect(guiPos.x, guiPos.y, size.x, size.y);
        }

        public static bool IsMouseOverRect(Rect rect)
        {
            Event e = Event.current;
            if (e == null)
                return false;
            return rect.Contains(e.mousePosition);
        }


        public static bool IsOnScreen(Vector3 position)
        {
            Vector3 onScreen = Camera.current.WorldToViewportPoint(position);
            return onScreen.z > 0f && onScreen.x > 0f &&
                   onScreen.y > 0f && onScreen.x < 1f &&
                   onScreen.y < 1f;
        }

        public static void DrawDottedLineToScreenPoint(Vector3 worldPosition, Rect screenRect, Color color, float screenSpaceSize = 1f)
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            Camera camera = sceneView.camera;

            // Convert Rect's center from GUI space to Screen Space
            Vector2 guiPosition = new Vector2(screenRect.center.x, screenRect.center.y);

            // Get a ray from the camera through the gui position
            Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);

            // Define a plane at the worldPosition and parallel to camera viewport
            Plane plane = new Plane(-camera.transform.forward, worldPosition);

            // If the ray and the plane intersects
            if (plane.Raycast(ray, out float enter))
            {
                // Find the intersection point
                Vector3 intersectionPoint = ray.GetPoint(enter);

                Color oldColor = Handles.color;
                Handles.color = color;
                // Draw a line between the worldPosition and the intersection point
                Handles.DrawDottedLine(worldPosition, intersectionPoint, screenSpaceSize);
                Handles.color = oldColor;
            }
        }
        public static void DrawStringShadow(string text, Vector3 worldPos, float shadowDistance = 1f, Vector2 screenOffset = default, GUIStyle guiStyle = null)
        {
            if (!IsOnScreen(worldPos))
                return;
            
            if (guiStyle == null)
                guiStyle = new GUIStyle(EditorStyles.label);
            
            Rect rect = GetGUILabelWorldRect(text, worldPos, screenOffset, guiStyle);
            
            Handles.BeginGUI();
            
            rect.x += shadowDistance;
            rect.y += shadowDistance;
            guiStyle.normal.textColor = Color.black;
            GUI.Label(rect, text, guiStyle);
            
            rect.x -= shadowDistance;
            rect.y -= shadowDistance;
            guiStyle.normal.textColor = Color.white;
            GUI.Label(rect, text, guiStyle);
            
            Handles.EndGUI();
        }
        public static void DrawString(string text, Vector3 worldPos, Vector2 screenOffset = default, GUIStyle guiStyle = null)
        {
            if (!IsOnScreen(worldPos))
                return;
            
            if (guiStyle == null)
                guiStyle = EditorStyles.numberField;
            
            Rect rect = GetGUILabelWorldRect(text, worldPos, screenOffset, guiStyle);

            Handles.BeginGUI();
    
            GUI.Label(rect, text, guiStyle);
            
            Handles.EndGUI();
        }
        
        public static void DrawDebug<T>(this AabbTree<T> aabbTree, bool drawLabel, int isolateDepth = -1)
        {
            int root = aabbTree.Root;
            if (root == -1)
                return;
            AabbNode[] nodes = aabbTree.Nodes;
            Color prevColor = Handles.color;
            int isolateHeight = nodes[root].Height - isolateDepth;
            bool[] nodesVisited = new bool[nodes.Length];
            for (int i = 0; i < nodesVisited.Length; ++i)
                nodesVisited[i] = false;
            for (int i = 0; i < nodes.Length; ++i)
            {
                if (nodes[i].IsFree)
                    continue;
                if (nodesVisited[i])
                    continue;
                if (isolateDepth >= 0)
                {
                    if (nodes[i].Height != isolateHeight)
                        continue;
                    Gizmos.color =
                        nodes[i].Height == isolateHeight - 1
                            ? Color.gray
                            : Color.white;
                    Handles.color = Color.gray;
                    aabbTree.DebugDrawNode(nodes[i].Parent, false, drawLabel, false);
                    aabbTree.DebugDrawLink(i, nodes[i].Parent);
                    aabbTree.DebugDrawNode(nodes[i].ChildA, true, drawLabel, false);
                    aabbTree.DebugDrawLink(i, nodes[i].ChildA);
                    aabbTree.DebugDrawNode(nodes[i].ChildB, true, drawLabel, false);
                    aabbTree.DebugDrawLink(i, nodes[i].ChildB);
                    if (nodes[i].ChildA != -1)
                        nodesVisited[nodes[i].ChildA] = true;
                    if (nodes[i].ChildB != -1)
                        nodesVisited[nodes[i].ChildB] = true;
                    Handles.color = Color.white;
                    aabbTree.DebugDrawNode(i, true, drawLabel, false);
                    nodesVisited[i] = true;
                    continue;
                }
                Handles.color = Color.white;
                aabbTree.DebugDrawLink(i, nodes[i].Parent);
                aabbTree.DebugDrawNode(i, true, drawLabel, true);
                nodesVisited[i] = true;
            }
            Gizmos.color = prevColor;
        }
        public static void DebugDrawLink<T>(this AabbTree<T> aabbTree, int from, int to)
        {
            if (from == -1 || to == -1)
                return;
            AabbNode[] nodes = aabbTree.Nodes;
            Aabb fromBounds = nodes[from].Bounds;
            Aabb toBounds = nodes[to].Bounds;
            Handles.DrawLine(fromBounds.Center, toBounds.Center);
        }
        public static void DebugDrawNode<T>(this AabbTree<T> aabbTree, int index, bool drawBounds, bool drawLabel, bool fullTreeMode)
        {
            if (index == -1)
                return;
            AabbNode[] nodes = aabbTree.Nodes;
            Aabb bounds = nodes[index].Bounds;
            if (drawBounds)
            {
                if (nodes[index].IsLeaf || !fullTreeMode)
                {
                    Handles.DrawWireCube(bounds.Center, bounds.Extents);
                }
            }
            Handles.SphereHandleCap(0, bounds.Center, Quaternion.identity, 0.05f, EventType.Repaint);
            if (drawLabel)
            {
                int root = aabbTree.Root;
                DrawString("Node  : " + index + (index == root ? " (root)" : "") + (nodes[index].IsLeaf ? " (Leaf)" : "")
                           + "\nParent: " + nodes[index].Parent
                           + "\nChildA: " + nodes[index].ChildA
                           + "\nChildB: " + nodes[index].ChildB
                           + "\nHeight: " + nodes[index].Height, bounds.Center);
            }
        }
        public static void DrawDebug(this AabbManager aabbManager, bool drawLabel) => aabbManager.Tree.DrawDebug(drawLabel);
        public static void Fractal(Fractal maskFractal)
        {
            EditorGUI.indentLevel++;
            maskFractal.Seed = EditorGUILayout.Slider("Seed", maskFractal.Seed, 0f, 65000f);
            maskFractal.Octaves = EditorGUILayout.IntSlider("Octaves", maskFractal.Octaves, 1, 12);
            maskFractal.Frequency = EditorGUILayout.Slider("Frequency", maskFractal.Frequency, 0f, 0.3f);
            maskFractal.Persistence = EditorGUILayout.Slider("Persistence", maskFractal.Persistence, 0f, 1f);
            maskFractal.Lacunarity = EditorGUILayout.Slider("Lacunarity", maskFractal.Lacunarity, 1.5f, 3.5f);
            EditorGUI.indentLevel--;
        }
        #endregion
        #endregion
        #region Methods
        /// <summary>
        /// Checks to see if GPU supports compute shader
        /// </summary>
        /// <returns></returns>
        public static bool ValidateComputeShader() => SystemInfo.supportsComputeShaders;
        public static IEnumerator Simulate(List<ResourceEntity> entities, PhysicsSimulatorSettings settings, MonoBehaviour reference)
        {
            IEnumerator routine = PhysicsSimulator.Simulate(entities, settings);
            yield return EditorCoroutineUtility.StartCoroutine(routine, reference);
        }
        public static void StartCoroutine(IEnumerator routine, MonoBehaviour monoBehaviour) => EditorCoroutineUtility.StartCoroutine(routine, monoBehaviour);
        #endregion
        public static void DisplayWarnings()
        {
#if GeNa_HDRP
            GeNaHDRPUtility.DisplayWarnings();
#endif

            // Gizmos Disabled Warning
            if (!IsGizmosEnabled())
            {
                EditorGUILayout.HelpBox("WARNING! Gizmos has been Disabled in the Scene View.\n" +
                                        "In order to use GeNa Spawners, you must have Gizmos turned on.", MessageType.Warning);
                if (GUILayout.Button("Turn on Gizmos"))
                {
                    SetSceneViewGizmos(true);
                }
            }

            // Inspector locked warning
            if (ActiveEditorTracker.sharedTracker.isLocked)
            {
                EditorGUILayout.HelpBox("WARNING! The inspector is locked.\n" +
                                        "If visualization is not working as expected, make sure that the spawner is selected.", MessageType.Warning);
            }
        }
        public static string TryGetGUID(Object @object)
        {
            if (@object != null)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(@object, out string guid, out long localID))
                    return guid;
            }
            return null;
        }
        public static bool IsGizmosEnabled()
        {
            SceneView sv = SceneView.lastActiveSceneView;
            return sv != null ? sv.drawGizmos : false;
        }
        public static void SetSceneViewGizmos(bool gizmosOn)
        {
            SceneView sv = SceneView.lastActiveSceneView;
            if (sv != null)
                sv.drawGizmos = gizmosOn;
        }
        public static IEnumerable<GameObject> GetAllPrefabsInPaths(IEnumerable<string> paths)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            // Handle speedtrees
            foreach (string path in paths)
            {
                // Update in case unity has messed with it 
                if (path.StartsWith("Assets"))
                {
                    // get the file attributes for file or directory
                    FileAttributes attr = File.GetAttributes(path);
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        // Check file type and process as we can
                        IEnumerable<GameObject> gos = LoadAllPrefabsOfType(path);
                        gameObjects.AddRange(gos);
                    }
                }
            }
            return gameObjects;
        }
        public static IEnumerable<GameObject> LoadAllPrefabsOfType(string path)
        {
            if (path != "")
                if (path.EndsWith("/"))
                    path = path.TrimEnd('/');
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] fileInf = dirInfo.GetFiles("*.prefab");
            //loop through directory loading the game object and checking if it has the component you want
            List<GameObject> prefabs = new List<GameObject>();
            foreach (FileInfo fileInfo in fileInf)
            {
                string fullPath = fileInfo.FullName.Replace(@"\", "/");
                string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");
                GameObject prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                if (prefab != null)
                    prefabs.Add(prefab);
            }
            return prefabs;
        }
        public static GeNaSpawnerData RevertToMain(GeNaSpawner spawner)
        {
            GameObject asset = GetPrefabAsset(spawner.gameObject);
            if (asset == null)
                return null;
            AssetDatabase.Refresh();
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            GeNaSpawnerData tempFile = spawner.SpawnerData;
            if (tempFile == null)
                return null;
            if (tempFile.name.Contains("Main"))
                return null;
            GeNaSpawnerData mainFile = default(GeNaSpawnerData);
            foreach (Object @object in objects)
            {
                mainFile = @object as GeNaSpawnerData;
                if (mainFile == null)
                    continue;
                if (mainFile.name.Contains("Main"))
                    break;
            }
            return mainFile;
        }
        public static GeNaSpawnerData ApplyToMain(GeNaSpawner spawner)
        {
            GameObject asset = GetPrefabAsset(spawner.gameObject);
            if (asset == null)
                return null;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            GeNaSpawnerData mainFile = default(GeNaSpawnerData);
            foreach (Object @object in objects)
            {
                if (@object == null)
                    continue;
                if (@object.name.Contains("Main"))
                {
                    mainFile = @object as GeNaSpawnerData;
                }
            }
            spawner.SpawnerData.name = "Spawner Data (Main)";
            if (mainFile == null)
                AssetDatabase.AddObjectToAsset(spawner.SpawnerData, asset);
            else
                EditorUtility.CopySerialized(spawner.SpawnerData, mainFile);
            spawner.SpawnerData.name = "Spawner Data (Temp)";
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            // AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mainFile));
            objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            mainFile = default(GeNaSpawnerData);
            foreach (Object @object in objects)
            {
                mainFile = @object as GeNaSpawnerData;
                if (mainFile == null)
                    continue;
                if (mainFile.name.Contains("Main"))
                    break;
            }
            // Change the Reference to the asset
            GeNaSpawner assetSpawner = asset.GetComponent<GeNaSpawner>();
            if (assetSpawner != null)
            {
                assetSpawner.SpawnerData = mainFile;
            }
            // EditorUtility.SetDirty(assetSpawner);
            return mainFile;
        }
        public static bool HasMainFile(GeNaSpawner spawner)
        {
            GameObject asset = GetPrefabAsset(spawner.gameObject);
            if (asset == null)
                return false;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            DataBufferScriptable dataFile = default(DataBufferScriptable);
            foreach (Object @object in objects)
            {
                dataFile = @object as DataBufferScriptable;
                if (dataFile == null)
                    continue;
                if (dataFile.name.Contains("Main"))
                    return true;
            }
            return false;
        }
        public static DataBufferScriptable GetMainDataBufferScriptable(GeNaSpawner spawner)
        {
            GameObject asset = GetPrefabAsset(spawner.gameObject);
            if (asset == null)
            {
                // Return a Temp one instead!
                return spawner.GenerateTempDataFile();
            }
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            DataBufferScriptable dataFile = default(DataBufferScriptable);
            foreach (Object @object in objects)
            {
                dataFile = @object as DataBufferScriptable;
                if (dataFile == null)
                    continue;
                if (dataFile.name.Contains("Main"))
                    break;
            }
            if (dataFile == null)
            {
                dataFile = ScriptableObject.CreateInstance<DataBufferScriptable>();
                dataFile.name = "Spawner Data (Main)";
                AssetDatabase.AddObjectToAsset(dataFile, asset);
                AssetDatabase.Refresh();
            }
            return dataFile;
        }
        public static GeNaSpawnerData GetMainSpawnerData(GeNaSpawner spawner)
        {
            // AssetDatabase.StartAssetEditing();
            string assetPath = AssetDatabase.GetAssetPath(spawner);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            GeNaSpawnerData spawnerData = default(GeNaSpawnerData);
            foreach (Object @object in objects)
            {
                spawnerData = @object as GeNaSpawnerData;
                if (spawnerData == null)
                    continue;
                if (spawnerData.name.Contains("Main"))
                    break;
            }
            if (spawnerData == null)
            {
                spawnerData = spawner.SpawnerData;
                spawnerData.name = "Spawner Data (Main)";
                AssetDatabase.AddObjectToAsset(spawnerData, spawner);
                AssetDatabase.Refresh();
            }
            objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            GeNaSpawnerData mainFile = default(GeNaSpawnerData);
            foreach (Object @object in objects)
            {
                mainFile = @object as GeNaSpawnerData;
                if (mainFile == null)
                    continue;
                if (mainFile.name.Contains("Main"))
                    break;
            }
            spawner.SpawnerData = mainFile;
            AssetDatabase.Refresh();
            // AssetDatabase.StopAssetEditing();
            return spawnerData;
        }
        public static void CreateMainDataBufferScriptable(GeNaSpawner spawner)
        {
            // AssetDatabase.StartAssetEditing();
            DataBufferScriptable dataFile = GetMainDataBufferScriptable(spawner);
            spawner.ConvertToFile(dataFile);
            // AssetDatabase.StopAssetEditing();
        }
        public static void RemoveDataBufferScriptable(GeNaSpawner spawner)
        {
            // AssetDatabase.StartAssetEditing();
            DataBufferScriptable dataFile = spawner.GetTempDataFile();
            if (dataFile == null)
                return;
            AssetDatabase.RemoveObjectFromAsset(dataFile);
            AssetDatabase.Refresh();
            // AssetDatabase.StopAssetEditing();
        }
        public static void RemoveAllTempDataBufferScriptables(GeNaSpawner spawner)
        {
            // AssetDatabase.StartAssetEditing();
            string assetPath = AssetDatabase.GetAssetPath(spawner);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            DataBufferScriptable dataFile = default(DataBufferScriptable);
            foreach (Object @object in objects)
            {
                dataFile = @object as DataBufferScriptable;
                if (dataFile == null)
                    continue;
                AssetDatabase.RemoveObjectFromAsset(dataFile);
                AssetDatabase.Refresh();
            }
            // AssetDatabase.StopAssetEditing();
        }
        public static void RemoveAllDuplicateScriptables(GeNaSpawner spawner)
        {
            // AssetDatabase.StartAssetEditing();
            string assetPath = AssetDatabase.GetAssetPath(spawner);
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (Object @object in objects)
            {
                GeNaSpawnerData spawnerData = @object as GeNaSpawnerData;
                if (spawnerData == null)
                    continue;
                if (spawnerData != spawner.SpawnerData)
                {
                    AssetDatabase.RemoveObjectFromAsset(@object);
                    AssetDatabase.Refresh();
                }
            }
            // AssetDatabase.StopAssetEditing();
        }
        public static bool IsSource()
        {
            string[] results = AssetDatabase.FindAssets("GeNa.Scripts.Core.dll");
            return results.Length == 0;
        }
        /// <summary>
        /// Add new terrain tree to the terrain
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static int AddTreeResourceToTerrain(GameObject prefab, Terrain terrain)
        {
            int ID = 0;
            if (prefab != null && terrain != null)
            {
                TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
                int index = prototypes.Length;
                TreePrototype[] newPrototypes = new TreePrototype[index + 1];
                for (int i = 0; i < newPrototypes.Length; i++)
                {
                    if (newPrototypes[i] == null)
                    {
                        newPrototypes[i] = new TreePrototype();
                    }
                    if (i != prototypes.Length)
                    {
                        newPrototypes[i].prefab = prototypes[i].prefab;
                    }
                    else
                    {
                        newPrototypes[i].prefab = prefab;
                    }
                }
                terrain.terrainData.treePrototypes = newPrototypes;
                ID = GetTreeID(newPrototypes, prefab);
                MarkObjectDirty(terrain);
            }
            return ID;
        }
        /// <summary>
        /// Add new terrain grass to the terrain
        /// </summary>
        /// <param name="activeDetailPrototype"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static int AddGrassResourceToTerrain(TerrainDetailPrototypeData activeDetailPrototype, Terrain terrain)
        {
            int ID = 0;
            if (activeDetailPrototype != null && terrain != null)
            {
                DetailPrototype[] prototypes = terrain.terrainData.detailPrototypes;
                int index = prototypes.Length;
                DetailPrototype[] newPrototypes = new DetailPrototype[index + 1];
                for (int i = 0; i < newPrototypes.Length; i++)
                {
                    if (newPrototypes[i] == null)
                    {
                        newPrototypes[i] = new DetailPrototype();
                    }
                    if (i != prototypes.Length)
                    {
#if !UNITY_2020_2_OR_NEWER
                        newPrototypes[i].bendFactor = prototypes[i].bendFactor;
#endif
                        newPrototypes[i].prototypeTexture = prototypes[i].prototypeTexture;
                        newPrototypes[i].dryColor = prototypes[i].dryColor;
                        newPrototypes[i].healthyColor = prototypes[i].healthyColor;
                        newPrototypes[i].maxHeight = prototypes[i].maxHeight;
                        newPrototypes[i].maxWidth = prototypes[i].maxWidth;
                        newPrototypes[i].minHeight = prototypes[i].minHeight;
                        newPrototypes[i].minWidth = prototypes[i].minWidth;
                        newPrototypes[i].noiseSpread = prototypes[i].noiseSpread;
                        newPrototypes[i].prototype = prototypes[i].prototype;
                        newPrototypes[i].renderMode = prototypes[i].renderMode;
                        newPrototypes[i].usePrototypeMesh = prototypes[i].usePrototypeMesh;
                    }
                    else
                    {
#if !UNITY_2020_2_OR_NEWER
                        newPrototypes[i].bendFactor = activeDetailPrototype.bendFactor;
#endif
                        newPrototypes[i].prototypeTexture = activeDetailPrototype.prototypeTexture;
                        newPrototypes[i].dryColor = activeDetailPrototype.dryColor;
                        newPrototypes[i].healthyColor = activeDetailPrototype.healthyColor;
                        newPrototypes[i].maxHeight = activeDetailPrototype.maxHeight;
                        newPrototypes[i].maxWidth = activeDetailPrototype.maxWidth;
                        newPrototypes[i].minHeight = activeDetailPrototype.minHeight;
                        newPrototypes[i].minWidth = activeDetailPrototype.minWidth;
                        newPrototypes[i].noiseSpread = activeDetailPrototype.noiseSpread;
                        newPrototypes[i].prototype = activeDetailPrototype.prototype;
                        newPrototypes[i].renderMode = activeDetailPrototype.renderMode;
                        newPrototypes[i].usePrototypeMesh = activeDetailPrototype.usePrototypeMesh;
                    }
                }
                terrain.terrainData.detailPrototypes = newPrototypes;

                Texture2D prototypeTexture = activeDetailPrototype.prototypeTexture;
                if (prototypeTexture != null)
                {
                    ID = GetGrassID(newPrototypes, activeDetailPrototype.prototypeTexture);
                }
                else
                {
                    GameObject prototypePrefab = activeDetailPrototype.prototype;
                    if (prototypePrefab != null)
                    {
                        ID = GetGrassID(newPrototypes, prototypePrefab);
                    }
                }
                
                MarkObjectDirty(terrain);
            }
            return ID;
        }
        /// <summary>
        /// Add new terrain grass to the terrain
        /// </summary>
        /// <param name="activeTexturePrototype"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static int AddTextureResourceToTerrain(TerrainTexturePrototypeData activeTexturePrototype, Terrain terrain)
        {
            int ID = 0;
            if (activeTexturePrototype != null && terrain != null)
            {
                TerrainLayer[] prototypes = terrain.terrainData.terrainLayers;
                int index = prototypes.Length;
                TerrainLayer[] newPrototypes = new TerrainLayer[index + 1];
                for (int i = 0; i < newPrototypes.Length; i++)
                {
                    if (newPrototypes[i] == null)
                    {
                        newPrototypes[i] = new TerrainLayer();
                    }
                    if (i != prototypes.Length)
                    {
                        newPrototypes[i].diffuseTexture = prototypes[i].diffuseTexture;
                        newPrototypes[i].diffuseRemapMax = prototypes[i].diffuseRemapMax;
                        newPrototypes[i].diffuseRemapMin = prototypes[i].diffuseRemapMin;
                        newPrototypes[i].maskMapRemapMax = prototypes[i].maskMapRemapMax;
                        newPrototypes[i].maskMapRemapMin = prototypes[i].maskMapRemapMin;
                        newPrototypes[i].maskMapTexture = prototypes[i].maskMapTexture;
                        newPrototypes[i].metallic = prototypes[i].metallic;
                        newPrototypes[i].normalMapTexture = prototypes[i].normalMapTexture;
                        newPrototypes[i].normalScale = prototypes[i].normalScale;
                        newPrototypes[i].smoothness = prototypes[i].smoothness;
                        newPrototypes[i].specular = prototypes[i].specular;
                        newPrototypes[i].tileOffset = prototypes[i].tileOffset;
                        newPrototypes[i].tileSize = prototypes[i].tileSize;
                    }
                    else
                    {
                        TerrainLayer terrainLayer = new TerrainLayer();
                        terrainLayer.diffuseTexture = activeTexturePrototype.terrainTexture2DAsset;
                        newPrototypes[i] = terrainLayer;
                    }
                }
                terrain.terrainData.terrainLayers = newPrototypes;
                ID = GetTextureID(newPrototypes, activeTexturePrototype.terrainTexture2DAsset);
                MarkObjectDirty(terrain);
            }
            return ID;
        }
        /// <summary>
        /// Gets the tree ID
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="treePrefab"></param>
        /// <returns></returns>
        public static int GetTreeID(Terrain terrain, GameObject treePrefab)
        {
            int ID = -1;
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                return GetTreeID(terrainData, treePrefab);
            }
            return ID;
        }
        /// <summary>
        /// Gets the tree ID
        /// </summary>
        /// <param name="terrainData"></param>
        /// <param name="treePrefab"></param>
        /// <returns></returns>
        public static int GetTreeID(TerrainData terrainData, GameObject treePrefab)
        {
            int ID = -1;
            if (terrainData != null)
            {
                TreePrototype[] prototypes = terrainData.treePrototypes;
                return GetTreeID(prototypes, treePrefab);
            }
            return ID;
        }
        /// <summary>
        /// Gets the tree ID
        /// </summary>
        /// <param name="prototypes"></param>
        /// <param name="treePrefab"></param>
        /// <returns></returns>
        public static int GetTreeID(TreePrototype[] prototypes, GameObject treePrefab)
        {
            int ID = -1;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].prefab == treePrefab)
                {
                    ID = i;
                    break;
                }
            }
            return ID;
        }
        /// <summary>
        /// Gets the grass ID
        /// </summary>
        /// <param name="prototypes"></param>
        /// <param name="grassTexture"></param>
        /// <returns></returns>
        public static int GetGrassID(DetailPrototype[] prototypes, Texture2D grassTexture)
        {
            int ID = -1;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].prototypeTexture == grassTexture)
                {
                    ID = i;
                    break;
                }
            }
            return ID;
        }
        /// <summary>
        /// Gets the grass ID
        /// </summary>
        /// <param name="prototypes"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static int GetGrassID(DetailPrototype[] prototypes, GameObject prefab)
        {
            int ID = -1;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].prototype == prefab)
                {
                    ID = i;
                    break;
                }
            }
            return ID;
        }
        /// <summary>
        /// Gets the grass ID
        /// </summary>
        /// <param name="prototypes"></param>
        /// <param name="terrainTexture"></param>
        /// <returns></returns>
        public static int GetTextureID(TerrainLayer[] prototypes, Texture2D terrainTexture)
        {
            int ID = -1;
            for (int i = 0; i < prototypes.Length; i++)
            {
                if (prototypes[i].diffuseTexture == terrainTexture)
                {
                    ID = i;
                    break;
                }
            }
            return ID;
        }
        /// <summary>
        /// Checks if the tree prototype is already on the terrain.
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsTreeOnTerrain(GameObject prefab, Terrain terrain)
        {
            if (prefab == null || terrain == null)
            {
                return true;
            }
            TreePrototype[] prototypes = terrain.terrainData.treePrototypes;
            foreach (TreePrototype prototype in prototypes)
            {
                if (prototype.prefab == prefab)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if the grass prototype is already on the terrain
        /// </summary>
        /// <param name="grassTexture"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsGrassOnTerrain(Texture2D grassTexture, Terrain terrain)
        {
            if (grassTexture == null || terrain == null)
            {
                return true;
            }
            DetailPrototype[] prototypes = terrain.terrainData.detailPrototypes;
            foreach (DetailPrototype prototype in prototypes)
            {
                if (prototype.prototypeTexture == grassTexture)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if the grass prototype is already on the terrain
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsGrassOnTerrain(GameObject prefab, Terrain terrain)
        {
            if (prefab == null || terrain == null)
            {
                return true;
            }
            DetailPrototype[] prototypes = terrain.terrainData.detailPrototypes;
            foreach (DetailPrototype prototype in prototypes)
            {
                if (prototype.prototype == prefab)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if the grass prototype is already on the terrain
        /// </summary>
        /// <param name="terrainTexture"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        public static bool IsTextureOnTerrain(Texture2D terrainTexture, Terrain terrain)
        {
            if (terrainTexture == null || terrain == null)
            {
                return true;
            }
            TerrainLayer[] prototypes = terrain.terrainData.terrainLayers;
            foreach (TerrainLayer prototype in prototypes)
            {
                if (prototype.diffuseTexture == terrainTexture)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Marks the object dirty
        /// </summary>
        /// <param name="systemObject"></param>
        public static void MarkObjectDirty(Object systemObject)
        {
            EditorUtility.SetDirty(systemObject);
        }
    }
}