using UnityEngine;
using UnityEditor;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireWind))]
    public class RayfireWindEditor : Editor
    {
        RayfireWind  wind;
        static Color color = Color.red;
        
        // Minimum & Maximum ranges
        const float global_min     = 1f;
        const float global_max     = 100f;
        const float length_min     = 1f;
        const float length_max     = 300f;
        const float width_min      = 1f;
        const float width_max      = 300f;
        const float speed_min      = -200f;
        const float speed_max      = 200f;
        const float strength_min   = -5f;
        const float strength_max   = 5f;
        const float torque_min     = 0f;
        const float torque_max     = 10f;
        const float divergency_min = 0;
        const float divergency_max = 180f;
        const float turbulence_min = 0.01f;
        const float turbulence_max = 2f;
        const float density_min    =  0.5f;
        const float density_max    = 5f;
        const float size_min       = 0.1f;
        const float size_max       = 5f;
        
        // Serialized properties
        SerializedProperty sp_giz_show;
        SerializedProperty sp_giz_size;
        SerializedProperty sp_nse_show;
        SerializedProperty sp_nse_global;
        SerializedProperty sp_nse_length;
        SerializedProperty sp_nse_width;
        SerializedProperty sp_nse_speed;
        SerializedProperty sp_str_min;
        SerializedProperty sp_str_max;
        SerializedProperty sp_str_tor;
        SerializedProperty sp_str_frc;
        SerializedProperty sp_dir_div;
        SerializedProperty sp_dir_tur;
        SerializedProperty sp_prv_dens;
        SerializedProperty sp_prv_size;
        SerializedProperty sp_tag;
        SerializedProperty sp_mask;
        
        private void OnEnable()
        {
            // Get component
            wind = (RayfireWind)target;
            
            // Set tag list
            GUICommon.SetTags();
            
            // Find properties
            sp_giz_show   = serializedObject.FindProperty(nameof(wind.showGizmo));
            sp_giz_size   = serializedObject.FindProperty(nameof(wind.gizmoSize));
            sp_nse_show   = serializedObject.FindProperty(nameof(wind.showNoise));
            sp_nse_global = serializedObject.FindProperty(nameof(wind.globalScale));
            sp_nse_length = serializedObject.FindProperty(nameof(wind.lengthScale));
            sp_nse_width  = serializedObject.FindProperty(nameof(wind.widthScale));
            sp_nse_speed  = serializedObject.FindProperty(nameof(wind.speed));
            sp_str_min    = serializedObject.FindProperty(nameof(wind.minimum));
            sp_str_max    = serializedObject.FindProperty(nameof(wind.maximum));
            sp_str_tor    = serializedObject.FindProperty(nameof(wind.torque));
            sp_str_frc    = serializedObject.FindProperty(nameof(wind.forceByMass));
            sp_dir_div    = serializedObject.FindProperty(nameof(wind.divergency));
            sp_dir_tur    = serializedObject.FindProperty(nameof(wind.turbulence));
            sp_prv_dens   = serializedObject.FindProperty(nameof(wind.previewDensity));
            sp_prv_size   = serializedObject.FindProperty(nameof(wind.previewSize));
            sp_tag        = serializedObject.FindProperty(nameof(wind.tagFilter));
            sp_mask       = serializedObject.FindProperty(nameof(wind.mask));
        }

        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////      
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            GUI_Gizmo();
            GUI_Noise();
            GUI_Strength();
            GUI_Direction();
            GUI_Preview();
            GUI_Filters();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////  

        void GUI_Gizmo()
        {
            GUICommon.CaptionBox (TextWnd.gui_cap_giz);
            GUICommon.PropertyField (sp_giz_show, TextWnd.gui_giz_show);
            GUICommon.PropertyField (sp_giz_size, TextWnd.gui_giz_size);
        }

        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////  
        
        void GUI_Noise()
        {
            GUICommon.CaptionBox (TextWnd.gui_cap_nse);
            GUICommon.PropertyField (sp_nse_show, TextWnd.gui_nse_show);
            GUICommon.Slider (sp_nse_global, global_min, global_max, TextWnd.gui_nse_global);
            GUICommon.Slider (sp_nse_length, length_min, length_max, TextWnd.gui_nse_length);
            GUICommon.Slider (sp_nse_width,  width_min,  width_max,  TextWnd.gui_nse_width);
            GUICommon.Slider (sp_nse_speed,  speed_min,  speed_max,  TextWnd.gui_nse_speed);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Strength
        /// /////////////////////////////////////////////////////////  

        void GUI_Strength()
        {
            GUICommon.CaptionBox (TextWnd.gui_cap_str);
            GUICommon.Slider (sp_str_min, strength_min, strength_max, TextWnd.gui_str_min);
            GUICommon.Slider (sp_str_max, strength_min, strength_max, TextWnd.gui_str_max);
            GUICommon.Slider (sp_str_tor, torque_min,   torque_max,   TextWnd.gui_str_tor);
            GUICommon.PropertyField (sp_str_frc, TextWnd.gui_str_frc);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Direction
        /// /////////////////////////////////////////////////////////  
        
        void GUI_Direction()
        {
            GUICommon.CaptionBox (TextWnd.gui_cap_dir);
            GUICommon.Slider (sp_dir_div, divergency_min, divergency_max, TextWnd.gui_dir_div);
            GUICommon.Slider (sp_dir_tur, turbulence_min, turbulence_max, TextWnd.gui_dir_tur);
        }

        /// /////////////////////////////////////////////////////////
        /// Preview
        /// /////////////////////////////////////////////////////////  
        
        void GUI_Preview()
        {
            GUICommon.CaptionBox (TextWnd.gui_cap_prev);
            GUICommon.Slider (sp_prv_dens, density_min, density_max, TextWnd.gui_prv_dens);
            GUICommon.Slider (sp_prv_size, size_min,    size_max, TextWnd.gui_prv_size);
        }

        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////  
        
        void GUI_Filters()
        {
            GUICommon.CaptionBox (TextWnd.gui_cap_flt);
            GUICommon.TagField (sp_tag, TextWnd.gui_tag);
            GUICommon.MaskField (sp_mask, TextWnd.gui_lay);
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////  
        
        // Draw gizmo
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireWind wind, GizmoType gizmoType)
        {
            // Vars
            int     stepX;
            int     stepZ;
            float   windStr;
            float   x,  y,  z;
            Vector3 p1, p2, p3, p4, p5, p6, p7, p8, p10, p11, to;
            Vector3 vector;
            Vector3 localPos;
            float   perlinVal;
            color = Color.red;
            color.b = 0.0f;
            
            // Gizmo preview
            if (wind.showGizmo == true)
            {
                // Offsets
                x = wind.gizmoSize.x / 2f;
                y = wind.gizmoSize.y;
                z = wind.gizmoSize.z / 2f;

                // Get points
                p1 = new Vector3 (-x, 0, -z);
                p2 = new Vector3 (-x, 0, +z);
                p3 = new Vector3 (+x, 0, -z);
                p4 = new Vector3 (+x, 0, +z);
                p5 = new Vector3 (-x, y, -z);
                p6 = new Vector3 (-x, y, +z);
                p7 = new Vector3 (+x, y, -z);
                p8 = new Vector3 (+x, y, +z);

                p10 = new Vector3 (-x, 0, 0);
                p11 = new Vector3 (+x, 0, 0);
                to  = new Vector3 (+0, 0, z);

                // Gizmo properties
                Gizmos.color  = GUICommon.color_blue;
                Gizmos.matrix = wind.transform.localToWorldMatrix;
                
                // Gizmo Lines
                Gizmos.DrawLine (p1, p2);
                Gizmos.DrawLine (p3, p4);
                Gizmos.DrawLine (p5, p6);
                Gizmos.DrawLine (p7, p8);
                Gizmos.DrawLine (p1, p5);
                Gizmos.DrawLine (p2, p6);
                Gizmos.DrawLine (p3, p7);
                Gizmos.DrawLine (p4, p8);
                Gizmos.DrawLine (p1, p3);
                Gizmos.DrawLine (p2, p4);
                Gizmos.DrawLine (p5, p7);
                Gizmos.DrawLine (p6, p8);

                // Arrow
                Gizmos.DrawLine (p1,  Vector3.zero);
                Gizmos.DrawLine (p3,  Vector3.zero);
                Gizmos.DrawLine (p10, to);
                Gizmos.DrawLine (p11, to);

                // Selectable sphere
                float sphereSize = (x + y + z) * 0.02f;
                if (sphereSize < 0.1f)
                    sphereSize = 0.1f;
                float ySph = y / 2f;
                Gizmos.color = GUICommon.color_orange;
                Gizmos.DrawSphere (new Vector3 (x,  ySph, 0f), sphereSize);
                Gizmos.DrawSphere (new Vector3 (-x, ySph, 0f), sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f, ySph, z),  sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f, ySph, -z), sphereSize);
                
                // Force preview
                if (wind.showNoise == true)
                {
                    // Preview rate
                    stepX = (int)(wind.gizmoSize.x / wind.previewDensity);
                    stepZ = (int)(wind.gizmoSize.z / wind.previewDensity);

                    // Create preview helpers
                    for (int xx = -(stepX / 2); xx < stepX / 2 + 1; xx++)
                    {
                        for (int zz = -(stepZ / 2); zz < stepZ / 2 + 1; zz++)
                        {
                            // Local position
                            localPos   = Vector3.zero;
                            localPos.x = xx * wind.previewDensity;
                            localPos.z = zz * wind.previewDensity;
                            localPos.y = 0.2f;

                            // Get perlin value for local position
                            perlinVal = wind.PerlinFixedLocal (localPos);

                            // Get final strength for local position by min and max str
                            windStr = wind.WindStrength (perlinVal);

                            // Get vector for current point
                            vector = wind.GetVectorLocalPreview (localPos) * wind.previewSize;
                            
                            // Set color
                            if (windStr >= 0)
                            {
                                color.r = perlinVal;
                                color.g = 1f - perlinVal;
                                color.b = 0f;
                            }
                            else
                            {
                                color.r = 0f;
                                color.g = perlinVal;
                                color.b = 1f - perlinVal;
                            }

                            Gizmos.color = color;
                            Gizmos.DrawWireSphere (localPos, windStr * 0.1f * wind.previewSize);
                            Gizmos.DrawLine (localPos, localPos + vector * windStr);
                        }
                    }
                }
            }
        }
    }
}