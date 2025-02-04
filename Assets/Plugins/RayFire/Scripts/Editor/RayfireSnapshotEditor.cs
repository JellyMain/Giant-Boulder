using UnityEngine;
using UnityEditor;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireSnapshot))]
    public class RayfireSnapshotEditor : Editor
    {
        RayfireSnapshot snap;

        // Minimum & Maximum ranges
        const float size_min = 0f; 
        const float size_max = 1f; 
        
        // Serialized properties
        SerializedProperty sp_saveName;
        SerializedProperty sp_saveComp;
        SerializedProperty sp_loadSnap;
        SerializedProperty sp_loadSize;

        private void OnEnable()
        {
            // Get component
            snap = (RayfireSnapshot)target;
            
            // Find properties
            sp_saveName = serializedObject.FindProperty(nameof(snap.assetName));
            sp_saveComp = serializedObject.FindProperty(nameof(snap.compress));
            sp_loadSnap = serializedObject.FindProperty(nameof(snap.snapshotAsset));
            sp_loadSize = serializedObject.FindProperty(nameof(snap.sizeFilter));
        }

        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            GUI_Save();
            GUI_Load();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Save
        /// /////////////////////////////////////////////////////////

        void GUI_Save()
        {
            GUICommon.Caption (TextSnp.gui_cap_save);
            if (snap.transform.childCount > 0)
            {
                if (GUILayout.Button (TextSnp.gui_btn_snap, GUILayout.Height (25)))
                    snap.Snapshot();
                GUICommon.Space();
            }
            GUICommon.PropertyField (sp_saveName, TextSnp.gui_saveName);
            GUICommon.PropertyField (sp_saveComp, TextSnp.gui_saveComp);
        }

        /// /////////////////////////////////////////////////////////
        /// Load
        /// /////////////////////////////////////////////////////////

        void GUI_Load()
        {
            GUICommon.Caption (TextSnp.gui_cap_load);
            GUICommon.PropertyField (sp_loadSnap, TextSnp.gui_loadSnap);
            GUICommon.Slider (sp_loadSize, size_min, size_max, TextSnp.gui_loadSize);
            if (GUILayout.Button (TextSnp.gui_btn_load, GUILayout.Height (25)))
                snap.Load();
        }
    }
}