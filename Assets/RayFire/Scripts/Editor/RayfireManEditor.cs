using UnityEngine;
using UnityEditor;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireMan))]
    public class RayfireManEditor : Editor
    {
        RayfireMan man;
        Texture2D  logo;
        Texture2D  icon;
        
        // Foldout
        static bool fld_adv;
        static bool fld_mat;


        // Minimum & Maximum ranges
        const float multiplier_min     = 0;
        const float multiplier_max     = 1f;
        const float collider_size_min  = 0;
        const float collider_size_max  = 1f;
        const int   coplanar_verts_min = 0;
        const int   coplanar_verts_max = 999;
        const float minimum_mass_min   = 0.001f;
        const float minimum_mass_max   = 1f;
        const float maximum_mass_min   = 0.1f;
        const float maximum_mass_max   = 4000f;
        const float solidity_min       = 0f;
        const float solidity_max       = 5f;
        const float quota_min          = 0f;
        const float quota_max          = 0.1f;
        const int   bad_min            = 1;
        const int   bad_max            = 10;
        const float shadow_min         = 0;
        const float shadow_max         = 1f;
        const int   frag_cap_min       = 0;
        const int   frag_cap_max       = 10000;
        const int   mat_sol_min        = 0;
        const int   mat_sol_max        = 100;
        const float mat_dens_min       = 0.01f;
        const float mat_dens_max       = 100f;
        const float mat_drag_min       = 0f;
        const float mat_drag_max       = 1f;
        const float mat_ang_min        = 0f;
        const float mat_ang_max        = 1f;
        const float mat_dyn_min        = 0f;
        const float mat_dyn_max        = 1f;

        // Serialized properties
        SerializedProperty sp_phy_set;
        SerializedProperty sp_phy_int;
        SerializedProperty sp_phy_mul;
        SerializedProperty sp_phy_col;
        SerializedProperty sp_phy_cop;
        SerializedProperty sp_phy_cok;
        SerializedProperty sp_col_mesh;
        SerializedProperty sp_col_cls;
        
        SerializedProperty sp_mat_min;
        SerializedProperty sp_mat_max;
        SerializedProperty sp_mat_type;
        SerializedProperty sp_mat_dest;
        SerializedProperty sp_mat_sol;
        SerializedProperty sp_mat_dens;
        SerializedProperty sp_mat_drag;
        SerializedProperty sp_mat_ang;
        SerializedProperty sp_mat_mat;
        SerializedProperty sp_mat_dyn;
        SerializedProperty sp_mat_stat;
        SerializedProperty sp_mat_bnc;
        
        SerializedProperty sp_act_par;
        SerializedProperty sp_dml_sol;
        SerializedProperty sp_dml_time;
        SerializedProperty sp_dml_quota;
        SerializedProperty sp_adv_parent;
        SerializedProperty sp_adv_global;
        SerializedProperty sp_adv_current;
        SerializedProperty sp_adv_amount;
        SerializedProperty sp_adv_bad;
        SerializedProperty sp_adv_size;
        SerializedProperty sp_pol_frg;
        SerializedProperty sp_pol_prt;
        SerializedProperty sp_pol_reu;
        SerializedProperty sp_pol_min;
        SerializedProperty sp_pol_max;
        SerializedProperty sp_dbg_msg;
        SerializedProperty sp_dbg_bld;
        SerializedProperty sp_dbg_edt;
        
        private void OnEnable()
        {
            // Get component
            man = (RayfireMan)target;
            
            // Find properties
            sp_phy_set  = serializedObject.FindProperty(nameof(man.setGravity));
            sp_phy_mul  = serializedObject.FindProperty(nameof(man.multiplier));
            sp_phy_int  = serializedObject.FindProperty(nameof(man.interpolation));
            sp_phy_col  = serializedObject.FindProperty(nameof(man.colliderSize));
            sp_phy_cop  = serializedObject.FindProperty(nameof(man.coplanarVerts));
            sp_phy_cok  = serializedObject.FindProperty(nameof(man.cookingOptions));
            sp_col_mesh = serializedObject.FindProperty(nameof(man.meshCollision));
            sp_col_cls  = serializedObject.FindProperty(nameof(man.clusterCollision));
            
            sp_mat_min  = serializedObject.FindProperty(nameof(man.minimumMass));
            sp_mat_max  = serializedObject.FindProperty(nameof(man.maximumMass));
            sp_mat_type = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.type));
            sp_mat_dest = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.dest));
            sp_mat_sol  = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.sol));
            sp_mat_dens = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.dens));
            sp_mat_drag = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.drag));
            sp_mat_ang  = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.ang));
            sp_mat_mat  = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.mat));
            sp_mat_dyn  = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.dyn));
            sp_mat_stat = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.stat));
            sp_mat_bnc  = serializedObject.FindProperty(nameof(man.materialPresets) + "." + nameof(man.materialPresets.bnc));

            sp_act_par     = serializedObject.FindProperty(nameof(man.parent));
            sp_dml_sol     = serializedObject.FindProperty(nameof(man.globalSolidity));
            sp_dml_time    = serializedObject.FindProperty(nameof(man.timeQuota));
            sp_dml_quota    = serializedObject.FindProperty(nameof(man.quotaAction));
            sp_adv_parent  = serializedObject.FindProperty(nameof(man.advancedDemolitionProperties) + "." + nameof(man.advancedDemolitionProperties.parent));
            sp_adv_global  = serializedObject.FindProperty(nameof(man.advancedDemolitionProperties) + "." + nameof(man.advancedDemolitionProperties.globalParent));
            sp_adv_current = serializedObject.FindProperty(nameof(man.advancedDemolitionProperties) + "." + nameof(man.advancedDemolitionProperties.currentAmount));
            sp_adv_amount  = serializedObject.FindProperty(nameof(man.advancedDemolitionProperties) + "." + nameof(man.advancedDemolitionProperties.maximumAmount));
            sp_adv_bad     = serializedObject.FindProperty(nameof(man.advancedDemolitionProperties) + "." + nameof(man.advancedDemolitionProperties.badMeshTry));
            sp_adv_size    = serializedObject.FindProperty(nameof(man.advancedDemolitionProperties) + "." + nameof(man.advancedDemolitionProperties.sizeThreshold));
            sp_pol_frg     = serializedObject.FindProperty(nameof(man.fragments) + "." + nameof(man.fragments.enable));
            sp_pol_reu     = serializedObject.FindProperty(nameof(man.fragments) + "." + nameof(man.fragments.reuse));
            sp_pol_min     = serializedObject.FindProperty(nameof(man.fragments) + "." + nameof(man.fragments.minCap));
            sp_pol_max     = serializedObject.FindProperty(nameof(man.fragments) + "." + nameof(man.fragments.maxCap));
            sp_pol_prt     = serializedObject.FindProperty(nameof(man.particles) + "." + nameof(man.particles.enable));
            sp_dbg_msg     = serializedObject.FindProperty(nameof(man.debugState));
            sp_dbg_bld     = serializedObject.FindProperty(nameof(man.debugBuild));
            sp_dbg_edt     = serializedObject.FindProperty(nameof(man.debugEditor));
            
            // Set material preset properties
            SetMatToUi ((MaterialType)sp_mat_type.intValue);
            
            // Foldouts
            if (EditorPrefs.HasKey (TextKeys.man_fld_adv) == true) fld_adv = EditorPrefs.GetBool (TextKeys.man_fld_adv);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            GUILayout.Label ("Velocity " + man.velActList.Count, EditorStyles.boldLabel);
            
            // Set new static instance
            if (RayfireMan.inst == null)
                RayfireMan.inst = man;
            
            if (Application.isPlaying == true)
            {
                if (GUILayout.Button (TextMan.gui_btn_dest_frags, GUILayout.Height (20)))
                    RayfireMan.inst.storage.DestroyAll();
                
                GUICommon.Space ();
            }
            
            GUI_Physics();
            GUI_Collision();
            GUI_Materials();
            GUI_Activation();
            UI_Demolition();
            UI_Pooling();
            UI_Info();
            UI_About();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Physics
        /// /////////////////////////////////////////////////////////
        
        void GUI_Physics()
        {
            GUICommon.CaptionBox (TextMan.gui_cap_phy);
            GUICommon.PropertyField (sp_phy_set, TextMan.gui_phy_set);
            if (man.setGravity == true)
                GUICommon.Slider (sp_phy_mul, multiplier_min, multiplier_max, TextMan.gui_phy_mul);
            GUICommon.PropertyField (sp_phy_int, TextMan.gui_phy_int);

            GUICommon.CaptionBox (TextMan.gui_cap_col);
            GUICommon.Slider (sp_phy_col, collider_size_min,  collider_size_max,  TextMan.gui_phy_col);
            GUICommon.IntSlider (sp_phy_cop, coplanar_verts_min, coplanar_verts_max, TextMan.gui_phy_cop);
            GUICommon.PropertyField (sp_phy_cok, TextMan.gui_phy_cok);
        }

        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////
        
        void GUI_Collision()
        {
            GUICommon.CaptionBox (TextMan.gui_cap_det);
            GUICommon.PropertyField (sp_col_mesh, TextMan.gui_col_mesh);
            GUICommon.PropertyField (sp_col_cls,  TextMan.gui_col_cls);
        }

        /// /////////////////////////////////////////////////////////
        /// Materials
        /// /////////////////////////////////////////////////////////
        
        void GUI_Materials()
        {
            GUICommon.CaptionBox (TextMan.gui_cap_mat);
            GUICommon.Slider (sp_mat_min, minimum_mass_min, minimum_mass_max, TextMan.gui_mat_min);
            GUICommon.Slider (sp_mat_max, maximum_mass_min, maximum_mass_max, TextMan.gui_mat_max);
            
            fld_mat = EditorGUILayout.Foldout (fld_mat, TextMan.gui_mat_pres, true);
            if (fld_mat == true)
            {
                GUI_Preset();
                //UI_Materials_Presets();
            }
        }

        void GUI_Preset()
        {
            GUICommon.Space ();
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            GUICommon.PropertyField (sp_mat_type, TextMan.gui_mat_type);
            if (EditorGUI.EndChangeCheck() == true)
                SetMatToUi ((MaterialType)sp_mat_type.intValue);
            EditorGUI.BeginChangeCheck();
            GUICommon.Caption (TextMan.gui_cap_dm);
            GUICommon.PropertyField (sp_mat_dest, TextMan.gui_mat_dest);
            GUICommon.IntSlider (sp_mat_sol, mat_sol_min, mat_sol_max, TextMan.gui_mat_sol);
            GUICommon.Caption (TextMan.gui_cap_rb);
            GUICommon.Slider (sp_mat_dens, mat_dens_min, mat_dens_max, TextMan.gui_mat_dens);
            GUICommon.Slider (sp_mat_drag, mat_drag_min, mat_drag_max, TextMan.gui_mat_drag);
            GUICommon.Slider (sp_mat_ang,  mat_ang_min,  mat_ang_max,  TextMan.gui_mat_ang);
            GUICommon.Caption (TextMan.gui_cap_ph);
            GUICommon.PropertyField (sp_mat_mat, TextMan.gui_mat_mat);
            GUICommon.Slider (sp_mat_dyn,  mat_dyn_min, mat_dyn_max, TextMan.gui_mat_dyn);
            GUICommon.Slider (sp_mat_stat, mat_dyn_min, mat_dyn_max, TextMan.gui_mat_stat);
            GUICommon.Slider (sp_mat_bnc,  mat_dyn_min, mat_dyn_max, TextMan.gui_mat_bnc);
            if (EditorGUI.EndChangeCheck() == true)
                SetUiToMat ((MaterialType)sp_mat_type.intValue);
            EditorGUI.indentLevel--;
        }
        
        void SetUiToMat(MaterialType type)
        {
            switch (type)
            { 
                case MaterialType.Concrete: { SetUiToMat (man.materialPresets.concrete); break; }
                case MaterialType.Brick: { SetUiToMat (man.materialPresets.brick); break; }
                case MaterialType.Glass: { SetUiToMat (man.materialPresets.glass); break; }
                case MaterialType.Rubber: { SetUiToMat (man.materialPresets.rubber); break; }
                case MaterialType.Ice: { SetUiToMat (man.materialPresets.ice); break; }
                case MaterialType.Wood: { SetUiToMat (man.materialPresets.wood); break; }
                case MaterialType.HeavyMetal: { SetUiToMat (man.materialPresets.heavyMetal); break; }
                case MaterialType.LightMetal: { SetUiToMat (man.materialPresets.lightMetal); break; }
                case MaterialType.DenseRock: { SetUiToMat (man.materialPresets.denseRock); break; }
                case MaterialType.PorousRock: { SetUiToMat (man.materialPresets.porousRock); break; }
            }
        }

        void SetUiToMat (RFMaterial mat)
        {
            mat.destructible    = sp_mat_dest.boolValue;
            mat.solidity        = sp_mat_sol.intValue;
            mat.density         = sp_mat_dens.floatValue;
            mat.drag            = sp_mat_drag.floatValue;
            mat.angularDrag     = sp_mat_ang.floatValue;
            mat.material        = (PhysicMaterial)sp_mat_mat.objectReferenceValue;
            mat.dynamicFriction = sp_mat_dyn.floatValue;
            mat.staticFriction  = sp_mat_stat.floatValue;
            mat.bounciness      = sp_mat_bnc.floatValue;
        }

        void SetMatToUi (MaterialType type)
        {
            switch (type)
            { 
                case MaterialType.Concrete: { SetMatToUi (man.materialPresets.concrete); break; }
                case MaterialType.Brick: { SetMatToUi (man.materialPresets.brick); break; }
                case MaterialType.Glass: { SetMatToUi (man.materialPresets.glass); break; }
                case MaterialType.Rubber: { SetMatToUi (man.materialPresets.rubber); break; }
                case MaterialType.Ice: { SetMatToUi (man.materialPresets.ice); break; }
                case MaterialType.Wood: { SetMatToUi (man.materialPresets.wood); break; }
                case MaterialType.HeavyMetal: { SetMatToUi (man.materialPresets.heavyMetal); break; }
                case MaterialType.LightMetal: { SetMatToUi (man.materialPresets.lightMetal); break; }
                case MaterialType.DenseRock: { SetMatToUi (man.materialPresets.denseRock); break; }
                case MaterialType.PorousRock: { SetMatToUi (man.materialPresets.porousRock); break; }
            }
        }

        void SetMatToUi (RFMaterial mat)
        {
            sp_mat_dest.boolValue           = mat.destructible;
            sp_mat_sol.intValue             = mat.solidity;
            sp_mat_dens.floatValue          = mat.density;
            sp_mat_drag.floatValue          = mat.drag;
            sp_mat_ang.floatValue           = mat.angularDrag;
            sp_mat_mat.objectReferenceValue = mat.material;
            sp_mat_dyn.floatValue           = mat.dynamicFriction;
            sp_mat_stat.floatValue          = mat.staticFriction;
            sp_mat_bnc.floatValue           = mat.bounciness;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Activation
        /// /////////////////////////////////////////////////////////

        void GUI_Activation()
        {
            GUICommon.CaptionBox (TextMan.gui_cap_axt);
            GUICommon.PropertyField (sp_act_par, TextMan.gui_act_par);
        }

        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////
        
        void UI_Demolition()
        {
            GUICommon.CaptionBox (TextMan.gui_cap_dml);
            GUICommon.Slider (sp_dml_sol,  solidity_min, solidity_max, TextMan.gui_dml_sol);
            GUICommon.Slider (sp_dml_time, quota_min,    quota_max, TextMan.gui_dml_time);
            if (sp_dml_time.floatValue > 0)
                GUICommon.PropertyField (sp_dml_quota, TextMan.gui_dml_quota);
            UI_Demolition_Adv();
        }

        void UI_Demolition_Adv()
        {
            GUICommon.Foldout (ref fld_adv, TextKeys.man_fld_adv, TextMan.gui_adv_expand.text);
            if (fld_adv == true)
            {
                EditorGUI.indentLevel++;

                GUICommon.Caption (TextMan.gui_cap_frg);
                GUICommon.PropertyField (sp_adv_parent, TextMan.gui_adv_parent);
                if (man.advancedDemolitionProperties.parent == FragmentParentType.GlobalParent)
                    GUICommon.PropertyField (sp_adv_global, TextMan.gui_adv_global);
                GUICommon.PropertyField (sp_adv_current, TextMan.gui_adv_current);
                GUICommon.PropertyField (sp_adv_amount,  TextMan.gui_adv_amount);
                GUICommon.IntSlider (sp_adv_bad, bad_min, bad_max, TextMan.gui_adv_bad);
   
                GUICommon.Caption (TextMan.gui_cap_shad);
                GUICommon.Slider (sp_adv_size, shadow_min, shadow_max, TextMan.gui_adv_size);

                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Pooling
        /// /////////////////////////////////////////////////////////
        
        void UI_Pooling()
        {
            GUICommon.CaptionBox (TextMan.gui_cap_pol);
            GUICommon.PropertyField (sp_pol_frg, TextMan.gui_pol_frg);
            if (man.fragments.enable == true)
            {
                GUICommon.PropertyField (sp_pol_reu, TextMan.gui_pol_reu);
                GUICommon.IntSlider (sp_pol_min, frag_cap_min, frag_cap_max, TextMan.gui_pol_min);
                if (man.fragments.reuse == true)
                    GUICommon.IntSlider (sp_pol_max, frag_cap_min, frag_cap_max, TextMan.gui_pol_max);
            }

            EditorGUI.BeginChangeCheck();
            GUICommon.PropertyField (sp_pol_prt, TextMan.gui_pol_prt);
            if (EditorGUI.EndChangeCheck() == true)
                man.particles.Enable = man.particles.enable;
            
            // Info
            if (man.particles.enable == true)
            {
                if (man.particles.emitters != null && man.particles.emitters.Count > 0)
                {
                    GUICommon.Space ();
                    GUILayout.Label (TextMan.str_prt_emit + man.particles.emitters.Count,        EditorStyles.boldLabel);
                    GUICommon.Space ();
                    GUILayout.Label (TextMan.str_prt_amount+ man.particles.GetTotalPoolAmount(), EditorStyles.boldLabel);
                    GUICommon.Space ();
                    GUILayout.Label (TextMan.str_prt_reu + man.particles.reused,                  EditorStyles.boldLabel);
                    GUICommon.Space ();
                    GUILayout.Label (TextMan.str_prt_scene + man.particles.ResetCheck(),          EditorStyles.boldLabel);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Info
        /// /////////////////////////////////////////////////////////

        void UI_Info()
        {
            if (Application.isPlaying == true)
            {
                GUICommon.Space ();
                GUILayout.Label (TextMan.str_info, EditorStyles.boldLabel);
                GUICommon.Space ();
                
                if (man.fragments.enable == true && man.fragments.queue.Count > 0)
                    GUILayout.Label (TextMan.str_rigs+ man.fragments.queue.Count);
                
                GUICommon.Space ();
                
                if (man.advancedDemolitionProperties.currentAmount > 0)
                    GUILayout.Label (TextMan.str_frags+ man.advancedDemolitionProperties.currentAmount + "/" + man.advancedDemolitionProperties.maximumAmount);

                GUICommon.Space ();

                if (man.physicList != null && man.physicList.Count > 0)
                    GUILayout.Label (TextMan.str_vel+ man.physicList.Count);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// About
        /// /////////////////////////////////////////////////////////
        
        void UI_About()
        {
            GUICommon.CaptionBox (TextMan.gui_cap_abt);
            GUICommon.PropertyField (sp_dbg_msg, TextMan.gui_dbg_msg);
            if (man.debugState == true)
            {
                GUICommon.PropertyField (sp_dbg_edt, TextMan.gui_dbg_edt);
                GUICommon.PropertyField (sp_dbg_bld, TextMan.gui_dbg_bld);
            }
            
            GUICommon.Space ();
            GUILayout.Label (TextMan.str_build + RayfireMan.buildMajor + '.' + RayfireMan.buildMinor.ToString ("D2"));
            GUICommon.Space ();

            // Logo TODO remove if component removed
            if (logo == null)
                logo = (Texture2D)AssetDatabase.LoadAssetAtPath ("Assets/RayFire/Info/Logo/logo_small.png", typeof(Texture2D));
            if (logo != null)
                GUILayout.Box (logo, GUILayout.Width ((int)EditorGUIUtility.currentViewWidth - 19f), GUILayout.Height (64));
            
            if (GUILayout.Button (TextMan.gui_change, GUILayout.Height (20)))
                Application.OpenURL (TextMan.str_url);
        }
    }
}