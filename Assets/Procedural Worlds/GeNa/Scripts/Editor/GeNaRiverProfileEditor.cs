using UnityEngine;
using UnityEditor;
using PWCommon5;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaRiverProfile))]
    public class GeNaRiverProfileEditor : PWEditor
    {
        public GeNaRiverProfile m_profile;
        private bool isAsset = false;
        protected virtual void OnEnable()
        {
            GeNaRiverProfile profile = target as GeNaRiverProfile;
            isAsset = AssetDatabase.Contains(profile);
        }
        public void PerformUpgrade()
        {
            if (isAsset)
                GeNaEditorUpgrader.PerformUpgradeOperation();
            else
                GeNaEditorUpgrader.PerformSceneUpgrade();
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Warning! GeNa has detected that a maintenence is required before use. \nThis process cannot be undone.", MessageType.Warning);
            if (GUILayout.Button("Perform Maintenance"))
            {
                Selection.objects = new Object[0];
                Repaint();
                PerformUpgrade();
            }
        }
        /// <summary>
        /// Sets the profile when using extensions
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="editor"></param>
        public static void SetProfile(GeNaRiverProfile profile, GeNaRiverProfileEditor editor)
        {
            editor.m_profile = profile;
        }
    }
}