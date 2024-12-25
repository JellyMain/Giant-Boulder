using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaTools))]
    public class GeNaToolsEditor : GeNaEditor
    {
        private GeNaTools m_tools;

        private void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaToolsEditor", null);
            m_tools = target as GeNaTools;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_editorUtils.GUIHeader();
            m_editorUtils.GUINewsHeader();
            EditorGUILayout.LabelField("Selected Object Count: " + Selection.gameObjects.Length);
            EditorGUI.BeginChangeCheck();
            m_editorUtils.Panel("GlobalSettings", GlobalSettingsPanel);
            m_editorUtils.Panel("KeyBindings", KeyBindingsPanel);
            // m_editorUtils.Panel("CombineMeshes", CombineMeshesPanel);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_tools);
            }
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();
            if (m_tools == null)
                return;
            Event e = Event.current;
            if (e.type != EventType.KeyDown)
                return;
            GeNaToolsSettings settings = m_tools.Settings;
            GeNaToolsKeyBindings keyBindings = m_tools.KeyBindings;
            // GeNaCombineMeshes combineMeshes = m_tools.CombineMeshes;
            if (!settings.AlwaysKeepSnappedToGround)
            {
                bool process = false;
                bool raiseOrLower = false;
                if (e != null)
                {
                    if (e.control)
                    {
                        if (e.keyCode == keyBindings.SnapToGroundKey)
                        {
                            settings.SnapToGround = true;
                            settings.AlignToGround = false;
                            settings.MoveUp = false;
                            settings.MoveDown = false;
                            process = true;
                        }
                        else if (e.keyCode == keyBindings.AlignToGroundKey)
                        {
                            settings.SnapToGround = false;
                            settings.AlignToGround = true;
                            settings.MoveUp = false;
                            settings.MoveDown = false;
                            process = true;
                        }
                        else if (e.keyCode == keyBindings.AlignAndSnapToGroundKey)
                        {
                            settings.SnapToGround = true;
                            settings.AlignToGround = true;
                            settings.MoveUp = false;
                            settings.MoveDown = false;
                            process = true;
                        }
                        else if (e.keyCode == keyBindings.RaiseFromGroundKey || e.keyCode == KeyCode.KeypadPlus)
                        {
                            settings.MoveUp = true;
                            settings.MoveDown = false;
                            raiseOrLower = true;
                        }
                        else if (e.keyCode == keyBindings.LowerInGroundKey || e.keyCode == KeyCode.KeypadMinus)
                        {
                            settings.MoveDown = true;
                            settings.MoveUp = false;
                            raiseOrLower = true;
                        }

                        // else if (e.keyCode == keyBindings.CombineMeshesKey && combineMeshes.Enabled)
                        // {
                        //     Undo.SetCurrentGroupName("Combine Meshes");
                        //     int group = Undo.GetCurrentGroup();
                        //     foreach (GameObject gameObject in Selection.gameObjects)
                        //         Undo.RegisterFullObjectHierarchyUndo(gameObject, "transform selected objects");
                        //     m_tools.ProcessCombineMeshes(Selection.gameObjects);
                        //     Undo.CollapseUndoOperations(group);
                        // }
                        if (process)
                        {
                            Undo.SetCurrentGroupName("Processed Selection");
                            int group = Undo.GetCurrentGroup();
                            foreach (GameObject gameObject in Selection.gameObjects)
                                Undo.RegisterFullObjectHierarchyUndo(gameObject, "transform selected objects");
                            m_tools.ProcessSelectedObjects(Selection.gameObjects);
                            Undo.CollapseUndoOperations(group);
                        }
                        else if (raiseOrLower)
                        {
                            Undo.SetCurrentGroupName("Raised or Lowered");
                            int group = Undo.GetCurrentGroup();
                            foreach (GameObject gameObject in Selection.gameObjects)
                                Undo.RegisterFullObjectHierarchyUndo(gameObject, "Raised or Lowered");
                            m_tools.RaiseOrLower(Selection.gameObjects);
                            Undo.CollapseUndoOperations(group);
                        }
                    }
                }
            }
            else
            {
                settings.SnapToGround = true;
                settings.AlignToGround = true;
                m_tools.ProcessSelectedObjects(Selection.gameObjects);
            }
        }

        #region Editor Panels

        private void GlobalSettingsPanel(bool helpEnabled)
        {
            GeNaToolsSettings settings = m_tools.Settings;
            settings.SnapMode =
                (GeNaToolsSettings.GeNaSnapMode)m_editorUtils.EnumPopup("SnapMode", settings.SnapMode, helpEnabled);
            settings.OffsetCheck = m_editorUtils.FloatField("OffsetCheck", settings.OffsetCheck, helpEnabled);
            settings.DistanceCheck = m_editorUtils.FloatField("DistanceCheck", settings.DistanceCheck, helpEnabled);
            settings.RaiseAndLowerAmount =
                m_editorUtils.FloatField("RaiseAndLowerAmount", settings.RaiseAndLowerAmount, helpEnabled);
            m_editorUtils.LabelField("Experimental");
            EditorGUI.indentLevel++;
            settings.AlwaysKeepSnappedToGround =
                m_editorUtils.Toggle("AlwaysSnapAlign", settings.AlwaysKeepSnappedToGround);
            EditorGUI.indentLevel--;
        }

        private void KeyBindingsPanel(bool helpEnabled)
        {
            GeNaToolsKeyBindings keyBindings = m_tools.KeyBindings;
            GUI.enabled = false;
            GeNaToolsUtility.m_firstKey = (KeyCode)m_editorUtils.EnumPopup("HoldDownKey", GeNaToolsUtility.m_firstKey);
            GUI.enabled = true;
            keyBindings.SnapToGroundKey = (KeyCode)m_editorUtils.EnumPopup("SnapToGroundKey", keyBindings.SnapToGroundKey);
            keyBindings.AlignToGroundKey = (KeyCode)m_editorUtils.EnumPopup("AlignToSlopeKey", keyBindings.AlignToGroundKey);
            keyBindings.AlignAndSnapToGroundKey = (KeyCode)m_editorUtils.EnumPopup("SnapAndAlignToGroundKey", keyBindings.AlignAndSnapToGroundKey);
            keyBindings.RaiseFromGroundKey = (KeyCode)m_editorUtils.EnumPopup("RaiseFromGroundKey", keyBindings.RaiseFromGroundKey);
            keyBindings.LowerInGroundKey = (KeyCode)m_editorUtils.EnumPopup("LowerInGroundKey", keyBindings.LowerInGroundKey);
        }

        private void PhysicsSimulatorPanel(bool helpEnabled)
        {
            
        }

        #endregion
    }
}