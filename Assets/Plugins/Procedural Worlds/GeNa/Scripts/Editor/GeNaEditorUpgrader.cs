using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    [InitializeOnLoad]
    public class GeNaEditorUpgrader
    {
        static GeNaEditorUpgrader()
        {
            AssetDatabase.importPackageCompleted -= DetectMaintenence;
            AssetDatabase.importPackageCompleted += DetectMaintenence;
        }
        public static void Pass1(GeNaSpawner spawner)
        {
            if (spawner == null)
                return;
            if (!GeNaEditorUtility.IsPrefab(spawner.gameObject))
                return;
            if (spawner.Version == 1)
            {
                if (spawner.SpawnerData is SpawnerData spawnerData)
                {
                    spawner.Upgrade(2);
                    return;
                }
                spawner.Load();
                DataBufferScriptable dataFile = spawner.GetDataFile();
                if (dataFile != null)
                    return;
                dataFile = GeNaEditorUtility.GetMainDataBufferScriptable(spawner);
                if (dataFile == null)
                    return;
                spawner.ConvertToFile(dataFile);
                spawner.Upgrade(2);
                EditorUtility.SetDirty(spawner);
            }
        }
        public static void Pass2(GeNaSpawner spawner)
        {
        }
        public static void PerformUpgrade()
        {
            if (Application.isPlaying)
                return;
            int passes = 4;
            // Perform Pass 1 - Checking if GeNa needs Maintenance
            AssetDatabase.StartAssetEditing();
            int pass = 1;
            EditorUtility.DisplayProgressBar("Processing GeNa Update", $"Pass {pass} - Upgrading Spawners", (float)pass / (float)passes);
            string filter = "t:Prefab";
            string[] guids = AssetDatabase.FindAssets(filter);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object myObject in objects)
                {
                    if (myObject is Spawner spawner)
                    {
                        Pass1(spawner);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.StopAssetEditing();

            // Perform Pass 2 - Converting Scripts over to Non-DLL Types
            pass++;
            AssetDatabase.StartAssetEditing();
            EditorUtility.DisplayProgressBar("Processing GeNa Update", $"Pass {pass} - Converting Script References", (float)pass / (float)passes);
            AssetDatabase.Refresh();
            ConvertScriptableAssets<GeNaSpawnerData, SpawnerData>();
            ConvertScriptableAssets<Palette, PaletteData>();
            ConvertScriptableAssets<DataBufferScriptable, ByteData>();
            ConvertScriptableAssets<GeNaRiverProfile, RiverProfile>();
            ConvertScriptableAssets<GeNaRoadProfile, RoadProfile>();
            ConvertMonoBehaviourAssets<GeNaSpline, Spline>();
            ConvertMonoBehaviourAssets<GeNaSpawner, Spawner>();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.StopAssetEditing();

            // Perform Pass 3 - Deserialize all Spawner Data down to Serialized versions of the Scripts
            AssetDatabase.StartAssetEditing();
            pass++;
            EditorUtility.DisplayProgressBar("Processing GeNa Update", $"Pass {pass} - Deserializing & Re-Serializing Spawners", (float)pass / (float)passes);
            guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object myObject in objects)
                {
                    if (myObject is Spawner spawner)
                    {
                        if (spawner.Version == 1)
                        {
                            // spawner.Deserialize();
                            GeNaEditorUtility.GetMainSpawnerData(spawner);
                            spawner.Deserialize();
                            GeNaEditorUtility.RemoveAllTempDataBufferScriptables(spawner);
                            spawner.Upgrade(2);
                            EditorUtility.SetDirty(spawner);
                        }
                    }
                    else if (myObject is GeNaSubSpawnerDecorator decorator)
                    {
                        EditorUtility.SetDirty(decorator.gameObject);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.StopAssetEditing();

            // Perform Pass 4 - Remove any duplicate ScriptableObjects in Spawners
            AssetDatabase.StartAssetEditing();
            pass++;
            EditorUtility.DisplayProgressBar("Processing GeNa Update", $"Pass {pass} - Cleaning up Spawners", (float)pass / (float)passes);
            guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object myObject in objects)
                {
                    if (myObject is Spawner spawner)
                    {
                        GeNaEditorUtility.RemoveAllDuplicateScriptables(spawner);
                        EditorUtility.SetDirty(spawner);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.StopAssetEditing();

            // Clear the Progress bar
            EditorUtility.ClearProgressBar();
            CompilationPipeline.RequestScriptCompilation();
        }
        static void DeselectAll()
        {
            Selection.objects = new Object[0];
        }
        public static void DetectMaintenence(string packageName)
        {
            if (Directory.Exists(GetAssetPath("Dev Utilities")))
                return;
            if (Application.isPlaying)
                return;
            string maintenanceFilePath = "Assets/Procedural Worlds/GeNa/Scripts/GeNaMaintenanceToken.dat";
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(maintenanceFilePath);
            if (asset != null)
            {
                PerformUpgrade();
                // Delete Maintainence File
                AssetDatabase.DeleteAsset(maintenanceFilePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        private static void ConvertMonoBehaviour<T, Other>() where T : MonoBehaviour where Other : MonoBehaviour
        {
            Type type = typeof(T);
            GameObject tmpGO = new GameObject("tempOBJ");
            Other inst = tmpGO.AddComponent<Other>();
            MonoScript yourReplacementScript = MonoScript.FromMonoBehaviour(inst);
            T[] myObjects = GeNaEvents.FindObjectsOfType<T>();
            foreach (T myObject in myObjects)
            {
                if (myObject is T tObject)
                {
                    if (tObject.GetType() != type)
                        continue;
                    SerializedObject serializedObject = new SerializedObject(myObject);
                    SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
                    serializedObject.Update();
                    if (scriptProperty.objectReferenceValue.name != yourReplacementScript.name)
                    {
                        scriptProperty.objectReferenceValue = yourReplacementScript;
                        serializedObject.ApplyModifiedProperties();
                    }
                    Object.DestroyImmediate(tmpGO);
                }
            }
        }
        private static void ConvertScriptable<T, Other>() where T : ScriptableObject where Other : ScriptableObject
        {
            Type type = typeof(T);
            Other newObject = ScriptableObject.CreateInstance<Other>();
            MonoScript yourReplacementScript = MonoScript.FromScriptableObject(newObject);
            T[] objects = GeNaEvents.FindObjectsOfType<T>();
            foreach (T myObject in objects)
            {
                if (myObject is T tObject)
                {
                    if (tObject.GetType() != type)
                        continue;
                    SerializedObject serializedObject = new SerializedObject(tObject);
                    SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
                    if (scriptProperty.objectReferenceValue.name != yourReplacementScript.name)
                    {
                        scriptProperty.objectReferenceValue = yourReplacementScript;
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }
                }
            }
        }
        private static void ConvertScriptableAssets<T, Other>() where T : ScriptableObject where Other : ScriptableObject
        {
            Type type = typeof(T);
            string filter = $"t:{type.Name}";
            string[] guids = AssetDatabase.FindAssets(filter);
            Other newObject = ScriptableObject.CreateInstance<Other>();
            MonoScript yourReplacementScript = MonoScript.FromScriptableObject(newObject);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object myObject in objects)
                {
                    if (myObject is T tObject)
                    {
                        if (tObject.GetType() != type)
                            continue;
                        SerializedObject serializedObject = new SerializedObject(tObject);
                        SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
                        if (scriptProperty.objectReferenceValue.name != yourReplacementScript.name)
                        {
                            scriptProperty.objectReferenceValue = yourReplacementScript;
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                        }
                    }
                }
            }
            Object.DestroyImmediate(newObject);
        }
        private static void ConvertMonoBehaviourAssets<T, Other>(bool markDirty = false) where T : MonoBehaviour where Other : MonoBehaviour
        {
            Type type = typeof(T);
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            if (guids.Length == 0)
                return;
            GameObject tmpGO = new GameObject("tempOBJ");
            Other inst = tmpGO.AddComponent<Other>();
            MonoScript yourReplacementScript = MonoScript.FromMonoBehaviour(inst);
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Object[] myObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (Object myObject in myObjects)
                {
                    if (myObject is T spawner)
                    {
                        if (spawner.GetType() != type)
                            continue;
                        SerializedObject serializedObject = new SerializedObject(spawner);
                        SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
                        serializedObject.Update();
                        if (scriptProperty.objectReferenceValue.name != yourReplacementScript.name)
                        {
                            scriptProperty.objectReferenceValue = yourReplacementScript;
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            Object.DestroyImmediate(tmpGO);
        }
        public static void PerformUpgradeOperation()
        {
            // TextAsset textAsset = new TextAsset();
            // AssetDatabase.CreateAsset(textAsset, "Assets/Procedural Worlds/GeNa/Scripts/GeNaMaintenanceToken.dat");
            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();
            PerformUpgrade();
        }
        public static void PerformSceneUpgrade()
        {
            GeNaSpawner[] spawners = GeNaEvents.FindObjectsOfType<GeNaSpawner>();
            foreach (GeNaSpawner spawner in spawners)
                Pass1(spawner);
            ConvertScriptable<GeNaSpawnerData, SpawnerData>();
            ConvertScriptable<Palette, PaletteData>();
            ConvertScriptable<DataBufferScriptable, ByteData>();
            ConvertMonoBehaviour<GeNaSpline, Spline>();
            ConvertMonoBehaviour<GeNaSpawner, Spawner>();
            EditorSceneManager.MarkAllScenesDirty();
        }
        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="fileName">File name to search for</param>
        /// <returns></returns>
        private static string GetAssetPath(string fileName)
        {
            string fName = Path.GetFileNameWithoutExtension(fileName);
            string[] assets = AssetDatabase.FindAssets(fName, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
                if (Path.GetFileName(path) == fileName)
                {
                    return path;
                }
            }
            return "";
        }
        public static void DetectedUpgrade()
        {
            if (EditorUtility.DisplayDialog("Maintainence Detected!",
                    "GeNa has detected that your project needs to perform a Maintainenece, would you like to perform it now?",
                    "Yes",
                    "No"))
            {
                PerformUpgradeOperation();
            }
        }
    }
}