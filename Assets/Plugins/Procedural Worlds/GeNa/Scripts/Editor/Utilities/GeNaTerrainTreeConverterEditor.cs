using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaTerrainTreeConverter))]
    public class GeNaTerrainTreeConverterEditor : Editor
    {
        private GeNaTerrainTreeConverter m_converter;

        private void OnEnable()
        {
            m_converter = target as GeNaTerrainTreeConverter;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
            //m_converter.m_conversionType = (ConversionType)EditorGUILayout.EnumPopup(new GUIContent("Conversion Type"), m_converter.m_conversionType);
            if (m_converter.m_conversionType != ConversionType.GameObject)
            {
                GUI.enabled = false;
            }
            m_converter.m_conversionDisableMode = (ConversionDisableMode)EditorGUILayout.EnumPopup(new GUIContent("Conversion Disable Mode"), m_converter.m_conversionDisableMode);
            GUI.enabled = true;
            m_converter.m_syncTreeInstancesToGameObjects = EditorGUILayout.Toggle("Sync Tree Instances", m_converter.m_syncTreeInstancesToGameObjects);
            GUI.enabled = false;
            m_converter.m_ingestIntoFlora = EditorGUILayout.Toggle("Ingest Into Flora", m_converter.m_ingestIntoFlora);
            GUI.enabled = true;

            switch (m_converter.m_conversionType)
            {
                case ConversionType.GameObject:
                {
                    if (GUILayout.Button("Convert To Trees"))
                    {
                        EditorUtility.SetDirty(ConvertToTrees(m_converter.gameObject, FindAllPrefabInstances(m_converter.gameObject)));
                    }

                    break;
                }
                case ConversionType.TerrainTree:
                {
                    if (GUILayout.Button("Convert Back To GameObjects"))
                    {
                        EditorUtility.SetDirty(m_converter.ConvertBackToGameObjects());
                    }
                    break;
                }
            }

            EditorGUILayout.LabelField("Current Tree Instance Count: " + m_converter.m_storedTreeInstanceData.Count, EditorStyles.boldLabel);
        }

        private List<GameObject> FindAllPrefabInstances(GameObject myPrefab)
        {
            List<GameObject> result = new List<GameObject>();
            Transform[] allObjects = myPrefab.GetComponentsInChildren<Transform>();
            foreach (Transform transform in allObjects)
            {
                GameObject gameObject = transform.gameObject;
                if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
                {
                    GameObject prefabInstanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
                    if (prefabInstanceRoot != null)
                    {
                        if (!result.Contains(prefabInstanceRoot))
                        {
                            result.Add(gameObject);
                        }

                        if (!m_converter.m_storedGameObjects.Contains(prefabInstanceRoot))
                        {
                            m_converter.m_storedGameObjects.Add(gameObject);
                        }
                    }
                }
            }
            return result;
        }
        public Terrain ConvertToTrees(GameObject parent, List<GameObject> prefabInstances)
        {
            //Clean up cached data
            m_converter.CleanCachedData();
            // Check if the Parent is part of a Terrain
            Terrain terrain = parent.GetComponentInParent<Terrain>();
            if (terrain == null)
            {
                GeNaDebug.LogWarning("You can only convert GameObjects to Trees if they are children of a Terrain!");
                return null;
            }

            //Check sync settings
            m_converter.SyncTreesWithTerrainTreeInstances(terrain);

            // Loop over each prefab instance
            foreach (GameObject prefabInstance in prefabInstances)
            {
                if (!prefabInstance.activeInHierarchy)
                {
                    continue;
                }
                // Get Prefab asset from Instance
                GameObject prefabAsset = GeNaEditorUtility.GetPrefabAsset(prefabInstance);
                LODGroup lodGroup = prefabAsset.GetComponentInChildren<LODGroup>();
                if (lodGroup != null)
                {
                    // TODO : Manny : Find a way to detect if the Bounds exists before doing this!
                    lodGroup.RecalculateBounds();
                }

                // Get Prototype Index, if it doesn't exist, Add the Prefab to Terrain.
                int prototypeIndex = GeNaEditorUtility.GetTreeID(terrain, prefabAsset);
                if (prototypeIndex < 0)
                {
                    prototypeIndex = GeNaEditorUtility.AddTreeResourceToTerrain(prefabAsset, terrain);
                }

                m_converter.AddTreeInstanceToTerrain(prefabInstance, terrain, prototypeIndex);
                m_converter.m_instasiateData.Add(new TreeInstasiateData
                {
                    m_prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabInstance)),
                    m_position = prefabInstance.transform.position,
                    m_rotation = prefabInstance.transform.eulerAngles,
                    m_scale = prefabInstance.transform.localScale
                });

                //Disable instance in the scene
                switch (m_converter.m_conversionDisableMode)
                {
                    case ConversionDisableMode.Disable:
                    {
                        prefabInstance.SetActive(false);
                        break;
                    }
                    case ConversionDisableMode.DestroyAndInstantiate:
                    {
                        DestroyImmediate(prefabInstance);
                        break;
                    }
                }
            }

            m_converter.SetConversionType(ConversionType.TerrainTree);

            return terrain;
        }
    }
}