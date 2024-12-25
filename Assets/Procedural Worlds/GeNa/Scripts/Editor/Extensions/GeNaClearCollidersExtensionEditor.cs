using System.Collections.Generic;
using PWCommon5;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaClearCollidersExtension))]
    public class GeNaClearCollidersExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaClearCollidersExtension m_extension;
        private bool m_isAsset;

        private ReorderableList m_ignoredReorderable;

        protected void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            m_extension = target as GeNaClearCollidersExtension;
            if (m_extension != null)
            {
                m_isAsset = AssetDatabase.Contains(m_extension);
                CreateColliderList();
            }
        }

        /// <summary>
        /// Handle drop area for new objects
        /// </summary>
        public bool DropCollidersGUI()
        {
            // Ok - set up for drag and drop
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            string dropMsg = m_editorUtils.GetTextValue("Drop Colliders");
            GUI.Box(dropArea, dropMsg, Styles.gpanel);
            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (!dropArea.Contains(evt.mousePosition))
                    return false;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    //Handle game objects / prefabs
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        Collider collider = null;
                        switch (draggedObject)
                        {
                            case GameObject go:
                                collider = go.GetComponent<Collider>();
                                break;
                            case Collider col:
                                collider = col;
                                break;
                        }

                        if (collider != null)
                        {
                            ColliderEntry colliderEntry = new ColliderEntry();
                            colliderEntry.Collider = collider;
                            m_extension.IgnoredColliders.Add(colliderEntry);
                        }
                    }

                    return true;
                }
            }

            return false;
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
                m_extension = target as GeNaClearCollidersExtension;
            EditorGUI.BeginChangeCheck();
            {
                m_extension.Width = m_editorUtils.FloatField("Width", m_extension.Width, HelpEnabled);
                m_extension.LayerMask = m_editorUtils.LayerMaskField("Layer Mask", m_extension.LayerMask, HelpEnabled);
                DropCollidersGUI();
                DrawExtensionList(m_ignoredReorderable, m_editorUtils);
                m_editorUtils.InlineHelp("Ignored Colliders", HelpEnabled);
                if (!m_isAsset)
                {
                    if (m_editorUtils.Button("Clear Colliders Btn", HelpEnabled))
                        m_extension.Clear();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_extension);
                AssetDatabase.SaveAssets();
            }
        }

        #region Spline Extension Reorderable

        private void CreateColliderList()
        {
            if (m_extension == null)
                return;
            m_ignoredReorderable = new ReorderableList(m_extension.IgnoredColliders, typeof(ColliderEntry), true, true,
                true, true)
            {
                elementHeightCallback = OnElementHeightExtensionListEntry,
                drawElementCallback = DrawExtensionListElement,
                drawHeaderCallback = DrawExtensionListHeader,
                onAddCallback = OnAddExtensionListEntry,
                onRemoveCallback = OnRemoveExtensionListEntry,
                onReorderCallback = OnReorderExtensionList
            };
        }

        private void OnReorderExtensionList(ReorderableList reorderableList)
        {
            //Do nothing, changing the order does not immediately affect anything in the stamper
        }

        private void OnRemoveExtensionListEntry(ReorderableList reorderableList)
        {
            int indexToRemove = reorderableList.index;
            m_extension.IgnoredColliders.RemoveAt(indexToRemove);
            reorderableList.list = m_extension.IgnoredColliders;
            if (indexToRemove >= reorderableList.list.Count)
                indexToRemove = reorderableList.list.Count - 1;
            reorderableList.index = indexToRemove;
        }

        private void OnAddExtensionListEntry(ReorderableList reorderableList)
        {
            m_extension.IgnoredColliders.Add(new ColliderEntry());
            reorderableList.index = reorderableList.count - 1;
        }

        private void DrawExtensionListHeader(Rect rect)
        {
            DrawExtensionListHeader(rect, true, m_extension.IgnoredColliders, m_editorUtils);
        }

        private void DrawExtensionListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            ColliderEntry entry = m_extension.IgnoredColliders[index];
            DrawExtensionListElement(rect, entry, m_editorUtils, isFocused);
        }

        private float OnElementHeightExtensionListEntry(int index)
        {
            return OnElementHeight();
        }

        public float OnElementHeight()
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }

        public void DrawExtensionListHeader(Rect rect, bool currentFoldOutState, List<ColliderEntry> extensionList,
            EditorUtils editorUtils)
        {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(rect, editorUtils.GetContent("Ignored Colliders"));
            EditorGUI.indentLevel = oldIndent;
        }

        public void DrawExtensionList(ReorderableList list, EditorUtils editorUtils)
        {
            Rect maskRect = EditorGUILayout.GetControlRect(true, list.GetHeight());
            list.DoList(maskRect);
        }

        public void DrawExtensionListElement(Rect rect, ColliderEntry entry, EditorUtils editorUtils, bool isFocused)
        {
            // Spawner Object
            EditorGUI.BeginChangeCheck();
            {
                int oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y + 1f, rect.width * 0.18f, EditorGUIUtility.singleLineHeight),
                    editorUtils.GetContent("ColliderEntryActive"));
                entry.IsActive =
                    EditorGUI.Toggle(
                        new Rect(rect.x + rect.width * 0.18f, rect.y, rect.width * 0.1f,
                            EditorGUIUtility.singleLineHeight), entry.IsActive);
                bool oldEnabled = GUI.enabled;
                GUI.enabled = entry.IsActive;
                entry.Collider = (Collider)EditorGUI.ObjectField(
                    new Rect(rect.x + rect.width * 0.4f, rect.y + 1f, rect.width * 0.6f,
                        EditorGUIUtility.singleLineHeight), entry.Collider, typeof(Collider), false);
                GUI.enabled = oldEnabled;
                EditorGUI.indentLevel = oldIndent;
            }
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        #endregion
    }
}