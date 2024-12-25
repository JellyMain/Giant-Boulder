using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [InitializeOnLoad]
    public static class GeNaEditorEvents
    {
        #region Variables
        public static Action<string> onImportPackageCompleted;
        public static Action<string> onImportPackageCancelled;
        public static Action<string, string> onImportPackageFailed;
        public static Action onHierarchyChanged;
        public static Action onEditorUpdate;
        public static Action onBeforeAssemblyReloads;
        public static Action onAfterAssemblyReloads;
        #endregion
        #region Constructors
        static GeNaEditorEvents()
        {
            // On Import Package Completed
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            // On Import Package Cancelled
            AssetDatabase.importPackageCancelled -= OnImportPackageCancelled;
            AssetDatabase.importPackageCancelled += OnImportPackageCancelled;
            // On Import Package Failed
            AssetDatabase.importPackageFailed -= OnImportPackageFailed;
            AssetDatabase.importPackageFailed += OnImportPackageFailed;
            // On Before Assembly Reloads
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReloads;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReloads;
            // On After Assembly Reloads
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReloads;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReloads;
            // On Editor Update
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            // On Hierarchy Changed
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Initialize();
        }
        #endregion
        #region Methods

        /// <summary>
        /// Adds TerrainEvents Component on an Active Terrain 
        /// </summary>
        private static Dictionary<int, Terrain> terrains = new Dictionary<int, Terrain>(); 
        private static void AddTerrainEvents()
        {
            GeNaManager geNaManager = GeNaGlobalReferences.GeNaManagerInstance;
            // If geNa Manager is not in the scene
            if (geNaManager == null)
                return; // Exit
            
            Terrain[] activeTerrains = Terrain.activeTerrains;
        
            // Check for added terrains
            foreach (Terrain terrain in activeTerrains)
            {
                if (!terrains.ContainsKey(terrain.gameObject.GetInstanceID()))
                {
                    // This is a new terrain. Do something with it.
                    GameObject gameObject = terrain.gameObject;
                    GeNaTerrainEvents terrainEvents = gameObject.GetComponent<GeNaTerrainEvents>();
                    if (terrainEvents == null)
                        gameObject.AddComponent<GeNaTerrainEvents>();

                    // Then add it to the tracked terrains
                    terrains.Add(terrain.gameObject.GetInstanceID(), terrain);
                }
            }

            // Check for removed terrains
            List<int> keysToRemove = new List<int>();
            foreach (KeyValuePair<int, Terrain> pair in terrains)
            {
                if (!activeTerrains.Contains(pair.Value))
                {
                    // Terrain was removed, do something if needed
                    // Note: at this point the GameObject has been destroyed so you can't access its components

                    // Add the key to a list of keys to remove
                    keysToRemove.Add(pair.Key);
                }
            }

            // Remove the terrains from the dictionary
            foreach (int key in keysToRemove)
            {
                terrains.Remove(key);
            }
        }
        // Sets up Default Events
        private static void Initialize()
        {
            // When the Hierarchy Changes, add the Terrain Events Script to an Active Terrain
            onHierarchyChanged -= AddTerrainEvents;
            onHierarchyChanged += AddTerrainEvents;
            // Call it once
            AddTerrainEvents();
            // Setup GeNaEvents
            GeNaEditorUtility.SubscribeEvents();
        }
        private static void OnImportPackageCompleted(string packageName) => onImportPackageCompleted?.Invoke(packageName);
        /// <summary>
        /// Called when a package import is Cancelled.
        /// </summary>
        private static void OnImportPackageCancelled(string packageName) => onImportPackageCancelled?.Invoke(packageName);
        /// <summary>
        /// Called when a package import fails.
        /// </summary>
        private static void OnImportPackageFailed(string packageName, string error) => onImportPackageFailed?.Invoke(packageName, error);
        /// <summary>
        /// Called Before Assembly Reloads
        /// </summary>
        private static void OnBeforeAssemblyReloads()
        {
            onBeforeAssemblyReloads?.Invoke();
            GeNaFactory.Dispose();
        }
        /// <summary>
        /// Called After Assembly Reloads
        /// </summary> 
        private static void OnAfterAssemblyReloads() => onAfterAssemblyReloads?.Invoke();
        /// <summary>
        /// Called when Editor Updates
        /// </summary>
        private static void OnEditorUpdate() => onEditorUpdate?.Invoke();
        /// <summary>
        /// Event that is raised when an object or group of objects in the hierarchy changes.
        /// </summary>
        private static void OnHierarchyChanged() => onHierarchyChanged?.Invoke();
        #endregion
    }
}