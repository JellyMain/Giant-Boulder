// Copyright © 2018 Procedural Worlds Pty Limited.  All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PWCommon5;
using GeNa.Core;
namespace GeNa.Core
{
    /// <summary>
    /// Editor Window
    /// </summary>
    public class GlobalSpawnDialog : EditorWindow, IPWEditor
    {
        #region Variables
        protected EditorUtils m_editorUtils;
        [SerializeField] protected GeNaSpawner m_spawner;
        [SerializeField] protected Transform m_hitTransform;
        [SerializeField] protected bool m_initialized = false;
        [SerializeField] protected bool m_okFocused = false;
        protected const float BTN_WIDTH = 70f;
        #endregion
        #region Properties
        public bool PositionChecked { get; set; }
        public GeNaSpawnerData SpawnerData
        {
            get
            {
                if (m_spawner == null)
                    return null;
                return m_spawner.SpawnerData;
            }
        }
        #endregion
        #region Methods
        private void RemoveWarnings()
        {
            if (m_spawner)
                m_spawner.name = name;
        }
        private void OnDestroy() => m_editorUtils?.Dispose();
        private void OnEnable()
        {
            // Get editor utils for this
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, null, null);
            minSize = maxSize = new Vector2(330f, 100f);
        }
        private void OnGUI()
        {
            m_editorUtils.Initialize(); // Do not remove this!
            GUILayout.Space(5f);
            m_editorUtils.Label("Are you sure?");
            GUILayout.Space(15f);
            if (m_spawner != null)
            {
                GeNaSpawnerData spawnerData = m_spawner.SpawnerData;
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80f;
                PlacementCriteria crit = spawnerData.PlacementCriteria;
                crit.GlobalSpawnJitterPct = m_editorUtils.Slider("Jitter", crit.GlobalSpawnJitterPct * 100f, 0f, 100f) * 0.01f;
                EditorGUIUtility.labelWidth = labelWidth;
            }
            // Need to use this, otherwise the progress bar can rarely cause unidentifiable GUI errors.
            bool doSpawn = false;
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                // Spawn
                GUI.SetNextControlName("Ok button");
                if (m_editorUtils.Button("Ok Btn", GUILayout.Width(BTN_WIDTH)))
                {
                    if (m_initialized && SpawnerData != null)
                        doSpawn = true;
                    else
                    {
                        Debug.LogErrorFormat("[GeNa]: Global Spawn dialogue unable to proceed ({0}). This is likely due to a coding error. Aborting Global Spawn...",
                            m_initialized == false ? "Not initialized" : "Spawner is " + SpawnerData);
                        GUIUtility.hotControl = 0;
                        Close();
                    }
                }
                // or close
                if (m_editorUtils.Button("Cancel Btn", GUILayout.Width(BTN_WIDTH)))
                {
                    GUIUtility.hotControl = 0;
                    Close();
                }
            }
            GUILayout.EndHorizontal();
            if (!m_okFocused)
            {
                GUI.FocusControl("Ok button");
                m_okFocused = true;
            }
            // Need to use this, otherwise the progress bar can rarely cause unidentifiable GUI errors.
            if (doSpawn)
            {
                SpawnerEntry entry = new SpawnerEntry(m_spawner)
                {
                    Target = m_hitTransform,
                    Description = "Global Spawn"
                };
                List<SpawnCall> spawnCalls = GeNaSpawnerInternal.GenerateGlobalSpawnCalls(m_spawner.SpawnerData);
                entry.AddSpawnCalls(spawnCalls);
                GeNaEditorUtility.ScheduleSpawn(entry);
                Close();
            }
        }
        /// <summary>
        /// This custom dialogue needs to be initialized with the Editor that called and the hit info this call belongs to (for the callback).
        /// </summary>
        public void Init(GeNaSpawner actingSpawner, RaycastHit hitInfo)
        {
            Init(actingSpawner, hitInfo.transform);
        }
        /// <summary>
        /// This custom dialogue needs to be initialized with the Editor that called and the hit info this call belongs to (for the callback).
        /// </summary>
        public void Init(GeNaSpawner actingSpawner, Transform transform)
        {
            m_spawner = actingSpawner;
            m_hitTransform = transform;
            m_initialized = true;
        }
        #endregion
    }
}