using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace GeNa.Core
{
    public static class PrefabUnpackerUtility
    {
        #region Methods
        /// <summary>
        /// Function used to perform the GeNa Unpack decorators
        /// This will get all of the decorators on the gameobject 'currentGameObject'
        /// Then unpacks all based on the decorator setup
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <param name="destroyDuplicatedObject"></param>
        /// <param name="unpackMode"></param>
        /// <param name="interactionMode"></param>
        /// <returns></returns>
        public static GameObject ExecuteUnpackMasterGameObject(GameObject currentGameObject, bool destroyDuplicatedObject = false, PrefabUnpackMode unpackMode = PrefabUnpackMode.OutermostRoot, InteractionMode interactionMode = InteractionMode.AutomatedAction)
        {
            if (currentGameObject == null)
            {
                Debug.LogError("No gameobject was provided to perform the prefab unpack");
                return null;
            }
            GameObject duplicatedGameObject = DuplicateHierarchyObject(currentGameObject);
            IDecorator[] decorators = GetAllGeNaPrefabUnpackerFromObject(duplicatedGameObject);
            List<GameObject> currentGameObjects = new List<GameObject>();
            foreach (IDecorator decorator in decorators)
            {
                if (decorator is MonoBehaviour mono)
                    currentGameObjects.Add(mono.gameObject);
            }
            bool unpackPrefab = decorators.Length > 0;
            if (unpackPrefab)
            {
                UnpackGroupOfGameObjects(currentGameObjects.ToArray(), true, unpackMode, interactionMode);
            }
            if (destroyDuplicatedObject)
                Object.DestroyImmediate(duplicatedGameObject);
            return duplicatedGameObject;
        }
        /// <summary>
        /// Used to repack the an object to the source.
        /// </summary>
        /// <param name="sourcePrefab"></param>
        /// <param name="sourceNonPrefab"></param>
        /// <param name="interactionMode"></param>
        public static void ExecuteRepackPrefab(GameObject sourcePrefab, GameObject sourceNonPrefab, InteractionMode interactionMode = InteractionMode.AutomatedAction)
        {
            if (sourcePrefab == null)
            {
                Debug.LogError("Source prefab was not provided");
                return;
            }
            if (sourceNonPrefab == null)
            {
                Debug.LogError("Non prefab source gameobject was not provided");
                return;
            }
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(sourcePrefab);
            if (!string.IsNullOrEmpty(path))
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(sourceNonPrefab, path, interactionMode);
            }
        }
        /// <summary>
        /// Used to destroy a gameobject immediately no undo recorded
        /// </summary>
        /// <param name="currentGameObject"></param>
        public static void ExecuteDeleteGameObject(GameObject currentGameObject)
        {
            if (currentGameObject == null)
            {
                Debug.LogError("No Gameobject was provided");
                return;
            }
            Object.DestroyImmediate(currentGameObject);
        }
        private static bool ContainsPrefabs(GameObject currentGameObject)
        {
            Transform[] children = currentGameObject.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.gameObject == currentGameObject)
                    continue;
                if (GeNaEditorUtility.IsPrefab(child.gameObject))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Performs the unpacking of the current GameObject
        /// </summary>
        /// <param name="currentGameObjects"></param>
        /// <param name="destroyDecorator"></param>
        /// <param name="unpackMode"></param>
        /// <param name="interactionMode"></param>
        private static void UnpackGroupOfGameObjects(GameObject[] currentGameObjects, bool destroyDecorator = true, PrefabUnpackMode unpackMode = PrefabUnpackMode.OutermostRoot, InteractionMode interactionMode = InteractionMode.AutomatedAction)
        {
            if (currentGameObjects.Length < 1)
                return;
            foreach (GameObject gameObject in currentGameObjects)
            {
                IDecorator[] decorators = gameObject.GetComponents<IDecorator>();
                bool unpackPrefab = false;
                if (decorators.Length > 0)
                    unpackPrefab = decorators.Any(item => item.UnpackPrefab);
                if (destroyDecorator)
                {
                    IEnumerable<IDecorator> unpackers = decorators.Where(item => item.UnpackPrefab);
                    foreach (IDecorator decorator in unpackers)
                    {
                        if (decorator is MonoBehaviour mono)
                            GeNaEvents.Destroy(mono);
                    }
                }
                if (unpackPrefab)
                    UnPackPrefab(gameObject, unpackMode, interactionMode);
            }
        }
        /// <summary>
        /// Duplicates the prefab object in the Hierarchy
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <returns></returns>
        private static GameObject DuplicateHierarchyObject(GameObject currentGameObject)
        {
            if (currentGameObject == null)
            {
                return null;
            }
            bool isPrefab = false;
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(currentGameObject);
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabRoot != null)
            {
                if (prefabRoot != null)
                {
                    isPrefab = true;
                }
            }
            GameObject instantiateGameObject = InstantiateGameObject(currentGameObject, prefabRoot, isPrefab);
            return instantiateGameObject;
        }
        /// <summary>
        /// Instantiate the GameObject if it's a prefab or not
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <param name="prefabGameObject"></param>
        /// <param name="isPrefab"></param>
        /// <returns></returns>
        private static GameObject InstantiateGameObject(GameObject currentGameObject, GameObject prefabGameObject, bool isPrefab)
        {
            if (currentGameObject == null)
            {
                return null;
            }
            if (isPrefab)
            {
                return PrefabUtility.InstantiatePrefab(prefabGameObject) as GameObject;
            }
            else
            {
                GameObject newObject = Object.Instantiate(currentGameObject);
                newObject.name = currentGameObject.name;
                return newObject;
            }
        }
        /// <summary>
        /// Gets all the prefab unpacker components
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <returns></returns>
        private static IDecorator[] GetAllGeNaPrefabUnpackerFromObject(GameObject currentGameObject)
        {
            List<IDecorator> prefabUnpackers = new List<IDecorator>();
            if (currentGameObject != null)
            {
                IDecorator[] datas = currentGameObject.GetComponentsInChildren<IDecorator>();
                if (datas.Length > 0)
                {
                    foreach (IDecorator data in datas)
                    {
                        if (data.UnpackPrefab)
                            prefabUnpackers.Add(data);
                    }
                }
            }
            return prefabUnpackers.ToArray();
        }
        /// <summary>
        /// Unpacks the prefab based on the settings
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <param name="unpackMode"></param>
        /// <param name="interactionMode"></param>
        private static void UnPackPrefab(GameObject currentGameObject, PrefabUnpackMode unpackMode = PrefabUnpackMode.OutermostRoot, InteractionMode interactionMode = InteractionMode.AutomatedAction)
        {
            if (currentGameObject == null)
                return;
            if (PrefabUtility.IsOutermostPrefabInstanceRoot(currentGameObject))
                PrefabUtility.UnpackPrefabInstance(currentGameObject, unpackMode, interactionMode);
        }
        #endregion
    }
}