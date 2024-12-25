using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaSpawnerExtension))]
    public class GeNaSpawnerExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaSpawnerExtension m_extension;
        private bool m_isAsset;

        private static Color SPAWN_TARGET_COLOR = new Color(0.8f, 0.8f, 0.0f, 0.5f);
        protected bool m_modifySpawner = false;
        public AabbTest[] m_fitnessArray = Array.Empty<AabbTest>();

        public override void OnSelected()
        {
            if (m_extension == null)
                m_extension = target as GeNaSpawnerExtension;
            if (m_extension != null)
            {

                m_extension.Load();
                m_extension.PreExecute();
            }
            Validate();
        }

        protected void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            m_extension = target as GeNaSpawnerExtension;
            if (m_extension == null)
                return;
            m_isAsset = AssetDatabase.Contains(m_extension);
        }

        private void Validate()
        {
            if (m_extension == null)
                m_extension = target as GeNaSpawnerExtension;
            if (m_extension == null)
                return;
            GeNaSpawner spawner = m_extension.Spawner;
            if (spawner == null)
                return;
            spawner.Load();
            GeNaSpawnerData spawnerData = spawner.SpawnerData;
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                SpawnerEditor.ValidateSpawnerPrototypes(spawner, spawnerData, terrain, true);
            }
        }

        public override void OnSceneGUI()
        {
            if (m_extension == null)
                m_extension = target as GeNaSpawnerExtension;
            if (m_extension == null)
                return;
            Handles.color = new Color(0f, 1f, 0f, 0.25f);
            List<SpawnCall> spawnCalls = m_extension.SpawnCalls;
            foreach (SpawnCall spawnCall in spawnCalls)
            {
                if (!spawnCall.IsActive)
                    continue;
                if (spawnCall.CanSpawn)
                    Handles.color = new Color(0f, 1f, 0f, 0.25f);
                else
                    Handles.color = new Color(1f, 0f, 0f, 0.25f);
                float radius = Mathf.Clamp(spawnCall.SpawnRange / 6f, 0.25f, 1f);
                Handles.DrawSolidArc(spawnCall.Location, Vector3.up, Vector3.forward, 360f, radius);
                // Handles.color = new Color(0f, 0f, 1f, 0.25f);
                radius = spawnCall.SpawnRange * .5f;
                Handles.DrawWireArc(spawnCall.Location, Vector3.up, Vector3.forward, 360f, radius);
            }

            if (m_modifySpawner)
            {
                GeNaSpawner spawner = m_extension.Spawner;
                SpawnerEditor.GeNaSpawnerEditor(spawner);
            }

            Transform groundObject = m_extension.GetTarget(out RaycastHit hitInfo);
            if (groundObject != null)
            {
                Vector3 point = hitInfo.point;
                Vector3 normal = hitInfo.normal;
                Vector3 up = Vector3.Cross(normal, Vector3.up);
                float angle = 360f;
                float radius = 2f;
                Handles.color = SPAWN_TARGET_COLOR;
                Handles.DrawSolidArc(point, normal, up, angle, radius);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
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
            if (m_extension == null)
                m_extension = target as GeNaSpawnerExtension;
            EditorGUI.BeginChangeCheck();
            {
                GeNaSpawner oldSpawner = m_extension.Spawner;
                m_extension.Spawner = (GeNaSpawner)m_editorUtils.ObjectField("Spawner", m_extension.Spawner, typeof(GeNaSpawner), true, HelpEnabled);
                if (oldSpawner != m_extension.Spawner)
                {
                    if (m_extension.Spawner != null)
                    {
                        var spawner = m_extension.Spawner;
                        if (spawner != null)
                        {
                            var data = spawner.SpawnerData;
                            if (data != null)
                            {
                                m_extension.SpawnRange = data.SpawnRange;
                                m_extension.FlowRate = data.SpawnRange;
                            }
                        }

                        Validate();
                    }
                }

                if (m_extension.Spawner != null)
                {
                    GeNaSpawnerData spawnerData = m_extension.SpawnerData;
                    if (spawnerData != null)
                    {
                        SpawnerEntry entry = m_extension.SpawnerEntry;
                        if (entry != null)
                        {
                            entry.UseLargeRanges = m_editorUtils.Toggle("Use Large Ranges", entry.UseLargeRanges, HelpEnabled);
                            if (entry.UseLargeRanges)
                            {
                                // Display Spawner Data
                                entry.FlowRate = m_editorUtils.FloatField("Flow Rate", entry.FlowRate, HelpEnabled);
                                entry.SpawnRange = m_editorUtils.FloatField("Spawn Range", entry.SpawnRange, HelpEnabled);
                                entry.ThrowDistance = m_editorUtils.FloatField("Throw Distance", entry.ThrowDistance, HelpEnabled);
                                // Display Offset
                                entry.OffsetPosition = m_editorUtils.Vector3Field("Offset Position", entry.OffsetPosition, HelpEnabled);
                                entry.OffsetRotation = m_editorUtils.Vector3Field("Offset Rotation", entry.OffsetRotation, HelpEnabled);
                            }
                            else
                            {
                                // Display Spawner Data
                                entry.FlowRate = m_editorUtils.FloatField("Flow Rate", entry.FlowRate, HelpEnabled);
                                entry.SpawnRange = m_editorUtils.Slider("Spawn Range", entry.SpawnRange, 1f, 200f, HelpEnabled);
                                entry.ThrowDistance = m_editorUtils.Slider("Throw Distance", entry.ThrowDistance, 0f, entry.SpawnRange, HelpEnabled);
                                // Display Offset
                                Vector3 offsetPosition = entry.OffsetPosition;
                                Vector3 offsetRotation = entry.OffsetRotation;
                                offsetPosition.x = m_editorUtils.Slider("Offset Position X", offsetPosition.x, -20f, 20f, HelpEnabled);
                                offsetPosition.y = m_editorUtils.Slider("Offset Position Y", offsetPosition.y, -2f, 2f, HelpEnabled);
                                offsetRotation.y = m_editorUtils.Slider("Offset Rotation Y", offsetRotation.y, -180f, 180f, HelpEnabled);
                                entry.OffsetPosition = offsetPosition;
                                entry.OffsetRotation = offsetRotation;
                            }

                            entry.FlowRateAssigned = true;
                        }
                    }

                    m_modifySpawner = m_editorUtils.Toggle("Modify Spawner", m_modifySpawner, HelpEnabled);
                    m_extension.AutoIterate = m_editorUtils.Toggle("Auto Iterate", m_extension.AutoIterate, HelpEnabled);
                    m_extension.AlignToSpline = m_editorUtils.Toggle("Align to Spline", m_extension.AlignToSpline, HelpEnabled);
                    EditorGUI.indentLevel++;
                    m_extension.AlignChildrenToSpline = m_editorUtils.Toggle("Align Children", m_extension.AlignChildrenToSpline, HelpEnabled);
                    EditorGUI.indentLevel--;
                    m_extension.ConformToSlope = m_editorUtils.Toggle("Conform to Slope", m_extension.ConformToSlope, HelpEnabled);
                    EditorGUI.indentLevel++;
                    m_extension.ConformChildrenToSlope = m_editorUtils.Toggle("Conform Children", m_extension.ConformChildrenToSlope, HelpEnabled);
                    EditorGUI.indentLevel--;
                    m_extension.SnapToGround = m_editorUtils.Toggle("Snap to Ground", m_extension.SnapToGround, HelpEnabled);
                    EditorGUI.indentLevel++;
                    m_extension.SnapChildrenToGround = m_editorUtils.Toggle("Snap Children", m_extension.SnapChildrenToGround, HelpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.LabelField("Experimental", EditorStyles.boldLabel);
                    m_extension.CheckIntersectionBounds = m_editorUtils.Toggle("Check Intersection Bounds", m_extension.CheckIntersectionBounds, HelpEnabled);
                    float size = m_extension.IntersectionBoundsSize.x;
                    if (m_extension.CheckIntersectionBounds)
                    {
                        EditorGUI.indentLevel++;
                        size = m_editorUtils.FloatField("Intersection Bounds Size", size, HelpEnabled);
                        EditorGUI.indentLevel--;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        m_extension.IntersectionBoundsSize = new Vector3(size, size, size);
                        m_extension.PreExecute();
                    }

                    GUI.enabled = spawnerData != null;
                    bool isProcessing = m_extension.IsProcessing;
                    if (isProcessing)
                    {
                        GUI.enabled = true;
                        GUIContent cancelContent = new GUIContent("\u00D7 Cancel");
                        Color oldColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(cancelContent, Styles.cancelBtn, GUILayout.MaxHeight(25f)))
                        {
                            GeNaGlobalReferences.GeNaManagerInstance.Cancel = true;
                            //GUIUtility.ExitGUI();
                        }

                        GUI.backgroundColor = oldColor;
                    }

                    if (!m_isAsset)
                    {
                        GUI.enabled = !isProcessing;
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (m_editorUtils.Button("Spawn"))
                                m_extension.Spawn();
                            if (m_editorUtils.Button("Iterate"))
                            {
                                // EditorApplication.delayCall += Undo.PerformUndo;
                                m_extension.Iterate();
                            }

                            GUI.enabled = true;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (m_editorUtils.Button("Bake"))
                        {
                            if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("BakeTitleSpawner"), m_editorUtils.GetTextValue("BakeMessageSpawner"), "Yes", "No"))
                            {
                                m_extension.Bake();
                            }
                        }

                        m_editorUtils.InlineHelp("Spawn", HelpEnabled);
                        m_editorUtils.InlineHelp("Iterate", HelpEnabled);
                        GUI.enabled = true;
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_extension);
                AssetDatabase.SaveAssets();
            }
        }
    }
}