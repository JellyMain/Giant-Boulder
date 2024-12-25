// Engine

using System.Collections.Generic;
using System.IO;
using System.Linq;
using PWCommon5;
using UnityEngine;

// Editor
using UnityEditor;
using UnityEditorInternal;

// Procedural Worlds
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaManager))]
    public class GeNaManagerSceneEditor : GeNaEditor
    {
        #region Variables

        private GeNaManager m_manager;
        private PhysicsSimulator m_physicsSimulator;
        private ReorderableList m_objectsToIgnoreList;

        #endregion

        #region Properties

        private List<Rigidbody> objectsToIgnore => m_physicsSimulator.m_objectsToIgnore;

        #endregion

        #region Methods

        #region Unity

        protected void OnEnable()
        {
            #region Initialization

            // If there isn't any Editor Utils Initialized
            if (m_editorUtils == null)
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this, null, null, null);
            // If there is no target associated with Editor Script
            if (target == null)
                // Exit the method
                return;
            // Get target Spline
            m_manager = (GeNaManager)target;
            m_manager.Initialize();
            m_physicsSimulator = m_manager.PhysicsSimulator;

            CreateExtensionList();

            #endregion
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            m_editorUtils.GUINewsHeader();

            #region Panel

            m_editorUtils.Panel("GeneralPanel", ManagerPanel, true);
            m_editorUtils.Panel("SpawnCallPanel", SpawnCallPanel, true);
            m_editorUtils.Panel("PhysicsSimulatorPanel", PhysicsSimulatorPanel, true);

            #endregion

            m_editorUtils.GUINewsFooter(false);
        }

        private static int SPLINE_QUAD_SIZE = 25;
        private static int SPLINE_STYLE_QUAD_SIZE = 15;

        public static bool Button(Vector2 position, Texture2D texture2D, Color color)
        {
            Vector2 quadSize = new Vector2(SPLINE_QUAD_SIZE, SPLINE_QUAD_SIZE);
            Vector2 halfQuadSize = quadSize * .5f;
            Rect buttonRect = new Rect(position - halfQuadSize, quadSize);
            Color oldColor = GUI.color;
            GUI.color = color;
            bool result = GUI.Button(buttonRect, texture2D, GUIStyle.none);
            GUI.color = oldColor;
            return result;
        }

        public static bool Button(Vector2 position, GUIStyle style, Color color)
        {
            Vector2 quadSize = new Vector2(SPLINE_STYLE_QUAD_SIZE, SPLINE_STYLE_QUAD_SIZE);
            Vector2 halfQuadSize = quadSize * .5f;
            Rect buttonRect = new Rect(position - halfQuadSize, quadSize);
            Color oldColor = GUI.color;
            GUI.color = color;
            bool result = GUI.Button(buttonRect, GUIContent.none, style);
            GUI.color = oldColor;
            return result;
        }

        private SpawnCall m_selectedSpawnCall = null;

        public override void OnSceneGUI()
        {
            Initialize();
            if (m_selectedSpawnCall != null)
            {
                switch (Tools.current)
                {
                    case Tool.Rotate:
                    {
                        Vector3 point = m_selectedSpawnCall.Location;
                        var rotation = Quaternion.Euler(m_selectedSpawnCall.Rotation);
                        var result = Handles.RotationHandle(rotation, point);
                        // place a handle on the node and manage m_position change
                        if (result != rotation)
                        {
                            m_selectedSpawnCall.Rotation = result.eulerAngles;
                            m_selectedSpawnCall.UpdateEntities();
                        }

                        break;
                    }
                    default:
                    {
                        var rotation = Quaternion.Euler(m_selectedSpawnCall.Rotation);
                        Vector3 point = m_selectedSpawnCall.Location;
                        Vector3 result = Handles.PositionHandle(point, rotation);
                        // place a handle on the node and manage m_position change
                        if (result != point)
                        {
                            m_selectedSpawnCall.Location = result;
                            m_selectedSpawnCall.UpdateEntities();
                        }
                    }
                        break;
                }
            }

            Handles.BeginGUI();
            var spawnCalls = m_manager.ActiveSpawnCalls;
            foreach (var spawnCall in spawnCalls)
            {
                if (spawnCall == null)
                    continue;
                Vector2 guiPos = HandleUtility.WorldToGUIPoint(spawnCall.Location);
                if (Button(guiPos, Styles.knobTexture2D, Color.green))
                {
                    m_selectedSpawnCall = spawnCall;
                    break;
                }
            }

            Handles.EndGUI();
        }

        #endregion

        #region Panel

        private void ManagerPanel(bool helpEnabled)
        {
            // m_editorUtils.Text("WelcomeToGeNaManager");
            if (m_editorUtils.Button("ShowGeNaManager"))
            {
                ShowGeNaManager();
            }
        }

        private void SpawnCallPanel(bool helpEnabled)
        {
            if (m_editorUtils.Button("ClearEmptySpawnCalls"))
            {
                m_manager.ClearEmptySpawnCalls();
            }

            if (m_selectedSpawnCall == null)
                return;
            EditorGUILayout.LabelField(m_selectedSpawnCall.Location.ToString());
            EditorGUI.BeginChangeCheck();
            {
                m_selectedSpawnCall.AlignChildrenToRotation = EditorGUILayout.Toggle("Align Children",
                    m_selectedSpawnCall.AlignChildrenToRotation);
                m_selectedSpawnCall.ConformChildrenToSlope = EditorGUILayout.Toggle("Conform Children",
                    m_selectedSpawnCall.ConformChildrenToSlope);
                m_selectedSpawnCall.SnapChildrenToGround =
                    EditorGUILayout.Toggle("Snap Children", m_selectedSpawnCall.SnapChildrenToGround);
            }
            if (EditorGUI.EndChangeCheck())
            {
                m_selectedSpawnCall.UpdateEntities();
            }
        }

        private void PhysicsSimulatorPanel(bool helpEnabled)
        {
            if (DrawRigidGUI(out var rigidbodies))
            {
                objectsToIgnore.AddRange(rigidbodies);
            }
            
            GUILayout.Space(5f);

            m_objectsToIgnoreList.DoLayoutList();
        }

        #endregion

        #region Objects To Ignore List

        private void CreateExtensionList()
        {
            m_objectsToIgnoreList = new ReorderableList(objectsToIgnore,
                typeof(GeNaSplineExtension), true, true, true, true)
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
            objectsToIgnore.RemoveAt(indexToRemove);
            if (indexToRemove >= objectsToIgnore.Count)
                indexToRemove = objectsToIgnore.Count - 1;
            reorderableList.index = indexToRemove;
        }

        private void OnAddExtensionListEntry(ReorderableList reorderableList)
        {
            objectsToIgnore.Add(null);
            reorderableList.index = objectsToIgnore.Count - 1;
        }

        private void DrawExtensionListHeader(Rect rect)
        {
            int oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(rect, "Rigidbodies");
            EditorGUI.indentLevel = oldIndent;
        }

        private void DrawExtensionListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            objectsToIgnore[index] = (Rigidbody)EditorGUI.ObjectField(
                new Rect(rect.x, rect.y + 1f, rect.width, EditorGUIUtility.singleLineHeight),
                objectsToIgnore[index], typeof(Rigidbody), true);
        }

        private float OnElementHeightExtensionListEntry(int index)
        {
            return OnElementHeight();
        }

        public float OnElementHeight()
        {
            return EditorGUIUtility.singleLineHeight + 4f;
        }

        #endregion

        #region Utilities

        private void ShowGeNaManager()
        {
            GeNaManagerEditor.MenuGeNaMainWindow();
        }

        /// <summary>
        /// Handle drop area for new objects
        /// </summary>
        public bool DrawRigidGUI(out List<Rigidbody> result)
        {
            result = new List<Rigidbody>();
            // Ok - set up for drag and drop
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            string dropMsg = "Drop Rigidbodies Here..."; //m_editorUtils.GetTextValue("Add proto drop box msg");
            GUI.Box(dropArea, dropMsg, Styles.gpanel);
            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (!dropArea.Contains(evt.mousePosition))
                    return false;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    Dictionary<int, Rigidbody> rigidCollection = new Dictionary<int, Rigidbody>();
                    // Handle game objects / prefabs
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            var rigidbodies = go.GetComponentsInChildren<Rigidbody>();
                            foreach (var rigid in rigidbodies)
                            {
                                var instanceID = rigid.GetInstanceID();
                                if (rigidCollection.ContainsKey(instanceID))
                                    continue;
                                rigidCollection.Add(instanceID, rigid);
                            }
                        }
                    }

                    if (rigidCollection.Count > 0)
                    {
                        result = rigidCollection.Values.ToList();
                    }

                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion
    }
}