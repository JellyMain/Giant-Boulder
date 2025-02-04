using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireCombine))]
    public class RayfireCombineEditor : Editor
    {
        RayfireCombine  combine;
        ReorderableList rl_objects; 
        
        // Minimum & Maximum ranges
        const float sizeThreshold_min   = 0;
        const float sizeThreshold_max   = 10f;
        const int   vertexThreshold_min = 0;
        const int   vertexThreshold_max = 100;
        
        // Serialized properties
        SerializedProperty sp_type;
        SerializedProperty sp_source;
        SerializedProperty sp_objects;
        SerializedProperty sp_meshFilters;
        SerializedProperty sp_skinnedMeshes;
        SerializedProperty sp_particleSystems;
        SerializedProperty sp_sizeThreshold;
        SerializedProperty sp_vertexThreshold;
        SerializedProperty sp_indexFormat;
        
        private void OnEnable()
        {
            // Get component
            combine = (RayfireCombine)target;
            
            // Find properties
            sp_type            = serializedObject.FindProperty(nameof(combine.type));
            sp_source          = serializedObject.FindProperty(nameof(combine.source));
            sp_objects         = serializedObject.FindProperty(nameof(combine.objects));
            sp_meshFilters     = serializedObject.FindProperty(nameof(combine.meshFilters));
            sp_skinnedMeshes   = serializedObject.FindProperty(nameof(combine.skinnedMeshes));
            sp_particleSystems = serializedObject.FindProperty(nameof(combine.particleSystems));
            sp_sizeThreshold   = serializedObject.FindProperty(nameof(combine.sizeThreshold));
            sp_vertexThreshold = serializedObject.FindProperty(nameof(combine.vertexThreshold));
            sp_indexFormat     = serializedObject.FindProperty(nameof(combine.indexFormat));
            
            // Reorderable list
            rl_objects = new ReorderableList(serializedObject, sp_objects, true, true, true, true)
            {
                drawElementCallback = DrawInitListItems,
                drawHeaderCallback  = DrawInitHeader,
                onAddCallback       = AddInit,
                onRemoveCallback    = RemoveInit
            };
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();

            // Button
            if (GUILayout.Button (TextCmb.gui_btn_combine, GUILayout.Height (25)))
                combine.Combine();
            GUICommon.Space ();
            
            UI_Source();
            UI_Mesh();
            UI_Filters();
            UI_Comb();
            UI_Export();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Source
        /// /////////////////////////////////////////////////////////
        
        void UI_Source()
        {
            GUICommon.CaptionBox (TextCmb.gui_cap_source);
            GUICommon.PropertyField (sp_type, TextCmb.gui_type);
            if (combine.type == RayfireCombine.CombType.ObjectsList)
            {
                rl_objects.DoLayoutList();
                GUICommon.Space ();
            }
        }

        void UI_Mesh()
        {
            GUICommon.Caption (TextCmb.gui_cap_mesh);
            GUICommon.PropertyField (sp_meshFilters,     TextCmb.gui_meshFilters);
            GUICommon.PropertyField (sp_skinnedMeshes,   TextCmb.gui_skinnedMeshes);
            GUICommon.PropertyField (sp_particleSystems, TextCmb.gui_particleSystems);
        }

        void UI_Filters()
        {
            GUICommon.CaptionBox (TextCmb.gui_cap_filters);
            GUICommon.Slider (sp_sizeThreshold, sizeThreshold_min, sizeThreshold_max, TextCmb.gui_sizeThreshold);
            GUICommon.IntSlider (sp_vertexThreshold, vertexThreshold_min, vertexThreshold_max, TextCmb.gui_vertexThreshold);
        }

        void UI_Comb()
        {
            GUICommon.CaptionBox (TextCmb.gui_cap_index);
            GUICommon.PropertyField (sp_indexFormat, TextCmb.gui_indexFormat);
        }

        void UI_Export()
        {
            GUICommon.CaptionBox (TextCmb.gui_cap_export);

            if (GUILayout.Button (TextCmb.gui_btn_export, GUILayout.Height (25)))
            {
                MeshFilter mf = combine.GetComponent<MeshFilter>();
                RFMeshAsset.SaveMesh (mf, combine.name);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawInitListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_objects.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawInitHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, TextCmb.gui_objects);
        }

        void AddInit(ReorderableList list)
        {
            if (combine.objects == null)
                combine.objects = new List<GameObject>();
            combine.objects.Add (null);
            list.index = list.count;
        }
        
        void RemoveInit(ReorderableList list)
        {
            if (combine.objects != null)
            {
                combine.objects.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
    }
}