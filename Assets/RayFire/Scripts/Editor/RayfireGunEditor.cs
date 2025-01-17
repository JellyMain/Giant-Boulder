using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireGun))]
    public class RayfireGunEditor : Editor
    {
        RayfireGun   gun;
        
        // Minimum & Maximum ranges
        const float distance_shoot_min = 0.1f;
        const float distance_shoot_max = 100f;
        const int   rounds_min         = 2;
        const int   rounds_max         = 20;
        const float rate_min           = 0.01f;
        const float rate_max           = 5f;
        const float strength_min       = 0f;
        const float strength_max       = 20f;
        const float radius_min         = 0f;
        const float radius_max         = 10f;
        const float offset_min         = -5f;
        const float offset_max         = 5f;
        const float damage_min         = 0;
        const float damage_max         = 100f;
        const float intensity_min      = 0.1f;
        const float intensity_max      = 5f;
        const float range_min          = 0.01f;
        const float range_max          = 10f;
        const float distance_flash_min = 0.01f;
        const float distance_flash_max = 2f;
        
        // Serialized properties
        SerializedProperty sp_dir_show;
        SerializedProperty sp_dir_axis;
        SerializedProperty sp_dir_targ;
        SerializedProperty sp_dir_dist;
        SerializedProperty sp_bur_rnd;
        SerializedProperty sp_bur_rate;
        SerializedProperty sp_imp_show;
        SerializedProperty sp_imp_tp;
        SerializedProperty sp_imp_str;
        SerializedProperty sp_imp_rad;
        SerializedProperty sp_imp_ofs;
        SerializedProperty sp_imp_cls;
        SerializedProperty sp_imp_ina;
        SerializedProperty sp_comp_rg;
        SerializedProperty sp_comp_rt;
        SerializedProperty sp_comp_rb;
        SerializedProperty sp_dmg_val;
        SerializedProperty sp_dmg_shtp;
        SerializedProperty sp_vfx_debris;
        SerializedProperty sp_vfx_dust;
        SerializedProperty sp_vfx_flash;
        SerializedProperty sp_fl_int_min;
        SerializedProperty sp_fl_int_max;
        SerializedProperty sp_fl_rng_min;
        SerializedProperty sp_fl_rng_max;
        SerializedProperty sp_fl_distance;
        SerializedProperty sp_fl_color;
        SerializedProperty sp_tag;
        SerializedProperty sp_mask;
        
        private void OnEnable()
        {
            // Get component
            gun = (RayfireGun)target;
            
            // Set tag list
            GUICommon.SetTags();
            
            // Find properties
            sp_dir_show    = serializedObject.FindProperty(nameof(gun.showRay));
            sp_dir_axis    = serializedObject.FindProperty(nameof(gun.axis));
            sp_dir_targ    = serializedObject.FindProperty(nameof(gun.target));
            sp_dir_dist    = serializedObject.FindProperty(nameof(gun.maxDistance));
            sp_bur_rnd     = serializedObject.FindProperty(nameof(gun.rounds));
            sp_bur_rate    = serializedObject.FindProperty(nameof(gun.rate));
            sp_imp_show    = serializedObject.FindProperty(nameof(gun.showHit));
            sp_imp_tp      = serializedObject.FindProperty(nameof(gun.type));
            sp_imp_str     = serializedObject.FindProperty(nameof(gun.strength));
            sp_imp_rad     = serializedObject.FindProperty(nameof(gun.radius));
            sp_imp_ofs     = serializedObject.FindProperty(nameof(gun.offset));
            sp_imp_cls     = serializedObject.FindProperty(nameof(gun.demolishCluster));
            sp_imp_ina     = serializedObject.FindProperty(nameof(gun.affectInactive));
            sp_comp_rg     = serializedObject.FindProperty(nameof(gun.rigid));
            sp_comp_rt     = serializedObject.FindProperty(nameof(gun.rigidRoot));
            sp_comp_rb     = serializedObject.FindProperty(nameof(gun.rigidBody));
            sp_dmg_val     = serializedObject.FindProperty(nameof(gun.damage));
            sp_dmg_shtp    = serializedObject.FindProperty(nameof(gun.pShardTp));
            sp_vfx_debris  = serializedObject.FindProperty(nameof(gun.debris));
            sp_vfx_dust    = serializedObject.FindProperty(nameof(gun.dust));
            sp_vfx_flash   = serializedObject.FindProperty(nameof(gun.flash));
            sp_fl_int_min  = serializedObject.FindProperty(nameof(gun.Flash) + "." + nameof(gun.Flash.intensityMin));
            sp_fl_int_max  = serializedObject.FindProperty(nameof(gun.Flash) + "." + nameof(gun.Flash.intensityMax));
            sp_fl_rng_min  = serializedObject.FindProperty(nameof(gun.Flash) + "." + nameof(gun.Flash.rangeMin));
            sp_fl_rng_max  = serializedObject.FindProperty(nameof(gun.Flash) + "." + nameof(gun.Flash.rangeMax));
            sp_fl_distance = serializedObject.FindProperty(nameof(gun.Flash) + "." + nameof(gun.Flash.distance));
            sp_fl_color    = serializedObject.FindProperty(nameof(gun.Flash) + "." + nameof(gun.Flash.color));
            sp_tag         = serializedObject.FindProperty(nameof(gun.tagFilter));
            sp_mask        = serializedObject.FindProperty(nameof(gun.mask));
        }

        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();

            GUI_Buttons();
            GUI_Direction();
            GUI_Burst();
            GUI_Impact();
            GUI_Components();
            GUI_Damage();
            GUI_VFX();
            GUI_Filters();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////

        void GUI_Buttons()
        {
            if (Application.isPlaying == true)
            {
                if (GUILayout.Toggle (gun.shooting, TextGun.gui_btn_shooting, "Button", GUILayout.Height (25)) == true)
                    gun.StartShooting();
                else
                    gun.StopShooting();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button (TextGun.gui_btn_shot, GUILayout.Height (22)))
                    foreach (var targ in targets)
                        ((RayfireGun)targ).Shoot();

                if (GUILayout.Button (TextGun.gui_btn_burst, GUILayout.Height (22)))
                    foreach (var targ in targets)
                        ((RayfireGun)targ).Burst();

                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Direction
        /// /////////////////////////////////////////////////////////
        
        void GUI_Direction()
        {
            GUICommon.CaptionBox (TextGun.gui_cap_dir);
            GUICommon.PropertyField (sp_dir_show, TextGun.gui_dir_show);
            GUICommon.PropertyField (sp_dir_axis, TextGun.gui_dir_axis);
            GUICommon.PropertyField (sp_dir_targ, TextGun.gui_dir_targ);
            GUICommon.Slider (sp_dir_dist, distance_shoot_min, distance_shoot_max, TextGun.gui_dir_dist);
        }

        /// /////////////////////////////////////////////////////////
        /// Burst
        /// /////////////////////////////////////////////////////////
        
        void GUI_Burst()
        {
            GUICommon.CaptionBox (TextGun.gui_cap_bur);
            GUICommon.IntSlider (sp_bur_rnd, rounds_min, rounds_max, TextGun.gui_bur_rnd);
            GUICommon.Slider (sp_bur_rate, rate_min, rate_max, TextGun.gui_bur_rate);
        }

        /// /////////////////////////////////////////////////////////
        /// Impact
        /// /////////////////////////////////////////////////////////
        
        void GUI_Impact()
        {
            GUICommon.CaptionBox (TextGun.gui_cap_imp);
            GUICommon.PropertyField (sp_imp_show, TextGun.gui_imp_show);
            GUICommon.PropertyField (sp_imp_tp,   TextGun.gui_imp_tp);
            GUICommon.Slider (sp_imp_str, strength_min, strength_max, TextGun.gui_imp_str);
            GUICommon.Slider (sp_imp_rad, radius_min,   radius_max,   TextGun.gui_imp_rad);
            
            if (gun.type == RayfireGun.ImpactType.AddExplosionForce)
                GUICommon.Slider (sp_imp_ofs, offset_min, offset_max, TextGun.gui_imp_ofs);
            
            GUICommon.PropertyField (sp_imp_ina, TextGun.gui_imp_ina);
            GUICommon.PropertyField (sp_imp_cls, TextGun.gui_imp_cls);
        }

        /// /////////////////////////////////////////////////////////
        /// Components
        /// /////////////////////////////////////////////////////////

        void GUI_Components()
        {
            GUICommon.CaptionBox (TextGun.gui_cap_comp);
            GUICommon.PropertyField (sp_comp_rg, TextGun.gui_comp_rg);
            GUICommon.PropertyField (sp_comp_rt, TextGun.gui_comp_rt);
            GUICommon.PropertyField (sp_comp_rb, TextGun.gui_comp_rb);
        }

        /// /////////////////////////////////////////////////////////
        /// Damage
        /// /////////////////////////////////////////////////////////

        void GUI_Damage()
        {
            GUICommon.CaptionBox (TextGun.gui_cap_dmg);
            GUICommon.Slider (sp_dmg_val, damage_min, damage_max, TextGun.gui_dmg_val);
            GUICommon.PropertyField (sp_dmg_shtp, TextGun.gui_dmg_shtp);
        }

        /// /////////////////////////////////////////////////////////
        /// VFX
        /// /////////////////////////////////////////////////////////

        void GUI_VFX()
        {
            GUICommon.CaptionBox (TextGun.gui_cap_vfx);
            GUICommon.PropertyField (sp_vfx_debris, TextGun.gui_vfx_debris);
            GUICommon.PropertyField (sp_vfx_dust,   TextGun.gui_vfx_dust);
            GUICommon.PropertyField (sp_vfx_flash,  TextGun.gui_vfx_flash);

            if (gun.flash == true)
                GUI_Flash();
        }
        
        void GUI_Flash()
        {
            EditorGUI.indentLevel++;
            GUICommon.Caption (TextGun.gui_cap_int);
            GUICommon.Slider (sp_fl_int_min, intensity_min, intensity_max, TextGun.gui_fl_int_min);
            GUICommon.Slider (sp_fl_int_max, intensity_min, intensity_max, TextGun.gui_fl_int_max);
            GUICommon.Caption (TextGun.gui_cap_rng);
            GUICommon.Slider (sp_fl_rng_min, range_min, range_max, TextGun.gui_fl_rng_min);
            GUICommon.Slider (sp_fl_rng_max, range_min, range_max, TextGun.gui_fl_rng_max);
            GUICommon.Caption (TextGun.gui_cap_other);
            GUICommon.Slider (sp_fl_distance, distance_flash_min, distance_flash_max, TextGun.gui_fl_distance);
            GUICommon.PropertyField (sp_fl_color, TextGun.gui_fl_color);
            EditorGUI.indentLevel--;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void GUI_Filters()
        {
            GUICommon.CaptionBox (TextGun.gui_cap_filt);
            GUICommon.TagField (sp_tag, TextGun.gui_tag);
            GUICommon.MaskField (sp_mask, TextGun.gui_mask);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireGun gun, GizmoType gizmoType)
        {
            // Ray
            if (gun.showRay == true)
            {
                Gizmos.DrawRay (gun.transform.position, gun.ShootVector * gun.maxDistance);
            }

            // Hit
            if (gun.showHit == true)
            {
                RaycastHit hit;
                bool       hitState = Physics.Raycast (gun.transform.position, gun.ShootVector, out hit, gun.maxDistance, gun.mask);
                if (hitState == true)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere (hit.point, gun.radius);
                }
            }
        }
    }
}