using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RayFireEditor
{
    /// <summary>
    /// Rayfire GUI class for most used GUI elements.
    /// </summary>
    public static class GUICommon
    {

        static          string[] tags;
        static readonly int      space      = 3;
        
        public const    string   str_add    = "Add";
        public const    string   str_remove = "Remove";
        public const    string   str_clear  = "Clear";
        public const    string   str_start  = "Start";
        public const    string   str_stop   = "Stop";
        public const    string   str_reset  = "Reset";

        // Colors
        public static readonly Color color_blue   = new Color (0.58f, 0.77f, 1f);
        public static readonly Color color_orange = new Color (1.0f,  0.60f, 0f);

        // Space between properties
        public static void Space()
        {
            GUILayout.Space (space);
        }

        // Properties caption
        public static void Caption(GUIContent caption)
        {
            GUILayout.Space (space);
            GUILayout.Space (space);
            GUILayout.Label (caption, EditorStyles.boldLabel);
            GUILayout.Space (space);
        }
        
        // Properties caption
        public static void CaptionBox(GUIContent caption)
        {
            GUILayout.Space (space);
            GUILayout.Space (space);
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label (caption, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space (space);
        }
        
        public static void BeginWindow()
        {
            EditorGUILayout.BeginVertical("window");
        }
        
        public static void EndWindow()
        {
            EditorGUILayout.EndVertical();
        }

        public static void PropertyField(SerializedProperty sp, GUIContent content)
        {
            EditorGUILayout.PropertyField (sp, content);
            Space();
        }

        public static void Slider(SerializedProperty sp, float min, float max, GUIContent content)
        {
            EditorGUILayout.Slider (sp, min, max, content);
            Space();
        }

        public static void IntSlider(SerializedProperty sp, int min, int max, GUIContent content)
        {
            EditorGUILayout.IntSlider (sp, min, max, content);
            Space();
        }

        public static void Foldout(ref bool val, string pref, string caption)
        {
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Foldout (val, caption, true);
            if (EditorGUI.EndChangeCheck() == true)
                EditorPrefs.SetBool (pref, val);
            Space();
        }

        public static void Foldout(ref bool val, string caption)
        {
            val = EditorGUILayout.Foldout (val, caption, true);
            Space();
        }

        public static void HelpBox(string str, MessageType type, bool wide)
        {
            EditorGUILayout.HelpBox (str, type, wide);
            Space();
        }

        public static void MaskField(SerializedProperty sp, GUIContent content)
        {
            EditorGUI.BeginChangeCheck();
            int mask = EditorGUILayout.MaskField (content, sp.intValue, InternalEditorUtility.layers);
            if (EditorGUI.EndChangeCheck())
                sp.intValue = mask;
            Space();
        }
        
        public static void LayerField(SerializedProperty sp, GUIContent content)
        {
            EditorGUI.BeginChangeCheck();
            int layer = EditorGUILayout.LayerField (content, sp.intValue);
            if (EditorGUI.EndChangeCheck())
                sp.intValue = layer;
            Space();
        }

        public static void SetTags()
        {
            tags = InternalEditorUtility.tags;
        }

        public static void TagField(SerializedProperty sp, GUIContent content)
        {
            int tagIndex = System.Array.IndexOf(tags, sp.stringValue);
            if (tagIndex == -1) 
                tagIndex = 0; 

            int newIndex = EditorGUILayout.Popup(content, tagIndex, tags);
            if (newIndex != tagIndex)
                sp.stringValue = tags[newIndex];
            Space();
        }
        
        public static void Toggle (SerializedProperty sp, GUIContent content, int height = 22)
        {
            bool newState = GUILayout.Toggle (sp.boolValue, content, "Button", GUILayout.Height (height));
            sp.boolValue = newState;
            Space();
        }
    }
    
    /// <summary>
    /// Rayfire plugin EditorPrefs keys for Foldout GUI elements.
    /// </summary>
    public static class TextKeys
    {
        // Bomb
        public const string bmb_fld_det = "rf_bd"; public const string bmb_fld_obs = "rf_bo";
        public const string bmb_fld_aud = "rf_bu"; public const string bmb_fld_fil = "rf_bf";
        
        // Connectivity
        public const string con_fld_clp = "rf_cc"; public const string con_fld_flt = "rf_cf";
        public const string con_fld_str = "rf_cs"; public const string con_fld_jnt = "rf_cj";

        // Debris
        public const string deb_fld_emt = "rf_de"; public const string deb_fld_dyn = "rf_dd";
        public const string deb_fld_nse = "rf_dn"; public const string deb_fld_col = "rf_dc"; 
        public const string deb_fld_lim = "rf_dl"; public const string deb_fld_rnd = "rf_dr";
        public const string deb_fld_pol = "rf_dp";
        
        // Dust
        public const string dst_fld_mat = "rf_um"; public const string dst_fld_emt = "rf_ue";
        public const string dst_fld_dyn = "rf_ud"; public const string dst_fld_nse = "rf_un";
        public const string dst_fld_col = "rf_uc"; public const string dst_fld_lim = "rf_ul";
        public const string dst_fld_rnd = "rf_ur"; public const string dst_fld_pol = "rf_up";
        
        // Man
        public const string man_fld_adv = "rf_ma";
        
        // Rigid 
        public const string rig_fld_phy = "rf_rp"; public const string rig_fld_act = "rf_ra";
        public const string rig_fld_lim = "rf_rl"; public const string rig_fld_msh = "rf_rm"; 
        public const string rig_fld_cls = "rf_rc"; public const string rig_fld_clp = "rf_rs";
        public const string rig_fld_ref = "rf_rr"; public const string rig_fld_mat = "rf_rt";
        public const string rig_fld_dmg = "rf_rd"; public const string rig_fld_fad = "rf_rf";
        public const string rig_fld_res = "rf_re";

        // RigidRoot
        public const string rot_fld_phy = "rf_tp"; public const string rot_fld_act = "rf_ta";
        public const string rot_fld_lim = "rf_tl"; public const string rot_fld_cls = "rf_tc";
        public const string rot_fld_clp = "rf_ts"; public const string rot_fld_fad = "rf_tf";
        public const string rot_fld_res = "rf_te";
        
        // Unyielding
        public const string uny_fld_ali = "rf_ua";
    }
    
    /// <summary>
    /// Rayfire Activator text for GUI elements.
    /// </summary>
    public static class TextAct
    {
        // Help
        public const string hlp_select = "Rayfire Activator works only for Rigid or RigidRoot components, enable at least one.";

        // String
        public const string str_rad = "Change Radius";
        public const string str_box = "Change Bounds";
        
        // Tooltips
        const string tlp_positionAnimation = 
            "By Global Position List: Use Position list of Vector3 points.Object will be animated from one point to another starting from the first point in global world coordinates.\n" +
            " By Static Line: Use predefined Line. Path will be cached at start. \n" +
            " By Dynamic Line: Use predefined Line. Path will be calculated at every frame by Line. \n" +
            " By Local Position List: Use Position list of Vector3 points. Object will be animated from one point to another starting from the first point in local coordinates.";
        const string tlp_type = " On Enter: Object will be activated when Activator trigger collider will enter object's collider.\n" +
                                " On Exit: Object will be activated when Activator trigger collider will exit object's collider.";

        // GUI
        public static readonly GUIContent gui_cap_components    = new GUIContent ("  Components",       "");
        public static readonly GUIContent gui_checkRigid        = new GUIContent ("Rigid",              "Activate objects with Rigid component with Inactive or Kinematic simulation type.");
        public static readonly GUIContent gui_checkRigidRoot    = new GUIContent ("RigidRoot",          "Activate RigidRoot component objects with Inactive or Kinematic simulation type.");
        public static readonly GUIContent gui_cap_gizmo         = new GUIContent ("  Gizmo",            "");
        public static readonly GUIContent gui_gizmoType         = new GUIContent ("Type:",              "Gizmo which will be used to create collider to activate objects.");
        public static readonly GUIContent gui_sphereRadius      = new GUIContent ("Radius",             "Defines size of Sphere gizmo.");
        public static readonly GUIContent gui_boxSize           = new GUIContent ("Size",               "Defines size of Box gizmo.");
        public static readonly GUIContent gui_showGizmo         = new GUIContent ("Show",               "");
        public static readonly GUIContent gui_cap_act           = new GUIContent ("  Activation",       "");
        public static readonly GUIContent gui_type              = new GUIContent ("Type:",              tlp_type);
        public static readonly GUIContent gui_delay             = new GUIContent ("Delay",              "Activation Delay in seconds.");
        public static readonly GUIContent gui_demolishCluster   = new GUIContent ("Demolish Cluster",   "Allows to demolish Connected Cluster and detach it's children into separate objects.");
        public static readonly GUIContent gui_cap_frc           = new GUIContent ("  Force",            "");
        public static readonly GUIContent gui_apply             = new GUIContent ("Apply",              "Add velocity and spin to activated objects");
        public static readonly GUIContent gui_velocity          = new GUIContent ("Velocity",           "Applied Velocity in world coordinates.");
        public static readonly GUIContent gui_spin              = new GUIContent ("Spin",               "Applied Angular Velocity in world coordinates.");
        public static readonly GUIContent gui_mode              = new GUIContent ("Mode",               "");
        public static readonly GUIContent gui_cap_anim          = new GUIContent ("  Animation",        "");
        public static readonly GUIContent gui_coord             = new GUIContent ("Local Space",        "");
        public static readonly GUIContent gui_showAnimation     = new GUIContent ("Show Animation",     "Show animation properties.");
        public static readonly GUIContent gui_duration          = new GUIContent ("Duration",           "Total animation duration.");
        public static readonly GUIContent gui_scaleAnimation    = new GUIContent ("Scale Animation",    "Animate scale of Activator gizmo.");
        public static readonly GUIContent gui_positionAnimation = new GUIContent ("Position Animation", tlp_positionAnimation);
        public static readonly GUIContent gui_line              = new GUIContent ("Line",               "Line which will be used as animation path.");
        public static readonly GUIContent gui_positionList      = new GUIContent ("Position List",      "List of Vector3 points in global space. Object will be animated from one point to another starting from the first point in list.");
        public static readonly GUIContent gui_cap_play          = new GUIContent ("  Playback",         "");
        public static readonly GUIContent gui_start             = new GUIContent ("Start",              "Evaluate StartAnimation() method.");
        public static readonly GUIContent gui_stop              = new GUIContent ("Stop",               "Evaluate StopAnimation() method.");
        public static readonly GUIContent gui_reset             = new GUIContent ("Reset",              "Evaluate ResetAnimation() method.");
    }
    
    /// <summary>
    /// Rayfire Blade text for GUI elements.
    /// </summary>
    public static class TextBld
    {
        // Tooltips
        const string tlp_act = "Slice: Object will be Sliced accordingly to Slice Type plane.\n" +
                               "Demolish: Object will demolished accordingly to it's Mesh or Cluster Demolition properties.";
        const string tlp_trig = " Enter: Object will be sliced when Blade's trigger collider enter object's collider.\n" +
                               " Exit: Object will be sliced when Blade's trigger collider exit object's collider.\n" +
                               " Enter Exit: Object will be sliced when Blade's trigger collider exit object's collider and angle for slicing plane will be average angle between enter and exit. " +
                               " This type should be used if object with Blade will be rotated while it is inside sliced object so slicing plane at least will have average angle.";
        const string tlp_skn = "In order to detect collider collision one of the objects has to have RigidBody component and " +
                               "Skinned Mesh object may not have RigidBody. " +
                               "when this property enabled Blade object will get its own kinematic RigidBody component" +
                               "to detect collision with skinned mesh objects.";
        const string tlp_targ = "Slicing also can be initiated by Slice Target button or by public SliceTarget() method. " +
                                "In this case object with Blade doesn't have to enter or exit sliced object collider, but you need to define Target Gameobject for slice.";
        
        // GUI
        public static readonly GUIContent gui_cap_prop       = new GUIContent ("  Properties",    "");
        public static readonly GUIContent gui_actionType     = new GUIContent ("Action",          tlp_act);
        public static readonly GUIContent gui_onTrigger      = new GUIContent ("On Trigger",      tlp_trig);
        public static readonly GUIContent gui_sliceType      = new GUIContent ("Slice Plane",     "Defines slicing plane which will be used to slice target object.");
        public static readonly GUIContent gui_damage         = new GUIContent ("Damage",          "Applies damage to sliced object.");
        public static readonly GUIContent gui_skin           = new GUIContent ("Skin",            tlp_skn);
        public static readonly GUIContent gui_cap_force      = new GUIContent ("  Force",         "");
        public static readonly GUIContent gui_force          = new GUIContent ("Force",           "Add to sliced fragments additional velocity impulse to separate them.");
        public static readonly GUIContent gui_affectInactive = new GUIContent ("Affect Inactive", "Force will be applied to Inactive objects as well.");
        public static readonly GUIContent gui_cap_filter     = new GUIContent ("  Filters",       "");
        public static readonly GUIContent gui_cooldown       = new GUIContent ("Cooldown",        "Allows to temporary disable Blade component for defined time to prevent constant slicing.");
        public static readonly GUIContent gui_tagFilter      = new GUIContent ("Tag",             "");
        public static readonly GUIContent gui_mask           = new GUIContent ("Mask",            "");
        public static readonly GUIContent gui_cap_targets    = new GUIContent ("  Targets",       "");
        public static readonly GUIContent gui_targ           = new GUIContent ("Target List",     tlp_targ);
        public static readonly GUIContent gui_btn_slice      = new GUIContent (" Slice Target ",  "Evaluate SliceTarget() method.");
    }
    
    /// <summary>
    /// Rayfire Bomb text for GUI elements.
    /// </summary>
    public static class TextBmb
    {
        // Foldout
        public static readonly GUIContent gui_fld_det = new GUIContent ("Detonation", "");
        public static readonly GUIContent gui_fld_obs = new GUIContent ("Obstacles",  "");
        public static readonly GUIContent gui_fld_aud = new GUIContent ("Audio",      "");
        public static readonly GUIContent gui_fld_fil = new GUIContent ("Filters",    "");
        
        // Buttons
        public static readonly GUIContent gui_btn_explode = new GUIContent ("Explode", "");
        public static readonly GUIContent gui_btn_restore = new GUIContent ("Restore", "");
        
        // GUI
        public static readonly GUIContent gui_cap_actions     = new GUIContent ("  Actions",     "");
        public static readonly GUIContent gui_cap_range       = new GUIContent ("  Range",       "");
        public static readonly GUIContent gui_showGizmo       = new GUIContent ("Show",          "");
        public static readonly GUIContent gui_rangeType       = new GUIContent ("Type",          "Explosion direction.");
        public static readonly GUIContent gui_range           = new GUIContent ("Range",         "Only objects in Range distance will be affected by explosion.");
        public static readonly GUIContent gui_boxSize         = new GUIContent ("Size",          "Directional explosion area size."); 
        public static readonly GUIContent gui_deletion        = new GUIContent ("Deletion",      "Destroy objects close to Bomb. Measures in percentage relative to Range value.");
        public static readonly GUIContent gui_cap_impulse     = new GUIContent ("  Impulse",     "");
        public static readonly GUIContent gui_fadeType        = new GUIContent ("Fade",          "Explosion strength decay over distance.");
        public static readonly GUIContent gui_strength        = new GUIContent ("Strength",      "Maximum explosion impulse which will be applied to objects.");
        public static readonly GUIContent gui_curve           = new GUIContent ("Curve",         "");
        public static readonly GUIContent gui_variation       = new GUIContent ("Variation",     "Random variation to final explosion strength for every object in percents relative to Strength value.");
        public static readonly GUIContent gui_chaos           = new GUIContent ("Chaos",         "Random rotation velocity to exploded objects.");
        public static readonly GUIContent gui_forceByMass     = new GUIContent ("Force By Mass", "Add different final explosion impulse to objects with different mass.");
        public static readonly GUIContent gui_cap_act         = new GUIContent ("  Activation",  "");
        public static readonly GUIContent gui_affectInactive  = new GUIContent ("Inactive",      "Activate Inactive objects and explode them as well.");
        public static readonly GUIContent gui_affectKinematic = new GUIContent ("Kinematic",     "Activate Kinematic objects and explode them as well.");
        public static readonly GUIContent gui_cap_dmg         = new GUIContent ("  Damage",      "");
        public static readonly GUIContent gui_applyDamage     = new GUIContent ("Apply",         "Apply damage to objects with Rigid component in case they have enabled Damage.");
        public static readonly GUIContent gui_damageValue     = new GUIContent ("Value",         "Damage value  which will take object at explosion.");
        public static readonly GUIContent gui_cap_prop        = new GUIContent ("  Properties",  "");
        public static readonly GUIContent gui_heightOffset    = new GUIContent ("Height Offset", "Allows to offset downward Explosion position over global Y axis.");
        public static readonly GUIContent gui_delay           = new GUIContent ("Delay",         "Explosion delay in seconds.");
        public static readonly GUIContent gui_atStart         = new GUIContent ("At Start",      "Automatically explode Bomb at Gameobject activation.");
        public static readonly GUIContent gui_destroy         = new GUIContent ("Destroy",       "Destroy Gameobject after explosion.");
        public static readonly GUIContent gui_obst_enable     = new GUIContent ("Enable",        "Enable other colliders in scene as obstacles for explosion.");
        public static readonly GUIContent gui_obst_static     = new GUIContent ("Static",        "Use all colliders without RigidBody component as obstacle.");
        public static readonly GUIContent gui_obst_kinematik  = new GUIContent ("Kinematik",     "Use all colliders with Kinematik RigidBody as obstacles for explosion.");
        public static readonly GUIContent gui_obst_list       = new GUIContent ("Collider List", "Colliders you want to use as obstacles");
        public static readonly GUIContent gui_play            = new GUIContent ("Play",          "Play audio clip at explosion.");
        public static readonly GUIContent gui_volume          = new GUIContent ("Volume",        "");
        public static readonly GUIContent gui_clip            = new GUIContent ("Clip",          "Audio Clip to play at explosion.");
        public static readonly GUIContent gui_tagFilter       = new GUIContent ("Tag",           "");
        public static readonly GUIContent gui_mask            = new GUIContent ("Mask",          "");
    }
    
    /// <summary>
    /// Rayfire Combine text for GUI elements.
    /// </summary>
    public static class TextCmb
    {
        // Buttons
        public static readonly GUIContent gui_btn_combine = new GUIContent ("Combine",     "");
        public static readonly GUIContent gui_btn_export  = new GUIContent ("Export Mesh", "");
        
        // GUI
        public static readonly GUIContent gui_cap_source      = new GUIContent ("  Source",         "");
        public static readonly GUIContent gui_type            = new GUIContent ("Type",             "");
        public static readonly GUIContent gui_cap_mesh        = new GUIContent ("  Mesh Source",    "");
        public static readonly GUIContent gui_objects         = new GUIContent ("Objects List",     "");
        public static readonly GUIContent gui_meshFilters     = new GUIContent ("Mesh Filters",     "");
        public static readonly GUIContent gui_skinnedMeshes   = new GUIContent ("Skinned Meshes",   "");
        public static readonly GUIContent gui_particleSystems = new GUIContent ("Particle Systems", "");
        public static readonly GUIContent gui_cap_filters     = new GUIContent ("  Filters",        "");
        public static readonly GUIContent gui_sizeThreshold   = new GUIContent ("Size",             "Do not combine meshes with size less than defined value");
        public static readonly GUIContent gui_vertexThreshold = new GUIContent ("Vertices",         "Do not combine meshes with amount of vertices less than defined value");
        public static readonly GUIContent gui_cap_index       = new GUIContent ("  Combined Mesh",  "");
        public static readonly GUIContent gui_indexFormat     = new GUIContent ("Index Format",     "Mesh with more than 65k vertices should use 32 bit Index Format");
        public static readonly GUIContent gui_cap_export      = new GUIContent ("  Export",         "");
    }
    
    /// <summary>
    /// Rayfire Connectivity text for GUI elements.
    /// </summary>
    public static class TextCnt
    {
        // Strings
        public const string str_setup  = "  Setup Info";
        public const string str_shards = "Cluster Shards: ";
        public const string str_amount = "Amount Integrity: ";

        // Foldout
        public static readonly GUIContent gui_fld_filters = new GUIContent ("Filters",    "");
        public static readonly GUIContent gui_fld_props   = new GUIContent ("Properties", "");
        
        // Buttons
        public static readonly GUIContent gui_btn_gizmo     = new GUIContent (" Show Gizmo ",     "");
        public static readonly GUIContent gui_btn_cnt       = new GUIContent ("Show Connections", "");
        public static readonly GUIContent gui_btn_nodes     = new GUIContent ("Show Nodes",       "");
        public static readonly GUIContent gui_btn_clp_start = new GUIContent ("Start Collapse",   "Evaluate public static void RFCollapse.StartCollapse (RayfireConnectivity component) method");
        public static readonly GUIContent gui_btn_clp_stop  = new GUIContent ("Stop Collapse",    "Evaluate public static void RFCollapse.StopCollapse (RayfireConnectivity component) method");
        public static readonly GUIContent gui_btn_fracture  = new GUIContent ("Fracture",         "Evaluate public void Fracture (Collider collider, int debris) Connectivity method");
        public static readonly GUIContent gui_btn_str_start = new GUIContent ("Start Stress",     "Evaluate public static void RFCollapse.StartStress (RayfireConnectivity component) method");
        public static readonly GUIContent gui_btn_str_stop  = new GUIContent ("Stop Stress",      "Evaluate public static void RFCollapse.StopStress (RayfireConnectivity component) method");
        
        // Tooltips
        const string tlp_clp_init = "Collapse allows you start break connections among shards and activate single Shards or " +
                                    "Group of Shards if they are not connected with any of Unyielding Shard. ";
        
        // GUI
        public static readonly GUIContent gui_cap_conn        = new GUIContent ("  Connectivity", "");
        public static readonly GUIContent gui_type            = new GUIContent ("Type",           "Define the the way connections among Shards will be calculated.");
        public static readonly GUIContent gui_expand          = new GUIContent ("Expand",         "Increase size of bounding box for By Bounding Box types.");
        public static readonly GUIContent gui_minimumArea     = new GUIContent ("Minimum Area",   "Two shards will have connection if their shared area is bigger than this value.");
        public static readonly GUIContent gui_minimumSize     = new GUIContent ("Minimum Size",   "Two shards will have connection if their size is bigger than this value.");
        public static readonly GUIContent gui_percentage      = new GUIContent ("Percentage",     "Random percentage of connections will be discarded.");
        public static readonly GUIContent gui_seed            = new GUIContent ("  Seed",         "Seed for random percentage.");
        public static readonly GUIContent gui_cap_cluster     = new GUIContent ("  Cluster",      "");
        public static readonly GUIContent gui_clusterize      = new GUIContent ("Clusterize",     "Create Connected Cluster for group of Shards connected with each other but not connected with any Unyielding Shard.");
        public static readonly GUIContent gui_demolishable    = new GUIContent ("Demolishable",   "Set Demolition type to Runtime for Connected Clusters created during activation.");
        public static readonly GUIContent gui_cap_collapse    = new GUIContent ("  Collapse",     "");
        public static readonly GUIContent gui_startCollapse   = new GUIContent ("Initiate",       tlp_clp_init);
        public static readonly GUIContent gui_integrity       = new GUIContent ("Integrity",      "");
        public static readonly GUIContent gui_cap_stress      = new GUIContent ("  Stress",       "");
        public static readonly GUIContent gui_showStress      = new GUIContent ("Preview",        "");
        public static readonly GUIContent gui_startStress     = new GUIContent ("Initiate",       "");
        public static readonly GUIContent gui_cap_fracture    = new GUIContent ("  Fracture",     "");
        public static readonly GUIContent gui_triggerCollider = new GUIContent ("Trigger",        "Trigger Collider which will activate all overlapped shards. Only Box and Sphere colliders supported for now.");
        public static readonly GUIContent gui_triggerDebris   = new GUIContent ("Debris",         "Percentage of solo shards at the edges of fractured cluster.");
        public static readonly GUIContent gui_cap_joint       = new GUIContent ("  Joints WIP",       "");    }

    /// <summary>
    /// RFCollapse text for GUI elements.
    /// </summary>
    public static class TextClp
    {
        // Strings
        public const string str_area = "By Area:";
        public const string str_size = "By Size:";
        public const string str_rand = "Random:";
        
        // Tooltips
        const string tlp_type = " By Area: Shard will loose it's connections if it's shared area surface is less then defined value.\n" +
                                " By Size: Shard will loose it's connections if it's Size is less then defined value.\n" +
                                " Random: Shard will loose it's connections if it's random value in range from 0 to 100 is less then defined value.";
        
        public static readonly GUIContent gui_type     = new GUIContent ("Type",      tlp_type);
        public static readonly GUIContent gui_start    = new GUIContent ("Start",     "Defines start value in percentage relative to whole range of picked type.");
        public static readonly GUIContent gui_end      = new GUIContent ("End",       "Defines end value in percentage relative to whole range of picked type.");
        public static readonly GUIContent gui_steps    = new GUIContent ("Steps",     "Amount of times when defined threshold value will be set during Duration period.");
        public static readonly GUIContent gui_duration = new GUIContent ("Duration",  "Time which it will take Start value to be increased to End value.");
        public static readonly GUIContent gui_var      = new GUIContent ("Variation", "Percentage Variation for By Area and By Size collapse.");
        public static readonly GUIContent gui_seed     = new GUIContent ("Seed",      "Seed for Random collapse.");
    }

    /// <summary>
    /// RFStress text for GUI elements.
    /// </summary>
    public static class TextStr
    {
        public static readonly GUIContent gui_enable     = new GUIContent ("Enable",            "");
        public static readonly GUIContent gui_cap_conn   = new GUIContent ("      Connections", "");
        public static readonly GUIContent gui_threshold  = new GUIContent ("Threshold",         "Amount of stress every connection can take to break.");
        public static readonly GUIContent gui_erosion    = new GUIContent ("Erosion",           "Multiplier for stress which get connection every Interval.");
        public static readonly GUIContent gui_interval   = new GUIContent ("Interval",          "Connection stress will be increased every Interval. Measures in Seconds.");
        public static readonly GUIContent gui_cap_shards = new GUIContent ("      Shards",      "");
        public static readonly GUIContent gui_support    = new GUIContent ("Support",           "Angle to define which shards above Shard should be considered as supported shards.");
        public static readonly GUIContent gui_exposed    = new GUIContent ("Exposed",           "Erode connections only for shards which lost their neighbor.");
        public static readonly GUIContent gui_bySize     = new GUIContent ("By Size",           "");
    }

    /// <summary>
    /// Rayfire Debris text for GUI elements.
    /// </summary>
    public static class TextDbr
    {
        // Help
        public const string hlp_select = "Rayfire Debris has disabled Emit Events, enable at least one.";

        // Strings
        public const string str_avail = "     Available : ";
        
        // Foldout
        public static readonly GUIContent gui_fld_pol = new GUIContent ("Pool",        "");
        public static readonly GUIContent gui_fld_emt = new GUIContent ("Emission",    "");
        public static readonly GUIContent gui_fld_dyn = new GUIContent ("Dynamic",     "");
        public static readonly GUIContent gui_fld_nse = new GUIContent ("Noise",       "");
        public static readonly GUIContent gui_fld_col = new GUIContent ("Collision",   "");
        public static readonly GUIContent gui_fld_lim = new GUIContent ("Limitations", "");
        public static readonly GUIContent gui_fld_ren = new GUIContent ("Rendering", "");
        
        // Buttons
        public static readonly GUIContent gui_btn_emit  = new GUIContent ("Emit",  "");
        public static readonly GUIContent gui_btn_clean = new GUIContent ("Clean", "");
        public static readonly GUIContent gui_btn_edit = new GUIContent ("Edit Emitter Particles", "Evaluate EditEmitterParticles() method.");
        
        // GUI
        public static readonly GUIContent gui_cap_event     = new GUIContent ("  Emit Event",           "");
        public static readonly GUIContent gui_main_dml      = new GUIContent ("Demolition",             "Emit particles when object with Rayfire Rigid component demolishes. Particles will be emitted from fragments.");
        public static readonly GUIContent gui_main_act      = new GUIContent ("Activation",             "Emit particles when object with Rayfire Rigid activated to Dynamic state from Inactive or Kinematik states.");
        public static readonly GUIContent gui_main_imp      = new GUIContent ("Impact",                 "Emit particles by Rayfire Gun shot at impact position.");
        public static readonly GUIContent gui_cap_debris    = new GUIContent ("  Debris",               "");
        public static readonly GUIContent gui_main_ref      = new GUIContent ("Reference",              "Debris reference");
        public static readonly GUIContent gui_main_mat      = new GUIContent ("Material",               "Debris material");
        public static readonly GUIContent gui_cap_props     = new GUIContent ("  Properties",           "");
        public static readonly GUIContent gui_pol_id        = new GUIContent ("Id",                     "Emitter Pool Id. All Debris components on other objects will share particle pool with the same id.");
        public static readonly GUIContent gui_pol_en        = new GUIContent ("Enable",                 "Generate Capacity amount of particle emitters in pool. One particle emitter will be created anyway, even if pooling is disabled.");
        public static readonly GUIContent gui_pol_war       = new GUIContent ("Warmup",                 "Create all pool particles in Awake");
        public static readonly GUIContent gui_pol_cap       = new GUIContent ("Capacity",               "Maximum amount of particle emitters that can be generated in pool");
        public static readonly GUIContent gui_pol_rat       = new GUIContent ("Rate",                   "Amount of particle systems that will be instantiated in pool every frame");
        public static readonly GUIContent gui_pol_skp       = new GUIContent ("Skip",                   "Do not instantiate debris particles if there are no any particles in the pool.");
        public static readonly GUIContent gui_pol_reu       = new GUIContent ("Reuse",                  "Do not destroy particle emitters and move them back to pool so they will be reused.");
        public static readonly GUIContent gui_pol_ovf       = new GUIContent ("   Overflow",            "Amount of particle emitters that can be send back to pool on top of Capacity property amount in case pool is full");
        public static readonly GUIContent gui_cap_burst     = new GUIContent ("      Burst",            "");
        public static readonly GUIContent gui_ems_tp        = new GUIContent ("Type",                   "");
        public static readonly GUIContent gui_ems_am        = new GUIContent ("Amount",                 "");
        public static readonly GUIContent gui_ems_var       = new GUIContent ("Variation",              "");
        public static readonly GUIContent gui_cap_dist      = new GUIContent ("      Distance",         "");
        public static readonly GUIContent gui_ems_rate      = new GUIContent ("Rate",                   "");
        public static readonly GUIContent gui_ems_dur       = new GUIContent ("Duration",               "");
        public static readonly GUIContent gui_cap_life      = new GUIContent ("      Lifetime",         "");
        public static readonly GUIContent gui_ems_life_min  = new GUIContent ("Life Min",               "");
        public static readonly GUIContent gui_ems_life_max  = new GUIContent ("Life Max",               "");
        public static readonly GUIContent gui_cap_size      = new GUIContent ("      Size",             "");
        public static readonly GUIContent gui_ems_size_min  = new GUIContent ("Size Min",               "");
        public static readonly GUIContent gui_ems_size_max  = new GUIContent ("Size Max",               "");
        public static readonly GUIContent gui_cap_mat       = new GUIContent ("      Material",         "");
        public static readonly GUIContent gui_ems_mat       = new GUIContent ("Material",               "");
        public static readonly GUIContent gui_cap_speed     = new GUIContent ("      Speed",            "");
        public static readonly GUIContent gui_dyn_speed_min = new GUIContent ("Speed Min",              "");
        public static readonly GUIContent gui_dyn_speed_max = new GUIContent ("Speed Max",              "");
        public static readonly GUIContent gui_cap_vel       = new GUIContent ("      Inherit Velocity", "");
        public static readonly GUIContent gui_dyn_vel_min   = new GUIContent ("Velocity Min",           "");
        public static readonly GUIContent gui_dyn_vel_max   = new GUIContent ("Velocity Max",           "");
        public static readonly GUIContent gui_cap_grav      = new GUIContent ("      Gravity",          "");
        public static readonly GUIContent gui_dyn_grav_min  = new GUIContent ("Gravity Min",            "");
        public static readonly GUIContent gui_dyn_grav_max  = new GUIContent ("Gravity Max",            "");
        public static readonly GUIContent gui_cap_rot       = new GUIContent ("      Rotation",         "");
        public static readonly GUIContent gui_dyn_rot       = new GUIContent ("Rotation Speed",         "");
        public static readonly GUIContent gui_cap_main      = new GUIContent ("      Main",             "");
        public static readonly GUIContent gui_nse_en        = new GUIContent ("Enable",                 "");
        public static readonly GUIContent gui_nse_qual      = new GUIContent ("Quality",                "");
        public static readonly GUIContent gui_cap_str       = new GUIContent ("      Strength",         "");
        public static readonly GUIContent gui_nse_str_min   = new GUIContent ("Strength Min",           "");
        public static readonly GUIContent gui_nse_str_max   = new GUIContent ("Strength Max",           "");
        public static readonly GUIContent gui_cap_other     = new GUIContent ("      Other",            "");
        public static readonly GUIContent gui_nse_freq      = new GUIContent ("Frequency",              "");
        public static readonly GUIContent gui_nse_scroll    = new GUIContent ("Scroll Speed",           "");
        public static readonly GUIContent gui_nse_damp      = new GUIContent ("Damping",                "");
        public static readonly GUIContent gui_cap_common    = new GUIContent ("      Common",           "");
        public static readonly GUIContent gui_col_mask      = new GUIContent ("Collides With",          "");
        public static readonly GUIContent gui_col_qual      = new GUIContent ("Quality",                "");
        public static readonly GUIContent gui_col_rad       = new GUIContent ("Radius Scale",           "");
        public static readonly GUIContent gui_cap_dampen    = new GUIContent ("      Dampen",           "");
        public static readonly GUIContent gui_col_dmp_tp    = new GUIContent ("Type",                   "");
        public static readonly GUIContent gui_col_dmp_min   = new GUIContent ("Minimum",                "");
        public static readonly GUIContent gui_col_dmp_max   = new GUIContent ("Maximum",                "");
        public static readonly GUIContent gui_cap_bounce    = new GUIContent ("      Bounce",           "");
        public static readonly GUIContent gui_col_bnc_tp    = new GUIContent ("Type",                   "");
        public static readonly GUIContent gui_col_bnc_min   = new GUIContent ("Minimum",                "");
        public static readonly GUIContent gui_col_bnc_max   = new GUIContent ("Maximum",                "");
        public static readonly GUIContent gui_cap_prt       = new GUIContent ("      Particle System",  "");
        public static readonly GUIContent gui_lim_prt_min   = new GUIContent ("Min Particles",          "");
        public static readonly GUIContent gui_lim_prt_max   = new GUIContent ("Max Particles",          "");
        public static readonly GUIContent gui_lim_prt_vis   = new GUIContent ("Visible",                "Emit debris if emitting object is visible in camera view");
        public static readonly GUIContent gui_cap_frags     = new GUIContent ("      Fragments",        "");
        public static readonly GUIContent gui_lim_perc      = new GUIContent ("Percentage",             "");
        public static readonly GUIContent gui_lim_size      = new GUIContent ("Size Threshold",         "");
        public static readonly GUIContent gui_cap_shad      = new GUIContent ("      Shadows",          "");
        public static readonly GUIContent gui_ren_cast      = new GUIContent ("Cast",                   "");
        public static readonly GUIContent gui_ren_rec       = new GUIContent ("Receive",                "");
        public static readonly GUIContent gui_ren_prob      = new GUIContent ("Light Probes",           "");
        public static readonly GUIContent gui_ren_vect      = new GUIContent ("Motion Vectors",         "");
        public static readonly GUIContent gui_ren_t         = new GUIContent ("Set Tag",                "");
        public static readonly GUIContent gui_ren_tag       = new GUIContent ("    Tag",                    "");
        public static readonly GUIContent gui_ren_l         = new GUIContent ("Set Layer",              "");
        public static readonly GUIContent gui_ren_lay       = new GUIContent ("    Layer",                  "");
    }
    
    /// <summary>
    /// Rayfire Dust text for GUI elements.
    /// </summary>
    public static class TextDst
    {
        // Help
        public const string hlp_select = "Rayfire Dust has disabled Emit Events, enable at least one.";
        
        // Strings
        public const string str_avail = "     Available : ";
        
        // Foldout
        public static readonly GUIContent gui_fld_pol = new GUIContent ("Pool",        "");
        public static readonly GUIContent gui_fld_emt = new GUIContent ("Emission",    "");
        public static readonly GUIContent gui_fld_dyn = new GUIContent ("Dynamic",     "");
        public static readonly GUIContent gui_fld_nse = new GUIContent ("Noise",       "");
        public static readonly GUIContent gui_fld_col = new GUIContent ("Collision",   "");
        public static readonly GUIContent gui_fld_lim = new GUIContent ("Limitations", "");
        public static readonly GUIContent gui_fld_ren = new GUIContent ("Rendering",   "");
        
        // Buttons
        public static readonly GUIContent gui_btn_emit  = new GUIContent ("Emit",                   "");
        public static readonly GUIContent gui_btn_clean = new GUIContent ("Clean",                  "");
        public static readonly GUIContent gui_btn_edit  = new GUIContent ("Edit Emitter Particles", "Evaluate EditEmitterParticles() method.");
        
        // GUI
        public static readonly GUIContent gui_cap_event     = new GUIContent ("  Emit Event",          "");
        public static readonly GUIContent gui_main_dml      = new GUIContent ("Demolition",            "Emit particles when object with Rayfire Rigid component demolishes. Particles will be emitted from fragments.");
        public static readonly GUIContent gui_main_act      = new GUIContent ("Activation",            "Emit particles when object with Rayfire Rigid activated to Dynamic state from Inactive or Kinematik states.");
        public static readonly GUIContent gui_main_imp      = new GUIContent ("Impact",                "Emit particles by Rayfire Gun shot at impact position.");
        public static readonly GUIContent gui_cap_dust      = new GUIContent ("  Dust",                "");
        public static readonly GUIContent gui_main_op       = new GUIContent ("Opacity",               "");
        public static readonly GUIContent gui_main_mat      = new GUIContent ("Material",              "");
        public static readonly GUIContent gui_main_mat_list = new GUIContent ("Random Materials",      "");
        public static readonly GUIContent gui_cap_props     = new GUIContent ("  Properties",          "");
        public static readonly GUIContent gui_pol_id        = new GUIContent ("Id",                    "Emitter Pool Id. All Dust components on other objects will share particle pool with the same id.");
        public static readonly GUIContent gui_pol_en        = new GUIContent ("Enable",                "Generate Capacity amount of particle emitters in pool. One particle emitter will be created anyway, even if pooling is disabled.");
        public static readonly GUIContent gui_pol_war       = new GUIContent ("Warmup",                "Create all pool particles in Awake");
        public static readonly GUIContent gui_pol_cap       = new GUIContent ("Capacity",              "Maximum amount of particle emitters that can be generated in pool");
        public static readonly GUIContent gui_pol_rat       = new GUIContent ("Rate",                  "Amount of particle systems that will be instantiated in pool every frame");
        public static readonly GUIContent gui_pol_skp       = new GUIContent ("Skip",                  "Do not instantiate debris particles if there are no any particles in the pool.");
        public static readonly GUIContent gui_pol_reu       = new GUIContent ("Reuse",                 "Do not destroy particle emitters and move them back to pool so they will be reused.");
        public static readonly GUIContent gui_pol_ovf       = new GUIContent ("   Overflow",           "Amount of particle emitters that can be send back to pool on top of Capacity property amount in case pool is full");
        public static readonly GUIContent gui_cap_burst     = new GUIContent ("      Burst",           "");
        public static readonly GUIContent gui_ems_tp        = new GUIContent ("Type",                  "");
        public static readonly GUIContent gui_ems_am        = new GUIContent ("Amount",                "");
        public static readonly GUIContent gui_ems_var       = new GUIContent ("Variation",             "");
        public static readonly GUIContent gui_cap_dist      = new GUIContent ("      Distance",        "");
        public static readonly GUIContent gui_ems_rate      = new GUIContent ("Rate",                  "");
        public static readonly GUIContent gui_ems_dur       = new GUIContent ("Duration",              "");
        public static readonly GUIContent gui_cap_life      = new GUIContent ("      Lifetime",        "");
        public static readonly GUIContent gui_ems_life_min  = new GUIContent ("Life Min",              "");
        public static readonly GUIContent gui_ems_life_max  = new GUIContent ("Life Max",              "");
        public static readonly GUIContent gui_cap_size      = new GUIContent ("      Size",            "");
        public static readonly GUIContent gui_ems_size_min  = new GUIContent ("Size Min",              "");
        public static readonly GUIContent gui_ems_size_max  = new GUIContent ("Size Max",              "");
        public static readonly GUIContent gui_cap_mat       = new GUIContent ("      Material",        "");
        public static readonly GUIContent gui_ems_mat       = new GUIContent ("Material",              "");
        public static readonly GUIContent gui_cap_speed     = new GUIContent ("      Speed",           "");
        public static readonly GUIContent gui_dyn_speed_min = new GUIContent ("Speed Min",             "");
        public static readonly GUIContent gui_dyn_speed_max = new GUIContent ("Speed Max",             "");
        public static readonly GUIContent gui_cap_grav      = new GUIContent ("      Gravity",         "");
        public static readonly GUIContent gui_dyn_grav_min  = new GUIContent ("Gravity Min",           "");
        public static readonly GUIContent gui_dyn_grav_max  = new GUIContent ("Gravity Max",           "");
        public static readonly GUIContent gui_cap_rot       = new GUIContent ("      Rotation",        "");
        public static readonly GUIContent gui_dyn_rot       = new GUIContent ("Rotation Speed",        "");
        public static readonly GUIContent gui_cap_main      = new GUIContent ("      Main",            "");
        public static readonly GUIContent gui_nse_en        = new GUIContent ("Enable",                "");
        public static readonly GUIContent gui_nse_qual      = new GUIContent ("Quality",               "");
        public static readonly GUIContent gui_cap_str       = new GUIContent ("      Strength",        "");
        public static readonly GUIContent gui_nse_str_min   = new GUIContent ("Strength Min",          "");
        public static readonly GUIContent gui_nse_str_max   = new GUIContent ("Strength Max",          "");
        public static readonly GUIContent gui_cap_other     = new GUIContent ("      Other",           "");
        public static readonly GUIContent gui_nse_freq      = new GUIContent ("Frequency",             "");
        public static readonly GUIContent gui_nse_scroll    = new GUIContent ("Scroll Speed",          "");
        public static readonly GUIContent gui_nse_damp      = new GUIContent ("Damping",               "");
        public static readonly GUIContent gui_cap_common    = new GUIContent ("      Common",          "");
        public static readonly GUIContent gui_col_mask      = new GUIContent ("Collides With",         "");
        public static readonly GUIContent gui_col_qual      = new GUIContent ("Quality",               "");
        public static readonly GUIContent gui_col_rad       = new GUIContent ("Radius Scale",          "");
        public static readonly GUIContent gui_cap_prt       = new GUIContent ("      Particle System", "");
        public static readonly GUIContent gui_lim_prt_min   = new GUIContent ("Min Particles",         "");
        public static readonly GUIContent gui_lim_prt_max   = new GUIContent ("Max Particles",         "");
        public static readonly GUIContent gui_lim_prt_vis   = new GUIContent ("Visible",               "Emit debris if emitting object is visible in camera view");
        public static readonly GUIContent gui_cap_frags     = new GUIContent ("      Fragments",       "");
        public static readonly GUIContent gui_lim_perc      = new GUIContent ("Percentage",            "");
        public static readonly GUIContent gui_lim_size      = new GUIContent ("Size Threshold",        "");
        public static readonly GUIContent gui_cap_shad      = new GUIContent ("      Shadows",         "");
        public static readonly GUIContent gui_ren_cast      = new GUIContent ("Cast",                  "");
        public static readonly GUIContent gui_ren_rec       = new GUIContent ("Receive",               "");
        public static readonly GUIContent gui_ren_prob      = new GUIContent ("Light Probes",          "");
        public static readonly GUIContent gui_ren_vect      = new GUIContent ("Motion Vectors",        "");
        public static readonly GUIContent gui_ren_t         = new GUIContent ("Set Tag",               "");
        public static readonly GUIContent gui_ren_tag       = new GUIContent ("Tag",                   "");
        public static readonly GUIContent gui_ren_l         = new GUIContent ("Set Layer",             "");
        public static readonly GUIContent gui_ren_lay       = new GUIContent ("Layer",                 "");
    }
    
    /// <summary>
    /// Rayfire Gun text for GUI elements.
    /// </summary>
    public static class TextGun
    {
        // Buttons
        public static readonly GUIContent gui_btn_shooting  = new GUIContent ("Start Shooting",  "");
        public static readonly GUIContent gui_btn_shot  = new GUIContent ("Single Shot",  "");
        public static readonly GUIContent gui_btn_burst = new GUIContent ("    Burst   ", "");
        
        // Tooltips
        const string tlp_dmg_shtp = "Single Shard: Damage will be applied to single shard intersected by shooting ray. \n" +
                                    "Shards In Impact Radius: Damage will be applied to all shards in impact radius.";
        
        // GUI
        public static readonly GUIContent gui_cap_dir     = new GUIContent ("  Direction",      "");
        public static readonly GUIContent gui_dir_show    = new GUIContent ("Show",             "Show shooting ray");
        public static readonly GUIContent gui_dir_axis    = new GUIContent ("Axis",             "Shooting direction if Target is not defined.");
        public static readonly GUIContent gui_dir_targ    = new GUIContent ("Target",           "");
        public static readonly GUIContent gui_dir_dist    = new GUIContent ("Distance",         "Maximum shooting distance.");
        public static readonly GUIContent gui_cap_bur     = new GUIContent ("  Burst",          "");
        public static readonly GUIContent gui_bur_rnd     = new GUIContent ("Rounds",           "");
        public static readonly GUIContent gui_bur_rate    = new GUIContent ("Rate",             "");
        public static readonly GUIContent gui_cap_imp     = new GUIContent ("  Impact",         "");
        public static readonly GUIContent gui_imp_show    = new GUIContent ("Show",             "Show impact position and radius. Visible when shooting ray intersects with collider.");
        public static readonly GUIContent gui_imp_tp      = new GUIContent ("Type",             "");
        public static readonly GUIContent gui_imp_str     = new GUIContent ("Strength",         "");
        public static readonly GUIContent gui_imp_rad     = new GUIContent ("Radius",           "");
        public static readonly GUIContent gui_imp_ofs     = new GUIContent ("Offset",           "Negative value offset Impact point towards Gun position. Positive farther from Gun position");
        public static readonly GUIContent gui_imp_cls     = new GUIContent ("Demolish Cluster", "");
        public static readonly GUIContent gui_imp_ina     = new GUIContent ("Affect Inactive",  "");
        public static readonly GUIContent gui_cap_comp    = new GUIContent ("  Components",     "");
        public static readonly GUIContent gui_comp_rg     = new GUIContent ("Rigid",            "");
        public static readonly GUIContent gui_comp_rt     = new GUIContent ("RigidRoot",        "");
        public static readonly GUIContent gui_comp_rb     = new GUIContent ("RigidBody",        "");
        public static readonly GUIContent gui_cap_dmg     = new GUIContent ("  Damage",         "");
        public static readonly GUIContent gui_dmg_val     = new GUIContent ("Damage",           "");
        public static readonly GUIContent gui_dmg_shtp    = new GUIContent ("Per Shard",        tlp_dmg_shtp);
        public static readonly GUIContent gui_cap_vfx     = new GUIContent ("  VFX",            "");
        public static readonly GUIContent gui_vfx_debris  = new GUIContent ("Debris",           "");
        public static readonly GUIContent gui_vfx_dust    = new GUIContent ("Dust",             "");
        public static readonly GUIContent gui_vfx_flash   = new GUIContent ("Flash",            "");
        public static readonly GUIContent gui_cap_int     = new GUIContent ("      Intensity",  "");
        public static readonly GUIContent gui_fl_int_min  = new GUIContent ("Minimum",          "");
        public static readonly GUIContent gui_fl_int_max  = new GUIContent ("Maximum",          "");
        public static readonly GUIContent gui_cap_rng     = new GUIContent ("      Range",      "");
        public static readonly GUIContent gui_fl_rng_min  = new GUIContent ("Minimum",          "");
        public static readonly GUIContent gui_fl_rng_max  = new GUIContent ("Maximum",          "");
        public static readonly GUIContent gui_cap_other   = new GUIContent ("      Other",      "");
        public static readonly GUIContent gui_fl_distance = new GUIContent ("Distance",         "");
        public static readonly GUIContent gui_fl_color    = new GUIContent ("Color",            "");
        public static readonly GUIContent gui_cap_filt    = new GUIContent ("  Filters",      "");
        public static readonly GUIContent gui_tag         = new GUIContent ("Tag",              "");
        public static readonly GUIContent gui_mask        = new GUIContent ("Mask",            "");
    }
    
    /// <summary>
    /// Rayfire Man text for GUI elements.
    /// </summary>
    public static class TextMan
    {
        // Strings
        public const string str_prt_emit   = "    Emitters: ";
        public const string str_prt_amount = "    Particles: ";
        public const string str_prt_reu    = "    Reused: ";
        public const string str_prt_scene  = "    In Scene: ";
        public const string str_info       = "    Info: ";
        public const string str_rigs       = "Rigid Pool: ";
        public const string str_frags      = "Fragments: ";
        public const string str_vel        = "Velocity Cache: ";
        public const string str_build      = "Plugin build: ";
        public const string str_url        = "https://assetstore.unity.com/packages/tools/game-toolkits/rayfire-for-unity-148690#releases";

        // Buttons
        public static readonly GUIContent gui_btn_dest_frags = new GUIContent ("Destroy Storage Fragments", "");
        
        // Tooltips
        const string tlp_ph_cop = "Used only when Planar Check enabled. All meshes with vertices amount less than defined value will perform planar check. " + 
                                  "If all vertices lay ALMOST on a plane then object will not get collider to avoid convex hull generation errors. ";
        const string tlp_mat_min = "Minimum mass value which will be assigned to simulated object" +
                                   " if it's mass calculated by it's volume and density will be less than this value.";
        const string tlp_mat_max = "Maximum mass value which will be assigned to simulated object" +
                                   " if it's mass calculated by it's volume and density will be higher than this value.";
        const string tlp_mat_mat = "Physic material which will be used for all objects with this material. " +
                                   "If Material is not defined then it will be created and defined here at Start using following Frictions and Bounciness properties.";
        const string tlp_dml_time = "Demolition time quota in milliseconds. Allows to prevent demolition at " +
                                    "the same frame if there was already another demolition " +
                                    "at the same frame and it took more time than Time Quota value.";
        const string tlp_adv_amount = "Maximum amount of allowed fragments. Object won't be demolished if existing amount of fragments " +
                                      "in scene higher that this value. Fading allows to decrease amount of fragments in scene.";
        
        // GUI
        public static readonly GUIContent gui_cap_phy     = new GUIContent ("  Physics",                "");
        public static readonly GUIContent gui_phy_set     = new GUIContent ("Set Gravity",              "Sets custom gravity for simulated objects.");
        public static readonly GUIContent gui_phy_mul     = new GUIContent ("    Multiplier",           "Custom gravity multiplier.");
        public static readonly GUIContent gui_phy_int     = new GUIContent ("Interpolation",            "");
        public static readonly GUIContent gui_cap_col     = new GUIContent ("  Collider",               "");
        public static readonly GUIContent gui_phy_col     = new GUIContent ("Collider Size",            "Minimum object size to get collider.");
        public static readonly GUIContent gui_phy_cop     = new GUIContent ("Vertices Amount",          tlp_ph_cop);
        public static readonly GUIContent gui_phy_cok     = new GUIContent ("Cooking Options",          "Mesh Collider cooking options.");
        public static readonly GUIContent gui_cap_det     = new GUIContent ("  Collision Detection",    "");
        public static readonly GUIContent gui_col_mesh    = new GUIContent ("Mesh",                     "Collision detection which will be used for simulated mesh objects.");
        public static readonly GUIContent gui_col_cls     = new GUIContent ("Cluster",                  "Collision detection which will be used for Connected and Nested clusters.");
        public static readonly GUIContent gui_cap_mat     = new GUIContent ("  Materials",              "");
        public static readonly GUIContent gui_mat_min     = new GUIContent ("Minimum Mass",             tlp_mat_min);
        public static readonly GUIContent gui_mat_max     = new GUIContent ("Maximum Mass",             tlp_mat_max);
        public static readonly GUIContent gui_mat_pres    = new GUIContent ("Material Presets",         "List of hardcoded materials with predefined simulation and demolition properties.");
        public static readonly GUIContent gui_mat_type    = new GUIContent ("Type",                     "");
        public static readonly GUIContent gui_cap_dm      = new GUIContent ("         Demolition",      "");
        public static readonly GUIContent gui_mat_dest    = new GUIContent ("Demolishable",             "Makes object with this material demolishable in runtime.");
        public static readonly GUIContent gui_mat_sol     = new GUIContent ("Solidity",                 "Global material solidity multiplier which used at collision to calculate if object should be demolished or not.");
        public static readonly GUIContent gui_cap_rb      = new GUIContent ("         Rigid Body",      "");
        public static readonly GUIContent gui_mat_dens    = new GUIContent ("Density",                  "Object mass depends on picked material density and collider volume.");
        public static readonly GUIContent gui_mat_drag    = new GUIContent ("Drag",                     "Allows to decrease position velocity over time.");
        public static readonly GUIContent gui_mat_ang     = new GUIContent ("Angular Drag",             "Allows to decrease rotation velocity over time.");
        public static readonly GUIContent gui_cap_ph      = new GUIContent ("         Physic Material", "");
        public static readonly GUIContent gui_mat_mat     = new GUIContent ("Material",                 tlp_mat_mat);
        public static readonly GUIContent gui_mat_dyn     = new GUIContent ("Dynamic Friction",         "");
        public static readonly GUIContent gui_mat_stat    = new GUIContent ("Static Friction",          "");
        public static readonly GUIContent gui_mat_bnc     = new GUIContent ("Bounciness",               "");
        public static readonly GUIContent gui_cap_axt     = new GUIContent ("  Activation",             "");
        public static readonly GUIContent gui_act_par     = new GUIContent ("Parent",                   "Object which will become parent of activated object");
        public static readonly GUIContent gui_cap_dml     = new GUIContent ("  Demolition",             "");
        public static readonly GUIContent gui_dml_sol     = new GUIContent ("Global Solidity",          "Global Solidity multiplier. Affect solidity of all simulated objects.");
        public static readonly GUIContent gui_dml_time    = new GUIContent ("Time Quota",               tlp_dml_time);
        public static readonly GUIContent gui_dml_quota   = new GUIContent ("Quota Action",             "Allows to skip object demolition or postpone it to the next frame if demolition time quota for this frame is reached.");
        public static readonly GUIContent gui_adv_expand  = new GUIContent ("Advanced Properties",      "");
        public static readonly GUIContent gui_cap_frg     = new GUIContent ("  Fragments",              "");
        public static readonly GUIContent gui_adv_parent  = new GUIContent ("Parent",                   "Defines parent for all new fragments.");
        public static readonly GUIContent gui_adv_global  = new GUIContent ("    Global Parent",        "Defines parent for all new fragments.");
        public static readonly GUIContent gui_adv_current = new GUIContent ("Current Amount",           "Amount of created fragments.");
        public static readonly GUIContent gui_adv_amount  = new GUIContent ("Maximum Amount",           tlp_adv_amount);
        public static readonly GUIContent gui_adv_bad     = new GUIContent ("Bad Mesh Try",             "Defines parent for all new fragments.");
        public static readonly GUIContent gui_cap_shad    = new GUIContent ("  Shadows",                "");
        public static readonly GUIContent gui_adv_size    = new GUIContent ("Size Threshold",           "Disable Shadow Casting for all objects with size less than this value.");
        public static readonly GUIContent gui_cap_pol     = new GUIContent ("  Pooling",                "");
        public static readonly GUIContent gui_pol_frg     = new GUIContent ("Fragments",                "Create gameobjects with MeshFilter, MeshRenderer and RayFireRigid components until Min Capacity value will be reached. Objects will be used for runtime fragments when needed");
        public static readonly GUIContent gui_pol_prt     = new GUIContent ("Particles",                "Create gameobjects with Particle System until Min Capacity value will be reached. Objects will be used for Debris or Dust when needed.");
        public static readonly GUIContent gui_pol_reu     = new GUIContent ("    Reuse",                "Do not destroy objects and send them back to pool until Max Capacity value will be reached");
        public static readonly GUIContent gui_pol_min     = new GUIContent ("    Capacity Min",         "");
        public static readonly GUIContent gui_pol_max     = new GUIContent ("    Capacity Max",         "");
        public static readonly GUIContent gui_cap_abt     = new GUIContent ("  About",                  "");
        public static readonly GUIContent gui_dbg_msg     = new GUIContent ("Debug Messages",           "");
        public static readonly GUIContent gui_dbg_edt     = new GUIContent ("Only In Editor",           "");
        public static readonly GUIContent gui_dbg_bld     = new GUIContent ("Only Debug Build",         "");
        public static readonly GUIContent gui_change      = new GUIContent ("     Changelog     ",      "");
    }
    
    /// <summary>
    /// Rayfire Recorder text for GUI elements.
    /// </summary>
    public static class TextRec
    {
        // Strings
        public const string rec = "Recording...   ";
     
        // Buttons
        public static readonly GUIContent gui_btn_rec_start = new GUIContent ("Start Record", "");
        public static readonly GUIContent gui_btn_rec_stop  = new GUIContent ("Stop Record",  "");
        public static readonly GUIContent gui_btn_pla_start = new GUIContent ("Start Play",   "");
        
        // GUI
        public static readonly GUIContent gui_mode       = new GUIContent ("Mode",            "");
        public static readonly GUIContent gui_cap_props  = new GUIContent ("  Properties",    "");
        public static readonly GUIContent gui_rec_start  = new GUIContent ("On Start",        "Automatically start recording at Start.");
        public static readonly GUIContent gui_rec_clip   = new GUIContent ("Clip Name",       "");
        public static readonly GUIContent gui_rec_dur    = new GUIContent ("Duration",        "Maximum duration for recorded animation clip.");
        public static readonly GUIContent gui_rec_rate   = new GUIContent ("Rate",            "Amount of keys per second.");
        public static readonly GUIContent gui_rec_reduce = new GUIContent ("Reduce Keys",     "Optimize amount of keys for still objects.");
        public static readonly GUIContent gui_rec_thresh = new GUIContent ("Threshold",       "Reduce Keys threshold.");
        public static readonly GUIContent gui_pla_start  = new GUIContent ("On Start",        "Automatically start playback at Start.");
        public static readonly GUIContent gui_pla_clip   = new GUIContent ("Clip",            "");
        public static readonly GUIContent gui_pla_cont   = new GUIContent ("Controller",      "");
        public static readonly GUIContent gui_cap_rigid  = new GUIContent ("  Rayfire Rigid", "");
        public static readonly GUIContent gui_pla_rigid  = new GUIContent ("Action",          "");
    }
    
    /// <summary>
    /// Rayfire Restriction text for GUI elements.
    /// </summary>
    public static class TextRst
    {
        // GUI
        public static readonly GUIContent gui_rigid     = new GUIContent ("RayFire Rigid", "");
        public static readonly GUIContent gui_cap_props = new GUIContent ("  Properties",  "");
        public static readonly GUIContent gui_prp_en    = new GUIContent ("Enable",        "Allows to Reset, Fade or perform Post demolition action when Rigid object breaks restriction.");
        public static readonly GUIContent gui_prp_act   = new GUIContent ("Action",        "");
        public static readonly GUIContent gui_prp_del   = new GUIContent ("Delay",         "Action delay in seconds.");
        public static readonly GUIContent gui_prp_int   = new GUIContent ("Interval",      "How often component will check if object broke restriction.");
        public static readonly GUIContent gui_cap_dst   = new GUIContent ("  Distance",    "");
        public static readonly GUIContent gui_dst_pos   = new GUIContent ("Position",      "Restriction will break if distance between object and Initial/Target position will be higher than this value.");
        public static readonly GUIContent gui_dst_val   = new GUIContent ("Distance",      "Object will break restriction if will be moved for this distance.");
        public static readonly GUIContent gui_dst_trg   = new GUIContent ("Target",        "");
        public static readonly GUIContent gui_cap_tri   = new GUIContent ("  Trigger",    "");
        public static readonly GUIContent gui_tri_reg   = new GUIContent ("Region",        "");
        public static readonly GUIContent gui_tri_col   = new GUIContent ("Collider",      "");
    }

    /// <summary>
    /// RFPhysic class text for GUI elements.
    /// </summary>
    public static class TextPhy
    {
        public static readonly GUIContent gui_phy     = new GUIContent ("Physics",            "Defines all physics properties for simulated object.");
        public static readonly GUIContent gui_cap_mat = new GUIContent ("  Material",         "");
        public static readonly GUIContent gui_phy_mtp = new GUIContent ("Type",               "Material preset with predefined density, friction, elasticity and solidity. Can be edited in Rayfire Man component.");
        public static readonly GUIContent gui_phy_mat = new GUIContent ("Material",           "Allows to define own Physic Material.");
        public static readonly GUIContent gui_cap_mas = new GUIContent ("  Mass",             "");
        public static readonly GUIContent gui_phy_mby = new GUIContent ("Mass By",            "");
        public static readonly GUIContent gui_phy_mss = new GUIContent ("Mass",               "Mass which will be applied to object if Mass By set to By Mass Property.");
        public static readonly GUIContent gui_cap_col = new GUIContent ("  Collider",         "");
        public static readonly GUIContent gui_phy_ctp = new GUIContent ("Type",               "");
        public static readonly GUIContent gui_phy_pln = new GUIContent ("Planar Check",       "Do not add Mesh Collider to objects with planar low poly mesh.");
        public static readonly GUIContent gui_phy_ign = new GUIContent ("Ignore Near",        "");
        public static readonly GUIContent gui_cap_oth = new GUIContent ("  Other",            "");
        public static readonly GUIContent gui_phy_grv = new GUIContent ("Use Gravity",        "Enables gravity for simulated object.");
        public static readonly GUIContent gui_phy_slv = new GUIContent ("Solver Iterations",  "");
        public static readonly GUIContent gui_phy_slt = new GUIContent ("Sleeping Threshold", "");
        public static readonly GUIContent gui_cap_frg = new GUIContent ("  Fragments",        "");
        public static readonly GUIContent gui_phy_dmp = new GUIContent ("Dampening",          "Multiplier for demolished fragments velocity.");
    }

    /// <summary>
    /// RFActivation class text for GUI elements.
    /// </summary>
    public static class TextAcv
    {
        public static readonly GUIContent gui_act     = new GUIContent ("Activation",      "Allows to activate ( make dynamic ) inactive and kinematic objects.");
        public static readonly GUIContent gui_cap_act = new GUIContent ("  Activation By", "");
        public static readonly GUIContent gui_act_off = new GUIContent ("Offset",          "Inactive object will be activated if will be pushed from it's original position farther than By Offset value.");
        public static readonly GUIContent gui_act_loc = new GUIContent ("    Local",       "Activation By Local Offset relative to parent.");
        public static readonly GUIContent gui_act_vel = new GUIContent ("Velocity",        "Inactive object will be activated when it's velocity will be higher than By Velocity value when pushed by other dynamic objects.");
        public static readonly GUIContent gui_act_dmg = new GUIContent ("Damage",          "Inactive object will be activated if will get total damage higher than this value.");
        public static readonly GUIContent gui_act_act = new GUIContent ("Activator",       "Inactive object will be activated by overlapping with object with RayFire Activator component.");
        public static readonly GUIContent gui_act_imp = new GUIContent ("Impact",          "Inactive object will be activated when it will be shot by RayFireGun component.");
        public static readonly GUIContent gui_act_con = new GUIContent ("Connectivity",    "Inactive object will be activated by Connectivity component if it will not be connected with Unyielding zone.");
        public static readonly GUIContent gui_act_uny = new GUIContent ("    Unyielding",  "Allows to define Inactive/Kinematic object as Unyielding to check for connection with other Inactive/Kinematic objects with enabled By Connectivity activation type.");
        public static readonly GUIContent gui_act_atb = new GUIContent ("    Activatable", "Unyielding object can not be activate by default. When On allows to activate Unyielding objects as well.");
        public static readonly GUIContent gui_cap_pst = new GUIContent ("  Post Activation", "");
        public static readonly GUIContent gui_act_l   = new GUIContent ("Change Layer",    "Change layer for activated objects.");
        public static readonly GUIContent gui_act_lay = new GUIContent ("Layer",           "Custom layer for activated objects.");

    }

    /// <summary>
    /// RFLimitations class text for GUI elements.
    /// </summary>
    public static class TextLim
    {
        public static readonly GUIContent gui_lim     = new GUIContent ("Limitations",    "");
        public static readonly GUIContent gui_cap_col = new GUIContent ("  Collision",    "");
        public static readonly GUIContent gui_lim_col = new GUIContent ("By Collision",   "Enables demolition by collision.");
        public static readonly GUIContent gui_lim_sol = new GUIContent ("Solidity",       "Local Object solidity multiplier for object. Low Solidity makes object more fragile at collision.");
        public static readonly GUIContent gui_lim_tag = new GUIContent ("Tag",            "Object will be demolished only if it will collide with other objects with defined Tag.");
        public static readonly GUIContent gui_cap_oth = new GUIContent ("  Other",    "");
        public static readonly GUIContent gui_lim_dep = new GUIContent ("Depth",          "Defines how deep object can be demolished. Depth is limitless if set to 0.");
        public static readonly GUIContent gui_lim_tim = new GUIContent ("Time",           "Safe time. Measures in seconds and allows to prevent fragments from being demolished right after they were just created.");
        public static readonly GUIContent gui_lim_siz = new GUIContent ("Size",           "Prevent objects with bounding box size less than defined value to be demolished.");
        public static readonly GUIContent gui_lim_vis = new GUIContent ("Visible",        "Object will be demolished only if it is visible to any camera including scene camera.");
        public static readonly GUIContent gui_lim_bld = new GUIContent ("Slice By Blade", "Allows object to be sliced by object with RayFire Blade component.");
    }

    /// <summary>
    /// RFDemolitionMesh class text for GUI elements.
    /// </summary>
    public static class TextMsh
    {
        public static readonly GUIContent gui_msh         = new GUIContent ("Mesh Demolition",    "");
        public static readonly GUIContent gui_cap_frg     = new GUIContent ("  Fragments",        "");
        public static readonly GUIContent gui_msh_am      = new GUIContent ("Amount",             "Defines amount of points in point cloud for fragments after demolition.");
        public static readonly GUIContent gui_msh_var     = new GUIContent ("Variation",          "Defines additional amount variation for object in percents.");
        public static readonly GUIContent gui_msh_dpf     = new GUIContent ("Depth Fade",         "Amount multiplier for next Depth level. Allows to decrease fragments amount of every next demolition level.");
        public static readonly GUIContent gui_msh_bias    = new GUIContent ("Contact Bias",       "Higher value allows to create more tiny fragments closer to collision contact point and bigger fragments far from it.");
        public static readonly GUIContent gui_msh_sd      = new GUIContent ("Seed",               "Defines Seed for fragmentation algorithm. Same Seed will produce same fragments for same object every time.");
        public static readonly GUIContent gui_msh_use     = new GUIContent ("Use Shatter",        "Allows to use RayFire Shatter properties for fragmentation. Works only if object has RayFire Shatter component.");
        public static readonly GUIContent gui_msh_cld     = new GUIContent ("Add Children",       "Add children mesh objects to fragments.");
        public static readonly GUIContent gui_cap_adv     = new GUIContent ("  Advanced",         "");
        public static readonly GUIContent gui_msh_sim     = new GUIContent ("Fragments Sim Type", "Simulation type for demolished fragments."); 
        public static readonly GUIContent gui_msh_cnv     = new GUIContent ("Convert",            "Convert fragments after fragmentation to Connected CLuster or MeshRoot with Connectivity Setup.");
        public static readonly GUIContent gui_msh_rnt     = new GUIContent ("Runtime Caching",    ""); 
        public static readonly GUIContent gui_msh_rnt_fr  = new GUIContent ("Frames",             "");
        public static readonly GUIContent gui_msh_rnt_fg  = new GUIContent ("Fragments",          "");
        public static readonly GUIContent gui_msh_rnt_sk  = new GUIContent ("Skip First",         "Only initiate Runtime Caching on first demolition and demolish at second.");
        public static readonly GUIContent gui_msh_adv     = new GUIContent ("Properties",         "");
        public static readonly GUIContent gui_msh_adv_rem = new GUIContent ("Remove Collinear",   "Remove collier vertices to decrease amount of triangles.");
        public static readonly GUIContent gui_msh_adv_dec = new GUIContent ("Decompose",          "Detach all disconnected triangles into separate fragments.");
        public static readonly GUIContent gui_msh_adv_cap = new GUIContent ("Precap",             "Cap open edges before fragment mesh.");
        public static readonly GUIContent gui_msh_adv_inp = new GUIContent ("Mesh Input",         "Defines time for Mesh Input to process it and prepare for demolition. Useful for mid and hi poly objects.");
        public static readonly GUIContent gui_msh_adv_col = new GUIContent ("Collider",           "");
        public static readonly GUIContent gui_msh_adv_szf = new GUIContent ("Size Filter",        "Fragments with size less than this value will not get collider.");
        public static readonly GUIContent gui_msh_adv_l   = new GUIContent ("Inherit Layer",      "Inherit Layer for fragments.");
        public static readonly GUIContent gui_msh_adv_lay = new GUIContent ("  Custom Layer",     "Custom layer for fragments.");
        public static readonly GUIContent gui_msh_adv_t   = new GUIContent ("Inherit Tag",        "Inherit Tag for fragments.");
        public static readonly GUIContent gui_msh_adv_tag = new GUIContent ("  Custom Tag",       "Custom Tag fr fragments.");
    }
    
    /// <summary>
    /// RFDemolitionCluster class text for GUI elements.
    /// </summary>
    public static class TextCls
    {
        public static readonly GUIContent gui_cls       = new GUIContent ("Cluster Demolition",    "");
        public static readonly GUIContent gui_cap_prp   = new GUIContent ("  Properties",          "");
        public static readonly GUIContent gui_cls_cnt   = new GUIContent ("Connectivity",          "Defines Connectivity algorithm for clusters.");
        public static readonly GUIContent gui_cls_sim   = new GUIContent ("Cluster Sim Type",      "Simulation type for demolished cluster."); 
        public static readonly GUIContent gui_cap_flt   = new GUIContent ("  Filters",             "");
        public static readonly GUIContent gui_cls_fl_ar = new GUIContent ("Minimum Area",          "Two shards will have connection if their shared area is bigger than this value.");
        public static readonly GUIContent gui_cls_fl_sz = new GUIContent ("Minimum Size",          "Two shards will have connection if their size is bigger than this value.");
        public static readonly GUIContent gui_cls_fl_pr = new GUIContent ("Percentage",            "Random percentage of connections will be discarded.");
        public static readonly GUIContent gui_cap_dml   = new GUIContent ("  Demolition Distance", "");
        public static readonly GUIContent gui_cls_fl_sd = new GUIContent ("Seed",                  "Seed for random percentage filter and for Random Collapse.");
        public static readonly GUIContent gui_cls_ds_tp = new GUIContent ("Type",                  "");
        public static readonly GUIContent gui_cls_ds_rt = new GUIContent ("Ratio",                 "Defines demolition distance from contact point in percentage relative to object's size.");
        public static readonly GUIContent gui_cls_ds_un = new GUIContent ("Units",                 "Defines demolition distance from contact point in world units.");
        public static readonly GUIContent gui_cap_shd   = new GUIContent ("  Shards",              "");
        public static readonly GUIContent gui_cls_sh_ar = new GUIContent ("Area",                  "");
        public static readonly GUIContent gui_cls_sh_dm = new GUIContent ("Demolition",            "");
        public static readonly GUIContent gui_cap_cls   = new GUIContent ("  Clusters",            "");
        public static readonly GUIContent gui_cls_min   = new GUIContent ("Minimum",               "");
        public static readonly GUIContent gui_cls_max   = new GUIContent ("Maximum",               "");
        public static readonly GUIContent gui_cls_dml   = new GUIContent ("Demolishable",          "");
        public static readonly GUIContent gui_cap_clp   = new GUIContent ("  Collapse",            "");
        public static readonly GUIContent gui_cls_prp   = new GUIContent ("Properties",            "");
        public static readonly GUIContent gui_cap_lt    = new GUIContent ("  Layer and Tag",       "");
    }

    /// <summary>
    /// RFFade class text for GUI elements.
    /// </summary>
    public static class TextFad
    {
        public static readonly GUIContent gui_fad       = new GUIContent ("Fading",        "");
        public static readonly GUIContent gui_cap_ini   = new GUIContent ("  Initiate",    "");
        public static readonly GUIContent gui_fad_dml   = new GUIContent ("On Demolition", "Fading will be applied to fragments that will be created after this object will be demolished.");
        public static readonly GUIContent gui_fad_act   = new GUIContent ("On Activation", "Fading will be applied to this object after it will be activated, turned to Dynamic from Inactive or Kinematik simulation type.");
        public static readonly GUIContent gui_fad_ofs   = new GUIContent ("By Offset",     "Fading will be applied to this object after it will be moved to Offset distance from its Initiaization position.");
        public static readonly GUIContent gui_cap_tp    = new GUIContent ("  Type",        "");
        public static readonly GUIContent gui_fad_tp    = new GUIContent ("Type",          "");
        public static readonly GUIContent gui_fad_tm    = new GUIContent ("Time",          "Fading duration time.");
        public static readonly GUIContent gui_cap_lf    = new GUIContent ("  Life",        "");
        public static readonly GUIContent gui_fad_lf_tp = new GUIContent ("Type",          "");
        public static readonly GUIContent gui_fad_lf_tm = new GUIContent ("Time",          "Time which object will be simulated before start to fade.");
        public static readonly GUIContent gui_fad_lf_vr = new GUIContent ("Variation",     "");
        public static readonly GUIContent gui_cap_flt   = new GUIContent ("  Filters",     "");
        public static readonly GUIContent gui_fad_sz    = new GUIContent ("Size",          "Fade won't affect objects with size bigger than this value. Disabled if set to 0.");
        public static readonly GUIContent gui_fad_sh    = new GUIContent ("Shards",        "Fade won't affect Connected clusters with shard amount bigger than this value. Disabled if set to 0.");
    }

    /// <summary>
    /// RFFade class text for GUI elements.
    /// </summary>
    public static class TextRes
    {
        public static readonly GUIContent gui_res     = new GUIContent ("Reset",         "");
        public static readonly GUIContent gui_cap_res = new GUIContent ("  Reset",       "");
        public static readonly GUIContent gui_res_tm  = new GUIContent ("Transform",     "Reset transform to position and rotation when object was initialized.");
        public static readonly GUIContent gui_res_dm  = new GUIContent ("Damage",        "Reset damage value.");
        public static readonly GUIContent gui_res_cn  = new GUIContent ("Connectivity",  "Reset Connectivity.");
        public static readonly GUIContent gui_cap_dml = new GUIContent ("  Demolition",  "");
        public static readonly GUIContent gui_res_ac  = new GUIContent ("Action",        "");
        public static readonly GUIContent gui_res_dl  = new GUIContent ("Destroy Delay", "Object will be destroyed after defined delay.");
        public static readonly GUIContent gui_cap_reu = new GUIContent ("  Reuse",  "");
        public static readonly GUIContent gui_res_ms  = new GUIContent ("Mesh",          "");
        public static readonly GUIContent gui_res_fr  = new GUIContent ("Fragments",     "");
    }

    /// <summary>
    /// Rayfire Rigid text for GUI elements.
    /// </summary>
    public static class TextRig
    {
        // Help
        public const string hlp_obj    = "This Object Type should be selected for object with multiple children.";
        public const string hlp_sh_dml = "Enabled property set Runtime Demolition type for all detached shards. Enable this property only if you are absolutely sure how it works.";
        public const string hlp_limit  = "Depth 0 makes all generated fragments demolishable endlessly.";
        public const string hlp_depth  = "High Depth may increase amount of genrated fragments exponentially.";
        public const string hlp_sol    = "Solidity 0 makes object demolishable by the slightest collision.";
        
        // Buttons
        public static readonly GUIContent gui_btn_init      = new GUIContent ("Initialize",         "");
        public static readonly GUIContent gui_btn_reset     = new GUIContent ("Reset Rigid",        "");
        public static readonly GUIContent gui_btn_dml       = new GUIContent ("Demolish",           "");
        public static readonly GUIContent gui_btn_act       = new GUIContent ("Activate",           "");
        public static readonly GUIContent gui_btn_fad       = new GUIContent ("Fade",               "");
        public static readonly GUIContent gui_btn_edt_setup = new GUIContent (" Editor Setup ",     "");
        public static readonly GUIContent gui_btn_edt_reset = new GUIContent ("Reset Setup",        "");
        public static readonly GUIContent gui_btn_clp_start = new GUIContent ("Start Collapse",     "");
        public static readonly GUIContent gui_btn_conns     = new GUIContent ("Show Connections",   "");
        public static readonly GUIContent gui_btn_nodes     = new GUIContent ("    Show Nodes    ", "");
        
        // Strings
        public const string rfRig          = "RayFire Rigid: ";
        public const string str_info       = "Info";
        public const string str_mis_shards = " has missing shards. Reset or Setup cluster.";
        public const string str_precache   = "    Precached Unity Meshes: ";
        public const string str_frags      = "    Fragments: ";
        public const string str_excluded   = "    Fragments: ";
        public const string str_size       = "    Size: ";
        public const string str_depth      = "    Demolition depth: ";
        public const string str_damage     = "    Damage applied: ";
        public const string str_fade_pre   = "    Object about to fade...";
        public const string str_fade_prg   = "    Fading in progress...";
        public const string str_bad        = "    Object has bad mesh and will not be demolished anymore";
        public const string str_ignore     = "    Ignore Pairs: ";
        public const string str_cls_coll   = "    Cluster Colliders ";
        public const string str_cls_shards = "    Cluster Shards: ";
        public const string str_integrity  = "    Amount Integrity: ";
        public const string str_mesh_root  = "  MeshRoot";
        public const string str_cls        = "  Cluster";

        // GUI
        public static readonly GUIContent gui_cap_mn  = new GUIContent ("  Main",               "");
        public static readonly GUIContent gui_mn_ini  = new GUIContent ("Initialization",       "");
        public static readonly GUIContent gui_mn_obj  = new GUIContent ("Object Type",          "");
        public static readonly GUIContent gui_cap_sim = new GUIContent ("  Simulation",         "");
        public static readonly GUIContent gui_mn_sim  = new GUIContent ("Simulation Type",      "Defines behaviour of object during simulation.");
        public static readonly GUIContent gui_cap_dml = new GUIContent ("  Demolition",         "");
        public static readonly GUIContent gui_mn_dml  = new GUIContent ("Demolition Type",      "Defines when and how object will be demolished.");
        public static readonly GUIContent gui_cap_prp = new GUIContent ("  Properties",         "");
        public static readonly GUIContent gui_ref     = new GUIContent ("Reference Demolition", "");
        public static readonly GUIContent gui_ref_rfs = new GUIContent ("Reference",            "");
        public static readonly GUIContent gui_ref_lst = new GUIContent ("Random List",          "");
        public static readonly GUIContent gui_ref_act = new GUIContent ("Action",               "");
        public static readonly GUIContent gui_ref_add = new GUIContent ("Add Rigid",            "Add RayFire Rigid component to reference with mesh.");
        public static readonly GUIContent gui_ref_scl = new GUIContent ("Inherit Scale",        "");
        public static readonly GUIContent gui_ref_mat = new GUIContent ("Inherit Materials",    "");
        public static readonly GUIContent gui_cap_src = new GUIContent ("  Source",             "");
        public static readonly GUIContent gui_mat     = new GUIContent ("Materials",            "");
        public static readonly GUIContent gui_mat_scl = new GUIContent ("Mapping",              "Mapping scale for inner surface");
        public static readonly GUIContent gui_mat_inn = new GUIContent ("Inner",                "Material for inner fragments surface");
        public static readonly GUIContent gui_mat_out = new GUIContent ("Outer",                "Material for outer fragments surface");
        public static readonly GUIContent gui_dmg     = new GUIContent ("Damage",               "Allows to demolish object by it's own floating Damage value.");
        public static readonly GUIContent gui_dmg_en  = new GUIContent ("Enable",               "");
        public static readonly GUIContent gui_dmg_max = new GUIContent ("Max Damage",           "Defines maximum allowed damage for object to be demolished.");
        public static readonly GUIContent gui_dmg_cur = new GUIContent ("Current Damage",       "Shows current damage value. Can be increased by public method: \nApplyDamage(float damageValue, Vector3 damagePosition)");
        public static readonly GUIContent gui_cap_col = new GUIContent ("  Collision",          "");
        public static readonly GUIContent gui_dmg_col = new GUIContent ("Collect",              "Allows to accumulate damage value by collisions during dynamic simulation.");
        public static readonly GUIContent gui_dmg_mlt = new GUIContent ("Multiplier",           "Multiplier for every collision damage.");
        public static readonly GUIContent gui_dmg_shr = new GUIContent ("To Shards",            "Apply damage to Connected Cluster shards.");
        public static readonly GUIContent gui_cap_com = new GUIContent ("  Common",             "");
    }
    
    /// <summary>
    /// Rayfire RigidRoot text for GUI elements.
    /// </summary>
    public static class TextRot
    {
        // GUI
        public static GUIContent gui_mn_ini = new GUIContent ("Initialization", "");
        public static GUIContent gui_props  = new GUIContent ("Properties",     "");
    }
    
    /// <summary>
    /// Rayfire Shatter text for GUI elements.
    /// </summary>
    public static class TextSht
    {
        // Buttons
        public static readonly GUIContent gui_btn_frag          = new GUIContent ("Fragment",              "");
        public static readonly GUIContent gui_btn_frag_last     = new GUIContent ("Fragment to Last",      "");
        public static readonly GUIContent gui_btn_frag_last_del = new GUIContent ("    Delete Last    ",   "");
        public static readonly GUIContent gui_btn_frag_del      = new GUIContent (" Delete All ",          "");
        public static readonly GUIContent gui_btn_inter         = new GUIContent ("Interactive",           "Preview fragments as one mesh. WARNING: Do not forget to Restore Original Mesh.");
        public static readonly GUIContent gui_btn_export_last   = new GUIContent ("Export Last Fragments", "");
        public static readonly GUIContent gui_btn_export_child  = new GUIContent ("Export Children",       "");
        public static readonly GUIContent gui_btn_scale         = new GUIContent ("Scale",                 "");
        public static readonly GUIContent gui_btn_color         = new GUIContent ("Color",                 "");
        public static readonly GUIContent gui_btn_show          = new GUIContent (" Show   ",              "");
        public static readonly GUIContent gui_btn_coll_add      = new GUIContent ("Add Mesh Colliders",    "");
        public static readonly GUIContent gui_btn_coll_rem      = new GUIContent (" Remove Colliders ",    "");
        
        // Strings
        public const string str_roots  = "Roots: ";
        public const string str_last   = "Last Fragments: ";
        public const string str_total  = "Total Fragments: ";
        public const string str_points = "    In/Out points: ";
        public const string str_move   = "Center Move";
        public const string str_rotate   = "Center Rotate";
        
        // Tooltips
        const string tlp_tp_tet = "Tetrahedron based fragments, this type is mostly useless as is and should be used with Gluing, " +
                                  "in this case it creates high poly concave fragments.";
        const string tlp_tp_elm = "Input mesh will be separated to not connected mesh elements, every element will be fragmented separately." +
                                  "This threshold value measures in percentage relative to original objects size and prevent element from being fragmented if its size is less.";
        
        public static readonly GUIContent gui_cap_prv         = new GUIContent ("  Preview",            "");
        public static readonly GUIContent gui_prv_scl         = new GUIContent ("Scale Preview",        "");
        public static readonly GUIContent gui_cap_frg         = new GUIContent ("  Fragments",          "");
        public static readonly GUIContent gui_tp              = new GUIContent ("Type",                 "Defines fragmentation type for object.");
        public static readonly GUIContent gui_cap_vor         = new GUIContent ("      Voronoi",        "Low poly, convex, physics friendly fragments.");
        public static readonly GUIContent gui_tp_vor_amount   = new GUIContent ("Amount",               "Defines amount of points in point cloud, every point represent rough center of  fragment.");
        public static readonly GUIContent gui_tp_vor_bias     = new GUIContent ("Center Bias",          "Defines offset of points in point cloud towards Center.");
        public static readonly GUIContent gui_cap_spl         = new GUIContent ("      Splinters",      "Low poly, convex, physics friendly fragments, stretched along one axis.");
        public static readonly GUIContent gui_tp_spl_axis     = new GUIContent ("Axis",                 "Fragments will be stretched over defined axis.");
        public static readonly GUIContent gui_tp_spl_str      = new GUIContent ("Strength",             "Defines sharpness of stretched fragments.");
        public static readonly GUIContent gui_cap_slb         = new GUIContent ("      Slabs",          "Low poly, convex, physics friendly fragments, stretched along two axes.");
        public static readonly GUIContent gui_cap_rad         = new GUIContent ("      Radial",         "Low poly, convex, physics friendly fragments, creates radial fragments pattern.");
        public static readonly GUIContent gui_tp_rad_axis     = new GUIContent ("Center Axis",          "");
        public static readonly GUIContent gui_tp_rad_radius   = new GUIContent ("Radius",               "");
        public static readonly GUIContent gui_tp_rad_div      = new GUIContent ("Divergence",           "");
        public static readonly GUIContent gui_tp_rad_rest     = new GUIContent ("  Restrict To Plane",  "");
        public static readonly GUIContent gui_cap_rings       = new GUIContent ("      Rings",          "");
        public static readonly GUIContent gui_tp_rad_rings    = new GUIContent ("Rings",                "");
        public static readonly GUIContent gui_tp_rad_focus    = new GUIContent ("Focus",                "");
        public static readonly GUIContent gui_tp_rad_str      = new GUIContent ("Focus Strength",       "");
        public static readonly GUIContent gui_tp_rad_randRing = new GUIContent ("Random Rings",         "");
        public static readonly GUIContent gui_cap_rays        = new GUIContent ("      Rays",           "");
        public static readonly GUIContent gui_tp_rad_rays     = new GUIContent ("Rays",                 "");
        public static readonly GUIContent gui_tp_rad_randRay  = new GUIContent ("Random Rays",          "");
        public static readonly GUIContent gui_tp_rad_twist    = new GUIContent ("Twist",                "");
        public static readonly GUIContent gui_cap_hex         = new GUIContent ("      Hex",            "");
        public static readonly GUIContent gui_cap_hex_grd     = new GUIContent ("      Grid",           "");
        public static readonly GUIContent gui_tp_hex_size     = new GUIContent ("Size",                 "Hex size");
        public static readonly GUIContent gui_tp_hex_am       = new GUIContent ("Amount",               "Amount of hexes in grid in two axes");
        public static readonly GUIContent gui_tp_hex_emp      = new GUIContent (" ",                    "");
        public static readonly GUIContent gui_cap_cus         = new GUIContent ("      Custom",         "Low poly, convex, physics friendly fragments, allows to use custom point cloud for fragments distribution.");
        public static readonly GUIContent gui_tp_cus_src      = new GUIContent ("Source",               "");
        public static readonly GUIContent gui_tp_cus_use      = new GUIContent ("Use As",               "");
        public static readonly GUIContent gui_tp_cus_tms      = new GUIContent ("Transform List",       "");
        public static readonly GUIContent gui_tp_cus_vec      = new GUIContent ("Vector3 List",         "");
        public static readonly GUIContent gui_cap_vol         = new GUIContent ("      Volume",         "");
        public static readonly GUIContent gui_tp_cus_am       = new GUIContent ("Amount",               "");
        public static readonly GUIContent gui_cap_prev        = new GUIContent ("      Preview",        "");
        public static readonly GUIContent gui_tp_cus_rad      = new GUIContent ("Radius",               "");
        public static readonly GUIContent gui_tp_cus_en       = new GUIContent ("Enable",               "");
        public static readonly GUIContent gui_tp_cus_sz       = new GUIContent ("Size",                 "");
        public static readonly GUIContent gui_cap_slc         = new GUIContent ("      Slice",          "Slice object by planes.");
        public static readonly GUIContent gui_tp_slc_pl       = new GUIContent ("Plane",                "Slicing plane.");
        public static readonly GUIContent gui_cap_brk         = new GUIContent ("      Bricks",         "");
        public static readonly GUIContent gui_tp_brk_type     = new GUIContent ("Lattice",              "");
        public static readonly GUIContent gui_tp_brk_mult     = new GUIContent ("Multiplier",           "");
        public static readonly GUIContent gui_cap_am          = new GUIContent ("      Amount",         "");
        public static readonly GUIContent gui_cap_size        = new GUIContent ("      Size",           "");
        public static readonly GUIContent gui_tp_brk_am_X     = new GUIContent ("X axis",               "");
        public static readonly GUIContent gui_tp_brk_am_Y     = new GUIContent ("Y axis",               "");
        public static readonly GUIContent gui_tp_brk_am_Z     = new GUIContent ("Z axis",               "");
        public static readonly GUIContent gui_tp_brk_lock     = new GUIContent ("Lock",                 "");
        public static readonly GUIContent gui_cap_var         = new GUIContent ("      Size Variation", "");
        public static readonly GUIContent gui_cap_ofs         = new GUIContent ("      Offset",         "");
        public static readonly GUIContent gui_cap_sp          = new GUIContent ("      Split",          "");
        public static readonly GUIContent gui_tp_brk_sp_prob  = new GUIContent ("Probability",          "");
        public static readonly GUIContent gui_tp_brk_sp_offs  = new GUIContent ("Offset",               "");
        public static readonly GUIContent gui_tp_brk_sp_rot   = new GUIContent ("Rotation",             "");
        public static readonly GUIContent gui_cap_vxl         = new GUIContent ("      Voxels",         "");
        public static readonly GUIContent gui_cap_tet         = new GUIContent ("      Tets",           tlp_tp_tet);
        public static readonly GUIContent gui_tp_tetDn        = new GUIContent ("Density",              "");
        public static readonly GUIContent gui_tp_tetNs        = new GUIContent ("Noise",                "");
        public static readonly GUIContent gui_cap_mat         = new GUIContent ("  Inner Surface",      "");
        public static readonly GUIContent gui_mat_scl         = new GUIContent ("Mapping Scale",        "Defines mapping scale for inner surface.");
        public static readonly GUIContent gui_mat_in          = new GUIContent ("Material",             "Defines material for fragment's inner surface.");
        public static readonly GUIContent gui_mat_col         = new GUIContent ("Color",                "Set custom Vertex Color for all inner surface vertices.");
        public static readonly GUIContent gui_mat_uve         = new GUIContent ("UV",                   "Set custom UV coordinate for all inner surface vertices.");
        public static readonly GUIContent gui_cap_cls         = new GUIContent ("  Clusters",           "Allows to glue groups of fragments into single mesh by deleting shared faces.");
        public static readonly GUIContent gui_cls_en          = new GUIContent ("Enable",               "Allows to glue groups of fragments into single mesh by deleting shared faces.");
        public static readonly GUIContent gui_cls_cnt         = new GUIContent ("Count",                "Amount of clusters defined by random point cloud.");
        public static readonly GUIContent gui_cls_seed        = new GUIContent ("Seed",                 "Random seed for clusters point cloud generator.");
        public static readonly GUIContent gui_cls_rel         = new GUIContent ("Relax",                "Smooth strength for cluster inner surface.");
        public static readonly GUIContent gui_cls_debris      = new GUIContent ("Debris",               "Preserve some fragments at the edges of clusters to create small debris around big chunks.");
        public static readonly GUIContent gui_cls_amount      = new GUIContent ("Amount",               "Amount of debris in last layer in percents relative to amount of fragments in cluster.");
        public static readonly GUIContent gui_cls_layers      = new GUIContent ("Layers",               "Amount of debris layers at cluster border.");
        public static readonly GUIContent gui_cls_scale       = new GUIContent ("Scale",                "Scale variation for inner debris.");
        public static readonly GUIContent gui_cls_min         = new GUIContent ("Minimum",              "Minimum amount of fragments in debris cluster.");
        public static readonly GUIContent gui_cls_max         = new GUIContent ("Maximum",              "Maximum amount of fragments in debris cluster.");
        public static readonly GUIContent gui_cap_prp         = new GUIContent ("  Properties",         "");
        public static readonly GUIContent gui_mode            = new GUIContent ("Mode",                 "");
        public static readonly GUIContent gui_adv_seed        = new GUIContent ("Seed",                 "Seed for point cloud generator. Set to 0 to get random point cloud every time.");
        public static readonly GUIContent gui_adv_copy        = new GUIContent ("Copy",                 "Copy components from original object to fragments");
        public static readonly GUIContent gui_adv_smooth      = new GUIContent ("Smooth",               "Smooth fragments inner surface.");
        public static readonly GUIContent gui_adv_combine     = new GUIContent ("Combine",              "Combine all children meshes into one mesh and fragment this mesh.");
        public static readonly GUIContent gui_adv_col         = new GUIContent ("Collinear",            "Remove vertices which lay on straight edge.");
        public static readonly GUIContent gui_adv_dec         = new GUIContent ("Decompose",            "Check output fragments and separate not connected parts of meshes into separate fragments.");
        public static readonly GUIContent gui_adv_input       = new GUIContent ("Input Precap",         "Create extra triangles to connect open edges and close mesh volume for correct fragmentation.");
        public static readonly GUIContent gui_adv_output      = new GUIContent ("    Output Precap",    "Keep fragment's faces created by Input Precap.");
        public static readonly GUIContent gui_adv_lim         = new GUIContent ("Limitations",          "");
        public static readonly GUIContent gui_adv_size_lim    = new GUIContent ("Size",                 "All fragments with size bigger than Max Size value will be fragmented to few more fragments.");
        public static readonly GUIContent gui_adv_size_am     = new GUIContent ("    Max Size",         "");
        public static readonly GUIContent gui_adv_vert_lim    = new GUIContent ("Vertex",               "All fragments with vertex amount higher than Max Amount value will be fragmented to few more fragments.");
        public static readonly GUIContent gui_adv_vert_am     = new GUIContent ("    Max Amount",       "");
        public static readonly GUIContent gui_adv_tri_lim     = new GUIContent ("Triangle",             "All fragments with triangle amount higher than Max Amount value will be fragmented to few more fragments.");
        public static readonly GUIContent gui_adv_tri_am      = new GUIContent ("    Max Amount",       "");
        public static readonly GUIContent gui_adv_flt         = new GUIContent ("Filters",              "");
        public static readonly GUIContent gui_adv_inner       = new GUIContent ("Inner",                "Do not output inner fragments which has no outer surface.");
        public static readonly GUIContent gui_adv_planar      = new GUIContent ("Planar",               "Do not output planar fragments which mesh vertices lie in the same plane.");
        public static readonly GUIContent gui_adv_rel         = new GUIContent ("Relative Size",        "Do not output small fragments. Measures is percentage relative to original object size.");
        public static readonly GUIContent gui_adv_abs         = new GUIContent ("Absolute Size",        "Do not output small fragments which size in world units is less than this value.");
        public static readonly GUIContent gui_cap_edt         = new GUIContent ("  Editor",             "");
        public static readonly GUIContent gui_adv_element     = new GUIContent ("Element Size",         tlp_tp_elm);
        public static readonly GUIContent gui_adv_remove      = new GUIContent ("Remove Double Faces",  "Delete faces which overlap with each other.");
       
        public static readonly GUIContent gui_cap_exp             = new GUIContent ("  Export",             "Export fragments meshes to Unity Asset and reference to this asset.");
        public static readonly GUIContent gui_exp_src         = new GUIContent ("Source",               "");
        public static readonly GUIContent gui_exp_sfx         = new GUIContent ("Suffix",               "");
        
        public static readonly GUIContent gui_cap_cent    = new GUIContent ("  Center",    "");
        public static readonly GUIContent gui_cn_pos      = new GUIContent ("Position",    "");
        public static readonly GUIContent gui_cn_rot      = new GUIContent ("Rotation",    "");
        public static readonly GUIContent gui_cn_res      = new GUIContent ("Reset  ",     "");
        public static readonly GUIContent gui_cap_col     = new GUIContent ("  Colliders", "");
        public static readonly GUIContent gui_pr_adv_bake = new GUIContent ("Bake",        "Prepares fragment meshes for use with a MeshCollider.");
        public static readonly GUIContent gui_cap_info    = new GUIContent ("  Info",      "");
        
    }
    
    /// <summary>
    /// Rayfire Snapshot text for GUI elements.
    /// </summary>
    public static class TextSnp
    {
        // Button
        public static readonly GUIContent gui_btn_snap = new GUIContent ("Snapshot", "");
        public static readonly GUIContent gui_btn_load = new GUIContent ("Load", "");
        
        // GUI
        public static readonly GUIContent gui_cap_save = new GUIContent ("  Save",         "");
        public static readonly GUIContent gui_saveName = new GUIContent ("Asset Name",     "");
        public static readonly GUIContent gui_saveComp = new GUIContent ("Compress",       "");
        public static readonly GUIContent gui_cap_load = new GUIContent ("  Load",         "");
        public static readonly GUIContent gui_loadSnap = new GUIContent ("Snapshot Asset", "");
        public static readonly GUIContent gui_loadSize = new GUIContent ("Size Filter",    "");
    }
    
    /// <summary>
    /// Rayfire Sound text for GUI elements.
    /// </summary>
    public static class TextSnd
    {
        // Button
        public static readonly GUIContent gui_btn_ini = new GUIContent ("Initialization Sound", "");
        public static readonly GUIContent gui_btn_act = new GUIContent ("Activation Sound",     "");
        public static readonly GUIContent gui_btn_dml = new GUIContent ("Demolition Sound",     "");
        public static readonly GUIContent gui_btn_col = new GUIContent ("Collision Sound",      "");
        
        // Strings
        public const string str_info   = "Info";
        public const string str_volume = "  Volume: ";
        public const string str_vel    = "    Last Collision Relative Velocity: ";
        public const string str_clips   = "Random Clips";
        
        // GUI
        public static readonly GUIContent gui_cap_vol   = new GUIContent ("  Load",            "");
        public static readonly GUIContent gui_vol_base  = new GUIContent ("Base Volume",       "Base volume. Can be increased by Size Volume property.");
        public static readonly GUIContent gui_vol_size  = new GUIContent ("Size Volume",       "Additional volume per one unit size.");
        public static readonly GUIContent gui_cap_eve   = new GUIContent ("  Events",          "");
        public static readonly GUIContent gui_eve_ini   = new GUIContent ("Initialization",    "Enable Initialization sound.");
        public static readonly GUIContent gui_eve_act   = new GUIContent ("Activation",        "Enable Activation sound");
        public static readonly GUIContent gui_eve_dml   = new GUIContent ("Demolition",        "Enable Demolition sound");
        public static readonly GUIContent gui_eve_col   = new GUIContent ("Collision",         "Enable Collision sound");
        public static readonly GUIContent gui_cap_snd   = new GUIContent ("  Sound",           "");
        public static readonly GUIContent gui_snd_once  = new GUIContent ("Play Once",         "");
        public static readonly GUIContent gui_snd_mlt   = new GUIContent ("Multiplier",        "Sound volume multiplier for this event.");
        public static readonly GUIContent gui_cap_col   = new GUIContent ("  Collision",       "");
        public static readonly GUIContent gui_snd_vel   = new GUIContent ("Relative Velocity", "Minimum Relative Velocity collision threshold to play collision sound.");
        public static readonly GUIContent gui_snd_clip  = new GUIContent ("AudioClip",         "The AudioClip AAsset played by the AudioSource");
        public static readonly GUIContent gui_out_group = new GUIContent ("Output",            "");
        public static readonly GUIContent gui_out_prior = new GUIContent ("Priority",          "Sets the priority of the source. Note that a sound with a larger priority value will more likely be stolen by sounds with smaller priority values.");
        public static readonly GUIContent gui_out_spat  = new GUIContent ("Spatial Blend",     "0 = 2D, 1 = 3D");
        public static readonly GUIContent gui_out_mind  = new GUIContent ("Min Distance",      "Within Min Distance, the volume will stay at the loudest possible. Outside of this Min Distance it begins to attenuate.");
        public static readonly GUIContent gui_out_maxd  = new GUIContent ("Max Distance",      "Max Distance is the distance a sound stops attenuating at.");
        public static readonly GUIContent gui_cap_flt   = new GUIContent ("  Filters",         "");
        public static readonly GUIContent gui_flt_size  = new GUIContent ("Minimum Size",      "Objects with size lower than defined value will not make sound.");
        public static readonly GUIContent gui_flt_dist  = new GUIContent ("Camera Distance",   "Objects with distance to main camera higher than defined value will not make sound.");
    }
    
    /// <summary>
    /// Rayfire Unyielding text for GUI elements.
    /// </summary>
    public static class TextUny
    {
        // Strings
        public const string rec_move = "Center Move";
        public const string rec_bnd  = "Change Bounds";
        
        // Button
        public static readonly GUIContent gui_btn_act = new GUIContent ("   Activate   ", "Activate.");
        public static readonly GUIContent gui_btn_res = new GUIContent ("   Reset   ",    "Reset.");
        public static readonly GUIContent gui_btn_cnt = new GUIContent ("Show Center",    "Show Center.");
        
        // GUI
        public static readonly GUIContent gui_cap_prp = new GUIContent ("  Properties",    "");
        public static readonly GUIContent gui_uny     = new GUIContent ("Unyielding",      "Set Unyielding property for children Rigids and Shards.");
        public static readonly GUIContent gui_act     = new GUIContent ("Activatable",     "Set Activatable property for children Rigids and Shards.");
        public static readonly GUIContent gui_sim     = new GUIContent ("Simulation Type", "Custom simulation type.");
        public static readonly GUIContent gui_cap_giz = new GUIContent ("  Gizmo",         "");
        public static readonly GUIContent gui_show    = new GUIContent ("Show",            "");
        public static readonly GUIContent gui_size    = new GUIContent ("Size",            "Unyielding gizmo size.");
        public static readonly GUIContent gui_center  = new GUIContent ("Center",          "Unyielding gizmo center.");
        public static readonly GUIContent gui_al_sz   = new GUIContent ("Size",            "Align size.");
    }
    
    /// <summary>
    /// Rayfire Vortex text for GUI elements.
    /// </summary>
    public static class TextVrt
    {
        // GUI
        public static readonly GUIContent gui_cap_anc  = new GUIContent ("  Anchor",      "");
        public static readonly GUIContent gui_anc_show = new GUIContent ("Show Handle",   "");
        public static readonly GUIContent gui_anc_top  = new GUIContent ("Top Point",     "");
        public static readonly GUIContent gui_anc_bot  = new GUIContent ("Bottom Point",  "");
        public static readonly GUIContent gui_cap_giz  = new GUIContent ("  Gizmo",       "");
        public static readonly GUIContent gui_giz_show = new GUIContent ("Show",          "");
        public static readonly GUIContent gui_giz_top  = new GUIContent ("Top Radius",    "");
        public static readonly GUIContent gui_giz_bot  = new GUIContent ("Bottom Radius", "");
        public static readonly GUIContent gui_cap_eye  = new GUIContent ("  Eye",         "");
        public static readonly GUIContent gui_eye      = new GUIContent ("Eye",           "");
        public static readonly GUIContent gui_cap_str  = new GUIContent ("  Strength",    "");
        public static readonly GUIContent gui_stiff    = new GUIContent ("Stiffness",     "");
        public static readonly GUIContent gui_swirl    = new GUIContent ("Swirl",         "");
        public static readonly GUIContent gui_strFrc   = new GUIContent ("Force By Mass", "");
        public static readonly GUIContent gui_cap_tor  = new GUIContent ("  Torque",      "");
        public static readonly GUIContent gui_tor_en   = new GUIContent ("Enable",        "");
        public static readonly GUIContent gui_tor_str  = new GUIContent ("Strength",      "");
        public static readonly GUIContent gui_tor_var  = new GUIContent ("Variation",     "");
        public static readonly GUIContent gui_cap_hei  = new GUIContent ("  Height Bias", "");
        public static readonly GUIContent gui_hei_en   = new GUIContent ("Enable",        "");
        public static readonly GUIContent gui_speed    = new GUIContent ("Speed",         "");
        public static readonly GUIContent gui_spread   = new GUIContent ("Spread",        "");
        public static readonly GUIContent gui_cap_seed = new GUIContent ("  Seed",        "");
        public static readonly GUIContent gui_seed     = new GUIContent ("Seed",          "");
        public static readonly GUIContent gui_cap_prev = new GUIContent ("  Preview",     "");
        public static readonly GUIContent gui_circles  = new GUIContent ("Circles ",      "");
        public static readonly GUIContent gui_cap_flt  = new GUIContent ("  Filters",     "");
        public static readonly GUIContent gui_tag      = new GUIContent ("Tag",           "");
        public static readonly GUIContent gui_lay      = new GUIContent ("Mask",         "");
    }
    
    /// <summary>
    /// Rayfire Wind text for GUI elements.
    /// </summary>
    public static class TextWnd
    {
        // GUI
        public static readonly GUIContent gui_cap_giz    = new GUIContent ("  Gizmo",       "");
        public static readonly GUIContent gui_giz_show   = new GUIContent ("Show",          "");
        public static readonly GUIContent gui_giz_size   = new GUIContent ("Size",          "");
        public static readonly GUIContent gui_cap_nse    = new GUIContent ("  Noise Scale", "");
        public static readonly GUIContent gui_nse_show   = new GUIContent ("Show",          "");
        public static readonly GUIContent gui_nse_global = new GUIContent ("Global",        "");
        public static readonly GUIContent gui_nse_length = new GUIContent ("Length",        "");
        public static readonly GUIContent gui_nse_width  = new GUIContent ("Width",         "");
        public static readonly GUIContent gui_nse_speed  = new GUIContent ("Speed",         "");
        public static readonly GUIContent gui_cap_str    = new GUIContent ("  Strength",    "");
        public static readonly GUIContent gui_str_min    = new GUIContent ("Minimum",       "");
        public static readonly GUIContent gui_str_max    = new GUIContent ("Maximum",       "");
        public static readonly GUIContent gui_str_tor    = new GUIContent ("Torque",        "");
        public static readonly GUIContent gui_str_frc    = new GUIContent ("Force By Mass", "");
        public static readonly GUIContent gui_cap_dir    = new GUIContent ("  Direction",   "");
        public static readonly GUIContent gui_dir_div    = new GUIContent ("Divergency",    "");
        public static readonly GUIContent gui_dir_tur    = new GUIContent ("Turbulence",    "");
        public static readonly GUIContent gui_cap_prev   = new GUIContent ("  Preview",     "");
        public static readonly GUIContent gui_prv_dens   = new GUIContent ("Density",       "");
        public static readonly GUIContent gui_prv_size   = new GUIContent ("Size",          "");
        public static readonly GUIContent gui_cap_flt    = new GUIContent ("  Filters",     "");
        public static readonly GUIContent gui_tag        = new GUIContent ("Tag",           "");
        public static readonly GUIContent gui_lay        = new GUIContent ("Mask",         "");
    }
}



                   
/*
if (prop.hasMultipleDifferentValues == true)
    ProgressBar (prop.intValue / 100.0f, "Armor");

prop.hasMultipleDifferentValues;
prop.boolValue;

// Custom GUILayout progress bar.
void ProgressBar (float value, string label)
{
    // Get a rect for the progress bar using the same margins as a textfield:
    Rect rect = GUILayoutUtility.GetRect (18, 18, "TextField");
    EditorGUI.ProgressBar (rect, value, label);
    EditorGUILayout.Space ();
}
*/





/*

EditorGUILayout.PrefixLabel ("MinMax");
EditorGUILayout.FloatField (wind.minimum, GUILayout.Width (50));

EditorGUI.BeginChangeCheck();
EditorGUILayout.MinMaxSlider (ref wind.minimum, ref wind.maximum, -5f, 5, GUILayout.Width (EditorGUIUtility.currentViewWidth - 400f));
if (EditorGUI.EndChangeCheck() == true)
{
    foreach (RayfireWind scr in targets)
    {
        scr.minimum = wind.minimum;
        scr.maximum = wind.maximum;
        SetDirty (scr);
    }
}

EditorGUILayout.FloatField (wind.maximum, GUILayout.Width (50));
GUILayout.EndHorizontal ();

*/