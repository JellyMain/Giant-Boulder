using System.Collections.Generic;
using UnityEngine;

namespace GeNa.Core
{
    public enum ConversionType { GameObject, TerrainTree, Flora }
    public enum ConversionDisableMode { Disable, DestroyAndInstantiate }

    [System.Serializable]
    public class TreeInstasiateData
    {
        public GameObject m_prefab;
        public Vector3 m_position;
        public Vector3 m_rotation;
        public Vector3 m_scale;
    }

    public class GeNaTerrainTreeConverter : MonoBehaviour
    {
        #region Variables

        public List<GameObject> m_storedGameObjects = new List<GameObject>();
        public List<TreeInstance> m_storedTreeInstanceData = new List<TreeInstance>();
        public List<TreeInstasiateData> m_instasiateData = new List<TreeInstasiateData>();
        public ConversionType m_conversionType = ConversionType.GameObject;
        public bool m_ingestIntoFlora = false;
        public bool m_syncTreeInstancesToGameObjects = true;
        public ConversionDisableMode m_conversionDisableMode = ConversionDisableMode.Disable;

        #endregion
        #region Unity Functions

        private void Start()
        {
            if (m_ingestIntoFlora)
            {
                foreach (GameObject storedGameObject in m_storedGameObjects)
                {
                    switch (m_conversionDisableMode)
                    {
                        case ConversionDisableMode.Disable:
                        {
                            storedGameObject.SetActive(false);
                            break;
                        }
                        case ConversionDisableMode.DestroyAndInstantiate:
                        {
                            DestroyImmediate(storedGameObject);
                            break;
                        }
                    }
                }

                //Add ingest flora code base
            }
        }

        #endregion
        #region Utils

        /// <summary>
        /// Cleans up the cached data
        /// </summary>
        /// <param name="removeTreeInstances"></param>
        public void CleanCachedData(bool removeTreeInstances = true)
        {
            if (removeTreeInstances)
            {
                m_storedTreeInstanceData.Clear();
            }

            if (m_storedGameObjects == null)
            {
                m_storedGameObjects = new List<GameObject>();
            }

            if (m_storedGameObjects.Count > 0)
            {
                for (int i = m_storedGameObjects.Count; i-- > 0;)
                {
                    if (m_storedGameObjects[i] == null)
                    {
                        m_storedGameObjects.RemoveAt(i);
                    }
                }
            }
        }
        /// <summary>
        /// Removes the tree instances from the list of instances
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="terrain"></param>
        public void RemoveTreeInstances(List<TreeInstance> instances, Terrain terrain)
        {
            List<TreeInstance> treeInstances = new List<TreeInstance>();
            treeInstances.AddRange(terrain.terrainData.treeInstances);
            if (treeInstances.Count > 0)
            {
                foreach (TreeInstance instance in instances)
                {
                    treeInstances.RemoveAll(inst => GeNaUtility.ApproximatelyEqual(inst.position.x, instance.position.x) && GeNaUtility.ApproximatelyEqual(inst.position.z, instance.position.z));
                }

                RefreshTerrainTreeInstances(ref treeInstances, terrain);
            }
        }
        /// <summary>
        /// Refreshes the terrain tree instances sets from current list then returns the new tree instance list from the terrain
        /// </summary>
        /// <param name="currenTreeInstances"></param>
        /// <param name="terrain"></param>
        public void RefreshTerrainTreeInstances(ref List<TreeInstance> currenTreeInstances, Terrain terrain)
        {
            terrain.terrainData.treeInstances = currenTreeInstances.ToArray();
            currenTreeInstances.Clear();
            currenTreeInstances.AddRange(terrain.terrainData.treeInstances);
            terrain.Flush();
        }
        /// <summary>
        /// Creates a new tree instance
        /// </summary>
        /// <param name="source"></param>
        /// <param name="terrain"></param>
        /// <param name="prototypeIndex"></param>
        /// <param name="storeInstance"></param>
        /// <returns></returns>
        public TreeInstance CreateNewTreeInstance(GameObject source, Terrain terrain, int prototypeIndex, bool storeInstance = true)
        {
            if (source != null)
            {
                // Create a new Tree Instance
                Transform instanceTransform = source.transform;
                Vector3 position = instanceTransform.position;
                //Fix y position
                float y = terrain.SampleHeight(position);
                position.y = y;
                Vector3 eulerAngles = instanceTransform.eulerAngles;
                Vector3 globalScale = instanceTransform.lossyScale;
                Vector3 normalizedLocalPos = GeNaMath.WorldToTerrainPosition(position, terrain);
                TreeInstance treeInstance = new TreeInstance
                {
                    prototypeIndex = prototypeIndex,
                    position = normalizedLocalPos,
                    widthScale = Mathf.Max(globalScale.x, globalScale.z),
                    heightScale = globalScale.y,
                    rotation = eulerAngles.y * Mathf.Deg2Rad,
                };

                if (storeInstance)
                {
                    //Add stored instance
                    m_storedTreeInstanceData.Add(new TreeInstance
                    {
                        color = treeInstance.color,
                        heightScale = treeInstance.heightScale,
                        lightmapColor = treeInstance.lightmapColor,
                        position = treeInstance.position,
                        prototypeIndex = treeInstance.prototypeIndex, 
                        rotation = treeInstance.rotation,
                        widthScale = treeInstance.widthScale
                    });
                }

                return treeInstance;
            }

            return new TreeInstance();
        }
        /// <summary>
        /// Adds tree instance to terrain
        /// </summary>
        /// <param name="source"></param>
        /// <param name="terrain"></param>
        /// <param name="prototypeIndex"></param>
        public void AddTreeInstanceToTerrain(GameObject source, Terrain terrain, int prototypeIndex)
        {
            terrain.AddTreeInstance(CreateNewTreeInstance(source, terrain, prototypeIndex));
        }
        /// <summary>
        /// Is part of the terrain
        /// </summary>
        /// <param name="terrainData"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public bool IsPartOfTerrain(TerrainData terrainData, GameObject prefab)
        {
            TreePrototype[] prototypes = terrainData.treePrototypes;
            foreach (var prototype in prototypes)
            {
                if (prototype.prefab == prefab)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Add prefab to the terrain
        /// </summary>
        /// <param name="terrainData"></param>
        /// <param name="prefab"></param>
        public void AddPrefabToTerrain(TerrainData terrainData, GameObject prefab)
        {
            var prototypes = new List<TreePrototype>(terrainData.treePrototypes)
            {
                new TreePrototype
                {
                    prefab = prefab
                }
            };
            terrainData.treePrototypes = prototypes.ToArray();
        }
        /// <summary>
        /// Sets the conversion type
        /// </summary>
        /// <param name="type"></param>
        public void SetConversionType(ConversionType type)
        {
            m_conversionType = type;
        }
        /// <summary>
        /// Converts back to gameobjects
        /// </summary>
        /// <returns></returns>
        public Terrain ConvertBackToGameObjects()
        {
            CleanCachedData(false);

            // Check if the Parent is part of a Terrain
            Terrain terrain = gameObject.GetComponentInParent<Terrain>();
            if (terrain == null)
            {
                GeNaDebug.LogWarning("You can only convert GameObjects to Trees if they are children of a Terrain!");
                return null;
            }

            //Check sync settings
            SyncTreesWithTerrainTreeInstances(terrain);

            //Removes the instances on the terrain
            RemoveTreeInstances(m_storedTreeInstanceData, terrain);

            switch (m_conversionDisableMode)
            {
                case ConversionDisableMode.Disable:
                {
                    for (int i = 0; i < m_storedGameObjects.Count; i++)
                    {
                        m_storedGameObjects[i].SetActive(true);
                    }

                    break;
                }
                case ConversionDisableMode.DestroyAndInstantiate:
                {
                    InstantiatePrefabs(m_instasiateData, gameObject);
                    break;
                }
            }

            SetConversionType(ConversionType.GameObject);
            m_storedTreeInstanceData.Clear();
            m_instasiateData.Clear();

            return terrain;
        }
        /// <summary>
        /// Checks trees synced with tree instances on the terrain to see if the instance still exists
        /// </summary>
        /// <param name="terrain"></param>
        public void SyncTreesWithTerrainTreeInstances(Terrain terrain)
        {
            if (m_syncTreeInstancesToGameObjects && terrain != null)
            {
                List<GameObject> sortGameObjects = new List<GameObject>();
                List<TreeInstasiateData> sortTreeInstasiateDatas = new List<TreeInstasiateData>();

                TreeInstance[] instances = terrain.terrainData.treeInstances;
                if (instances.Length > 0)
                {
                    for (int j = m_storedTreeInstanceData.Count; j-- > 0;)
                    {
                        TreeInstance storedInstance = m_storedTreeInstanceData[j];
                        for (int i = 0; i < instances.Length; i++)
                        {
                            if (!GeNaUtility.ApproximatelyEqual(instances[i].position.x, storedInstance.position.x) && 
                                !GeNaUtility.ApproximatelyEqual(instances[i].position.z, storedInstance.position.z))
                            {
                                sortGameObjects.Add(m_storedGameObjects[j]);
                                sortTreeInstasiateDatas.Add(m_instasiateData[j]);
                            }
                        }
                    }

                    foreach (GameObject storedGameObject in sortGameObjects)
                    {
                        m_storedGameObjects.Remove(storedGameObject);
                    }

                    foreach (TreeInstasiateData data in sortTreeInstasiateDatas)
                    {
                        m_instasiateData.Remove(data);
                    }
                }
            }
        }
        /// <summary>
        /// Instantiates all the prefabs
        /// </summary>
        /// <param name="treeData"></param>
        /// <param name="parent"></param>
        private void InstantiatePrefabs(List<TreeInstasiateData> treeData, GameObject parent)
        {
            for (int i = 0; i < treeData.Count; i++)
            {
                TreeInstasiateData data = treeData[i];
                if (ValidateTreeInstantiateData(data))
                {
                    SetObjectTransform(data, (GameObject) GeNaEvents.Instantiate(data.m_prefab), parent);
                }
            }
        }
        /// <summary>
        /// Sets up the object transform and parent
        /// </summary>
        /// <param name="data"></param>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        private void SetObjectTransform(TreeInstasiateData data, GameObject prefab, GameObject parent)
        {
            if (data != null)
            {
                prefab.transform.position = data.m_position;
                prefab.transform.eulerAngles = data.m_rotation;
                prefab.transform.localScale = data.m_scale;
                if (parent != null)
                {
                    prefab.transform.SetParent(parent.transform);
                }
            }
        }
        /// <summary>
        /// Validates the tree data see if there is a prefab to Instantiate from
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool ValidateTreeInstantiateData(TreeInstasiateData data)
        {
            if (data != null)
            {
                if (data.m_prefab != null)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}