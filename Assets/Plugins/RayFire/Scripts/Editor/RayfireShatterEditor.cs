using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireShatter))]
    public class RayfireShatterEditor : Editor
    {
        RayfireShatter  shat;
        Transform       transForm;
        Vector3         centerWorldPos;
        Quaternion      centerWorldQuat;
        ReorderableList rl_tp_cus_tms;
        ReorderableList rl_tp_cus_list;
        ReorderableList rl_tp_slc_list;

        // Foldout
        static bool        exp_deb;
        static bool        exp_lim;
        static bool        exp_fil;
        
        // Frag Types Minimum & Maximum ranges
        const float bias_min               = 0;
        const float bias_max               = 1f;
        const float strength_min           = 0;
        const float strength_max           = 1f;
        const float rad_radius_min         = 0.01f;
        const float rad_radius_max         = 30f;
        const float rad_div_min            = 0f;
        const float rad_div_max            = 1f;
        const int   rad_rings_min          = 3;
        const int   rad_rings_max          = 60;
        const int   rad_rand_min           = 0;
        const int   rad_rand_max           = 100;
        const int   rad_focus_min          = 0;
        const int   rad_focus_max          = 100;
        const int   rad_twist_min          = -90;
        const int   rad_twist_max          = 90;
        const float hex_size_min           = 0.01f;
        const float hex_size_max           = 10f;
        const int   hex_row_min            = 3;
        const int   hex_row_max            = 200;
        const int   cus_amount_min         = 3;
        const int   cus_amount_max         = 999;
        const float cus_radius_min         = 0.01f;
        const float cus_radius_max         = 10f;
        const float cus_size_min           = 0.01f;
        const float cus_size_max           = 0.4f;
        const float brick_mult_min         = 0.1f;
        const float brick_mult_max         = 10f;
        const int   brick_amount_min       = 0;
        const int   brick_amount_max       = 50;
        const float brick_size_min         = 0.01f;
        const float brick_size_max         = 10f;
        const int   brick_var_min          = 0;
        const int   brick_var_max          = 100;
        const float brick_offset_min       = 0f;
        const float brick_offset_max       = 1f;
        const int   brick_prob_min         = 0;
        const int   brick_prob_max         = 100;
        const int   brick_rotation_min     = 0;
        const int   brick_rotation_max     = 90;
        const float brick_split_offset_min = 0f;
        const float brick_split_offset_max = 0.95f;
        const float vox_size_min           = 0.05f;
        const float vox_size_max           = 10f;
        const int   tet_dens_min           = 1;
        const int   tet_dens_max           = 50;
        const int   tet_noise_min          = 0;
        const int   tet_noise_max          = 100;
        
        // Material Minimum & Maximum ranges
        const float mat_scale_min = 0.01f;
        const float mat_scale_max = 5f;
        
        // Clusters Minimum & Maximum ranges
        const int   cls_count_min  = 2;
        const int   cls_count_max  = 200;
        const int   cls_seed_min   = 0;
        const int   cls_seed_max   = 100;
        const float cls_relax_min  = 0;
        const float cls_relax_max  = 1f;
        const int   cls_amount_min = 0;
        const int   cls_amount_max = 100;
        const int   cls_layers_min = 0;
        const int   cls_layers_max = 5; 
        const float cls_scale_min  = 0;
        const float cls_scale_max  = 1f;
        const int   cls_frags_min  = 0;
        const int   cls_frags_max  = 5; 
        
        // Advanced Minimum & Maximum ranges
        const float adv_size_min    = 0.1f;
        const float adv_size_max    = 100f;
        const int   adv_vert_min    = 100;
        const int   adv_vert_max    = 1900;
        const int   adv_tris_min    = 100;
        const int   adv_tris_max    = 1900;
        const int   adv_rel_min     = 0;
        const int   adv_rel_max     = 10;
        const float adv_abs_min     = 0;
        const float adv_abs_max     = 1f;
        const int   adv_element_min = 1;
        const int   adv_element_max = 100;
        
        // Frag Types Serialized properties
        SerializedProperty sp_tp;
        SerializedProperty sp_interactive;
        SerializedProperty sp_tp_vor_amount;
        SerializedProperty sp_tp_vor_bias;
        SerializedProperty sp_tp_spl_axis;
        SerializedProperty sp_tp_spl_amount;
        SerializedProperty sp_tp_spl_str;
        SerializedProperty sp_tp_spl_bias;
        SerializedProperty sp_tp_slb_axis;
        SerializedProperty sp_tp_slb_amount;
        SerializedProperty sp_tp_slb_str;
        SerializedProperty sp_tp_slb_bias;
        SerializedProperty sp_tp_rad_axis;
        SerializedProperty sp_tp_rad_radius;
        SerializedProperty sp_tp_rad_div;
        SerializedProperty sp_tp_rad_rest;
        SerializedProperty sp_tp_rad_rings;
        SerializedProperty sp_tp_rad_focus;
        SerializedProperty sp_tp_rad_str;
        SerializedProperty sp_tp_rad_randRing;
        SerializedProperty sp_tp_rad_rays;
        SerializedProperty sp_tp_rad_randRay;
        SerializedProperty sp_tp_rad_twist;
        SerializedProperty sp_tp_hex_size;
        SerializedProperty sp_tp_hex_en;
        SerializedProperty sp_tp_hex_pl;
        SerializedProperty sp_tp_hex_row;
        SerializedProperty sp_tp_hex_col;
        SerializedProperty sp_tp_hex_div;
        SerializedProperty sp_tp_hex_rest;
        SerializedProperty sp_tp_cus_src;
        SerializedProperty sp_tp_cus_use;
        SerializedProperty sp_tp_cus_am;
        SerializedProperty sp_tp_cus_rad;
        SerializedProperty sp_tp_cus_en;
        SerializedProperty sp_tp_cus_sz;
        SerializedProperty sp_tp_cus_list;
        SerializedProperty sp_tp_cus_tms;
        SerializedProperty sp_tp_slc_pl;
        SerializedProperty sp_tp_slc_list;
        SerializedProperty sp_tp_brk_type;
        SerializedProperty sp_tp_brk_mult;
        SerializedProperty sp_tp_brk_am_X;
        SerializedProperty sp_tp_brk_am_Y;
        SerializedProperty sp_tp_brk_am_Z;
        SerializedProperty sp_tp_brk_lock;
        SerializedProperty sp_tp_brk_sz_X;
        SerializedProperty sp_tp_brk_sz_Y;
        SerializedProperty sp_tp_brk_sz_Z;
        SerializedProperty sp_tp_brk_sz_var_X;
        SerializedProperty sp_tp_brk_sz_var_Y;
        SerializedProperty sp_tp_brk_sz_var_Z;
        SerializedProperty sp_tp_brk_of_X;
        SerializedProperty sp_tp_brk_of_Y;
        SerializedProperty sp_tp_brk_of_Z;
        SerializedProperty sp_tp_brk_sp_X;
        SerializedProperty sp_tp_brk_sp_Y;
        SerializedProperty sp_tp_brk_sp_Z;
        SerializedProperty sp_tp_brk_sp_prob;
        SerializedProperty sp_tp_brk_sp_offs;
        SerializedProperty sp_tp_brk_sp_rot;
        SerializedProperty sp_tp_vxl_sz;
        SerializedProperty sp_tp_tet_dn;
        SerializedProperty sp_tp_tet_ns;
        
        // Material Serialized properties
        SerializedProperty sp_mat_scl;
        SerializedProperty sp_mat_in;
        SerializedProperty sp_mat_col_en;
        SerializedProperty sp_mat_uve_en;
        SerializedProperty sp_mat_col;
        SerializedProperty sp_mat_uve;
        
        // Clusters Serialized properties
        SerializedProperty sp_cls_en;
        SerializedProperty sp_cls_cnt;
        SerializedProperty sp_cls_seed;
        SerializedProperty sp_cls_rel;
        SerializedProperty sp_cls_amount;
        SerializedProperty sp_cls_layers;
        SerializedProperty sp_cls_scale;
        SerializedProperty sp_cls_min;
        SerializedProperty sp_cls_max;
        
        // Advanced Serialized properties
        SerializedProperty sp_mode;
        SerializedProperty sp_adv_seed;
        SerializedProperty sp_adv_copy;
        SerializedProperty sp_adv_smooth;
        SerializedProperty sp_adv_combine;
        SerializedProperty sp_adv_col;
        SerializedProperty sp_adv_dec;
        SerializedProperty sp_adv_input;
        SerializedProperty sp_adv_output;
        SerializedProperty sp_adv_lim;
        SerializedProperty sp_adv_size_lim;
        SerializedProperty sp_adv_size_am;
        SerializedProperty sp_adv_vert_lim;
        SerializedProperty sp_adv_vert_am;
        SerializedProperty sp_adv_tri_lim;
        SerializedProperty sp_adv_tri_am;
        SerializedProperty sp_adv_inner;
        SerializedProperty sp_adv_planar;
        SerializedProperty sp_adv_rel;
        SerializedProperty sp_adv_abs;
        SerializedProperty sp_adv_element;
        SerializedProperty sp_adv_remove;
        SerializedProperty sp_exp_src;
        SerializedProperty sp_exp_sfx;
        SerializedProperty sp_cn_pos;
        SerializedProperty sp_cn_sh;
            
        private void OnEnable()
        {
            // Get component
            shat = (RayfireShatter)target;
            
             // Find Frag Types Serialized properties
            sp_tp              = serializedObject.FindProperty(nameof(shat.type));
            sp_interactive     = serializedObject.FindProperty(nameof(shat.interactive));
            sp_tp_vor_amount   = serializedObject.FindProperty(nameof(shat.voronoi) + "." + nameof(shat.voronoi.amount));
            sp_tp_vor_bias     = serializedObject.FindProperty(nameof(shat.voronoi) + "." + nameof(shat.voronoi.centerBias));
            sp_tp_spl_axis     = serializedObject.FindProperty(nameof(shat.splinters) + "." + nameof(shat.splinters.axis));
            sp_tp_spl_amount   = serializedObject.FindProperty(nameof(shat.splinters) + "." + nameof(shat.splinters.amount));
            sp_tp_spl_str      = serializedObject.FindProperty(nameof(shat.splinters) + "." + nameof(shat.splinters.strength));
            sp_tp_spl_bias     = serializedObject.FindProperty(nameof(shat.splinters) + "." + nameof(shat.splinters.centerBias));
            sp_tp_slb_axis     = serializedObject.FindProperty(nameof(shat.slabs) + "." + nameof(shat.slabs.axis));
            sp_tp_slb_amount   = serializedObject.FindProperty(nameof(shat.slabs) + "." + nameof(shat.slabs.amount));
            sp_tp_slb_str      = serializedObject.FindProperty(nameof(shat.slabs) + "." + nameof(shat.slabs.strength));
            sp_tp_slb_bias     = serializedObject.FindProperty(nameof(shat.slabs) + "." + nameof(shat.slabs.centerBias));
            sp_tp_rad_axis     = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.centerAxis));
            sp_tp_rad_radius   = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.radius));
            sp_tp_rad_div      = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.divergence));
            sp_tp_rad_rest     = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.restrictToPlane));
            sp_tp_rad_rings    = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.rings));
            sp_tp_rad_focus    = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.focus));
            sp_tp_rad_str      = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.focusStr));
            sp_tp_rad_randRing = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.randomRings));
            sp_tp_rad_rays     = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.rays));
            sp_tp_rad_randRay  = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.randomRays));
            sp_tp_rad_twist    = serializedObject.FindProperty(nameof(shat.radial) + "." + nameof(shat.radial.twist));
            sp_tp_hex_size     = serializedObject.FindProperty(nameof(shat.hexagon) + "." + nameof(shat.hexagon.size));
            sp_tp_hex_en       = serializedObject.FindProperty(nameof(shat.hexagon) + "." + nameof(shat.hexagon.enable));
            sp_tp_hex_pl       = serializedObject.FindProperty(nameof(shat.hexagon) + "." + nameof(shat.hexagon.plane));
            sp_tp_hex_row      = serializedObject.FindProperty(nameof(shat.hexagon) + "." + nameof(shat.hexagon.row));
            sp_tp_hex_col      = serializedObject.FindProperty(nameof(shat.hexagon) + "." + nameof(shat.hexagon.col));
            sp_tp_hex_div      = serializedObject.FindProperty(nameof(shat.hexagon) + "." + nameof(shat.hexagon.div));
            sp_tp_hex_rest     = serializedObject.FindProperty(nameof(shat.hexagon) + "." + nameof(shat.hexagon.rest));
            sp_tp_cus_src      = serializedObject.FindProperty(nameof(shat.custom) + "." + nameof(shat.custom.source));
            sp_tp_cus_use      = serializedObject.FindProperty(nameof(shat.custom) + "." + nameof(shat.custom.useAs));
            sp_tp_cus_am       = serializedObject.FindProperty(nameof(shat.custom) + "." + nameof(shat.custom.amount));
            sp_tp_cus_rad      = serializedObject.FindProperty(nameof(shat.custom) + "." + nameof(shat.custom.radius));
            sp_tp_cus_en       = serializedObject.FindProperty(nameof(shat.custom) + "." + nameof(shat.custom.enable));
            sp_tp_cus_sz       = serializedObject.FindProperty(nameof(shat.custom) + "." + nameof(shat.custom.size));
            sp_tp_cus_list     = serializedObject.FindProperty(nameof(shat.custom) + "." + nameof(shat.custom.vector3));
            sp_tp_cus_tms      = serializedObject.FindProperty(nameof(shat.custom) + "." + nameof(shat.custom.transforms));
            sp_tp_slc_pl       = serializedObject.FindProperty(nameof(shat.slice) + "." + nameof(shat.slice.plane));
            sp_tp_slc_list     = serializedObject.FindProperty(nameof(shat.slice) + "." + nameof(shat.slice.sliceList));
            sp_tp_brk_type     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.amountType));
            sp_tp_brk_mult     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.mult));
            sp_tp_brk_am_X     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.amount_X));
            sp_tp_brk_am_Y     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.amount_Y));
            sp_tp_brk_am_Z     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.amount_Z));
            sp_tp_brk_lock     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.size_Lock));
            sp_tp_brk_sz_X     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.size_X));
            sp_tp_brk_sz_Y     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.size_Y));
            sp_tp_brk_sz_Z     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.size_Z));
            sp_tp_brk_sz_var_X = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.sizeVar_X));
            sp_tp_brk_sz_var_Y = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.sizeVar_Y));
            sp_tp_brk_sz_var_Z = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.sizeVar_Z));
            sp_tp_brk_of_X     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.offset_X));
            sp_tp_brk_of_Y     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.offset_Y));
            sp_tp_brk_of_Z     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.offset_Z));
            sp_tp_brk_sp_X     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.split_X));
            sp_tp_brk_sp_Y     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.split_Y));
            sp_tp_brk_sp_Z     = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.split_Z));
            sp_tp_brk_sp_prob  = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.split_probability));
            sp_tp_brk_sp_offs  = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.split_offset));
            sp_tp_brk_sp_rot   = serializedObject.FindProperty(nameof(shat.bricks) + "." + nameof(shat.bricks.split_rotation));
            sp_tp_vxl_sz       = serializedObject.FindProperty(nameof(shat.voxels) + "." + nameof(shat.voxels.size));
            sp_tp_tet_dn       = serializedObject.FindProperty(nameof(shat.tets) + "." + nameof(shat.tets.density));
            sp_tp_tet_ns       = serializedObject.FindProperty(nameof(shat.tets) + "." + nameof(shat.tets.noise));
            
            // Find Material Serialized properties
            sp_mat_scl    = serializedObject.FindProperty(nameof(shat.material) + "." + nameof(shat.material.mScl));
            sp_mat_in     = serializedObject.FindProperty(nameof(shat.material) + "." + nameof(shat.material.iMat));
            sp_mat_col_en = serializedObject.FindProperty(nameof(shat.material) + "." + nameof(shat.material.cE));
            sp_mat_uve_en = serializedObject.FindProperty(nameof(shat.material) + "." + nameof(shat.material.uvE));
            sp_mat_col    = serializedObject.FindProperty(nameof(shat.material) + "." + nameof(shat.material.cC));
            sp_mat_uve    = serializedObject.FindProperty(nameof(shat.material) + "." + nameof(shat.material.uvC));
            
            // Clusters Serialized properties
            sp_cls_en     = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.enable));
            sp_cls_cnt    = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.count));
            sp_cls_seed   = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.seed));
            sp_cls_rel    = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.relax));
            sp_cls_amount = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.amount));
            sp_cls_layers = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.layers));
            sp_cls_scale  = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.scale));
            sp_cls_min    = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.min));
            sp_cls_max    = serializedObject.FindProperty(nameof(shat.clusters) + "." + nameof(shat.clusters.max));
            
            // Advanced Serialized properties
            sp_mode         = serializedObject.FindProperty(nameof(shat.mode));
            sp_adv_seed     = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.seed));
            sp_adv_copy     = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.copyComponents));
            sp_adv_smooth   = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.smooth));
            sp_adv_combine  = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.combineChildren));
            sp_adv_col      = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.removeCollinear));
            sp_adv_dec      = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.decompose));
            sp_adv_input    = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.inputPrecap));
            sp_adv_output   = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.outputPrecap));
            sp_adv_size_lim = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.sizeLimitation));
            sp_adv_size_am  = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.sizeAmount));
            sp_adv_vert_lim = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.vertexLimitation));
            sp_adv_vert_am  = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.vertexAmount));
            sp_adv_tri_lim  = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.triangleLimitation));
            sp_adv_tri_am   = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.triangleAmount));
            sp_adv_inner    = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.inner));
            sp_adv_planar   = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.planar));
            sp_adv_rel      = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.relativeSize));
            sp_adv_abs      = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.absoluteSize));
            sp_adv_element  = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.elementSizeThreshold));
            sp_adv_remove   = serializedObject.FindProperty(nameof(shat.advanced) + "." + nameof(shat.advanced.removeDoubleFaces));
            
            sp_exp_src = serializedObject.FindProperty(nameof(shat.export) + "." + nameof(shat.export.source));
            sp_exp_sfx = serializedObject.FindProperty(nameof(shat.export) + "." + nameof(shat.export.suffix));
            sp_cn_pos  = serializedObject.FindProperty(nameof(shat.centerPosition));
            sp_cn_sh   = serializedObject.FindProperty(nameof(shat.showCenter));
                
            // Reorderable lists
            rl_tp_cus_tms = new ReorderableList (serializedObject, sp_tp_cus_tms, true, true, true, true)
            {
                drawElementCallback = DrawCustTmListItems,
                drawHeaderCallback  = DrawCustTmHeader,
                onAddCallback       = AddCustTm,
                onRemoveCallback    = RemoveCustTm
            };
            rl_tp_cus_list = new ReorderableList (serializedObject, sp_tp_cus_list, true, true, true, true)
            {
                drawElementCallback = DrawCustPointListItems,
                drawHeaderCallback  = DrawCustPointHeader,
                onAddCallback       = AddCustPoint,
                onRemoveCallback    = RemoveCustPoint
            };
            rl_tp_slc_list = new ReorderableList (serializedObject, sp_tp_slc_list, true, true, true, true)
            {
                drawElementCallback = DrawSliceTmListItems,
                drawHeaderCallback  = DrawSliceTmHeader,
                onAddCallback       = AddSliceTm,
                onRemoveCallback    = RemoveSliceTm
            };
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            if (shat == null)
                return;
            
            // Update changed properties
            serializedObject.Update();

            // Space
            GUILayout.Space (8);
            
            GUI_Fragment();
            GUI_Interactive();
            GUI_Preview();

            // Reset scale if fragments were deleted
            shat.ResetScale (shat.previewScale);

            GUICommon.Space ();
            GUI_Types();
            GUICommon.Space ();
            GUI_Material();
            GUICommon.Space ();
            GUI_Cluster();
            GUICommon.Space ();
            GUI_Advanced();
            GUICommon.Space ();
            GUI_Export();
            GUICommon.Space ();
            GUI_Collider();
            GUICommon.Space ();
            GUI_Center();
            GUICommon.Space ();
            GUI_Info();

            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Types
        /// /////////////////////////////////////////////////////////
        
        void GUI_Types()
        {
            EditorGUI.BeginChangeCheck();
            GUICommon.CaptionBox (TextSht.gui_cap_frg);
            GUICommon.PropertyField (sp_tp, TextSht.gui_tp);
            EditorGUI.indentLevel++;
            if (shat.type == FragType.Voronoi)        GUI_Type_Voronoi();
            else if (shat.type == FragType.Splinters) GUI_Type_Splinters();
            else if (shat.type == FragType.Slabs)     GUI_Type_Slabs();
            else if (shat.type == FragType.Radial)    GUI_Type_Radial();
            else if (shat.type == FragType.Hexagon)   GUI_Type_HexGrid();
            else if (shat.type == FragType.Custom)    GUI_Type_Custom();
            else if (shat.type == FragType.Slices)    GUI_Type_Slices();
            else if (shat.type == FragType.Bricks)    GUI_Type_Bricks();
            else if (shat.type == FragType.Voxels)    GUI_Type_Voxels();
            else if (shat.type == FragType.Tets)      GUI_Type_Tets();
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck() == true)
                InteractiveChange();
        }
        
        void GUI_Type_Voronoi()
        {
            GUICommon.Caption (TextSht.gui_cap_vor);
            GUICommon.PropertyField (sp_tp_vor_amount, TextSht.gui_tp_vor_amount);
            GUICommon.Slider (sp_tp_vor_bias, bias_min, bias_max, TextSht.gui_tp_vor_bias);
        }
        
        void GUI_Type_Splinters()
        {
            GUICommon.Caption (TextSht.gui_cap_spl);
            GUICommon.PropertyField (sp_tp_spl_axis, TextSht.gui_tp_spl_axis);
            GUICommon.PropertyField (sp_tp_spl_amount, TextSht.gui_tp_vor_amount);
            GUICommon.Slider (sp_tp_spl_str,  strength_min, strength_max, TextSht.gui_tp_spl_str);
            GUICommon.Slider (sp_tp_spl_bias, bias_min,     bias_max, TextSht.gui_tp_vor_bias);
        }
        
        void GUI_Type_Slabs()
        {
            GUICommon.Caption (TextSht.gui_cap_slb);
            GUICommon.PropertyField (sp_tp_slb_axis,   TextSht.gui_tp_spl_axis);
            GUICommon.PropertyField (sp_tp_slb_amount, TextSht.gui_tp_vor_amount);
            GUICommon.Slider (sp_tp_slb_str,  strength_min, strength_max, TextSht.gui_tp_spl_str);
            GUICommon.Slider (sp_tp_slb_bias, bias_min,     bias_max, TextSht.gui_tp_vor_bias);
        }
        
        void GUI_Type_Radial()
        {
            GUICommon.Caption (TextSht.gui_cap_rad);
            GUICommon.PropertyField (sp_tp_rad_axis, TextSht.gui_tp_rad_axis);
            GUICommon.Slider (sp_tp_rad_radius, rad_radius_min, rad_radius_max, TextSht.gui_tp_rad_radius);
            GUICommon.Slider (sp_tp_rad_div,    rad_div_min,    rad_div_max,    TextSht.gui_tp_rad_div);
            if (sp_tp_rad_div.floatValue > 0)
                GUICommon.PropertyField (sp_tp_rad_rest, TextSht.gui_tp_rad_rest);

            GUICommon.Caption (TextSht.gui_cap_rings);
            GUICommon.IntSlider (sp_tp_rad_rings,    rad_rings_min, rad_rings_max, TextSht.gui_tp_rad_rings);
            GUICommon.IntSlider (sp_tp_rad_randRing, rad_rand_min,  rad_rand_max,  TextSht.gui_tp_rad_randRing);
            GUICommon.IntSlider (sp_tp_rad_focus,    rad_focus_min, rad_focus_max, TextSht.gui_tp_rad_focus);
            GUICommon.IntSlider (sp_tp_rad_str,      rad_focus_min, rad_focus_max, TextSht.gui_tp_rad_str);
            
            GUICommon.Caption (TextSht.gui_cap_rays);
            GUICommon.IntSlider (sp_tp_rad_rays,    rad_rings_min, rad_rings_max, TextSht.gui_tp_rad_rays);
            GUICommon.IntSlider (sp_tp_rad_randRay, rad_rand_min,  rad_rand_max,  TextSht.gui_tp_rad_randRay);
            GUICommon.IntSlider (sp_tp_rad_twist,   rad_twist_min, rad_twist_max, TextSht.gui_tp_rad_twist);
        }

        void GUI_Type_HexGrid()
        {
            GUICommon.Caption (TextSht.gui_cap_hex);
            GUICommon.Slider (sp_tp_hex_size, hex_size_min, hex_size_max, TextSht.gui_tp_hex_size);
            GUICommon.PropertyField (sp_tp_hex_en, TextSht.gui_tp_cus_en);

            GUICommon.Caption (TextSht.gui_cap_hex_grd);
            GUICommon.PropertyField (sp_tp_hex_pl, TextSht.gui_tp_slc_pl);
            GUICommon.IntSlider (sp_tp_hex_row, hex_row_min, hex_row_max, TextSht.gui_tp_hex_am);
            GUICommon.IntSlider (sp_tp_hex_col, hex_row_min, hex_row_max, TextSht.gui_tp_hex_emp);
            GUICommon.Slider (sp_tp_hex_div, 0f, shat.hexagon.size, TextSht.gui_tp_rad_div);
            if (sp_tp_hex_div.floatValue > 0)
                GUICommon.PropertyField (sp_tp_hex_rest, TextSht.gui_tp_rad_rest);
        }
        
        void GUI_Type_Custom()
        {
            GUICommon.Caption (TextSht.gui_cap_cus);
            GUICommon.PropertyField (sp_tp_cus_src, TextSht.gui_tp_cus_src);
            GUICommon.PropertyField (sp_tp_cus_use, TextSht.gui_tp_cus_use);
            if (sp_tp_cus_src.intValue == (int)RFCustom.RFPointCloudSourceType.TransformList)
                rl_tp_cus_tms.DoLayoutList();
            if (sp_tp_cus_src.intValue == (int)RFCustom.RFPointCloudSourceType.Vector3List)
                rl_tp_cus_list.DoLayoutList();
            if (sp_tp_cus_use.intValue == (int)RFCustom.RFPointCloudUseType.VolumePoints)
            {
                GUICommon.Caption (TextSht.gui_cap_vol);
                GUICommon.IntSlider (sp_tp_cus_am,  cus_amount_min, cus_amount_max, TextSht.gui_tp_cus_am);
                GUICommon.Slider (sp_tp_cus_rad, cus_radius_min, cus_radius_max, TextSht.gui_tp_cus_rad);
                
                if (shat.custom.inBoundPoints.Count > 0)
                {
                    GUICommon.Space ();
                    GUILayout.Label (TextSht.str_points + shat.custom.inBoundPoints.Count + "/" + shat.custom.outBoundPoints.Count);
                }
            }

            GUICommon.Caption (TextSht.gui_cap_prev);
            GUICommon.PropertyField (sp_tp_cus_en, TextSht.gui_tp_cus_en);
            if (sp_tp_cus_en.boolValue == true)
                GUICommon.Slider (sp_tp_cus_sz, cus_size_min, cus_size_max, TextSht.gui_tp_cus_sz);
        }
        
        void GUI_Type_Slices()
        {
            GUICommon.Caption (TextSht.gui_cap_slc);
            GUICommon.PropertyField (sp_tp_slc_pl, TextSht.gui_tp_slc_pl);
            rl_tp_slc_list.DoLayoutList();
        }

        void GUI_Type_Bricks()
        {
            GUICommon.Caption (TextSht.gui_cap_brk);
            GUICommon.PropertyField (sp_tp_brk_type, TextSht.gui_tp_brk_type);
            GUICommon.Slider (sp_tp_brk_mult, brick_mult_min, brick_mult_max, TextSht.gui_tp_brk_mult);
            if (sp_tp_brk_type.intValue == (int)RFBricks.RFBrickType.ByAmount)
            //if (shat.bricks.amountType == RFBricks.RFBrickType.ByAmount)
            {
                GUICommon.Caption (TextSht.gui_cap_am);
                GUICommon.IntSlider (sp_tp_brk_am_X, brick_amount_min, brick_amount_max, TextSht.gui_tp_brk_am_X);
                GUICommon.IntSlider (sp_tp_brk_am_Y, brick_amount_min, brick_amount_max, TextSht.gui_tp_brk_am_Y);
                GUICommon.IntSlider (sp_tp_brk_am_Z, brick_amount_min, brick_amount_max, TextSht.gui_tp_brk_am_Z);
            }
            else
            {
                GUICommon.Caption (TextSht.gui_cap_size);
                
                EditorGUI.BeginChangeCheck();
                GUICommon.Slider (sp_tp_brk_sz_X, brick_size_min, brick_size_max, TextSht.gui_tp_brk_am_X);
                if (EditorGUI.EndChangeCheck() == true && sp_tp_brk_lock.boolValue == true)
                {
                    sp_tp_brk_sz_Y.floatValue = sp_tp_brk_sz_X.floatValue;
                    sp_tp_brk_sz_Z.floatValue = sp_tp_brk_sz_X.floatValue;
                }

                EditorGUI.BeginChangeCheck();
                GUICommon.Slider (sp_tp_brk_sz_Y, brick_size_min, brick_size_max, TextSht.gui_tp_brk_am_Y);
                if (EditorGUI.EndChangeCheck() == true && sp_tp_brk_lock.boolValue == true)
                {
                    sp_tp_brk_sz_X.floatValue = sp_tp_brk_sz_Y.floatValue;
                    sp_tp_brk_sz_Z.floatValue = sp_tp_brk_sz_Y.floatValue;
                }

                EditorGUI.BeginChangeCheck();
                GUICommon.Slider (sp_tp_brk_sz_Z, brick_size_min, brick_size_max, TextSht.gui_tp_brk_am_Z);
                if (EditorGUI.EndChangeCheck() == true && sp_tp_brk_lock.boolValue == true)
                {
                    sp_tp_brk_sz_X.floatValue = sp_tp_brk_sz_Z.floatValue;
                    sp_tp_brk_sz_Y.floatValue = sp_tp_brk_sz_Z.floatValue;
                }
                
                GUICommon.PropertyField (sp_tp_brk_lock, TextSht.gui_tp_brk_lock);
            }

            GUICommon.Caption (TextSht.gui_cap_var);
            GUICommon.IntSlider (sp_tp_brk_sz_var_X, brick_var_min, brick_var_max, TextSht.gui_tp_brk_am_X);
            GUICommon.IntSlider (sp_tp_brk_sz_var_Y, brick_var_min, brick_var_max, TextSht.gui_tp_brk_am_Y);
            GUICommon.IntSlider (sp_tp_brk_sz_var_Z, brick_var_min, brick_var_max, TextSht.gui_tp_brk_am_Z);
            
            GUICommon.Caption (TextSht.gui_cap_ofs);
            GUICommon.Slider (sp_tp_brk_of_X, brick_offset_min, brick_offset_max, TextSht.gui_tp_brk_am_X);
            GUICommon.Slider (sp_tp_brk_of_Y, brick_offset_min, brick_offset_max, TextSht.gui_tp_brk_am_Y);
            GUICommon.Slider (sp_tp_brk_of_Z, brick_offset_min, brick_offset_max, TextSht.gui_tp_brk_am_Z);
            
            GUICommon.Caption (TextSht.gui_cap_sp);
            GUICommon.PropertyField (sp_tp_brk_sp_X, TextSht.gui_tp_brk_am_X);
            GUICommon.PropertyField (sp_tp_brk_sp_Y, TextSht.gui_tp_brk_am_Y);
            GUICommon.PropertyField (sp_tp_brk_sp_Z, TextSht.gui_tp_brk_am_Z);
            GUICommon.IntSlider (sp_tp_brk_sp_prob, brick_prob_min,     brick_prob_max,     TextSht.gui_tp_brk_sp_prob);
            GUICommon.IntSlider (sp_tp_brk_sp_rot,  brick_rotation_min, brick_rotation_max, TextSht.gui_tp_brk_sp_rot);
            GUICommon.Slider (sp_tp_brk_sp_offs, brick_split_offset_min, brick_split_offset_max, TextSht.gui_tp_brk_sp_offs);
        }

        void GUI_Type_Voxels()
        {
            GUICommon.Caption (TextSht.gui_cap_vxl);
            GUICommon.Slider (sp_tp_vxl_sz, vox_size_min, vox_size_max, TextSht.gui_tp_cus_sz);
        }

        void GUI_Type_Tets()
        {
            GUICommon.Caption (TextSht.gui_cap_tet);
            GUICommon.IntSlider (sp_tp_tet_dn, tet_dens_min,  tet_dens_max, TextSht.gui_tp_tetDn);
            GUICommon.IntSlider (sp_tp_tet_ns, tet_noise_min, tet_noise_max, TextSht.gui_tp_tetNs);
        }

        void GUI_Material()
        {
            if (sp_tp.intValue == (int)FragType.Decompose)
                return;

            GUICommon.CaptionBox (TextSht.gui_cap_mat);
            GUICommon.Slider (sp_mat_scl, mat_scale_min, mat_scale_max, TextSht.gui_mat_scl);
            GUICommon.PropertyField (sp_mat_in,  TextSht.gui_mat_in);
            GUICommon.PropertyField (sp_mat_col_en, TextSht.gui_mat_col);
            if (sp_mat_col_en.boolValue == true)
            {
                EditorGUILayout.PropertyField (sp_mat_col, TextSht.gui_tp_hex_emp);
                GUICommon.Space ();
            }
            GUICommon.PropertyField (sp_mat_uve_en, TextSht.gui_mat_uve);
            if (sp_mat_uve_en.boolValue == true)
            {
                EditorGUILayout.PropertyField (sp_mat_uve, TextSht.gui_tp_hex_emp);
                GUICommon.Space ();
            }
        }

        void GUI_Cluster()
        {
            // Not for bricks, slices and decompose
            if (shat.type == FragType.Bricks || 
                shat.type == FragType.Decompose || 
                shat.type == FragType.Voxels || 
                shat.type == FragType.Slices)
                return;

            GUICommon.CaptionBox (TextSht.gui_cap_cls);
            GUICommon.PropertyField (sp_cls_en, TextSht.gui_cls_en);
            if (sp_cls_en.boolValue == true)
            {
                GUICommon.IntSlider (sp_cls_cnt,  cls_count_min, cls_count_max, TextSht.gui_cls_cnt);
                GUICommon.IntSlider (sp_cls_seed, cls_seed_min,  cls_seed_max,  TextSht.gui_cls_seed);
                GUICommon.Slider (sp_cls_rel, cls_relax_min, cls_relax_max, TextSht.gui_cls_rel);
                GUICommon.Foldout (ref exp_deb, TextSht.gui_cls_debris.text);
                if (exp_deb == true)
                {
                    EditorGUI.indentLevel++;
                    GUICommon.IntSlider (sp_cls_amount, cls_amount_min, cls_amount_max, TextSht.gui_cls_amount);
                    GUICommon.IntSlider (sp_cls_layers, cls_layers_min, cls_layers_max, TextSht.gui_cls_layers);
                    GUICommon.Slider (sp_cls_scale, cls_scale_min, cls_scale_max, TextSht.gui_cls_scale);
                    GUICommon.IntSlider (sp_cls_min, cls_frags_min, cls_frags_max, TextSht.gui_cls_min);
                    GUICommon.IntSlider (sp_cls_max, cls_frags_min, cls_frags_max, TextSht.gui_cls_max);
                    EditorGUI.indentLevel--;
                }
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void GUI_Advanced()
        {
            GUICommon.CaptionBox (TextSht.gui_cap_prp);
            GUICommon.PropertyField (sp_mode, TextSht.gui_mode);
            GUICommon.IntSlider (sp_adv_seed, cls_seed_min, cls_seed_max, TextSht.gui_adv_seed);
            GUICommon.PropertyField (sp_adv_copy,    TextSht.gui_adv_copy);
            GUICommon.PropertyField (sp_adv_smooth,  TextSht.gui_adv_smooth);
            GUICommon.PropertyField (sp_adv_combine, TextSht.gui_adv_combine);
            GUICommon.PropertyField (sp_adv_col,     TextSht.gui_adv_col);
            GUICommon.PropertyField (sp_adv_dec,     TextSht.gui_adv_dec);
            GUICommon.PropertyField (sp_adv_input,   TextSht.gui_adv_input);
            if (sp_adv_input.boolValue == true)
                GUICommon.PropertyField (sp_adv_output, TextSht.gui_adv_output);
            
            GUICommon.Foldout (ref exp_lim, TextSht.gui_adv_lim.text);
            if (exp_lim == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.PropertyField (sp_adv_size_lim, TextSht.gui_adv_size_lim);
                if (shat.advanced.sizeLimitation == true)
                    GUICommon.Slider (sp_adv_size_am, adv_size_min, adv_size_max, TextSht.gui_adv_size_am);
                GUICommon.PropertyField (sp_adv_vert_lim, TextSht.gui_adv_vert_lim);
                if (shat.advanced.vertexLimitation == true)
                    GUICommon.IntSlider (sp_adv_vert_am, adv_vert_min, adv_vert_max, TextSht.gui_adv_vert_am);
                GUICommon.PropertyField (sp_adv_tri_lim, TextSht.gui_adv_tri_lim);
                if (shat.advanced.triangleLimitation == true)
                    GUICommon.IntSlider (sp_adv_tri_am, adv_tris_min, adv_tris_max, TextSht.gui_adv_tri_am);
                EditorGUI.indentLevel--;
            }
            
            GUICommon.Space();
            GUICommon.Foldout (ref exp_fil, TextSht.gui_adv_flt.text);
            if (exp_fil == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.PropertyField (sp_adv_inner,  TextSht.gui_adv_inner);
                GUICommon.PropertyField (sp_adv_planar, TextSht.gui_adv_planar);
                GUICommon.IntSlider (sp_adv_rel, adv_rel_min, adv_rel_max, TextSht.gui_adv_rel);
                GUICommon.Slider (sp_adv_abs, adv_abs_min, adv_abs_max, TextSht.gui_adv_abs);
                EditorGUI.indentLevel--;
            }
            
            if (sp_mode.intValue == (int)FragmentMode.Editor)
            {
                GUICommon.CaptionBox (TextSht.gui_cap_edt);
                GUICommon.IntSlider (sp_adv_element, adv_element_min, adv_element_max, TextSht.gui_adv_element);
                GUICommon.PropertyField (sp_adv_remove, TextSht.gui_adv_remove);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////
        
        void GUI_Fragment()
        {
            if (GUILayout.Button (TextSht.gui_btn_frag, GUILayout.Height (25)))
            {
                foreach (var targ in targets)
                {
                    if (targ as RayfireShatter != null)
                    {
                        if ((targ as RayfireShatter).interactive == false)
                            (targ as RayfireShatter).Fragment();
                        else
                            (targ as RayfireShatter).InteractiveFragment();

                        // TODO APPLY LOCAL SHATTER PREVIEW PROPS TO ALL SELECTED
                    }
                }
                // Scale preview if preview turn on
                if (shat.previewScale > 0 && shat.scalePreview == true)
                    ScalePreview (shat);
            }

            GUILayout.Space (1);

            GUILayout.BeginHorizontal();

            // Delete last
            if (shat.fragmentsLast.Count > 0) // TODO SUPPORT MASS CHECK
            {
                if (GUILayout.Button (TextSht.gui_btn_frag_last, GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsLast (1);
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).Fragment (RayfireShatter.FragLastMode.ToLast);

                            // Scale preview if preview turn on
                            if ((targ as RayfireShatter).previewScale > 0 && (targ as RayfireShatter).scalePreview == true)
                                ScalePreview (targ as RayfireShatter);
                        }
                }

                if (GUILayout.Button (TextSht.gui_btn_frag_last_del, GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsLast();
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).ResetScale (0f);
                        }
                }
            }

            // Delete all fragments
            if (shat.fragmentsAll.Count > 0 && shat.fragmentsAll.Count > shat.fragmentsLast.Count)
            {
                if (GUILayout.Button (TextSht.gui_btn_frag_del, GUILayout.Height (22)))
                {
                    foreach (var targ in targets)
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).DeleteFragmentsAll();
                            (targ as RayfireShatter).resetState = true;
                            (targ as RayfireShatter).ResetScale (0f);
                        }
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        
        void GUI_Interactive()
        {
            /* Doesnt work
            EditorGUI.BeginChangeCheck();
            GUICommon.Toggle (sp_interactive, TextSht.gui_btn_inter, 25);
            if (EditorGUI.EndChangeCheck() == true)
            {
 
                shat.interactive = sp_interactive.boolValue;
                Debug.Log (shat.interactive);
                Debug.Log (sp_interactive.boolValue);
                if (shat.interactive == true)
                {
                    shat.InteractiveStart();
                }
                else
                {
                    shat.InteractiveStop();
                }
                
                Debug.Log (shat.interactive);
                Debug.Log (sp_interactive.boolValue);
            }
            GUICommon.Space ();
            */
          
           EditorGUI.BeginChangeCheck();
           shat.interactive = GUILayout.Toggle (shat.interactive, TextSht.gui_btn_inter, "Button", GUILayout.Height (25));
           if (EditorGUI.EndChangeCheck() == true)
           {
               if (shat.interactive == true)
               {
                   shat.InteractiveStart();
               }
               else
               {
                   shat.InteractiveStop();
               }
               SetDirty (shat);
           }
           GUICommon.Space ();
           
        }
        
        void GUI_Preview()
        {
            // Preview
            if (shat.fragmentsLast.Count == 0 && shat.interactive == false)
                return;
            
            GUICommon.Caption (TextSht.gui_cap_prv);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            shat.scalePreview = GUILayout.Toggle (shat.scalePreview, TextSht.gui_btn_scale, "Button");
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shat.scalePreview == true)
                    ScalePreview (shat);
                else
                {
                    shat.resetState = true;
                    shat.ResetScale (0f);
                }

                SetDirty (shat);
                InteractiveChange();
            }
            
            // Color preview toggle
            if (shat.fragmentsLast.Count > 0)
                shat.colorPreview = GUILayout.Toggle (shat.colorPreview, TextSht.gui_btn_color, "Button");
            EditorGUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUICommon.Space ();
            GUILayout.Label (TextSht.gui_prv_scl, GUILayout.Width (90));
            EditorGUI.BeginChangeCheck();
            shat.previewScale = GUILayout.HorizontalSlider (shat.previewScale, 0f, 0.99f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                if (shat.scalePreview == true)
                    ScalePreview (shat);
                SetDirty (shat);
                InteractiveChange(); // TODO only change scale, do not refrag. LIB update
            }
            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Export
        /// /////////////////////////////////////////////////////////
        
        void GUI_Export()
        {
            if (CanExport() == true)
            {
                GUICommon.Caption (TextSht.gui_cap_exp);
                GUICommon.PropertyField (sp_exp_src, TextSht.gui_exp_src);
                if (HasToExport() == true)
                    GUICommon.PropertyField (sp_exp_sfx, TextSht.gui_exp_sfx);

                // Export Last fragments
                if (shat.export.source == RFMeshExport.MeshExportType.LastFragments && shat.fragmentsLast.Count > 0)
                    if (GUILayout.Button (TextSht.gui_btn_export_last, GUILayout.Height (25)))
                        RFMeshAsset.SaveFragments (shat, null);

                // Export children
                if (shat.export.source == RFMeshExport.MeshExportType.Children && shat.transform.childCount > 0)
                    if (GUILayout.Button (TextSht.gui_btn_export_child, GUILayout.Height (25)))
                        RFMeshAsset.SaveFragments (shat, null);
            }
        }

        bool CanExport()
        {
            if (shat.fragmentsLast.Count > 0 || shat.transform.childCount > 0)
                return true;
            return false;
        }

        bool HasToExport()
        {
            if (shat.export.source == RFMeshExport.MeshExportType.LastFragments && shat.fragmentsLast.Count > 0)
                return true;
            if (shat.export.source == RFMeshExport.MeshExportType.Children && shat.transform.childCount > 0)
                return true;
            return false;
        }

        /// /////////////////////////////////////////////////////////
        /// Center
        /// /////////////////////////////////////////////////////////
        
        void GUI_Center()
        {
            if ((int)shat.type <= 5)
            {
                GUICommon.CaptionBox (TextSht.gui_cap_cent);
                GUILayout.BeginHorizontal();
                
                EditorGUI.BeginChangeCheck();
                GUICommon.Toggle (sp_cn_sh, TextSht.gui_btn_show, 18);
                if (EditorGUI.EndChangeCheck())
                    SceneView.RepaintAll();
                
                if (GUILayout.Button (TextSht.gui_cn_res))
                {
                    foreach (var targ in targets)
                    {
                        if (targ as RayfireShatter != null)
                        {
                            (targ as RayfireShatter).ResetCenter();
                            SetDirty (targ as RayfireShatter);
                        }
                    }
                    InteractiveChange();
                    SceneView.RepaintAll();
                }
                
                EditorGUILayout.EndHorizontal();
                GUICommon.Space ();
                GUICommon.PropertyField (sp_cn_pos, TextSht.gui_cn_pos);
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Info
        /// /////////////////////////////////////////////////////////
        
        void GUI_Info()
        {
            if (shat.fragmentsLast.Count > 0 || shat.fragmentsAll.Count > 0)
            {
                GUICommon.Caption (TextSht.gui_cap_info);
                GUILayout.BeginHorizontal();
                GUILayout.Label (TextSht.str_roots + shat.rootChildList.Count);
                GUILayout.Label (TextSht.str_last + shat.fragmentsLast.Count);
                GUILayout.Label (TextSht.str_total + shat.fragmentsAll.Count);
                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Preview
        /// /////////////////////////////////////////////////////////

        static void ColorPreview (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
            {
                Random.InitState (1);
                foreach (Transform root in scr.rootChildList)
                {
                    if (root != null)
                    {
                        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
                        foreach (var mf in meshFilters)
                        {
                            Gizmos.color = new Color (Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f), Random.Range (0.2f, 0.8f));
                            Gizmos.DrawMesh (mf.sharedMesh, mf.transform.position, mf.transform.rotation, mf.transform.lossyScale * 1.01f);
                        }
                    }
                }
            }
        }

        static void ScalePreview (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0 && scr.previewScale > 0f)
            {
                // Do not scale
                if (scr.skinnedMeshRend != null)
                    scr.skinnedMeshRend.enabled = false;
                if (scr.meshRenderer != null)
                    scr.meshRenderer.enabled = false;

                foreach (GameObject fragment in scr.fragmentsLast)
                    if (fragment != null)
                        fragment.transform.localScale = Vector3.one * scr.PreviewScale();
                scr.resetState = true;
            }

            if (scr.previewScale == 0f)
                scr.ResetScale (0f);
        }

        /// /////////////////////////////////////////////////////////
        /// Colliders
        /// /////////////////////////////////////////////////////////
        
        void GUI_Collider()
        {
            if (shat.fragmentsLast.Count == 0)
                return;

            GUICommon.Caption (TextSht.gui_cap_col);
            
            GUILayout.BeginHorizontal();

            if (GUILayout.Button (TextSht.gui_btn_coll_add))
            {
                foreach (var targ in targets)
                    if (targ as RayfireShatter != null)
                        AddColliders (targ as RayfireShatter);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button (TextSht.gui_btn_coll_rem))
            {
                foreach (var targ in targets)
                    if (targ as RayfireShatter != null)
                        RemoveColliders (targ as RayfireShatter);
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();
        }

        static void AddColliders (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
                foreach (var frag in scr.fragmentsLast)
                {
                    MeshCollider mc = frag.GetComponent<MeshCollider>();
                    if (mc == null)
                        mc = frag.AddComponent<MeshCollider>();
                    mc.convex = true;
                }
        }

        static void RemoveColliders (RayfireShatter scr)
        {
            if (scr.fragmentsLast.Count > 0)
                foreach (var frag in scr.fragmentsLast)
                {
                    MeshCollider mc = frag.gameObject.GetComponent<MeshCollider>();
                    if (mc != null)
                        DestroyImmediate (mc);
                }
        }

        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireShatter shatter, GizmoType gizmoType)
        {
            // Color preview
            if (shatter.colorPreview == true)
                ColorPreview (shatter);

            // HexGrid cloud preview
            if (shatter.type == FragType.Hexagon && shatter.hexagon.enable == true)
            {
                // Get bounds for preview
                Bounds bound = shatter.GetBound();
                if (bound.size.magnitude > 0)
                {
                    // Center position from local to global
                    Vector3 centerPos = shatter.transform.TransformPoint (shatter.centerPosition);

                    // Collect point cloud and draw
                    RFHexagon.GetHexPointCLoud (shatter.hexagon, shatter.transform, centerPos, shatter.centerDirection, shatter.advanced.seed, bound);
                    DrawSpheres (shatter.hexagon.pcBndIn, shatter.hexagon.pcBndOut, shatter.hexagon.size / 4f);
                }
            }

            // Custom point cloud preview
            if (shatter.type == FragType.Custom && shatter.custom.enable == true)
            {
                // Get bounds for preview
                Bounds bound = shatter.GetBound();
                if (bound.size.magnitude > 0)
                {
                    // Collect point cloud and draw
                    RFCustom.GetCustomPointCLoud (shatter.custom, shatter.transform, shatter.advanced.seed, bound);
                    DrawSpheres (shatter.custom.inBoundPoints, shatter.custom.outBoundPoints, shatter.custom.size);
                }
            }
        }

        // Draw In/Out points
        static void DrawSpheres(List<Vector3> inBoundPoints, List<Vector3> outBoundPoints, float size)
        {
            if (inBoundPoints != null && inBoundPoints.Count > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < inBoundPoints.Count; i++)
                    Gizmos.DrawSphere (inBoundPoints[i], size);
            }
            if (outBoundPoints != null && outBoundPoints.Count > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < outBoundPoints.Count; i++)
                    Gizmos.DrawSphere (outBoundPoints[i], size / 2f);
            }
        }

        // Show center move handle
        private void OnSceneGUI()
        {
            // Get shatter
            shat = (RayfireShatter)target;
            if (shat == null)
                return;

            transForm       = shat.transform;
            centerWorldPos  = transForm.TransformPoint (shat.centerPosition);
            centerWorldQuat = transForm.rotation * shat.centerDirection;

            // Point3 handle
            if (shat.showCenter == true)
            {
                EditorGUI.BeginChangeCheck();
                centerWorldPos = Handles.PositionHandle (centerWorldPos, centerWorldQuat.RFNormalize());
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (shat, TextSht.str_move);
                    InteractiveChange();
                    SetDirty (shat);
                }

                EditorGUI.BeginChangeCheck();
                centerWorldQuat = Handles.RotationHandle (centerWorldQuat, centerWorldPos);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (shat, TextSht.str_rotate);
                    InteractiveChange();
                    SetDirty (shat);
                }
            }

            shat.centerDirection = Quaternion.Inverse (transForm.rotation) * centerWorldQuat;
            shat.centerPosition  = transForm.InverseTransformPoint (centerWorldPos);
        }

        /// /////////////////////////////////////////////////////////
        /// ReorderableList Custom Transform
        /// /////////////////////////////////////////////////////////
        
        void DrawCustTmListItems (Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_tp_cus_tms.serializedProperty.GetArrayElementAtIndex (index);
            EditorGUI.PropertyField (new Rect (rect.x, rect.y + 2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        void DrawCustTmHeader (Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField (rect, TextSht.gui_tp_cus_tms);
        }

        void AddCustTm (ReorderableList list)
        {
            if (shat.custom.transforms == null)
                shat.custom.transforms = new List<Transform>();
            shat.custom.transforms.Add (null);
            list.index = list.count;
            InteractiveChange();
        }

        void RemoveCustTm (ReorderableList list)
        {
            if (shat.custom.transforms != null)
            {
                shat.custom.transforms.RemoveAt (list.index);
                list.index = list.index - 1;
                InteractiveChange();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// ReorderableList Custom Point 3
        /// /////////////////////////////////////////////////////////
      
        void DrawCustPointListItems (Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_tp_cus_list.serializedProperty.GetArrayElementAtIndex (index);
            EditorGUI.PropertyField (new Rect (rect.x, rect.y + 2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        void DrawCustPointHeader (Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField (rect, TextSht.gui_tp_cus_vec);
        }

        void AddCustPoint (ReorderableList list)
        {
            if (shat.custom.vector3 == null)
                shat.custom.vector3 = new List<Vector3>();
            shat.custom.vector3.Add (Vector3.zero);
            list.index = list.count;
            InteractiveChange();
        }

        void RemoveCustPoint (ReorderableList list)
        {
            if (shat.custom.vector3 != null)
            {
                shat.custom.vector3.RemoveAt (list.index);
                list.index = list.index - 1;
                InteractiveChange();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// ReorderableList Slice Transform
        /// /////////////////////////////////////////////////////////
       
        void DrawSliceTmListItems (Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_tp_slc_list.serializedProperty.GetArrayElementAtIndex (index);
            EditorGUI.PropertyField (new Rect (rect.x, rect.y + 2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        void DrawSliceTmHeader (Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField (rect, TextSht.gui_tp_cus_tms);
        }

        void AddSliceTm (ReorderableList list)
        {
            if (shat.slice.sliceList == null)
                shat.slice.sliceList = new List<Transform>();
            shat.slice.sliceList.Add (null);
            list.index = list.count;
            InteractiveChange();
        }

        void RemoveSliceTm (ReorderableList list)
        {
            if (shat.slice.sliceList != null)
            {
                shat.slice.sliceList.RemoveAt (list.index);
                list.index = list.index - 1;
                InteractiveChange();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Property change
        void InteractiveChange()
        {
            if (shat != null && shat.interactive == true)
            {
                // Apply changes
                serializedObject.ApplyModifiedProperties();
                
                // Refragment
                shat.InteractiveChange();
            }
        }
        
        // Set dirty
        void SetDirty (RayfireShatter scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
    }

    // Normalize quat in order to support Unity 2018.1
    public static class RFQuaternionExtension
    {
        public static Quaternion RFNormalize (this Quaternion q)
        {
            float f = 1f / Mathf.Sqrt (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion (q.x * f, q.y * f, q.z * f, q.w * f);
        }
    }
}

/*
 
 
            // Get inspector width
            // float width = EditorGUIUtility.currentViewWidth - 20f;

 
public class ExampleClass: EditorWindow
{
    GameObject gameObject;
    Editor     gameObjectEditor;

    [MenuItem("Example/GameObject Editor")]
    static void ShowWindow()
    {
        GetWindowWithRect<ExampleClass>(new Rect(0, 0, 256, 256));
    }

    void OnGUI()
    {
        gameObject = (GameObject) EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);

        GUIStyle bgColor = new GUIStyle();
        bgColor.normal.background = EditorGUIUtility.whiteTexture;

        if (gameObject != null)
        {
            if (gameObjectEditor == null)
                gameObjectEditor = Editor.CreateEditor(gameObject);

            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
        }
    }
}


[CustomPreview(typeof(GameObject))]
public class MyPreview : ObjectPreview
{
    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        GUI.Label(r, target.name + " is being previewed");
    }
}
*/