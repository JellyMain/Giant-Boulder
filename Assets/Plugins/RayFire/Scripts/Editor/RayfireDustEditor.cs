using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireDust))]
    public class RayfireDustEditor : Editor
    {
        RayfireDust     dust;
        ReorderableList rl_main_mat_list;
        
        // Foldout
        static bool fld_mat;
        static bool fld_emt;
        static bool fld_dyn;
        static bool fld_nse;
        static bool fld_col;
        static bool fld_lim;
        static bool fld_rnd;
        static bool fld_pol;

        // Minimum & Maximum ranges
        const float opacity_min = 0.01f;
        const float opacity_max = 1f;
        
        // Pool Minimum & Maximum ranges
        const int pl_id_min  = 0;
        const int pl_id_max  = 99;
        const int pl_cap_min = 3;
        const int pl_cap_max = 300;
        const int pl_rat_min = 1;
        const int pl_rat_max = 15;
        const int pl_ovf_min = 0;
        const int pl_ovf_max = 10;

        // Emission Minimum & Maximum ranges
        const int   ems_am_min       = 0;
        const int   ems_am_max       = 500;
        const int   ems_var_min      = 0;
        const int   ems_var_max      = 100;
        const float ems_rate_min     = 0;
        const float ems_rate_max     = 5f;
        const float ems_dur_min      = 0.5f;
        const float ems_dur_max      = 10f;
        const float ems_life_min     = 1f;
        const float ems_life_max     = 60f;
        const float ems_size_min_min = 0.001f;
        const float ems_size_min_max = 10f;
        const float ems_size_max_min = 0.1f;
        const float ems_size_max_max = 10f;
        
        // Dynamic Minimum & Maximum ranges
        const float dyn_speed_min = 0f;
        const float dyn_speed_max = 10f;
        const float dyn_grav_min  = -2f;
        const float dyn_grav_max  = 2f;
        const float dyn_rot_min   = 0f;
        const float dyn_rot_max   = 1f;
        
        // Noise Minimum & Maximum ranges
        const float nse_str_min    = 0;
        const float nse_str_max    = 3f;
        const float nse_freq_min   = 0.001f;
        const float nse_freq_max   = 3f;
        const float nse_scroll_min = 0;
        const float nse_scroll_max = 2f;
        
        // Collision Minimum & Maximum ranges
        const float col_rad_min = 0.1f;
        const float col_rad_max = 2f;
        
        // Limitations Minimum & Maximum ranges
        const int   lim_prt_min  = 3;
        const int   lim_prt_max  = 100;
        const int   lim_perc_min = 10;
        const int   lim_perc_max = 100;
        const float lim_size_min = 0.05f;
        const float lim_size_max = 5;
        
        // Serialized properties
        SerializedProperty sp_main_dml;
        SerializedProperty sp_main_act;
        SerializedProperty sp_main_imp;
        SerializedProperty sp_main_op;
        SerializedProperty sp_main_mat;
        SerializedProperty sp_main_mat_list;
        
        // Serialized Pool properties
        SerializedProperty sp_pol_id;
        SerializedProperty sp_pol_en;
        SerializedProperty sp_pol_war;
        SerializedProperty sp_pol_cap;
        SerializedProperty sp_pol_rat;
        SerializedProperty sp_pol_skp;
        SerializedProperty sp_pol_reu;
        SerializedProperty sp_pol_ovf;
        
        // Serialized Emission properties
        SerializedProperty sp_ems_tp;
        SerializedProperty sp_ems_am;
        SerializedProperty sp_ems_var;
        SerializedProperty sp_ems_rate;
        SerializedProperty sp_ems_dur;
        SerializedProperty sp_ems_life_min;
        SerializedProperty sp_ems_life_max;
        SerializedProperty sp_ems_size_min;
        SerializedProperty sp_ems_size_max;
        SerializedProperty sp_ems_mat;
        
        // Serialized Dynamic properties
        SerializedProperty sp_dyn_speed_min;
        SerializedProperty sp_dyn_speed_max;
        SerializedProperty sp_dyn_grav_min;
        SerializedProperty sp_dyn_grav_max;
        SerializedProperty sp_dyn_rot;
        
        // Serialized Noise properties
        SerializedProperty sp_nse_en;
        SerializedProperty sp_nse_qual;
        SerializedProperty sp_nse_str_min;
        SerializedProperty sp_nse_str_max;
        SerializedProperty sp_nse_freq;
        SerializedProperty sp_nse_scroll;
        SerializedProperty sp_nse_damp;
        
        // Serialized Collision properties
        SerializedProperty sp_col_mask;
        SerializedProperty sp_col_qual;
        SerializedProperty sp_col_rad;
        
        // Serialized Limitations properties
        SerializedProperty sp_lim_prt_min;
        SerializedProperty sp_lim_prt_max;
        SerializedProperty sp_lim_prt_vis;
        SerializedProperty sp_lim_perc;
        SerializedProperty sp_lim_size;
        
        // Serialized Rendering properties
        SerializedProperty sp_ren_cast;
        SerializedProperty sp_ren_rec;
        SerializedProperty sp_ren_prob;
        SerializedProperty sp_ren_vect;
        SerializedProperty sp_ren_t;
        SerializedProperty sp_ren_tag;
        SerializedProperty sp_ren_l;
        SerializedProperty sp_ren_lay;

        private void OnEnable()
        {
            // Get component
            dust = (RayfireDust)target;
            
            // Set tag list
            GUICommon.SetTags();
            
            // Find properties
            sp_main_dml      = serializedObject.FindProperty(nameof(dust.onDemolition));
            sp_main_act      = serializedObject.FindProperty(nameof(dust.onActivation));
            sp_main_imp      = serializedObject.FindProperty(nameof(dust.onImpact));
            sp_main_op       = serializedObject.FindProperty(nameof(dust.opacity));
            sp_main_mat      = serializedObject.FindProperty(nameof(dust.dustMaterial));
            sp_main_mat_list = serializedObject.FindProperty(nameof(dust.dustMaterials));
            sp_ems_mat       = serializedObject.FindProperty(nameof(dust.emissionMaterial));
            
            // Find Pool properties
            sp_pol_id  = serializedObject.FindProperty(nameof(dust.pool) + "." + nameof(dust.pool.id));
            sp_pol_en  = serializedObject.FindProperty(nameof(dust.pool) + "." + nameof(dust.pool.enable));
            sp_pol_war = serializedObject.FindProperty(nameof(dust.pool) + "." + nameof(dust.pool.warmup));
            sp_pol_cap = serializedObject.FindProperty(nameof(dust.pool) + "." + nameof(dust.pool.cap));
            sp_pol_rat = serializedObject.FindProperty(nameof(dust.pool) + "." + nameof(dust.pool.rate));
            sp_pol_skp = serializedObject.FindProperty(nameof(dust.pool) + "." + nameof(dust.pool.skip));
            sp_pol_reu = serializedObject.FindProperty(nameof(dust.pool) + "." + nameof(dust.pool.reuse));
            sp_pol_ovf = serializedObject.FindProperty(nameof(dust.pool) + "." + nameof(dust.pool.over));
            
            // Find Emission properties
            sp_ems_tp       = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.burstType));
            sp_ems_am       = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.burstAmount));
            sp_ems_var      = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.burstVar));
            sp_ems_rate     = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.distanceRate));
            sp_ems_dur      = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.duration));
            sp_ems_life_min = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.lifeMin));
            sp_ems_life_max = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.lifeMax));
            sp_ems_size_min = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.sizeMin));
            sp_ems_size_max = serializedObject.FindProperty(nameof(dust.emission) + "." + nameof(dust.emission.sizeMax));
            
            // Find Dynamic properties
            sp_dyn_speed_min = serializedObject.FindProperty(nameof(dust.dynamic) + "." + nameof(dust.dynamic.speedMin));
            sp_dyn_speed_max = serializedObject.FindProperty(nameof(dust.dynamic) + "." + nameof(dust.dynamic.speedMax));
            sp_dyn_grav_min  = serializedObject.FindProperty(nameof(dust.dynamic) + "." + nameof(dust.dynamic.gravityMin));
            sp_dyn_grav_max  = serializedObject.FindProperty(nameof(dust.dynamic) + "." + nameof(dust.dynamic.gravityMax));
            sp_dyn_rot       = serializedObject.FindProperty(nameof(dust.dynamic) + "." + nameof(dust.dynamic.rotation));
            
            // Find Noise properties
            sp_nse_en      = serializedObject.FindProperty(nameof(dust.noise) + "." + nameof(dust.noise.enabled));
            sp_nse_qual    = serializedObject.FindProperty(nameof(dust.noise) + "." + nameof(dust.noise.quality));
            sp_nse_str_min = serializedObject.FindProperty(nameof(dust.noise) + "." + nameof(dust.noise.strengthMin));
            sp_nse_str_max = serializedObject.FindProperty(nameof(dust.noise) + "." + nameof(dust.noise.strengthMax));
            sp_nse_freq    = serializedObject.FindProperty(nameof(dust.noise) + "." + nameof(dust.noise.frequency));
            sp_nse_scroll  = serializedObject.FindProperty(nameof(dust.noise) + "." + nameof(dust.noise.scrollSpeed));
            sp_nse_damp    = serializedObject.FindProperty(nameof(dust.noise) + "." + nameof(dust.noise.damping));
            
            // Find Collision properties
            sp_col_mask = serializedObject.FindProperty(nameof(dust.collision) + "." + nameof(dust.collision.collidesWith));
            sp_col_qual = serializedObject.FindProperty(nameof(dust.collision) + "." + nameof(dust.collision.quality));
            sp_col_rad  = serializedObject.FindProperty(nameof(dust.collision) + "." + nameof(dust.collision.radiusScale));
            
            // Find Limitations properties
            sp_lim_prt_min = serializedObject.FindProperty(nameof(dust.limitations) + "." + nameof(dust.limitations.minParticles));
            sp_lim_prt_max = serializedObject.FindProperty(nameof(dust.limitations) + "." + nameof(dust.limitations.maxParticles));
            sp_lim_prt_vis = serializedObject.FindProperty(nameof(dust.limitations) + "." + nameof(dust.limitations.visible));
            sp_lim_perc    = serializedObject.FindProperty(nameof(dust.limitations) + "." + nameof(dust.limitations.percentage));
            sp_lim_size    = serializedObject.FindProperty(nameof(dust.limitations) + "." + nameof(dust.limitations.sizeThreshold));
            
            // Find Rendering properties
            sp_ren_cast = serializedObject.FindProperty(nameof(dust.rendering) + "." + nameof(dust.rendering.castShadows));
            sp_ren_rec  = serializedObject.FindProperty(nameof(dust.rendering) + "." + nameof(dust.rendering.receiveShadows));
            sp_ren_prob = serializedObject.FindProperty(nameof(dust.rendering) + "." + nameof(dust.rendering.lightProbes));
            sp_ren_vect = serializedObject.FindProperty(nameof(dust.rendering) + "." + nameof(dust.rendering.motionVectors));
            sp_ren_t    = serializedObject.FindProperty(nameof(dust.rendering) + "." + nameof(dust.rendering.t));
            sp_ren_tag  = serializedObject.FindProperty(nameof(dust.rendering) + "." + nameof(dust.rendering.tag));
            sp_ren_l    = serializedObject.FindProperty(nameof(dust.rendering) + "." + nameof(dust.rendering.l));
            sp_ren_lay  = serializedObject.FindProperty(nameof(dust.rendering) + "." + nameof(dust.rendering.layer));
            
            // Reorderable List
            rl_main_mat_list = new ReorderableList(serializedObject, sp_main_mat_list, true, true, true, true)
            {
                drawElementCallback = DrawInitListItems,
                drawHeaderCallback  = DrawInitHeader,
                onAddCallback       = AddInit,
                onRemoveCallback    = RemoveInit
            };

            if (EditorPrefs.HasKey (TextKeys.dst_fld_mat) == true) fld_mat = EditorPrefs.GetBool (TextKeys.dst_fld_mat);
            if (EditorPrefs.HasKey (TextKeys.dst_fld_emt) == true) fld_emt = EditorPrefs.GetBool (TextKeys.dst_fld_emt);
            if (EditorPrefs.HasKey (TextKeys.dst_fld_dyn) == true) fld_dyn = EditorPrefs.GetBool (TextKeys.dst_fld_dyn);
            if (EditorPrefs.HasKey (TextKeys.dst_fld_nse) == true) fld_nse = EditorPrefs.GetBool (TextKeys.dst_fld_nse);
            if (EditorPrefs.HasKey (TextKeys.dst_fld_col) == true) fld_col = EditorPrefs.GetBool (TextKeys.dst_fld_col);
            if (EditorPrefs.HasKey (TextKeys.dst_fld_lim) == true) fld_lim = EditorPrefs.GetBool (TextKeys.dst_fld_lim);
            if (EditorPrefs.HasKey (TextKeys.dst_fld_rnd) == true) fld_rnd = EditorPrefs.GetBool (TextKeys.dst_fld_rnd);
            if (EditorPrefs.HasKey (TextKeys.dst_fld_pol) == true) fld_pol = EditorPrefs.GetBool (TextKeys.dst_fld_pol);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            GUI_Buttons();
            GUI_Emit();
            GUI_Main();
            UI_Properties();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////

        void GUI_Buttons()
        {
            GUILayout.BeginHorizontal();

            if (Application.isPlaying == true)
            {
                if (GUILayout.Button (TextDst.gui_btn_emit, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDust != null)
                            (targ as RayfireDust).Emit();

                if (GUILayout.Button (TextDst.gui_btn_clean, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDust != null)
                            (targ as RayfireDust).Clean();
            }

            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Emit
        /// /////////////////////////////////////////////////////////
        
        void GUI_Emit()
        {
            GUICommon.CaptionBox (TextDst.gui_cap_event);
            GUICommon.PropertyField (sp_main_dml, TextDst.gui_main_dml);
            GUICommon.PropertyField (sp_main_act, TextDst.gui_main_act);
            GUICommon.PropertyField (sp_main_imp, TextDst.gui_main_imp);
            if (sp_main_dml.boolValue == false && sp_main_act.boolValue == false && sp_main_imp.boolValue == false)
                GUICommon.HelpBox (TextDst.hlp_select, MessageType.Warning, true);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Main
        /// /////////////////////////////////////////////////////////
        
        void GUI_Main()
        {
            GUICommon.CaptionBox (TextDst.gui_cap_dust);
            GUICommon.Slider (sp_main_op, opacity_min, opacity_max, TextDst.gui_main_op);
            GUICommon.PropertyField (sp_main_mat, TextDst.gui_main_mat);
            GUICommon.Foldout (ref fld_mat, TextKeys.dst_fld_mat, TextDst.gui_main_mat_list.text);
            if (fld_mat == true)
            {
                GUICommon.Space ();
                rl_main_mat_list.DoLayoutList();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_Properties()
        {
            GUICommon.CaptionBox (TextDst.gui_cap_props);
            GUI_Pool();
            GUI_Emission();
            GUI_Dynamic();
            GUI_Noise();
            GUI_Collision();
            GUI_Limitations();
            GUI_Rendering();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Pool
        /// /////////////////////////////////////////////////////////

        void GUI_Pool()
        {
            GUICommon.Foldout (ref fld_pol, TextKeys.dst_fld_pol, TextDst.gui_fld_pol.text);
            if (fld_pol == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.IntSlider (sp_pol_id, pl_id_min, pl_id_max, TextDst.gui_pol_id);
                GUICommon.PropertyField (sp_pol_en,  TextDst.gui_pol_en);
                GUICommon.PropertyField (sp_pol_war, TextDst.gui_pol_war);
                GUICommon.IntSlider (sp_pol_cap, pl_cap_min, pl_cap_max, TextDst.gui_pol_cap);
                GUICommon.IntSlider (sp_pol_rat, pl_rat_min, pl_rat_max, TextDst.gui_pol_rat);
                GUICommon.PropertyField (sp_pol_skp, TextDst.gui_pol_skp);
                GUICommon.PropertyField (sp_pol_reu, TextDst.gui_pol_reu);
                if (dust.pool.reuse == true)
                    GUICommon.IntSlider (sp_pol_ovf, pl_ovf_min, pl_ovf_max, TextDst.gui_pol_ovf);

                // Caption
                if (dust.pool.enable == true && Application.isPlaying == true)
                {
                    GUICommon.Space ();
                    if (dust.pool.emitter != null)
                        GUILayout.Label (TextDst.str_avail + dust.pool.emitter.queue.Count, EditorStyles.boldLabel);
                }
                
                // Edit
                if (Application.isPlaying == true)
                {
                    GUICommon.Space ();
                    if (GUILayout.Button (TextDst.gui_btn_edit, GUILayout.Height (20)))
                        foreach (var targ in targets)
                            if (targ as RayfireDebris != null)
                                (targ as RayfireDebris).EditEmitterParticles();
                }
                
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////
        
        void GUI_Emission()
        {
            GUICommon.Foldout (ref fld_emt, TextKeys.dst_fld_emt, TextDst.gui_fld_emt.text);
            if (fld_emt == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDst.gui_cap_burst);
                GUICommon.PropertyField (sp_ems_tp, TextDst.gui_ems_tp);
                if (dust.emission.burstType != RFParticles.BurstType.None)
                {
                    GUICommon.IntSlider (sp_ems_am,  ems_am_min,  ems_am_max,  TextDst.gui_ems_am);
                    GUICommon.IntSlider (sp_ems_var, ems_var_min, ems_var_max, TextDst.gui_ems_var);
                }
                GUICommon.Caption (TextDst.gui_cap_dist);
                GUICommon.Slider (sp_ems_rate, ems_rate_min, ems_rate_max, TextDst.gui_ems_rate);
                GUICommon.Slider (sp_ems_dur,  ems_dur_min,  ems_dur_max,  TextDst.gui_ems_dur);
                GUICommon.Caption (TextDst.gui_cap_life);
                GUICommon.Slider (sp_ems_life_min, ems_life_min, ems_life_max, TextDst.gui_ems_life_min);
                GUICommon.Slider (sp_ems_life_max, ems_life_min, ems_life_max, TextDst.gui_ems_life_max);
                GUICommon.Caption (TextDst.gui_cap_size);
                GUICommon.Slider (sp_ems_size_min, ems_size_min_min, ems_size_min_max, TextDst.gui_ems_size_min);
                GUICommon.Slider (sp_ems_size_max, ems_size_max_min, ems_size_max_max, TextDst.gui_ems_size_max);
                GUICommon.Caption (TextDst.gui_cap_mat);
                GUICommon.PropertyField (sp_ems_mat, TextDst.gui_ems_mat);
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Dynamic
        /// /////////////////////////////////////////////////////////

        void GUI_Dynamic()
        {
            GUICommon.Foldout (ref fld_dyn, TextKeys.dst_fld_dyn, TextDst.gui_fld_dyn.text);
            if (fld_dyn == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDst.gui_cap_speed);
                GUICommon.Slider (sp_dyn_speed_min, dyn_speed_min, dyn_speed_max, TextDst.gui_dyn_speed_min);
                GUICommon.Slider (sp_dyn_speed_max, dyn_speed_min, dyn_speed_max, TextDst.gui_dyn_speed_max);
                GUICommon.Caption (TextDst.gui_cap_grav);
                GUICommon.Slider (sp_dyn_grav_min, dyn_grav_min, dyn_grav_max, TextDst.gui_dyn_grav_min);
                GUICommon.Slider (sp_dyn_grav_max, dyn_grav_min, dyn_grav_max, TextDst.gui_dyn_grav_max);
                GUICommon.Caption (TextDst.gui_cap_rot);
                GUICommon.Slider (sp_dyn_rot, dyn_rot_min, dyn_rot_max, TextDst.gui_dyn_rot);
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////
        
        void GUI_Noise()
        {
            GUICommon.Foldout (ref fld_nse, TextKeys.deb_fld_nse, TextDst.gui_fld_nse.text);
            if (fld_nse == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDst.gui_cap_main);
                GUICommon.PropertyField (sp_nse_en, TextDst.gui_nse_en);
                if (dust.noise.enabled == true)
                {
                    GUICommon.PropertyField (sp_nse_qual, TextDst.gui_nse_qual);
                    GUICommon.Caption (TextDst.gui_cap_str);
                    GUICommon.Slider (sp_nse_str_min, nse_str_min, nse_str_max, TextDst.gui_nse_str_min);
                    GUICommon.Slider (sp_nse_str_max, nse_str_min, nse_str_max, TextDst.gui_nse_str_max);
                    GUICommon.Caption (TextDst.gui_cap_other);
                    GUICommon.Slider (sp_nse_freq,   nse_freq_min,   nse_freq_max,   TextDst.gui_nse_freq);
                    GUICommon.Slider (sp_nse_scroll, nse_scroll_min, nse_scroll_max, TextDst.gui_nse_scroll);
                    GUICommon.PropertyField (sp_nse_damp, TextDst.gui_nse_damp);
                }
                EditorGUI.indentLevel--;
            }
        }
        
         /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        void GUI_Collision()
        {
            GUICommon.Foldout (ref fld_col, TextKeys.dst_fld_col, TextDst.gui_fld_col.text);
            if (fld_col == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDst.gui_cap_common);
                GUICommon.MaskField(sp_col_mask, TextDst.gui_col_mask);
                GUICommon.PropertyField (sp_col_qual, TextDst.gui_col_qual);
                GUICommon.Slider (sp_col_rad, col_rad_min, col_rad_max, TextDst.gui_col_rad);
                EditorGUI.indentLevel--;
            }
        }
         
        /// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////

        void GUI_Limitations()
        {
            GUICommon.Foldout (ref fld_lim, TextKeys.dst_fld_lim, TextDst.gui_fld_lim.text);
            if (fld_lim == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDst.gui_cap_prt);
                GUICommon.IntSlider (sp_lim_prt_min, lim_prt_min, lim_prt_max, TextDst.gui_lim_prt_min);
                GUICommon.IntSlider (sp_lim_prt_max, lim_prt_min, lim_prt_max, TextDst.gui_lim_prt_max);
                GUICommon.PropertyField (sp_lim_prt_vis, TextDst.gui_lim_prt_vis);
                GUICommon.Caption (TextDst.gui_cap_frags);
                GUICommon.IntSlider (sp_lim_perc, lim_perc_min, lim_perc_max, TextDst.gui_lim_perc);
                GUICommon.Slider (sp_lim_size, lim_size_min, lim_size_max, TextDst.gui_lim_size);
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rendering
        /// /////////////////////////////////////////////////////////

        void GUI_Rendering()
        {
            GUICommon.Foldout (ref fld_rnd, TextKeys.dst_fld_rnd, TextDst.gui_fld_ren.text);
            if (fld_rnd == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDst.gui_cap_shad);
                GUICommon.PropertyField (sp_ren_cast, TextDst.gui_ren_cast);
                GUICommon.PropertyField (sp_ren_rec,  TextDst.gui_ren_rec);
                GUICommon.Caption (TextDst.gui_cap_other);
                GUICommon.PropertyField (sp_ren_prob, TextDst.gui_ren_prob);
                GUICommon.PropertyField (sp_ren_vect, TextDst.gui_ren_vect);
                GUICommon.PropertyField (sp_ren_t,    TextDst.gui_ren_t);
                if (dust.rendering.t == true)
                    GUICommon.TagField (sp_ren_tag, TextDst.gui_ren_tag);
                GUICommon.PropertyField (sp_ren_l, TextDst.gui_ren_l);
                if (dust.rendering.l == true)
                    GUICommon.LayerField (sp_ren_lay, TextDbr.gui_ren_lay);
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawInitListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_main_mat_list.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawInitHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, TextDst.gui_main_mat_list);
        }

        void AddInit(ReorderableList list)
        {
            if (dust.dustMaterials == null)
                dust.dustMaterials = new List<Material>();
            dust.dustMaterials.Add (null);
            list.index = list.count;
        }
        
        void RemoveInit(ReorderableList list)
        {
            if (dust.dustMaterials != null)
            {
                dust.dustMaterials.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
    }
}