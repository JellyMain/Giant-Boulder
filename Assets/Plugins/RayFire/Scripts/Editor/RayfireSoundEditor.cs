using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireSound))]
    public class RayfireSoundEditor : Editor
    {
        RayfireSound       sound;
        
        // Minimum & Maximum ranges
        const float baseVolume_min = 0.01f;
        const float baseVolume_max = 1f;
        const float sizeVolume_min = 0f;
        const float sizeVolume_max = 1f;
        const float multiplier_min = 0.01f;
        const float multiplier_max = 10f;
        const int   priority_min   = 0;
        const int   priority_max   = 256;
        const float spatial_min    = 0;
        const float spatial_max    = 1f;
        const float size_min       = 0f;
        const float size_max       = 1f;
        const float distance_min   = 0f;
        const float distance_max   = 999f;
        
        // Reorderable lists
        ReorderableList rl_ini_сlips; 
        ReorderableList rl_act_сlips; 
        ReorderableList rl_dml_сlips;
        ReorderableList rl_col_сlips;
        
        // Serialized properties
        SerializedProperty sp_vol_base;
        SerializedProperty sp_vol_size;
        SerializedProperty sp_eve_ini;
        SerializedProperty sp_eve_act;
        SerializedProperty sp_eve_dml;
        SerializedProperty sp_eve_col;
        SerializedProperty sp_ini_once;
        SerializedProperty sp_ini_mlt;
        SerializedProperty sp_ini_clip;
        SerializedProperty sp_ini_сlips;
        SerializedProperty sp_ini_group;
        SerializedProperty sp_ini_prior;
        SerializedProperty sp_ini_spat;
        SerializedProperty sp_ini_mind;
        SerializedProperty sp_ini_maxd;
        SerializedProperty sp_act_once;
        SerializedProperty sp_act_mlt;
        SerializedProperty sp_act_clip;
        SerializedProperty sp_act_сlips;
        SerializedProperty sp_act_group;
        SerializedProperty sp_act_prior;
        SerializedProperty sp_act_spat;
        SerializedProperty sp_act_mind;
        SerializedProperty sp_act_maxd;
        SerializedProperty sp_dml_once;
        SerializedProperty sp_dml_mlt;
        SerializedProperty sp_dml_clip;
        SerializedProperty sp_dml_сlips;
        SerializedProperty sp_dml_group;
        SerializedProperty sp_dml_prior;
        SerializedProperty sp_dml_spat;
        SerializedProperty sp_dml_mind;
        SerializedProperty sp_dml_maxd;
        SerializedProperty sp_col_once;
        SerializedProperty sp_col_mlt;
        SerializedProperty sp_col_vel;
        SerializedProperty sp_col_clip;
        SerializedProperty sp_col_сlips;
        SerializedProperty sp_col_group;
        SerializedProperty sp_col_prior;
        SerializedProperty sp_col_spat;
        SerializedProperty sp_col_mind;
        SerializedProperty sp_col_maxd;
        SerializedProperty sp_flt_size;
        SerializedProperty sp_flt_dist;
        
        private void OnEnable()
        {
            // Get component
            sound = (RayfireSound)target;
            
            // Find properties
            sp_vol_base  = serializedObject.FindProperty(nameof(sound.baseVolume));
            sp_vol_size  = serializedObject.FindProperty(nameof(sound.sizeVolume));
            sp_eve_ini   = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.enable));
            sp_ini_once  = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.once));
            sp_ini_mlt   = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.multiplier));
            sp_ini_clip  = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.clip));
            sp_ini_сlips = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.clips));
            sp_ini_group = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.outputGroup));
            sp_ini_prior = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.priority));
            sp_ini_spat  = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.spatial));
            sp_ini_mind  = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.minDist));
            sp_ini_maxd  = serializedObject.FindProperty(nameof(sound.initialization) + "." + nameof(sound.initialization.maxDist));
            sp_eve_act   = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.enable));
            sp_act_once  = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.once));
            sp_act_mlt   = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.multiplier));
            sp_act_clip  = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.clip));
            sp_act_сlips = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.clips));
            sp_act_group = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.outputGroup));
            sp_act_prior = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.priority));
            sp_act_spat  = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.spatial));
            sp_act_mind  = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.minDist));
            sp_act_maxd  = serializedObject.FindProperty(nameof(sound.activation) + "." + nameof(sound.activation.maxDist));
            sp_eve_dml   = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.enable));
            sp_dml_once  = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.once));
            sp_dml_mlt   = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.multiplier));
            sp_dml_clip  = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.clip));
            sp_dml_сlips = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.clips));
            sp_dml_group = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.outputGroup));
            sp_dml_prior = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.priority));
            sp_dml_spat  = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.spatial));
            sp_dml_mind  = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.minDist));
            sp_dml_maxd  = serializedObject.FindProperty(nameof(sound.demolition) + "." + nameof(sound.demolition.maxDist));
            sp_eve_col   = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.enable));
            sp_col_once  = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.once));
            sp_col_mlt   = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.multiplier));
            sp_col_vel   = serializedObject.FindProperty(nameof(sound.relativeVelocity));
            sp_col_clip  = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.clip));
            sp_col_сlips = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.clips));
            sp_col_group = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.outputGroup));
            sp_col_prior = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.priority));
            sp_col_spat  = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.spatial));
            sp_col_mind  = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.minDist));
            sp_col_maxd  = serializedObject.FindProperty(nameof(sound.collision) + "." + nameof(sound.collision.maxDist));
            sp_flt_size  = serializedObject.FindProperty(nameof(sound.minimumSize));
            sp_flt_dist  = serializedObject.FindProperty(nameof(sound.cameraDistance));
            
            // Reorderable lists
            rl_ini_сlips  = new ReorderableList(serializedObject, sp_ini_сlips, true, true, true, true)
            {
                drawElementCallback = DrawInitListItems,
                drawHeaderCallback  = DrawInitHeader,
                onAddCallback       = AddInit,
                onRemoveCallback    = RemoveInit
            };
            rl_act_сlips = new ReorderableList(serializedObject, sp_act_сlips, true, true, true, true)
            {
                drawElementCallback = DrawActListItems,
                drawHeaderCallback  = DrawActHeader,
                onAddCallback       = AddAct,
                onRemoveCallback    = RemoveAct
            };
            rl_dml_сlips = new ReorderableList(serializedObject, sp_dml_сlips, true, true, true, true)
            {
                drawElementCallback = DrawDmlListItems,
                drawHeaderCallback  = DrawDmlHeader,
                onAddCallback       = AddDml,
                onRemoveCallback    = RemoveDml
            };
            rl_col_сlips = new ReorderableList(serializedObject, sp_col_сlips, true, true, true, true)
            {
                drawElementCallback = DrawColListItems,
                drawHeaderCallback  = DrawColHeader,
                onAddCallback       = AddCol,
                onRemoveCallback    = RemoveCol
            };
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();

            GUI_Vol();
            GUI_Events();
            GUI_Filters();
            GUI_Info();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Volume
        /// /////////////////////////////////////////////////////////

        void GUI_Vol()
        {
            GUICommon.Caption (TextSnd.gui_cap_vol);
            GUICommon.Slider (sp_vol_base, baseVolume_min, baseVolume_max, TextSnd.gui_vol_base);
            GUICommon.Slider (sp_vol_size, sizeVolume_min, sizeVolume_max, TextSnd.gui_vol_size);
        }

        /// /////////////////////////////////////////////////////////
        /// Events
        /// /////////////////////////////////////////////////////////
        
        void GUI_Events()
        {
            GUICommon.Caption (TextSnd.gui_cap_eve);
            GUICommon.PropertyField (sp_eve_ini, TextSnd.gui_eve_ini);
            if (sound.initialization.enable == true)
                GUI_PropsInit();
            GUICommon.PropertyField (sp_eve_act, TextSnd.gui_eve_act);
            if (sound.activation.enable == true)
                GUI_PropsAct();
            GUICommon.PropertyField (sp_eve_dml, TextSnd.gui_eve_dml);
            if (sound.demolition.enable == true)
                GUI_PropsDml();
            GUICommon.PropertyField (sp_eve_col, TextSnd.gui_eve_col);
            if (sound.collision.enable == true)
                GUI_PropsCol();
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void GUI_PropsInit()
        {
            if (Application.isPlaying == true)
            {
                GUICommon.Space ();
                if (GUILayout.Button (TextSnd.gui_btn_ini, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.InitializationSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).initialization.played = false;
                        }
                GUICommon.Space ();
            }
            
            EditorGUI.indentLevel++;
            
            GUICommon.PropertyField (sp_ini_once, TextSnd.gui_snd_once);
            GUICommon.Slider (sp_ini_mlt, multiplier_min, multiplier_max, TextSnd.gui_snd_mlt);
            GUICommon.PropertyField (sp_ini_clip, TextSnd.gui_snd_clip);
            
            if (sound.initialization.clip == null)
            {
                rl_ini_сlips.DoLayoutList();
                GUICommon.Space ();
            }
            
            GUICommon.PropertyField (sp_ini_group, TextSnd.gui_out_group);

            if (sound.initialization.outputGroup != null)
            {
                EditorGUI.indentLevel++;
                GUICommon.IntSlider (sp_ini_prior, priority_min, priority_max, TextSnd.gui_out_prior);
                GUICommon.Slider (sp_ini_spat,  spatial_min,  spatial_max,  TextSnd.gui_out_spat);
                GUICommon.PropertyField (sp_ini_mind, TextSnd.gui_out_mind);
                GUICommon.PropertyField (sp_ini_maxd, TextSnd.gui_out_maxd);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        void GUI_PropsAct()
        {
            if (Application.isPlaying == true)
            {
                GUICommon.Space ();
                if (GUILayout.Button (TextSnd.gui_btn_act, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.ActivationSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).activation.played = false;
                        }
                GUICommon.Space ();
            }
            
            EditorGUI.indentLevel++;
            
            GUICommon.PropertyField (sp_act_once, TextSnd.gui_snd_once);
            GUICommon.Slider (sp_act_mlt, multiplier_min, multiplier_max, TextSnd.gui_snd_mlt);
            GUICommon.PropertyField (sp_act_clip, TextSnd.gui_snd_clip);
            
            if (sound.activation.clip == null)
            {
                GUICommon.Space ();
                rl_act_сlips.DoLayoutList();
            }

            GUICommon.PropertyField (sp_act_group, TextSnd.gui_out_group);

            if (sound.activation.outputGroup != null)
            {
                EditorGUI.indentLevel++;
                GUICommon.IntSlider (sp_act_prior, priority_min, priority_max, TextSnd.gui_out_prior);
                GUICommon.Slider (sp_act_spat,  spatial_min,  spatial_max,  TextSnd.gui_out_spat);
                GUICommon.PropertyField (sp_act_mind, TextSnd.gui_out_mind);
                GUICommon.PropertyField (sp_act_maxd, TextSnd.gui_out_maxd);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        void GUI_PropsDml()
        {
            if (Application.isPlaying == true)
            {
                GUICommon.Space ();
                if (GUILayout.Button (TextSnd.gui_btn_dml, GUILayout.Height (25)))
                
                    foreach (var targ in targets)
                    
                        if (targ as RayfireSound != null)
                        {
                            RFSound.DemolitionSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).demolition.played = false;
                        }
                GUICommon.Space ();
            }
            
            EditorGUI.indentLevel++;
            
            GUICommon.PropertyField (sp_dml_once, TextSnd.gui_snd_once);
            GUICommon.Slider (sp_dml_mlt, multiplier_min, multiplier_max, TextSnd.gui_snd_mlt);
            GUICommon.PropertyField (sp_dml_clip, TextSnd.gui_snd_clip);

            if (sound.demolition.clip == null)
            {
                GUICommon.Space ();
                rl_dml_сlips.DoLayoutList();
            }

            GUICommon.PropertyField (sp_dml_group, TextSnd.gui_out_group);

            if (sound.demolition.outputGroup != null)
            {
                EditorGUI.indentLevel++;
                GUICommon.IntSlider (sp_dml_prior, priority_min, priority_max, TextSnd.gui_out_prior);
                GUICommon.Slider (sp_dml_spat,  spatial_min,  spatial_max,  TextSnd.gui_out_spat);
                GUICommon.PropertyField (sp_dml_mind, TextSnd.gui_out_mind);
                GUICommon.PropertyField (sp_dml_maxd, TextSnd.gui_out_maxd);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        void GUI_PropsCol()
        {
            if (Application.isPlaying == true)
            {
                GUICommon.Space ();
                if (GUILayout.Button (TextSnd.gui_btn_col, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireSound != null)
                        {
                            RFSound.CollisionSound (targ as RayfireSound, 0f);
                            (targ as RayfireSound).collision.played = false;
                        }
                GUICommon.Space ();
            }
            
            EditorGUI.indentLevel++;
            
            GUICommon.Caption (TextSnd.gui_cap_col);
            GUICommon.Slider (sp_col_vel, multiplier_min, multiplier_max, TextSnd.gui_snd_vel);
            
            GUICommon.Space ();
            GUILayout.Label (TextSnd.str_vel + sound.lastCollision);
            GUICommon.Space ();
            
            GUICommon.Caption (TextSnd.gui_cap_snd);
            
            GUICommon.PropertyField (sp_col_once, TextSnd.gui_snd_once);
            GUICommon.Slider (sp_col_mlt, multiplier_min, multiplier_max, TextSnd.gui_snd_mlt);
            GUICommon.PropertyField (sp_col_clip, TextSnd.gui_snd_clip);
            
            if (sound.collision.clip == null)
            {
                GUICommon.Space ();
                rl_col_сlips.DoLayoutList();
            }
            
            GUICommon.PropertyField (sp_col_group, TextSnd.gui_out_group);

            if (sound.collision.outputGroup != null)
            {
                EditorGUI.indentLevel++;
                GUICommon.IntSlider (sp_col_prior, priority_min, priority_max, TextSnd.gui_out_prior);
                GUICommon.Slider (sp_col_spat,  spatial_min,  spatial_max,  TextSnd.gui_out_spat);
                GUICommon.PropertyField (sp_col_mind, TextSnd.gui_out_mind);
                GUICommon.PropertyField (sp_col_maxd, TextSnd.gui_out_maxd);
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void GUI_Filters()
        {
            GUICommon.Caption (TextSnd.gui_cap_flt);
            GUICommon.Slider (sp_flt_size, size_min,     size_max,     TextSnd.gui_flt_size);
            GUICommon.Slider (sp_flt_dist, distance_min, distance_max, TextSnd.gui_flt_dist);
        }
        
        void GUI_Info()
        {
            if (Application.isPlaying == true)
            {
                GUILayout.Space (5);
                GUILayout.Label (TextSnd.str_info, EditorStyles.boldLabel);
                GUICommon.Space ();
                GUILayout.Label (TextSnd.str_volume+ RFSound.GeVolume(sound, 0f));
                GUILayout.Space (5);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawInitListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_ini_сlips.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawInitHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, TextSnd.str_clips);
        }

        void AddInit(ReorderableList list)
        {
            if (sound.initialization.clips == null)
                sound.initialization.clips = new List<AudioClip>();
            sound.initialization.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveInit(ReorderableList list)
        {
            if (sound.initialization.clips != null)
            {
                sound.initialization.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawActListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_act_сlips.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawActHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, TextSnd.str_clips);
        }

        void AddAct(ReorderableList list)
        {
            if (sound.activation.clips == null)
                sound.activation.clips = new List<AudioClip>();
            sound.activation.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveAct(ReorderableList list)
        {
            if (sound.activation.clips != null)
            {
                sound.activation.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawDmlListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_dml_сlips.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawDmlHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, TextSnd.str_clips);
        }

        void AddDml(ReorderableList list)
        {
            if (sound.demolition.clips == null)
                sound.demolition.clips = new List<AudioClip>();
            sound.demolition.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveDml(ReorderableList list)
        {
            if (sound.demolition.clips != null)
            {
                sound.demolition.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawColListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_col_сlips.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawColHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, TextSnd.str_clips);
        }

        void AddCol(ReorderableList list)
        {
            if (sound.collision.clips == null)
                sound.collision.clips = new List<AudioClip>();
            sound.collision.clips.Add (null);
            list.index = list.count;
        }
        
        void RemoveCol(ReorderableList list)
        {
            if (sound.collision.clips != null)
            {
                sound.collision.clips.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
    }
}