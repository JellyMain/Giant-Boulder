// Copyright © 2018 Procedural Worlds Pty Limited.  All Rights Reserved.
using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GeNa.Core
{
    /// <summary>
    /// Interface class for the GeNa settings. Handles storing and retrieving all scoped setting, including Editor scope settings.
    /// </summary>
    [XmlRoot("GeNaPreferences")]
    public class Preferences
    {
        #region Variables
        private const string ID = "GeNa";
        private const string PREF_FILE = "Preferences-" + ID;
        private const string PREF_EXTENSION = ".txt";
        private const string GENA_FOLDER_NAME = "GeNa";
        private const string GENA_RES_INTERNAL_PATH = "Resources";
        private static bool ms_dirty = false;
        private static Preferences m_instance;
        public bool m_defSpawnToTarget = true;
        #endregion
        #region Properties
        /// <summary>
        /// The instance that will hold the values in memory.
        /// </summary>
        private static Preferences Instance
        {
            get
            {
                if (m_instance == null)
                    Load();
                return m_instance;
            }
        }
        /// <summary>
        /// The default 'Spawn To Target' value for new spawners.
        /// </summary>
        public static bool DefaultSpawnToTarget
        {
            get => Instance.m_defSpawnToTarget;
            set
            {
                if (value != Instance.m_defSpawnToTarget)
                {
                    Instance.m_defSpawnToTarget = value;
#if UNITY_EDITOR
                    Save();
                    EditorPrefs.SetBool(Defaults.DEF_SPAWN_TO_TARGET_KEY, Instance.m_defSpawnToTarget);
#endif
                }
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Instances of this mean nothing to users. This should be only used by deserialisation.
        /// </summary>
        private Preferences()
        {
            //Removes warning
            if (!ms_dirty)
                ms_dirty = false;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Syncronises settings of an instance with the Editor. 
        /// See login in <seealso cref="EditorNeedsUpdate{T}(T, ref T, T)"/>.
        /// </summary>
        protected static void SyncWithEditor()
        {
#if UNITY_EDITOR
            if (m_instance == null)
            {
                Debug.LogErrorFormat("[GeNa] Can't call Editor syncing on null Preferences. Aborting.");
                return;
            }
            // Undo Settings
            // Advanced Settings
            SyncSetting(Defaults.DEF_SPAWN_TO_TARGET_KEY, ref m_instance.m_defSpawnToTarget, true);
            if (ms_dirty)
                Save();
#endif
        }
        /// <summary>
        /// Load the preferences.
        /// </summary>
        public static void Load()
        {
            TextAsset asset = Resources.Load<TextAsset>(PREF_FILE);
            if (asset != null)
            {
                m_instance = Deserialize(asset.text);
                if (m_instance == null)
                {
                    Debug.LogError("[GeNa] Unable to get data from preference storage. Creating a new one.");
                    m_instance = new Preferences();
                    // Force save to save the new storage.
                    ms_dirty = true;
                }
            }
            else
            {
                Debug.LogWarningFormat("[GeNa] Unable to locate GeNa preferences. Falling back to the defaults.");
                m_instance = new Preferences();
                // Force save to save to fix the corrupted storage.
                ms_dirty = true;
            }
            // Sync once loaded or created
            SyncWithEditor();
        }
        /// <summary>
        /// Deserialize preferences from XML string
        /// </summary>
        /// <param name="xmlString">XML code to deserialize</param>
        internal static Preferences Deserialize(string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Preferences));
            return serializer.Deserialize(new StringReader(xmlString)) as Preferences;
        }
#if UNITY_EDITOR
        /// <summary>
        /// Syncs the Editor settings with the ones stored in the project, so they can be used runtime.
        /// </summary>
        private static void SyncSetting(string key, ref bool value, bool nonSetValue)
        {
            bool editorVal = EditorPrefs.GetBool(key, nonSetValue);
            if (EditorNeedsUpdate(editorVal, ref value, nonSetValue))
                EditorPrefs.SetBool(key, value);
        }
        /// <summary>
        /// Syncs the Editor settings with the ones stored in the project, so they can be used runtime.
        /// </summary>
        private static void SyncSetting(string key, ref float value, float nonSetValue)
        {
            float editorVal = EditorPrefs.GetFloat(key, nonSetValue);
            if (EditorNeedsUpdate(editorVal, ref value, nonSetValue))
                EditorPrefs.SetFloat(key, value);
        }
        /// <summary>
        /// Syncs the Editor settings with the ones stored in the project, so they can be used runtime.
        /// </summary>
        private static void SyncSetting(string key, ref int value, int nonSetValue)
        {
            int editorVal = EditorPrefs.GetInt(key, nonSetValue);
            if (EditorNeedsUpdate(editorVal, ref value, nonSetValue))
                EditorPrefs.SetInt(key, value);
        }
        /// <summary>
        /// Syncs the Editor settings with the ones stored in the project, so they can be used runtime.
        /// </summary>
        private static void SyncSetting(string key, ref string value, string nonSetValue)
        {
            string editorVal = EditorPrefs.GetString(key, nonSetValue);
            if (EditorNeedsUpdate(editorVal, ref value, nonSetValue))
                EditorPrefs.SetString(key, value);
        }
        /// <summary>
        /// This is where the logic is implemented, which decides how we sync. Returns true if the Editor needs to be updated and handles everything else.
        /// </summary>
        /// <returns>Returns true if the Editor needs to be updated.</returns>
        private static bool EditorNeedsUpdate<T>(T editorVal, ref T storedVal, T nonSetValue) where T : IComparable
        {
            if (editorVal.CompareTo(storedVal) == 0)
                return false;

            // Editor setting takes precedence if it has been set
            if (editorVal.CompareTo(nonSetValue) != 0)
            {
                storedVal = editorVal;
                // Set dirty
                ms_dirty = true;
                return false;
            }

            // Let's update the editor setting otherwise.
            // For example, this could be a computer reinstall and we are getting the settings from version control.
            return true;
        }
        /// <summary>
        /// Save the preferences.
        /// </summary>
        public static void Save()
        {
            bool needDBRefresh = false;
            TextAsset asset = Resources.Load<TextAsset>(PREF_FILE);
            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarningFormat("Unable to locate GeNa preferences to save to. " +
                                       "Creating a new storage to store the settings.");
                path = GetPreferencesPath();
                needDBRefresh = true;
            }
            if (IsPathCorrect(path) == false)
            {
                Debug.LogErrorFormat("[GeNa] Preferences saving aborted. Were GeNa preferences moved or copied? Path is inccorrect: '{0}'.", path);
                return;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(Preferences));
            using (FileStream stream = new FileStream(path, FileMode.Create))
                serializer.Serialize(stream, m_instance);
            if (needDBRefresh)
                AssetDatabase.Refresh();
        }
        /// <summary>
        /// Gets path for a new preference file.
        /// </summary>
        private static string GetPreferencesPath()
        {
            string resFolder = GetResFolder();
            if (string.IsNullOrEmpty(resFolder))
            {
                Debug.LogErrorFormat("Unable to locate the GeNa resources folder at '{0}/{1}'", GENA_FOLDER_NAME, GENA_RES_INTERNAL_PATH);
                return null;
            }
            return resFolder + "/" + PREF_FILE + PREF_EXTENSION;
        }
        /// <summary>
        /// Checks if the path m_points to the desired folder.
        /// </summary>
        private static bool IsPathCorrect(string prefPath)
        {
            string resFolder = GetResFolder();
            if (string.IsNullOrEmpty(resFolder))
            {
                Debug.LogErrorFormat("Unable to locate the GeNa resources folder at '{0}/{1}'", GENA_FOLDER_NAME, GENA_RES_INTERNAL_PATH);
                return false;
            }
            if (Path.GetFullPath(prefPath) == Path.GetFullPath(resFolder + "/" + PREF_FILE + PREF_EXTENSION))
                return true;
            return false;
        }
        /// <summary>
        /// Returns the GeNa Resources folder or null.
        /// </summary>
        private static string GetResFolder() => PWCommon5.Utils.GetAppsSubfolder(GENA_FOLDER_NAME, GENA_RES_INTERNAL_PATH);
#endif
        #endregion
    }
}