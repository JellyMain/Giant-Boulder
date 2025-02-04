using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireBlade))]
    public class RayfireBladeEditor : Editor
    {
        RayfireBlade    blade;
        ReorderableList rl_targets_list; 
        
        // Minimum & Maximum ranges
        const float damage_min   = 0.01f;
        const float damage_max   = 50f;
        const float force_min    = 0;
        const float force_max    = 10f;
        const float cooldown_min = 0;
        const float cooldown_max = 10f;
        
        // Serialized properties
        SerializedProperty sp_actionType;
        SerializedProperty sp_onTrigger;
        SerializedProperty sp_sliceType;
        SerializedProperty sp_damage;
        SerializedProperty sp_skin;
        SerializedProperty sp_force;
        SerializedProperty sp_affectInactive;
        SerializedProperty sp_cooldown;
        SerializedProperty sp_targets;
        SerializedProperty sp_tag;
        SerializedProperty sp_mask;
        

        private void OnEnable()
        {
            // Get component
            blade = (RayfireBlade)target;
            
            // Set tag list
            GUICommon.SetTags();
            
            // Find properties
            sp_actionType     = serializedObject.FindProperty(nameof(blade.actionType));
            sp_onTrigger      = serializedObject.FindProperty(nameof(blade.onTrigger));
            sp_sliceType      = serializedObject.FindProperty(nameof(blade.sliceType));
            sp_damage         = serializedObject.FindProperty(nameof(blade.damage));
            sp_skin           = serializedObject.FindProperty(nameof(blade.skin));
            sp_force          = serializedObject.FindProperty(nameof(blade.force));
            sp_affectInactive = serializedObject.FindProperty(nameof(blade.affectInactive));
            sp_cooldown       = serializedObject.FindProperty(nameof(blade.cooldown));
            sp_targets        = serializedObject.FindProperty(nameof(blade.targets));
            sp_tag            = serializedObject.FindProperty(nameof(blade.tagFilter));
            sp_mask           = serializedObject.FindProperty(nameof(blade.mask));
            
            // Reorderable list
            rl_targets_list = new ReorderableList (serializedObject, sp_targets, true, true, true, true)
            {
                drawElementCallback = DrawInitListItems,
                drawHeaderCallback = DrawInitHeader,
                onAddCallback = AddInit,
                onRemoveCallback = RemoveInit
            };
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            // Cooldown
            if (Application.isPlaying == true && blade.coolDownState == true)
                GUILayout.Label (TextBld.gui_cooldown.text);
            
            // GUI
            GUI_Properties();
            GUI_Force();
            GUI_Filters();
            GUI_Targets();

            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////

        void GUI_Properties()
        {
            GUICommon.CaptionBox (TextBld.gui_cap_prop);
            EditorGUILayout.PropertyField (sp_actionType, TextBld.gui_actionType);
            GUICommon.Space ();
            EditorGUILayout.PropertyField (sp_onTrigger, TextBld.gui_onTrigger);
            GUICommon.Space ();
            EditorGUILayout.PropertyField (sp_sliceType, TextBld.gui_sliceType);
            GUICommon.Space ();
            EditorGUILayout.Slider (sp_damage, damage_min, damage_max, TextBld.gui_damage);
            GUICommon.Space ();
            EditorGUILayout.PropertyField (sp_skin, TextBld.gui_skin);
            GUICommon.Space ();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Force
        /// /////////////////////////////////////////////////////////

        void GUI_Force()
        {
            GUICommon.CaptionBox (TextBld.gui_cap_force);
            EditorGUILayout.Slider (sp_force, force_min, force_max, TextBld.gui_force);
            GUICommon.Space ();
            EditorGUILayout.PropertyField (sp_affectInactive, TextBld.gui_affectInactive);
            GUICommon.Space ();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void GUI_Filters()
        {
            GUICommon.CaptionBox (TextBld.gui_cap_filter);
            GUICommon.Slider (sp_cooldown, cooldown_min, cooldown_max, TextBld.gui_cooldown);
            GUICommon.TagField (sp_tag, TextBld.gui_tagFilter);
            GUICommon.MaskField (sp_mask, TextBld.gui_mask);
        }

        /// /////////////////////////////////////////////////////////
        /// Targets
        /// /////////////////////////////////////////////////////////

        void GUI_Targets()
        {
            GUICommon.CaptionBox (TextBld.gui_cap_targets);

            rl_targets_list.DoLayoutList();
            if (Application.isPlaying == true && blade.HasTargets == true)
            {
                if (GUILayout.Button (TextBld.gui_btn_slice, GUILayout.Height (25)))
                    foreach (var bl in targets)
                        if (bl as RayfireBlade != null)
                            (bl as RayfireBlade).SliceTarget();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawInitListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_targets_list.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawInitHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, TextBld.gui_targ);
        }

        void AddInit(ReorderableList list)
        {
            if (blade.targets == null)
                blade.targets = new List<GameObject>();
            blade.targets.Add (null);
            list.index = list.count;
        }
        
        void RemoveInit(ReorderableList list)
        {
            if (blade.HasTargets == true)
            {
                blade.targets.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
    }
}