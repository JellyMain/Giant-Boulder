using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireRigidRoot))]
    public class RayfireRigidRootEditor : Editor
    {
        RayfireRigidRoot root;
        
        // Foldout
        static bool fld_phy;
        static bool fld_act;
        static bool fld_lim;
        static bool fld_cls;
        static bool fld_clp;
        static bool fld_fad;
        static bool fld_res;
        
        // Physic Minimum & Maximum ranges
        const int   solver_min = 1;
        const int   solver_max = 20;
        const float sleep_min  = 0.001f;
        const float sleep_max  = 0.1f;
        const float dampen_min = 0f;
        const float dampen_max = 1f;
        
        // Activation Minimum & Maximum ranges
        const float offset_min     = 0;
        const float offset_max     = 10f;
        const float velocity_min   = 0;
        const float velocity_max   = 5f;

        // Limitation Minimum & Maximum ranges
        const float solidity_min = 0;
        const float solidity_max = 10f;
        const int   depth_min    = 0;
        const int   depth_max    = 7;
        const float time_min     = 0.05f;
        const float time_max     = 10f;
        const float size_dml_min = 0.01f;
        const float size_dml_max = 50f;
        
        // Cluster Minimum & Maximum ranges
        const float cls_flt_area_min   = 0f;
        const float cls_flt_area_max   = 1f;
        const float cls_flt_size_min   = 0;
        const float cls_flt_size_max   = 10f;
        const int   cls_flt_perc_min   = 0;
        const int   cls_flt_perc_max   = 100;
        const int   cls_flt_seed_min   = 0;
        const int   cls_flt_seed_max   = 100;
        const int   cls_ratio_min      = 1;
        const int   cls_ratio_max      = 100;
        const float cls_units_min      = 0;
        const float cls_units_max      = 10f;
        const int   cls_shard_area_min = 0;
        const int   cls_shard_area_max = 100;
        const int   cls_clusters_min   = 0;
        const int   cls_clusters_max   = 40;
        
        // Collapse Minimum & Maximum ranges
        const int   clp_start_min    = 0;
        const int   clp_start_max    = 99;
        const int   clp_end_min      = 1;
        const int   clp_end_max      = 100;
        const int   clp_steps_min    = 1;
        const int   clp_steps_max    = 100;
        const float clp_duration_min = 0;
        const float clp_duration_max = 60f;
        const int   clp_var_min      = 0;
        const int   clp_var_max      = 100;
        const int   clp_seed_min     = 0;
        const int   clp_seed_max     = 99;

        // Fade Minimum & Maximum ranges
        const float fade_offset_min = 0;
        const float fade_offset_max = 20f;
        const float fade_time_min   = 1f;
        const float fade_time_max   = 30f;
        const float fade_life_min   = 0f;
        const float fade_life_max   = 90f;
        const float fade_size_min   = 0f;
        const float fade_size_max   = 20f;
        const int   fade_shards_min = 0;
        const int   fade_shards_max = 50;
        
        // Main Serialized properties
        SerializedProperty sp_mn_ini;
        SerializedProperty sp_mn_sim;
        
        // Physic Serialized properties
        SerializedProperty sp_phy_mtp;
        SerializedProperty sp_phy_mat;
        SerializedProperty sp_phy_mby;
        SerializedProperty sp_phy_mss;
        SerializedProperty sp_phy_ctp;
        SerializedProperty sp_phy_pln;
        SerializedProperty sp_phy_ign;
        SerializedProperty sp_phy_grv;
        SerializedProperty sp_phy_slv;
        SerializedProperty sp_phy_slt;
        SerializedProperty sp_phy_dmp;
        
        // Activation Serialized properties
        SerializedProperty sp_act_off;
        SerializedProperty sp_act_loc;
        SerializedProperty sp_act_vel;
        //SerializedProperty sp_act_dmg;
        SerializedProperty sp_act_act;
        SerializedProperty sp_act_imp;
        SerializedProperty sp_act_con;
        SerializedProperty sp_act_uny;
        SerializedProperty sp_act_atb;
        SerializedProperty sp_act_l;
        SerializedProperty sp_act_lay;
        
        // limitations Serialized properties
        SerializedProperty sp_lim_col;
        SerializedProperty sp_lim_sol;
        SerializedProperty sp_lim_dep;
        SerializedProperty sp_lim_tim;
        SerializedProperty sp_lim_siz;
        SerializedProperty sp_lim_vis;
        SerializedProperty sp_lim_bld;

        // Cluster Demolition Serialized properties
        SerializedProperty sp_cls_cnt;
        SerializedProperty sp_cls_sim;
        SerializedProperty sp_cls_fl_ar;
        SerializedProperty sp_cls_fl_sz;
        SerializedProperty sp_cls_fl_pr;
        SerializedProperty sp_cls_fl_sd;
        SerializedProperty sp_cls_ds_tp;
        SerializedProperty sp_cls_ds_rt;
        SerializedProperty sp_cls_ds_un;
        SerializedProperty sp_cls_sh_ar;
        SerializedProperty sp_cls_sh_dm;
        SerializedProperty sp_cls_min;
        SerializedProperty sp_cls_max;
        SerializedProperty sp_cls_dml;
        
        // Collapse Serialized properties
        SerializedProperty sp_clp_type;
        SerializedProperty sp_clp_start;
        SerializedProperty sp_clp_end;
        SerializedProperty sp_clp_steps;
        SerializedProperty sp_clp_dur;
        SerializedProperty sp_clp_var;
        SerializedProperty sp_clp_seed;
        
        // Fade Serialized properties
        SerializedProperty sp_fad_dml;
        SerializedProperty sp_fad_act;
        SerializedProperty sp_fad_ofs;
        SerializedProperty sp_fad_tp;
        SerializedProperty sp_fad_tm;
        SerializedProperty sp_fad_lf_tp;
        SerializedProperty sp_fad_lf_tm;
        SerializedProperty sp_fad_lf_vr;
        SerializedProperty sp_fad_sz;
        SerializedProperty sp_fad_sh;
        
        // Reset Serialized properties
        SerializedProperty sp_res_tm;
        SerializedProperty sp_res_dm;
        SerializedProperty sp_res_cn;

        private void OnEnable()
        {
            // Get component
            root = (RayfireRigidRoot)target;
            
             // Find Main properties
            sp_mn_ini = serializedObject.FindProperty(nameof(root.initialization));
            sp_mn_sim = serializedObject.FindProperty(nameof(root.simTp));

            // Find Physic properties
            sp_phy_mtp = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.mt));
            sp_phy_mat = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.ma));
            sp_phy_mby = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.mb));
            sp_phy_mss = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.ms));
            sp_phy_ctp = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.ct));
            sp_phy_pln = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.pc));
            sp_phy_ign = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.ine));
            sp_phy_grv = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.gr));
            sp_phy_slv = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.si));
            sp_phy_slt = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.st));
            sp_phy_dmp = serializedObject.FindProperty(nameof(root.physics) + "." + nameof(root.physics.dm));
            
            // Find Activation properties
            sp_act_off = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.off));
            sp_act_loc = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.loc));
            sp_act_vel = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.vel));
            //sp_act_dmg = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.dmg));
            sp_act_act = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.act));
            sp_act_imp = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.imp));
            sp_act_con = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.con));
            sp_act_uny = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.uny));
            sp_act_atb = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.atb));
            sp_act_l   = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.l));
            sp_act_lay = serializedObject.FindProperty(nameof(root.activation) + "." + nameof(root.activation.lay));
            
            // Find limitations properties
            sp_lim_col = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.limitations) + "." + nameof(root.dml.limitations.col));
            sp_lim_sol = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.limitations) + "." + nameof(root.dml.limitations.sol));
            sp_lim_dep = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.limitations) + "." + nameof(root.dml.limitations.depth));
            sp_lim_tim = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.limitations) + "." + nameof(root.dml.limitations.time));
            sp_lim_siz = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.limitations) + "." + nameof(root.dml.limitations.size));
            sp_lim_vis = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.limitations) + "." + nameof(root.dml.limitations.vis));
            sp_lim_bld = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.limitations) + "." + nameof(root.dml.limitations.bld));
            
            // Find Cluster Demolition properties
            sp_cls_cnt   = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.cnt));
            sp_cls_sim   = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.sim));
            sp_cls_fl_ar = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.mAr));
            sp_cls_fl_sz = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.mSz));
            sp_cls_fl_pr = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.pct));
            sp_cls_fl_sd = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.seed));
            sp_cls_ds_tp = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.type));
            sp_cls_ds_rt = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.ratio));
            sp_cls_ds_un = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.units));
            sp_cls_sh_ar = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.sAr));
            sp_cls_sh_dm = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.sDm));
            sp_cls_min   = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.mnAm));
            sp_cls_max   = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.mxAm));
            sp_cls_dml   = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.cDm));
            
            // Find Collapse properties
            sp_clp_type  = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.collapse) + "." + nameof(root.dml.clsDemol.collapse.type));
            sp_clp_start = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.collapse) + "." + nameof(root.dml.clsDemol.collapse.start));
            sp_clp_end   = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.collapse) + "." + nameof(root.dml.clsDemol.collapse.end));
            sp_clp_steps = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.collapse) + "." + nameof(root.dml.clsDemol.collapse.steps));
            sp_clp_dur   = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.collapse) + "." + nameof(root.dml.clsDemol.collapse.duration));
            sp_clp_var   = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.collapse) + "." + nameof(root.dml.clsDemol.collapse.var));
            sp_clp_seed  = serializedObject.FindProperty(nameof(root.dml) + "." + nameof(root.dml.clsDemol) + "." + nameof(root.dml.clsDemol.collapse) + "." + nameof(root.dml.clsDemol.collapse.seed));
            
            // Find Fade properties
            sp_fad_dml   = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.onDemolition));
            sp_fad_act   = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.onActivation));
            sp_fad_ofs   = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.byOffset));
            sp_fad_tp    = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.fadeType));
            sp_fad_tm    = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.fadeTime));
            sp_fad_lf_tp = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.lifeType));
            sp_fad_lf_tm = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.lifeTime));
            sp_fad_lf_vr = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.lifeVariation));
            sp_fad_sz    = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.sizeFilter));
            sp_fad_sh    = serializedObject.FindProperty(nameof(root.fading) + "." + nameof(root.fading.shardAmount));
            
            // Reset Serialized properties
            sp_res_tm = serializedObject.FindProperty(nameof(root.reset) + "." + nameof(root.reset.transform));
            sp_res_dm = serializedObject.FindProperty(nameof(root.reset) + "." + nameof(root.reset.damage));
            sp_res_cn = serializedObject.FindProperty(nameof(root.reset) + "." + nameof(root.reset.connectivity));
            
            // Foldout
            if (EditorPrefs.HasKey (TextKeys.rot_fld_phy) == true) fld_phy = EditorPrefs.GetBool (TextKeys.rot_fld_phy);
            if (EditorPrefs.HasKey (TextKeys.rot_fld_act) == true) fld_act = EditorPrefs.GetBool (TextKeys.rot_fld_act);
            if (EditorPrefs.HasKey (TextKeys.rot_fld_lim) == true) fld_lim = EditorPrefs.GetBool (TextKeys.rot_fld_lim);
            if (EditorPrefs.HasKey (TextKeys.rot_fld_cls) == true) fld_cls = EditorPrefs.GetBool (TextKeys.rot_fld_cls);
            if (EditorPrefs.HasKey (TextKeys.rot_fld_clp) == true) fld_clp = EditorPrefs.GetBool (TextKeys.rot_fld_clp);
            if (EditorPrefs.HasKey (TextKeys.rot_fld_fad) == true) fld_fad = EditorPrefs.GetBool (TextKeys.rot_fld_fad);
            if (EditorPrefs.HasKey (TextKeys.rot_fld_res) == true) fld_res = EditorPrefs.GetBool (TextKeys.rot_fld_res);
        }

        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            GUICommon.Space ();
            GUICommon.Space ();
            GUI_Buttons();
            GUI_Info();
            GUICommon.Space ();
            
            GUICommon.CaptionBox (TextRig.gui_cap_mn);
            GUICommon.Space ();
            GUICommon.PropertyField (sp_mn_ini, TextRig.gui_mn_ini);
            GUI_Simulation();
            GUICommon.Space ();
            GUI_Demolition();
            
            GUICommon.CaptionBox (TextRig.gui_cap_com);
            GUI_Fade();
            GUICommon.Space ();
            GUI_Reset();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Simulation
        /// /////////////////////////////////////////////////////////

        void GUI_Simulation()
        {
            GUICommon.CaptionBox (TextRig.gui_cap_sim);
            GUICommon.Space ();
            GUICommon.PropertyField (sp_mn_sim, TextRig.gui_mn_sim);
            GUI_Physic();
            GUICommon.Space ();
            GUI_Activation();
        }
        
        void GUI_Physic()
        {
            GUICommon.Foldout (ref fld_phy, TextKeys.rot_fld_phy, TextPhy.gui_phy.text);
            if (fld_phy == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.PropertyField (sp_phy_mtp, TextPhy.gui_phy_mtp);
                GUICommon.PropertyField (sp_phy_mat, TextPhy.gui_phy_mat);

                GUICommon.Caption (TextPhy.gui_cap_mas);
                GUICommon.PropertyField (sp_phy_mby, TextPhy.gui_phy_mby);
                if (root.physics.mb == MassType.MassProperty)
                    GUICommon.PropertyField (sp_phy_mss, TextPhy.gui_phy_mss);

                GUICommon.Caption (TextPhy.gui_cap_col);
                GUICommon.PropertyField (sp_phy_ctp, TextPhy.gui_phy_ctp);
                GUICommon.PropertyField (sp_phy_pln, TextPhy.gui_phy_pln);
                GUICommon.PropertyField (sp_phy_ign, TextPhy.gui_phy_ign);
                    
                GUICommon.Caption (TextPhy.gui_cap_oth);
                GUICommon.PropertyField (sp_phy_grv, TextPhy.gui_phy_grv);
                    
                GUICommon.Space ();
                
                GUICommon.IntSlider (sp_phy_slv, solver_min, solver_max, TextPhy.gui_phy_slv);
                GUICommon.Slider (sp_phy_slt, sleep_min, sleep_max, TextPhy.gui_phy_slt);
                
                GUICommon.Caption (TextPhy.gui_cap_frg);
                GUICommon.Slider (sp_phy_dmp, dampen_min, dampen_max, TextPhy.gui_phy_dmp);
                
                EditorGUI.indentLevel--;
            }
        }
        
        void GUI_Activation()
        {
            if (ActivatableState() == false) 
                return;
            
            GUICommon.Foldout (ref fld_act, TextKeys.rot_fld_act, TextAcv.gui_act.text);
            if (fld_act == true)
            {
                EditorGUI.indentLevel++;
                
                GUICommon.Caption (TextAcv.gui_cap_act);
                GUICommon.Slider (sp_act_off, offset_min, offset_max, TextAcv.gui_act_off);
                if (root.activation.off > 0)
                    GUICommon.PropertyField (sp_act_loc, TextAcv.gui_act_loc);
                GUICommon.Slider (sp_act_vel, velocity_min,   velocity_max,   TextAcv.gui_act_vel);
                //GUICommon.Slider (sp_act_dmg, damage_act_min, damage_act_max, TextAcv.gui_act_dmg);
                GUICommon.PropertyField (sp_act_act, TextAcv.gui_act_act);
                GUICommon.PropertyField (sp_act_imp, TextAcv.gui_act_imp);
                GUICommon.PropertyField (sp_act_con, TextAcv.gui_act_con);
                if (root.activation.con == true)
                {
                    GUICommon.PropertyField (sp_act_uny, TextAcv.gui_act_uny);
                    GUICommon.PropertyField (sp_act_atb, TextAcv.gui_act_atb);
                }
                
                GUICommon.Caption (TextAcv.gui_cap_pst);
                GUICommon.PropertyField (sp_act_l, TextAcv.gui_act_l);
                if (sp_act_l.boolValue == true)
                    GUICommon.LayerField (sp_act_lay, TextAcv.gui_act_lay);
                
                EditorGUI.indentLevel--;
            }
        }
        
        bool ActivatableState()
        {
            foreach (RayfireRigidRoot scr in targets)
                if (ActivatableState(scr) == true)
                    return true;
            return false;
        }
        
        static bool ActivatableState(RayfireRigidRoot scr)
        {
            if (scr.simTp == SimType.Inactive || scr.simTp == SimType.Kinematic)
                return true;
            return false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        void GUI_Demolition()
        {
            if (DemolitionState() == false)
                return;
            
            GUICommon.Caption (TextRig.gui_cap_dml);
            GUICommon.Space ();
            GUI_Limitations();
            GUICommon.Space ();
            GUI_Cluster();
            GUICommon.Space ();
        }
        
        bool DemolitionState()
        {
            foreach (RayfireRigidRoot scr in targets)
                if (DemolitionState(scr) == true)
                    return true;
            return false;
        }

        static bool DemolitionState(RayfireRigidRoot scr)
        {
            if (scr.simTp == SimType.Inactive || scr.simTp == SimType.Kinematic)
                if (scr.activation.con == true)
                    return true;
            return false;
        }

        /// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////

        void GUI_Limitations()
        {
            GUICommon.Foldout (ref fld_lim, TextKeys.rot_fld_lim, TextLim.gui_lim.text);
            if (fld_lim == true)
            {
                EditorGUI.indentLevel++;
                
                GUICommon.Caption (TextLim.gui_cap_col);
                GUICommon.PropertyField (sp_lim_col, TextLim.gui_lim_col);
                if (root.dml.limitations.col == true)
                    GUICommon.Slider (sp_lim_sol, solidity_min, solidity_max, TextLim.gui_lim_sol);
                    
                EditorGUI.BeginChangeCheck();
                string tag = EditorGUILayout.TagField (TextLim.gui_lim_tag, root.dml.limitations.tag);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects (targets, TextLim.gui_lim_tag.text);
                    foreach (RayfireRigidRoot scr in targets)
                    {
                        scr.dml.limitations.tag = tag;
                        SetDirty (scr);
                    }
                }
                
                GUICommon.Caption (TextLim.gui_cap_oth);
                GUICommon.IntSlider (sp_lim_dep, depth_min, depth_max, TextLim.gui_lim_dep);
                GUICommon.Slider (sp_lim_tim, time_min,     time_max,     TextLim.gui_lim_tim);
                GUICommon.Slider (sp_lim_siz, size_dml_min, size_dml_max, TextLim.gui_lim_siz);
                GUICommon.PropertyField (sp_lim_vis, TextLim.gui_lim_vis);
                GUICommon.PropertyField (sp_lim_bld, TextLim.gui_lim_bld);
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Cluster
        /// /////////////////////////////////////////////////////////

        void GUI_Cluster()
        {
            GUICommon.Foldout (ref fld_cls, TextKeys.rot_fld_cls, TextCls.gui_cls.text);
            if (fld_cls == true)
            {
                EditorGUI.indentLevel++;
                
                GUICommon.Caption (TextCls.gui_cap_prp);
                GUICommon.PropertyField (sp_cls_cnt, TextCls.gui_cls_cnt);
                GUICommon.PropertyField (sp_cls_sim, TextCls.gui_cls_sim);
                
                GUICommon.Caption (TextCls.gui_cap_flt);
                if (root.dml.clsDemol.cnt != ConnectivityType.ByBoundingBox)
                    GUICommon.Slider (sp_cls_fl_ar, cls_flt_area_min, cls_flt_area_max, TextCls.gui_cls_fl_ar);
                GUICommon.Slider (sp_cls_fl_sz, cls_flt_size_min, cls_flt_size_max, TextCls.gui_cls_fl_sz);
                GUICommon.IntSlider(sp_cls_fl_pr,  cls_flt_perc_min, cls_flt_perc_max, TextCls.gui_cls_fl_pr);
                GUICommon.IntSlider (sp_cls_fl_sd, cls_flt_seed_min, cls_flt_seed_max, TextCls.gui_cls_fl_sd);

                GUICommon.Caption (TextCls.gui_cap_dml);
                GUICommon.PropertyField (sp_cls_ds_tp, TextCls.gui_cls_ds_tp);
                if (root.dml.clsDemol.type == RFDemolitionCluster.RFDetachType.RatioToSize)
                    GUICommon.IntSlider (sp_cls_ds_rt, cls_ratio_min, cls_ratio_max, TextCls.gui_cls_ds_rt);
                else
                    GUICommon.Slider (sp_cls_ds_un, cls_units_min, cls_units_max, TextCls.gui_cls_ds_un);
                
                GUICommon.Caption (TextCls.gui_cap_shd);
                GUICommon.IntSlider (sp_cls_sh_ar, cls_shard_area_min, cls_shard_area_max, TextCls.gui_cls_sh_ar);
                GUICommon.PropertyField (sp_cls_sh_dm, TextCls.gui_cls_sh_dm);
                
                GUICommon.Caption (TextCls.gui_cap_cls);
                GUICommon.IntSlider(sp_cls_min, cls_clusters_min, cls_clusters_max, TextCls.gui_cls_min);
                GUICommon.IntSlider(sp_cls_max, cls_clusters_min, cls_clusters_max, TextCls.gui_cls_max);
                GUICommon.PropertyField (sp_cls_dml, TextCls.gui_cls_dml);
                
                UI_Collapse();
                
                EditorGUI.indentLevel--;
            }
        }
        
        void UI_Collapse()
        {
            GUICommon.Caption (TextCls.gui_cap_clp);
            GUICommon.Foldout (ref fld_clp, TextKeys.rot_fld_clp, TextCls.gui_cls_prp.text);
            if (fld_clp == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.PropertyField (sp_clp_type, TextClp.gui_type);
                GUICommon.IntSlider (sp_clp_start, clp_start_min, clp_start_max, TextClp.gui_start);
                GUICommon.IntSlider (sp_clp_end,   clp_end_min,   clp_end_max,   TextClp.gui_end);
                GUICommon.IntSlider (sp_clp_steps, clp_steps_min, clp_steps_max, TextClp.gui_steps);
                GUICommon.Slider (sp_clp_dur, clp_duration_min, clp_duration_max, TextClp.gui_duration);
                if (root.dml.clsDemol.collapse.type != RFCollapse.RFCollapseType.Random)
                    GUICommon.IntSlider (sp_clp_var, clp_var_min, clp_var_max, TextClp.gui_var);
                GUICommon.IntSlider (sp_clp_seed, clp_seed_min, clp_seed_max, TextClp.gui_seed);
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Fade
        /// /////////////////////////////////////////////////////////

        void GUI_Fade()
        {
            GUICommon.Foldout (ref fld_fad, TextKeys.rot_fld_fad, TextFad.gui_fad.text);
            if (fld_fad == true)
            {
                EditorGUI.indentLevel++;

                GUICommon.Caption (TextFad.gui_cap_ini);
                GUICommon.PropertyField (sp_fad_dml, TextFad.gui_fad_dml);
                GUICommon.PropertyField (sp_fad_act, TextFad.gui_fad_act);
                GUICommon.Slider (sp_fad_ofs, fade_offset_min, fade_offset_max, TextFad.gui_fad_ofs);
                
                GUICommon.Caption (TextFad.gui_cap_tp);
                GUICommon.PropertyField (sp_fad_tp, TextFad.gui_fad_tp);
                GUICommon.Slider (sp_fad_tm, fade_time_min, fade_time_max, TextFad.gui_fad_tm);
                
                GUICommon.Caption (TextFad.gui_cap_lf);
                GUICommon.PropertyField (sp_fad_lf_tp, TextFad.gui_fad_lf_tp);
                GUICommon.Slider (sp_fad_lf_tm, fade_life_min, fade_life_max, TextFad.gui_fad_lf_tm);
                GUICommon.Slider (sp_fad_lf_vr, fade_life_min, fade_life_max, TextFad.gui_fad_lf_vr);
                
                GUICommon.Caption (TextFad.gui_cap_flt);
                GUICommon.Slider (sp_fad_sz, fade_size_min, fade_size_max, TextFad.gui_fad_sz);
                GUICommon.IntSlider (sp_fad_sh, fade_shards_min, fade_shards_max, TextFad.gui_fad_sh);

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Reset
        /// /////////////////////////////////////////////////////////

        void GUI_Reset()
        {
            GUICommon.Foldout (ref fld_res, TextKeys.rot_fld_res, TextRes.gui_res.text);

            if (fld_res == true )
            {
                EditorGUI.indentLevel++;
                
                GUICommon.Caption (TextRes.gui_cap_res);
                GUICommon.PropertyField (sp_res_tm, TextRes.gui_res_tm);
                GUICommon.PropertyField (sp_res_dm, TextRes.gui_res_dm);
                GUICommon.PropertyField (sp_res_cn, TextRes.gui_res_cn);

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons Info
        /// /////////////////////////////////////////////////////////

        void GUI_Buttons()
        {
            // Initialize
            if (Application.isPlaying == true)
            {
                if (root.initialized == false)
                {
                    if (GUILayout.Button (TextRig.gui_btn_init, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireRigidRoot != null)
                                if ((targ as RayfireRigidRoot).initialized == false)
                                    (targ as RayfireRigidRoot).Initialize();
                }
                
                // Reuse
                else
                {
                    if (GUILayout.Button ("Reset Rigid Root", GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireRigidRoot != null)
                                if ((targ as RayfireRigidRoot).initialized == true)
                                    (targ as RayfireRigidRoot).ResetRigidRoot();
                }
                GUICommon.Space ();
            }
            
            RigidRootSetupUI();
        }

        void RigidRootSetupUI()
        {
            if (Application.isPlaying == false)
            {
                GUILayout.Space (2);
                GUILayout.BeginHorizontal();

                if (root.cluster.shards.Count == 0)
                    if (GUILayout.Button (TextRig.gui_btn_edt_setup, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireRigidRoot != null)
                            {
                                (targ as RayfireRigidRoot).EditorSetup();
                                SetDirty (targ as RayfireRigidRoot);
                            }
                    
                if (root.cluster.shards.Count > 0)
                    if (GUILayout.Button (TextRig.gui_btn_edt_reset, GUILayout.Height (25)))
                        foreach (var targ in targets)
                            if (targ as RayfireRigidRoot != null)
                            {
                                //RFPhysic.DestroyColliders (targ as RayfireRigidRoot);
                                (targ as RayfireRigidRoot).ResetSetup();
                                SetDirty (targ as RayfireRigidRoot);
                            }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space (2);
            }
        }
        
        void GUI_Info()
        {
            if (root.cluster.shards.Count > 0)
                GUILayout.Label (TextRig.str_cls_shards + root.cluster.shards.Count);

            if (root.physics.HasIgnore == true)
                GUILayout.Label (TextRig.str_ignore + root.physics.ign.Count / 2);
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty(RayfireRigidRoot scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
    }
}

