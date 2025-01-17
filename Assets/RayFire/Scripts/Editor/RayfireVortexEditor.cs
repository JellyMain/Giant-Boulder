using UnityEngine;
using UnityEditor;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireVortex))]
    public class RayfireVortexEditor : Editor
    {
        RayfireVortex vortex;
        
        // Minimum & Maximum ranges
        const float gizmo_top_min  = 0.1f;
        const float gizmo_top_max  = 50f;
        const float gizmo_bot_min  = 0f;
        const float gizmo_bot_max  = 50f;
        const float eye_min        = 0.05f;
        const float eye_max        = 0.9f;
        const float stiff_min      = 1f;
        const float stiff_max      = 10f;
        const float swirl_min      = -40f;
        const float swirl_max      = 40f;
        const float torque_str_min = -1f;
        const float torque_str_max = 1f;
        const float torque_var_min = 0f;
        const float torque_var_max = 10f;
        const float speed_min      = 0f;
        const float speed_max      = 1f;
        const float spread_min     = 0f;
        const float spread_max     = 1f;
        const int   seed_min       = 0;
        const int   seed_max       = 100;
        const int   circles_min    = 2;
        const int   circles_max    = 10;
        
        // Serialized properties
        SerializedProperty sp_anc_show;
        SerializedProperty sp_anc_top;
        SerializedProperty sp_anc_bot;
        SerializedProperty sp_giz_show;
        SerializedProperty sp_giz_top;
        SerializedProperty sp_giz_bot;
        SerializedProperty sp_eye;
        SerializedProperty sp_stiff;
        SerializedProperty sp_swirl;
        SerializedProperty sp_strFrc;
        SerializedProperty sp_tor_en;
        SerializedProperty sp_tor_str;
        SerializedProperty sp_tor_var;
        SerializedProperty sp_hei_en;
        SerializedProperty sp_speed;
        SerializedProperty sp_spread;
        SerializedProperty sp_seed;
        SerializedProperty sp_circles;
        SerializedProperty sp_tag;
        SerializedProperty sp_mask;
        
        private void OnEnable()
        {
            // Get component
            vortex = (RayfireVortex)target;
            
            // Set tag list
            GUICommon.SetTags();
            
            // Find properties
            sp_anc_show = serializedObject.FindProperty(nameof(vortex.topHandle));
            sp_anc_top  = serializedObject.FindProperty(nameof(vortex.topAnchor));
            sp_anc_bot  = serializedObject.FindProperty(nameof(vortex.bottomAnchor));
            sp_giz_show = serializedObject.FindProperty(nameof(vortex.showGizmo));
            sp_giz_top  = serializedObject.FindProperty(nameof(vortex.topRadius));
            sp_giz_bot  = serializedObject.FindProperty(nameof(vortex.bottomRadius));
            sp_eye      = serializedObject.FindProperty(nameof(vortex.eye));
            sp_stiff    = serializedObject.FindProperty(nameof(vortex.stiffness));
            sp_swirl    = serializedObject.FindProperty(nameof(vortex.swirlStrength));
            sp_strFrc   = serializedObject.FindProperty(nameof(vortex.forceByMass));
            sp_tor_en   = serializedObject.FindProperty(nameof(vortex.enableTorque));
            sp_tor_str  = serializedObject.FindProperty(nameof(vortex.torqueStrength));
            sp_tor_var  = serializedObject.FindProperty(nameof(vortex.torqueVariation));
            sp_hei_en   = serializedObject.FindProperty(nameof(vortex.enableHeightBias));
            sp_speed    = serializedObject.FindProperty(nameof(vortex.biasSpeed));
            sp_spread   = serializedObject.FindProperty(nameof(vortex.biasSpread));
            sp_seed     = serializedObject.FindProperty(nameof(vortex.seed));
            sp_circles  = serializedObject.FindProperty(nameof(vortex.circles));
            sp_tag      = serializedObject.FindProperty(nameof(vortex.tagFilter));
            sp_mask     = serializedObject.FindProperty(nameof(vortex.mask));
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////   
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            GUI_Anchor();
            GUI_Gizmo();
            GUI_Eye();
            GUI_Strength();
            GUI_Torque();
            GUI_Height();
            GUI_Seed();
            GUI_Preview();
            GUI_Filters();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Anchor
        /// /////////////////////////////////////////////////////////  

        void GUI_Anchor()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_anc);
            GUICommon.PropertyField (sp_anc_show, TextVrt.gui_anc_show);
            GUICommon.PropertyField (sp_anc_top,  TextVrt.gui_anc_top);
            GUICommon.PropertyField (sp_anc_bot,  TextVrt.gui_anc_bot);
        }

        /// /////////////////////////////////////////////////////////
        /// Gizmo
        /// /////////////////////////////////////////////////////////  
        
        void GUI_Gizmo()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_giz);
            GUICommon.PropertyField (sp_giz_show, TextVrt.gui_giz_show);
            GUICommon.Slider (sp_giz_top, gizmo_top_min, gizmo_top_max, TextVrt.gui_giz_top);
            GUICommon.Slider (sp_giz_bot, gizmo_bot_min, gizmo_bot_max, TextVrt.gui_giz_bot);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Eye
        /// /////////////////////////////////////////////////////////  

        void GUI_Eye()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_eye);
            GUICommon.Slider (sp_eye, eye_min, eye_max, TextVrt.gui_eye);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Strength
        /// /////////////////////////////////////////////////////////  

        void GUI_Strength()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_str);
            GUICommon.Slider (sp_stiff, stiff_min, stiff_max, TextVrt.gui_stiff);
            GUICommon.Slider (sp_swirl, swirl_min, swirl_max, TextVrt.gui_swirl);
            GUICommon.PropertyField (sp_strFrc, TextVrt.gui_strFrc);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Torque
        /// /////////////////////////////////////////////////////////  

        void GUI_Torque()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_tor);
            GUICommon.PropertyField (sp_tor_en, TextVrt.gui_tor_en);
            if (vortex.enableTorque == true)
            {
                GUICommon.Slider (sp_tor_str, torque_str_min, torque_str_max, TextVrt.gui_tor_str);
                GUICommon.Slider (sp_tor_var, torque_var_min, torque_var_max, TextVrt.gui_tor_var);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Height
        /// /////////////////////////////////////////////////////////  

        void GUI_Height()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_hei);
            GUICommon.PropertyField (sp_hei_en, TextVrt.gui_hei_en);
            if (vortex.enableHeightBias == true)
            {
                GUICommon.Slider (sp_speed,  speed_min,  speed_max,  TextVrt.gui_speed);
                GUICommon.Slider (sp_spread, spread_min, spread_max, TextVrt.gui_spread);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Seed
        /// /////////////////////////////////////////////////////////  

        void GUI_Seed()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_seed);
            GUICommon.IntSlider (sp_seed, seed_min, seed_max, TextVrt.gui_seed);
        }

        /// /////////////////////////////////////////////////////////
        /// Preview
        /// /////////////////////////////////////////////////////////  

        void GUI_Preview()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_prev);
            GUICommon.IntSlider (sp_circles, circles_min, circles_max, TextVrt.gui_circles);
        }

        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////  
        
        void GUI_Filters()
        {
            GUICommon.CaptionBox (TextVrt.gui_cap_flt);
            GUICommon.TagField (sp_tag, TextWnd.gui_tag);
            GUICommon.MaskField (sp_mask, TextVrt.gui_lay);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected)]
        void OnSceneGUI()
        {
            if (vortex.showGizmo == true)
            {
                Transform transForm = vortex.transform;

                // Start check for changes and record undo
                EditorGUI.BeginChangeCheck();

                // Top Bottom circles
                Handles.DrawWireDisc (transForm.TransformPoint (vortex.topAnchor),    transForm.up, vortex.topRadius);
                Handles.DrawWireDisc (transForm.TransformPoint (vortex.bottomAnchor), transForm.up, vortex.bottomRadius);

                // Top Bottom radius handles
                vortex.topRadius    = Handles.RadiusHandle (transForm.rotation, transForm.TransformPoint (vortex.topAnchor),    vortex.topRadius,    true);
                vortex.bottomRadius = Handles.RadiusHandle (transForm.rotation, transForm.TransformPoint (vortex.bottomAnchor), vortex.bottomRadius, true);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObject (vortex, "Change Gizmo");
                }

                // Top point handle
                if (vortex.topHandle == true)
                {
                    vortex.topAnchor = transForm.InverseTransformPoint (Handles.PositionHandle (transForm.TransformPoint (vortex.topAnchor), transForm.rotation));
                    if (vortex.topAnchor.x > 20)
                        vortex.topAnchor.x = 20;
                    else if (vortex.topAnchor.z > 20)
                        vortex.topAnchor.z = 20;
                    if (vortex.topAnchor.x < -20)
                        vortex.topAnchor.x = -20;
                    else if (vortex.topAnchor.z < -20)
                        vortex.topAnchor.z = -20;
                }
            }
        }
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireVortex vortex, GizmoType gizmoType)
        {
            if (vortex.showGizmo)
            {
                // Vars
                Vector3 previousPoint = Vector3.zero;
                Vector3 nextPoint     = Vector3.zero;

                // Gizmo properties
                Gizmos.color  = GUICommon.color_blue;
                Gizmos.matrix = vortex.transform.localToWorldMatrix;

                // Gizmo center line
                Gizmos.DrawLine (vortex.topAnchor, vortex.bottomAnchor);

                // Draw main circles
                DrawCircle (vortex.topAnchor,    vortex.topRadius,    previousPoint, nextPoint);
                DrawCircle (vortex.bottomAnchor, vortex.bottomRadius, previousPoint, nextPoint);

                // Draw main eyes circles
                DrawCircle (vortex.topAnchor,    vortex.topRadius * vortex.eye,    previousPoint, nextPoint);
                DrawCircle (vortex.bottomAnchor, vortex.bottomRadius * vortex.eye, previousPoint, nextPoint);

                // Draw additional circles
                //if (vortex.circles > 2)
                //{
                //    float step = 1f / (vortex.circles - 1);
                //    for (int i = 1; i < vortex.circles - 1; i++)
                //    {
                //        Vector3 midPoint = Vector3.Lerp(vortex.bottomAnchor, vortex.topAnchor, step *i);
                //        float rad = Mathf.Lerp(vortex.bottomRadius, vortex.topRadius, step * i);
                //        DrawCircle(midPoint, rad);
                //        DrawCircle(midPoint, (vortex.topRadius + vortex.bottomRadius) / 2f * vortex.eye);
                //    }
                //}

                // Selectable sphere
                float sphereSize = (vortex.topRadius + vortex.bottomRadius) * 0.03f;
                if (sphereSize < 0.1f)
                    sphereSize = 0.1f;
                Gizmos.color = GUICommon.color_orange;
                Gizmos.DrawSphere (new Vector3 (vortex.bottomRadius,  0f, 0f),                                sphereSize);
                Gizmos.DrawSphere (new Vector3 (-vortex.bottomRadius, 0f, 0f),                                sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,                   0f, vortex.bottomRadius),               sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,                   0f, -vortex.bottomRadius),              sphereSize);
                Gizmos.DrawSphere (new Vector3 (vortex.topRadius,  0f, 0f) + vortex.topAnchor,                sphereSize);
                Gizmos.DrawSphere (new Vector3 (-vortex.topRadius, 0f, 0f) + vortex.topAnchor,                sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,                0f, vortex.topRadius) + vortex.topAnchor,  sphereSize);
                Gizmos.DrawSphere (new Vector3 (0f,                0f, -vortex.topRadius) + vortex.topAnchor, sphereSize);

                //// Draw circle gizmo
                //void DrawHelix()
                //{
                //    float detalization = 200f;
                //    // Starting position from bottom to top on vortex axis
                //    Vector3 bottomStartPos = vortex.bottomAnchor;
                //    Vector3 vectorToTop = vortex.topAnchor - vortex.bottomAnchor;
                //    Vector3 vectorToTopStep = vectorToTop / detalization;
                //    float swirlNow = 0f;
                //    float swirlRate = 0.1f;
                //    float heightRateNow = 0f;
                //    previousPoint = bottomStartPos;
                //    nextPoint = Vector3.zero;
                //    float heightRateStep = 1f / detalization;
                //    while (heightRateNow < 1f)
                //    {
                //        // Next swirl rate
                //        swirlNow += swirlRate;

                //        // Increase current rate for lerp
                //        heightRateNow += heightRateStep;

                //        // Get average radius by height
                //        float radius = Mathf.Lerp(vortex.bottomRadius, vortex.topRadius, heightRateNow);

                //        // Get next point on vortex axis
                //        bottomStartPos += vectorToTopStep;

                //        // Get local helix point
                //        Vector3 point = Vector3.zero;
                //        point.x = Mathf.Cos(swirlNow) * radius;
                //        point.z = Mathf.Sin(swirlNow) * radius;

                //        // Get final vortex point
                //        point += bottomStartPos;

                //        // Gizmos.DrawWireSphere(point, 0.1f);
                //        Gizmos.DrawLine(point, previousPoint);
                //        // Gizmos.DrawWireSphere(point, 0.1f);
                //        previousPoint = point;
                //    }
                //}
            }
        }
        
        static void DrawCircle (Vector3 point, float radius, Vector3 previousPoint, Vector3 nextPoint)
        {
            // Draw top eye
            const int size  = 45;
            float     rate  = 0f;
            float     scale = 1f / size;
            nextPoint.y     = point.y;
            previousPoint.y = point.y;
            previousPoint.x = radius * Mathf.Cos (rate) + point.x;
            previousPoint.z = radius * Mathf.Sin (rate) + point.z;
            for (int i = 0; i < size; i++)
            {
                rate        += 2.0f * Mathf.PI * scale;
                nextPoint.x =  radius * Mathf.Cos (rate) + point.x;
                nextPoint.z =  radius * Mathf.Sin (rate) + point.z;

                Gizmos.DrawLine (previousPoint, nextPoint);
                previousPoint = nextPoint;
            }
        }
    }
}