using UnityEngine;
using UnityEditor;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireDebris))]
    public class RayfireDebrisEditor : Editor
    {
        RayfireDebris debris;
        
        // Foldout
        static bool fld_pol;
        static bool fld_emt;
        static bool fld_dyn;
        static bool fld_nse;
        static bool fld_col;
        static bool fld_lim;
        static bool fld_rnd;
        
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
        const int   ems_am_max       = 100;
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
        const float dyn_vel_min   = 0f;
        const float dyn_vel_max   = 3f;
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
        const float col_dmp_min = 0f;
        const float col_dmp_max = 1f;
        const float col_bnc_min = 0f;
        const float col_bnc_max = 1f;
        
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
        SerializedProperty sp_main_ref;
        SerializedProperty sp_main_mat;
        SerializedProperty sp_ems_mat;
        
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
        
        // Serialized Dynamic properties
        SerializedProperty sp_dyn_speed_min;
        SerializedProperty sp_dyn_speed_max;
        SerializedProperty sp_dyn_vel_min;
        SerializedProperty sp_dyn_vel_max;
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
        SerializedProperty sp_col_dmp_tp;
        SerializedProperty sp_col_dmp_min;
        SerializedProperty sp_col_dmp_max;
        SerializedProperty sp_col_bnc_tp;
        SerializedProperty sp_col_bnc_min;
        SerializedProperty sp_col_bnc_max;
        
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
            debris = (RayfireDebris)target;
            
            // Set tag list
            GUICommon.SetTags();
            
            // Find properties
            sp_main_dml = serializedObject.FindProperty(nameof(debris.onDemolition));
            sp_main_act = serializedObject.FindProperty(nameof(debris.onActivation));
            sp_main_imp = serializedObject.FindProperty(nameof(debris.onImpact));
            sp_main_ref = serializedObject.FindProperty(nameof(debris.debrisReference));
            sp_main_mat = serializedObject.FindProperty(nameof(debris.debrisMaterial));
            sp_ems_mat  = serializedObject.FindProperty(nameof(debris.emissionMaterial));
            
            // Find Pool properties
            sp_pol_id  = serializedObject.FindProperty(nameof(debris.pool) + "." + nameof(debris.pool.id));
            sp_pol_en  = serializedObject.FindProperty(nameof(debris.pool) + "." + nameof(debris.pool.enable));
            sp_pol_war = serializedObject.FindProperty(nameof(debris.pool) + "." + nameof(debris.pool.warmup));
            sp_pol_cap = serializedObject.FindProperty(nameof(debris.pool) + "." + nameof(debris.pool.cap));
            sp_pol_rat = serializedObject.FindProperty(nameof(debris.pool) + "." + nameof(debris.pool.rate));
            sp_pol_skp = serializedObject.FindProperty(nameof(debris.pool) + "." + nameof(debris.pool.skip));
            sp_pol_reu = serializedObject.FindProperty(nameof(debris.pool) + "." + nameof(debris.pool.reuse));
            sp_pol_ovf = serializedObject.FindProperty(nameof(debris.pool) + "." + nameof(debris.pool.over));
            
            // Find Emission properties
            sp_ems_tp       = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.burstType));
            sp_ems_am       = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.burstAmount));
            sp_ems_var      = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.burstVar));
            sp_ems_rate     = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.distanceRate));
            sp_ems_dur      = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.duration));
            sp_ems_life_min = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.lifeMin));
            sp_ems_life_max = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.lifeMax));
            sp_ems_size_min = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.sizeMin));
            sp_ems_size_max = serializedObject.FindProperty(nameof(debris.emission) + "." + nameof(debris.emission.sizeMax));
            
            // Find Dynamic properties
            sp_dyn_speed_min = serializedObject.FindProperty(nameof(debris.dynamic) + "." + nameof(debris.dynamic.speedMin));
            sp_dyn_speed_max = serializedObject.FindProperty(nameof(debris.dynamic) + "." + nameof(debris.dynamic.speedMax));
            sp_dyn_vel_min   = serializedObject.FindProperty(nameof(debris.dynamic) + "." + nameof(debris.dynamic.velocityMin));
            sp_dyn_vel_max   = serializedObject.FindProperty(nameof(debris.dynamic) + "." + nameof(debris.dynamic.velocityMax));
            sp_dyn_grav_min  = serializedObject.FindProperty(nameof(debris.dynamic) + "." + nameof(debris.dynamic.gravityMin));
            sp_dyn_grav_max  = serializedObject.FindProperty(nameof(debris.dynamic) + "." + nameof(debris.dynamic.gravityMax));
            sp_dyn_rot       = serializedObject.FindProperty(nameof(debris.dynamic) + "." + nameof(debris.dynamic.rotationSpeed));
            
            // Find Noise properties
            sp_nse_en      = serializedObject.FindProperty(nameof(debris.noise) + "." + nameof(debris.noise.enabled));
            sp_nse_qual    = serializedObject.FindProperty(nameof(debris.noise) + "." + nameof(debris.noise.quality));
            sp_nse_str_min = serializedObject.FindProperty(nameof(debris.noise) + "." + nameof(debris.noise.strengthMin));
            sp_nse_str_max = serializedObject.FindProperty(nameof(debris.noise) + "." + nameof(debris.noise.strengthMax));
            sp_nse_freq    = serializedObject.FindProperty(nameof(debris.noise) + "." + nameof(debris.noise.frequency));
            sp_nse_scroll  = serializedObject.FindProperty(nameof(debris.noise) + "." + nameof(debris.noise.scrollSpeed));
            sp_nse_damp    = serializedObject.FindProperty(nameof(debris.noise) + "." + nameof(debris.noise.damping));
            
            // Find Collision properties
            sp_col_mask    = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.collidesWith));
            sp_col_qual    = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.quality));
            sp_col_rad     = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.radiusScale));
            sp_col_dmp_tp  = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.dampenType));
            sp_col_dmp_min = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.dampenMin));
            sp_col_dmp_max = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.dampenMax));
            sp_col_bnc_tp  = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.bounceType));
            sp_col_bnc_min = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.bounceMin));
            sp_col_bnc_max = serializedObject.FindProperty(nameof(debris.collision) + "." + nameof(debris.collision.bounceMax));
            
            // Find Limitations properties
            sp_lim_prt_min = serializedObject.FindProperty(nameof(debris.limitations) + "." + nameof(debris.limitations.minParticles));
            sp_lim_prt_max = serializedObject.FindProperty(nameof(debris.limitations) + "." + nameof(debris.limitations.maxParticles));
            sp_lim_prt_vis = serializedObject.FindProperty(nameof(debris.limitations) + "." + nameof(debris.limitations.visible));
            sp_lim_perc    = serializedObject.FindProperty(nameof(debris.limitations) + "." + nameof(debris.limitations.percentage));
            sp_lim_size    = serializedObject.FindProperty(nameof(debris.limitations) + "." + nameof(debris.limitations.sizeThreshold));
            
            // Find Rendering properties
            sp_ren_cast = serializedObject.FindProperty(nameof(debris.rendering) + "." + nameof(debris.rendering.castShadows));
            sp_ren_rec  = serializedObject.FindProperty(nameof(debris.rendering) + "." + nameof(debris.rendering.receiveShadows));
            sp_ren_prob = serializedObject.FindProperty(nameof(debris.rendering) + "." + nameof(debris.rendering.lightProbes));
            sp_ren_vect = serializedObject.FindProperty(nameof(debris.rendering) + "." + nameof(debris.rendering.motionVectors));
            sp_ren_t    = serializedObject.FindProperty(nameof(debris.rendering) + "." + nameof(debris.rendering.t));
            sp_ren_tag  = serializedObject.FindProperty(nameof(debris.rendering) + "." + nameof(debris.rendering.tag));
            sp_ren_l    = serializedObject.FindProperty(nameof(debris.rendering) + "." + nameof(debris.rendering.l));
            sp_ren_lay  = serializedObject.FindProperty(nameof(debris.rendering) + "." + nameof(debris.rendering.layer));
                
            // Foldouts
            if (EditorPrefs.HasKey (TextKeys.deb_fld_pol) == true) fld_pol = EditorPrefs.GetBool (TextKeys.deb_fld_pol);
            if (EditorPrefs.HasKey (TextKeys.deb_fld_emt) == true) fld_emt = EditorPrefs.GetBool (TextKeys.deb_fld_emt);
            if (EditorPrefs.HasKey (TextKeys.deb_fld_dyn) == true) fld_dyn = EditorPrefs.GetBool (TextKeys.deb_fld_dyn);
            if (EditorPrefs.HasKey (TextKeys.deb_fld_nse) == true) fld_nse = EditorPrefs.GetBool (TextKeys.deb_fld_nse);
            if (EditorPrefs.HasKey (TextKeys.deb_fld_col) == true) fld_col = EditorPrefs.GetBool (TextKeys.deb_fld_col);
            if (EditorPrefs.HasKey (TextKeys.deb_fld_lim) == true) fld_lim = EditorPrefs.GetBool (TextKeys.deb_fld_lim);
            if (EditorPrefs.HasKey (TextKeys.deb_fld_rnd) == true) fld_rnd = EditorPrefs.GetBool (TextKeys.deb_fld_rnd);
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
            GUI_Properties();
            
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
                if (GUILayout.Button (TextDbr.gui_btn_emit, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDebris != null)
                            (targ as RayfireDebris).Emit();

                if (GUILayout.Button (TextDbr.gui_btn_clean, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDebris != null)
                            (targ as RayfireDebris).Clean();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// /////////////////////////////////////////////////////////
        /// Emit
        /// /////////////////////////////////////////////////////////
        
        void GUI_Emit()
        {
            GUICommon.CaptionBox (TextDbr.gui_cap_event);
            GUICommon.PropertyField (sp_main_dml, TextDbr.gui_main_dml);
            GUICommon.PropertyField (sp_main_act, TextDbr.gui_main_act);
            GUICommon.PropertyField (sp_main_imp, TextDbr.gui_main_imp);
            if (sp_main_dml.boolValue == false && sp_main_act.boolValue == false && sp_main_imp.boolValue == false)
                GUICommon.HelpBox (TextDbr.hlp_select, MessageType.Warning, true);
        }

        /// /////////////////////////////////////////////////////////
        /// Main
        /// /////////////////////////////////////////////////////////
       
        void GUI_Main()
        {
            GUICommon.CaptionBox (TextDbr.gui_cap_debris);
            GUICommon.PropertyField (sp_main_ref, TextDbr.gui_main_ref);
            GUICommon.PropertyField (sp_main_mat, TextDbr.gui_main_mat);
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void GUI_Properties()
        {
            GUICommon.CaptionBox (TextDbr.gui_cap_props);
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
            GUICommon.Foldout (ref fld_pol, TextKeys.deb_fld_pol, TextDbr.gui_fld_pol.text);
            if (fld_pol == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.IntSlider (sp_pol_id, pl_id_min, pl_id_max, TextDbr.gui_pol_id);
                GUICommon.PropertyField (sp_pol_en,  TextDbr.gui_pol_en);
                GUICommon.PropertyField (sp_pol_war, TextDbr.gui_pol_war);
                GUICommon.IntSlider (sp_pol_cap, pl_cap_min, pl_cap_max, TextDbr.gui_pol_cap);
                GUICommon.IntSlider (sp_pol_rat, pl_rat_min, pl_rat_max, TextDbr.gui_pol_rat);
                GUICommon.PropertyField (sp_pol_skp, TextDbr.gui_pol_skp);
                GUICommon.PropertyField (sp_pol_reu, TextDbr.gui_pol_reu);
                if (debris.pool.reuse == true)
                    GUICommon.IntSlider (sp_pol_ovf, pl_ovf_min, pl_ovf_max, TextDbr.gui_pol_ovf);

                // Caption
                if (debris.pool.enable == true && Application.isPlaying == true)
                {
                    GUICommon.Space ();
                    if (debris.pool.emitter != null)
                        GUILayout.Label (TextDbr.str_avail + debris.pool.emitter.queue.Count, EditorStyles.boldLabel);
                }
                
                // Edit
                if (Application.isPlaying == true)
                {
                    GUICommon.Space ();
                    if (GUILayout.Button (TextDbr.gui_btn_edit, GUILayout.Height (20)))
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
            GUICommon.Foldout (ref fld_emt, TextKeys.deb_fld_emt, TextDbr.gui_fld_emt.text);
            if (fld_emt == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDbr.gui_cap_burst);
                GUICommon.PropertyField (sp_ems_tp, TextDbr.gui_ems_tp);
                if (debris.emission.burstType != RFParticles.BurstType.None)
                {
                    GUICommon.IntSlider (sp_ems_am,  ems_am_min,  ems_am_max,  TextDbr.gui_ems_am);
                    GUICommon.IntSlider (sp_ems_var, ems_var_min, ems_var_max, TextDbr.gui_ems_var);
                }
                GUICommon.Caption (TextDbr.gui_cap_dist);
                GUICommon.Slider (sp_ems_rate, ems_rate_min, ems_rate_max, TextDbr.gui_ems_rate);
                GUICommon.Slider (sp_ems_dur,  ems_dur_min,  ems_dur_max,  TextDbr.gui_ems_dur);
                GUICommon.Caption (TextDbr.gui_cap_life);
                GUICommon.Slider (sp_ems_life_min, ems_life_min, ems_life_max, TextDbr.gui_ems_life_min);
                GUICommon.Slider (sp_ems_life_max, ems_life_min, ems_life_max, TextDbr.gui_ems_life_max);
                GUICommon.Caption (TextDbr.gui_cap_size);
                GUICommon.Slider (sp_ems_size_min, ems_size_min_min, ems_size_min_max, TextDbr.gui_ems_size_min);
                GUICommon.Slider (sp_ems_size_max, ems_size_max_min, ems_size_max_max, TextDbr.gui_ems_size_max);
                GUICommon.Caption (TextDbr.gui_cap_mat);
                GUICommon.PropertyField (sp_ems_mat, TextDbr.gui_ems_mat);
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Dynamic
        /// /////////////////////////////////////////////////////////

        void GUI_Dynamic()
        {
            GUICommon.Foldout (ref fld_dyn, TextKeys.deb_fld_dyn, TextDbr.gui_fld_dyn.text);
            if (fld_dyn == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDbr.gui_cap_speed);
                GUICommon.Slider (sp_dyn_speed_min, dyn_speed_min, dyn_speed_max, TextDbr.gui_dyn_speed_min);
                GUICommon.Slider (sp_dyn_speed_max, dyn_speed_min, dyn_speed_max, TextDbr.gui_dyn_speed_max);
                GUICommon.Caption (TextDbr.gui_cap_vel);
                GUICommon.Slider (sp_dyn_vel_min, dyn_vel_min, dyn_vel_max, TextDbr.gui_dyn_vel_min);
                GUICommon.Slider (sp_dyn_vel_max, dyn_vel_min, dyn_vel_max, TextDbr.gui_dyn_vel_max);
                GUICommon.Caption (TextDbr.gui_cap_grav);
                GUICommon.Slider (sp_dyn_grav_min, dyn_grav_min, dyn_grav_max, TextDbr.gui_dyn_grav_min);
                GUICommon.Slider (sp_dyn_grav_max, dyn_grav_min, dyn_grav_max, TextDbr.gui_dyn_grav_max);
                GUICommon.Caption (TextDbr.gui_cap_rot);
                GUICommon.Slider (sp_dyn_rot, dyn_rot_min, dyn_rot_max, TextDbr.gui_dyn_rot);
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////
        
        void GUI_Noise()
        {
            GUICommon.Foldout (ref fld_nse, TextKeys.deb_fld_nse, TextDbr.gui_fld_nse.text);
            if (fld_nse == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDbr.gui_cap_main);
                GUICommon.PropertyField (sp_nse_en, TextDbr.gui_nse_en);
                if (debris.noise.enabled == true)
                {
                    GUICommon.PropertyField (sp_nse_qual, TextDbr.gui_nse_qual);
                    GUICommon.Caption (TextDbr.gui_cap_str);
                    GUICommon.Slider (sp_nse_str_min, nse_str_min, nse_str_max, TextDbr.gui_nse_str_min);
                    GUICommon.Slider (sp_nse_str_max, nse_str_min, nse_str_max, TextDbr.gui_nse_str_max);
                    GUICommon.Caption (TextDbr.gui_cap_other);
                    GUICommon.Slider (sp_nse_freq,   nse_freq_min,   nse_freq_max,   TextDbr.gui_nse_freq);
                    GUICommon.Slider (sp_nse_scroll, nse_scroll_min, nse_scroll_max, TextDbr.gui_nse_scroll);
                    GUICommon.PropertyField (sp_nse_damp, TextDbr.gui_nse_damp);
                }
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        void GUI_Collision()
        {
            GUICommon.Foldout (ref fld_col, TextKeys.deb_fld_col, TextDbr.gui_fld_col.text);
            if (fld_col == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDbr.gui_cap_common);
                GUICommon.MaskField(sp_col_mask, TextDbr.gui_col_mask);
                GUICommon.PropertyField (sp_col_qual, TextDbr.gui_col_qual);
                GUICommon.Slider (sp_col_rad, col_rad_min, col_rad_max, TextDbr.gui_col_rad);
                GUICommon.Caption (TextDbr.gui_cap_dampen);
                GUICommon.PropertyField (sp_col_dmp_tp, TextDbr.gui_col_dmp_tp);
                if (debris.collision.dampenType == RFParticleCollisionDebris.RFParticleCollisionMatType.ByProperties)
                {
                    GUICommon.Slider (sp_col_dmp_min, col_dmp_min, col_dmp_max, TextDbr.gui_col_dmp_min);
                    GUICommon.Slider (sp_col_dmp_max, col_dmp_min, col_dmp_max, TextDbr.gui_col_dmp_max);
                }
                GUICommon.Caption (TextDbr.gui_cap_bounce);
                GUICommon.PropertyField (sp_col_bnc_tp, TextDbr.gui_col_bnc_tp);
                if (debris.collision.bounceType == RFParticleCollisionDebris.RFParticleCollisionMatType.ByProperties)
                {
                    GUICommon.Slider (sp_col_bnc_min, col_bnc_min, col_bnc_max, TextDbr.gui_col_bnc_min);
                    GUICommon.Slider (sp_col_bnc_max, col_bnc_min, col_bnc_max, TextDbr.gui_col_bnc_max);
                }
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////

        void GUI_Limitations()
        {
            GUICommon.Foldout (ref fld_lim, TextKeys.deb_fld_lim, TextDbr.gui_fld_lim.text);
            if (fld_lim == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDbr.gui_cap_prt);
                GUICommon.IntSlider (sp_lim_prt_min, lim_prt_min, lim_prt_max, TextDbr.gui_lim_prt_min);
                GUICommon.IntSlider (sp_lim_prt_max, lim_prt_min, lim_prt_max, TextDbr.gui_lim_prt_max);
                GUICommon.PropertyField (sp_lim_prt_vis, TextDbr.gui_lim_prt_vis);
                GUICommon.Caption (TextDbr.gui_cap_frags);
                GUICommon.IntSlider (sp_lim_perc, lim_perc_min, lim_perc_max, TextDbr.gui_lim_perc);
                GUICommon.Slider (sp_lim_size, lim_size_min, lim_size_max, TextDbr.gui_lim_size);
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Rendering
        /// /////////////////////////////////////////////////////////

        void GUI_Rendering()
        {
            GUICommon.Foldout (ref fld_rnd, TextKeys.deb_fld_rnd, TextDbr.gui_fld_ren.text);
            if (fld_rnd == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Caption (TextDbr.gui_cap_shad);
                GUICommon.PropertyField (sp_ren_cast, TextDbr.gui_ren_cast);
                GUICommon.PropertyField (sp_ren_rec, TextDbr.gui_ren_rec);
                GUICommon.Caption (TextDbr.gui_cap_other);
                GUICommon.PropertyField (sp_ren_prob, TextDbr.gui_ren_prob);
                GUICommon.PropertyField (sp_ren_vect, TextDbr.gui_ren_vect);
                GUICommon.PropertyField (sp_ren_t,    TextDbr.gui_ren_t);
                if (debris.rendering.t == true)
                    GUICommon.TagField (sp_ren_tag, TextDbr.gui_ren_tag);
                GUICommon.PropertyField (sp_ren_l, TextDbr.gui_ren_l);
                if (debris.rendering.l == true)
                    GUICommon.LayerField (sp_ren_lay, TextDbr.gui_ren_lay);
                EditorGUI.indentLevel--;
            }
        }
    }
}