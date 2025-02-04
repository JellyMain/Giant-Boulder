using UnityEditor;
using UnityEngine;
namespace GeNa.Core.Archived
{
    [CustomEditor(typeof(DataBufferScriptable))]
    public class DataBufferScriptableEditor : Editor
    {
        private bool isAsset = false;
        private void OnEnable()
        {
            DataBufferScriptable dataBufferScriptable = target as DataBufferScriptable;
            isAsset = AssetDatabase.Contains(dataBufferScriptable);
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
    }
}