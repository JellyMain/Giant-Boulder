using GeNa.Core;
using PWCommon5;
using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.Utility
{
    public class GeNaToolsEditorWindow : EditorWindow
    {
        #region Variables
        protected bool m_inited = false;
        protected GeNaTools m_tools;
        protected GeNaToolsEditor m_editor;
        #endregion
        #region Properties
        public GeNaTools Tools
        {
            get
            {
                if (m_tools == null)
                    m_tools = Resources.Load<GeNaTools>("Editor/GeNa Tools");
                return m_tools;
            }
        }
        public GeNaToolsEditor ToolsEditor
        {
            get
            {
                if (m_editor == null)
                    m_editor = Editor.CreateEditor(Tools) as GeNaToolsEditor;
                return m_editor;
            }
        }
        #endregion
        [MenuItem("Window/Procedural Worlds/GeNa/GeNa Tools...", priority = 41)]
        public static void Open()
        {
            GeNaToolsEditorWindow win = GetWindow<GeNaToolsEditorWindow>();
            win.titleContent = new GUIContent("GeNa Tools");
            win.minSize = new Vector2(300f, 300f);
            win.Show();
        }
        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        private void OnFocus()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }
        private void OnGUI()
        {
            ToolsEditor.OnInspectorGUI();
        }
        public void OnSceneGUI(SceneView sceneView)
        {
            ToolsEditor.OnSceneGUI();
        }
    }
}