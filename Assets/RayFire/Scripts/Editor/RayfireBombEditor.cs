using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEditor.IMGUI.Controls;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireBomb))]
    public class RayfireBombEditor : Editor
    {
        RayfireBomb     bomb;
        ReorderableList rl_obst_list;

        // Handles
        BoxBoundsHandle    m_BoxHandle;
        SphereBoundsHandle m_SphHandle;

        // Foldout
        static bool fld_det;
        static bool fld_obs;
        static bool fld_aud;
        static bool fld_fil = true;
        
        // Minimum & Maximum ranges
        const int   deletion_min     = 0;
        const int   deletion_max     = 30;
        const int   variation_min    = 0;
        const int   variation_max    = 100;
        const int   chaos_min        = 0;
        const int   chaos_max        = 90;
        const float damageValue_min  = 0;
        const float damageValue_max  = 100f;
        const float heightOffset_min = -10;
        const float heightOffset_max = 10;
        const float delay_min        = 0;
        const float delay_max        = 10f;
        const float volume_min       = 0.01f;
        const float volume_max       = 1f;
            
        // Serialized properties
        SerializedProperty sp_showGizmo;
        SerializedProperty sp_rangeType;
        SerializedProperty sp_range;
        SerializedProperty sp_boxSize;
        SerializedProperty sp_deletion;
        SerializedProperty sp_fadeType;
        SerializedProperty sp_strength;
        SerializedProperty sp_curve;
        SerializedProperty sp_variation;
        SerializedProperty sp_chaos;
        SerializedProperty sp_forceByMass;
        SerializedProperty sp_affectInactive;
        SerializedProperty sp_affectKinematic;
        SerializedProperty sp_applyDamage;
        SerializedProperty sp_damageValue;
        SerializedProperty sp_heightOffset;
        SerializedProperty sp_delay;
        SerializedProperty sp_atStart;
        SerializedProperty sp_destroy;
        SerializedProperty sp_obst_enable;
        SerializedProperty sp_obst_static;
        SerializedProperty sp_obst_kinematik;
        SerializedProperty sp_obst_list;
        SerializedProperty sp_play;
        SerializedProperty sp_volume;
        SerializedProperty sp_clip;
        SerializedProperty sp_tag;
        SerializedProperty sp_mask;
        
        private void OnEnable()
        {
            // Get component
            bomb = (RayfireBomb)target;
            
            // Set tag list
            GUICommon.SetTags();
            
            // Box handle
            m_BoxHandle = new BoxBoundsHandle
            {
                wireframeColor = GUICommon.color_blue,
                handleColor    = GUICommon.color_orange,
            };

            // Sphere handle
            m_SphHandle = new SphereBoundsHandle
            {
                wireframeColor = GUICommon.color_blue,
                handleColor    = GUICommon.color_orange,
            };
            
            // Find properties
            sp_showGizmo       = serializedObject.FindProperty(nameof(bomb.showGizmo));
            sp_rangeType       = serializedObject.FindProperty(nameof(bomb.rangeType));
            sp_range           = serializedObject.FindProperty(nameof(bomb.range));
            sp_boxSize         = serializedObject.FindProperty(nameof(bomb.boxSize));
            sp_deletion        = serializedObject.FindProperty(nameof(bomb.deletion));
            sp_fadeType        = serializedObject.FindProperty(nameof(bomb.fadeType));
            sp_strength        = serializedObject.FindProperty(nameof(bomb.strength));
            sp_curve           = serializedObject.FindProperty(nameof(bomb.curve));
            sp_variation       = serializedObject.FindProperty(nameof(bomb.variation));
            sp_chaos           = serializedObject.FindProperty(nameof(bomb.chaos));
            sp_forceByMass     = serializedObject.FindProperty(nameof(bomb.forceByMass));
            sp_affectInactive  = serializedObject.FindProperty(nameof(bomb.affectInactive));
            sp_affectKinematic = serializedObject.FindProperty(nameof(bomb.affectKinematic));
            sp_applyDamage     = serializedObject.FindProperty(nameof(bomb.applyDamage));
            sp_damageValue     = serializedObject.FindProperty(nameof(bomb.damageValue));
            sp_heightOffset    = serializedObject.FindProperty(nameof(bomb.heightOffset));
            sp_delay           = serializedObject.FindProperty(nameof(bomb.delay));
            sp_atStart         = serializedObject.FindProperty(nameof(bomb.atStart));
            sp_destroy         = serializedObject.FindProperty(nameof(bomb.destroy));
            sp_obst_enable     = serializedObject.FindProperty(nameof(bomb.obst_enable));
            sp_obst_static     = serializedObject.FindProperty(nameof(bomb.obst_static));
            sp_obst_kinematik  = serializedObject.FindProperty(nameof(bomb.obst_kinematik));
            sp_obst_list       = serializedObject.FindProperty(nameof(bomb.obst_list));
            sp_play            = serializedObject.FindProperty(nameof(bomb.play));
            sp_volume          = serializedObject.FindProperty(nameof(bomb.volume));
            sp_clip            = serializedObject.FindProperty(nameof(bomb.clip));
            sp_tag            = serializedObject.FindProperty(nameof(bomb.tagFilter));
            sp_mask            = serializedObject.FindProperty(nameof(bomb.mask));

            // Reorderable list
            rl_obst_list = new ReorderableList (serializedObject, sp_obst_list, true, true, true, true)
            {
                drawElementCallback = DrawObstColListItems,
                drawHeaderCallback  = DrawObstColHeader,
                onAddCallback       = AddObstCol,
                onRemoveCallback    = RemoveObstCol
            };
            
            // Foldouts
            if (EditorPrefs.HasKey (TextKeys.bmb_fld_det) == true) fld_det = EditorPrefs.GetBool (TextKeys.bmb_fld_det);
            if (EditorPrefs.HasKey (TextKeys.bmb_fld_obs) == true) fld_obs = EditorPrefs.GetBool (TextKeys.bmb_fld_obs);
            if (EditorPrefs.HasKey (TextKeys.bmb_fld_aud) == true) fld_aud = EditorPrefs.GetBool (TextKeys.bmb_fld_aud);
            if (EditorPrefs.HasKey (TextKeys.bmb_fld_fil) == true) fld_fil = EditorPrefs.GetBool (TextKeys.bmb_fld_fil);

            // Set curve
            sp_curve.animationCurveValue = bomb.curve;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            // GUI
            GUI_Actions();
            GUI_Range();
            GUI_Impulse();
            GUI_Activation();
            GUI_Damage();
            GUI_Detonation();
            GUI_Obstacles();
            GUI_Audio();
            GUI_Filters();

            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Actions
        /// /////////////////////////////////////////////////////////

        void GUI_Actions()
        {
            if (Application.isPlaying == true)
            {
                GUICommon.Caption (TextBmb.gui_cap_actions);
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button (TextBmb.gui_btn_explode, GUILayout.Height (25)))
                {
                    foreach (RayfireBomb script in targets)
                    {
                        script.Explode (script.delay);
                        SetDirty (script);
                    }
                }
                
                if (GUILayout.Button (TextBmb.gui_btn_restore, GUILayout.Height (25)))
                {
                    foreach (RayfireBomb script in targets)
                    {
                        script.Restore ();
                        SetDirty (script);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Range
        /// /////////////////////////////////////////////////////////

        void GUI_Range()
        {
            GUICommon.CaptionBox (TextBmb.gui_cap_range);
            GUICommon.PropertyField (sp_showGizmo, TextBmb.gui_showGizmo);
            GUICommon.PropertyField (sp_rangeType, TextBmb.gui_rangeType);
            GUICommon.PropertyField (sp_range,     TextBmb.gui_range);

            if (bomb.rangeType == RayfireBomb.RangeType.Directional)
                GUICommon.PropertyField (sp_boxSize, TextBmb.gui_boxSize);

            GUICommon.IntSlider (sp_deletion, deletion_min, deletion_max, TextBmb.gui_deletion);
        }

        /// /////////////////////////////////////////////////////////
        /// Impulse
        /// /////////////////////////////////////////////////////////
        
        void GUI_Impulse()
        {
            GUICommon.CaptionBox (TextBmb.gui_cap_impulse);
            GUICommon.PropertyField (sp_fadeType, TextBmb.gui_fadeType);
            
            if (bomb.fadeType == RayfireBomb.BombFadeType.ByCurve)
            {
                EditorGUILayout.CurveField (sp_curve, GUICommon.color_orange, new Rect (new Vector2 (0, 0), new Vector2 (1, 1)), TextBmb.gui_curve);
                GUICommon.Space ();
            }
            
            GUICommon.PropertyField (sp_strength, TextBmb.gui_strength);
            GUICommon.IntSlider (sp_variation, variation_min, variation_max, TextBmb.gui_variation);
            GUICommon.IntSlider (sp_chaos,     chaos_min,     chaos_max,     TextBmb.gui_chaos);
            GUICommon.PropertyField (sp_forceByMass, TextBmb.gui_forceByMass);
        }

        /// /////////////////////////////////////////////////////////
        /// Activation
        /// /////////////////////////////////////////////////////////

        void GUI_Activation()
        {
            GUICommon.CaptionBox (TextBmb.gui_cap_act);
            GUICommon.PropertyField (sp_affectInactive,  TextBmb.gui_affectInactive);
            GUICommon.PropertyField (sp_affectKinematic, TextBmb.gui_affectKinematic);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Damage
        /// /////////////////////////////////////////////////////////
        
        void GUI_Damage()
        {
            GUICommon.CaptionBox (TextBmb.gui_cap_dmg);
            GUICommon.PropertyField (sp_applyDamage, TextBmb.gui_applyDamage);
            if (sp_applyDamage.boolValue == true)
                GUICommon.Slider (sp_damageValue, damageValue_min, damageValue_max, TextBmb.gui_damageValue);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Detonation
        /// /////////////////////////////////////////////////////////
        
        void GUI_Detonation()
        {
            GUICommon.CaptionBox (TextBmb.gui_cap_prop);
            GUICommon.Foldout (ref fld_det, TextKeys.bmb_fld_det, TextBmb.gui_fld_det.text);
            if (fld_det == true)
            {
                EditorGUI.indentLevel++;
                
                // Height offset only for spherical bomb
                if (bomb.rangeType == RayfireBomb.RangeType.Spherical)
                    GUICommon.Slider (sp_heightOffset, heightOffset_min, heightOffset_max, TextBmb.gui_heightOffset);

                GUICommon.Slider (sp_delay, delay_min, delay_max, TextBmb.gui_delay);
                GUICommon.PropertyField (sp_atStart, TextBmb.gui_atStart);
                GUICommon.PropertyField (sp_destroy, TextBmb.gui_destroy);

                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Obstacles
        /// /////////////////////////////////////////////////////////
        
        void GUI_Obstacles()
        {
            GUICommon.Foldout (ref fld_obs, TextKeys.bmb_fld_obs, TextBmb.gui_fld_obs.text);
            if (fld_obs == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.PropertyField (sp_obst_enable,    TextBmb.gui_obst_enable);
                GUICommon.PropertyField (sp_obst_static,    TextBmb.gui_obst_static);
                GUICommon.PropertyField (sp_obst_kinematik, TextBmb.gui_obst_kinematik);
                rl_obst_list.DoLayoutList();
                GUICommon.Space ();
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Audio
        /// /////////////////////////////////////////////////////////
        
        void GUI_Audio()
        {
            GUICommon.Foldout (ref fld_aud, TextKeys.bmb_fld_aud, TextBmb.gui_fld_aud.text);
            if (fld_aud == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.PropertyField (sp_play, TextBmb.gui_play);
                GUICommon.Slider (sp_volume, volume_min, volume_max, TextBmb.gui_volume);
                EditorGUILayout.ObjectField (sp_clip, TextBmb.gui_clip);
                GUICommon.Space ();
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Filters
        /// /////////////////////////////////////////////////////////
        
        void GUI_Filters()
        {
            GUICommon.Foldout (ref fld_fil, TextKeys.bmb_fld_fil, TextBmb.gui_fld_fil.text);
            if (fld_fil == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.TagField (sp_tag, TextBmb.gui_tagFilter);
                GUICommon.MaskField (sp_mask, TextBmb.gui_mask);
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Draw
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireBomb bomb, GizmoType gizmoType)
        {
            if (bomb.showGizmo == true)
            {
                // Gizmo properties
                Matrix4x4 mat = bomb.transform.localToWorldMatrix;
                mat.SetTRS (bomb.transform.position, bomb.transform.rotation, Vector3.one);
                Gizmos.matrix = mat;
                Gizmos.color  = GUICommon.color_blue;
                
                if (bomb.rangeType == RayfireBomb.RangeType.Spherical)
                    DrawGizmoSphere (bomb);
                else if (bomb.rangeType == RayfireBomb.RangeType.Directional)
                    DrawGizmoBox (bomb);
            }
        }
        
        static void DrawGizmoSphere (RayfireBomb bomb)
        {
            // Vars
            const int   size          = 45;
            const float scale         = 1f / size;
            Vector3     previousPoint = Vector3.zero;
            Vector3     nextPoint     = Vector3.zero;
            float       h             = bomb.heightOffset;

            // Draw top eye
            float rate            = 0f;
            nextPoint.y     = h;
            previousPoint.y = h;
            previousPoint.x = bomb.range * Mathf.Cos (rate);
            previousPoint.z = bomb.range * Mathf.Sin (rate);
            for (int i = 0; i < size; i++)
            {
                rate        += 2.0f * Mathf.PI * scale;
                nextPoint.x =  bomb.range * Mathf.Cos (rate);
                nextPoint.z =  bomb.range * Mathf.Sin (rate);
                Gizmos.DrawLine (previousPoint, nextPoint);
                previousPoint = nextPoint;
            }

            // Draw top eye
            rate            = 0f;
            nextPoint.x     = 0f;
            previousPoint.x = 0f;
            previousPoint.y = bomb.range * Mathf.Cos (rate) + h;
            previousPoint.z = bomb.range * Mathf.Sin (rate);
            for (int i = 0; i < size; i++)
            {
                rate        += 2.0f * Mathf.PI * scale;
                nextPoint.y =  bomb.range * Mathf.Cos (rate) + h;
                nextPoint.z =  bomb.range * Mathf.Sin (rate);
                Gizmos.DrawLine (previousPoint, nextPoint);
                previousPoint = nextPoint;
            }

            // Draw top eye
            rate            = 0f;
            nextPoint.z     = 0f;
            previousPoint.z = 0f;
            previousPoint.y = bomb.range * Mathf.Cos (rate) + h;
            previousPoint.x = bomb.range * Mathf.Sin (rate);
            for (int i = 0; i < size; i++)
            {
                rate        += 2.0f * Mathf.PI * scale;
                nextPoint.y =  bomb.range * Mathf.Cos (rate) + h;
                nextPoint.x =  bomb.range * Mathf.Sin (rate);
                Gizmos.DrawLine (previousPoint, nextPoint);
                previousPoint = nextPoint;
            }

            // Selectable sphere
            float sphereSize = bomb.range * 0.07f;
            if (sphereSize < 0.1f)
                sphereSize = 0.1f;
            Gizmos.color = GUICommon.color_orange;
            Gizmos.DrawSphere (new Vector3 (0f,          bomb.range + h,  0f),          sphereSize);
            Gizmos.DrawSphere (new Vector3 (0f,          -bomb.range + h, 0f),          sphereSize);
            Gizmos.DrawSphere (new Vector3 (bomb.range,  h,               0f),          sphereSize);
            Gizmos.DrawSphere (new Vector3 (-bomb.range, h,              0f),          sphereSize);
            Gizmos.DrawSphere (new Vector3 (0f,          h,              bomb.range),  sphereSize);
            Gizmos.DrawSphere (new Vector3 (0f,          h,              -bomb.range), sphereSize);

            // Center helper
            Gizmos.color = Color.red;
            Gizmos.DrawSphere (new Vector3 (0f, 0f, 0f), sphereSize / 3f);

            // Height offset helper
            if (bomb.heightOffset != 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere (new Vector3 (0f, bomb.heightOffset, 0f), sphereSize / 3f);
            }
        }

        static void DrawGizmoBox(RayfireBomb bomb)
        {
            // Draw cube
            Vector3 center = Vector3.zero;
            center.z += bomb.range / 2f;
            Vector3 size = new Vector3(bomb.boxSize.x, bomb.boxSize.y, bomb.range);
            Gizmos.DrawWireCube (center, size);
            
            // Draw arrow
            Gizmos.DrawLine (Vector3.zero, center);
            Gizmos.DrawLine (center,       new Vector3 (center.x - center.z*0.1f, 0, center.z*0.8f));
            Gizmos.DrawLine (center,       new Vector3 (center.x + center.z*0.1f, 0, center.z*0.8f));
            
            // Draw center
            float sphereSize = bomb.range * 0.05f;
            if (sphereSize < 0.1f)
                sphereSize = 0.1f;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere (new Vector3 (0f, 0f, 0f), sphereSize / 2f);
            
            // Selectable sphere
            Gizmos.color = GUICommon.color_orange;
            Gizmos.DrawSphere (new Vector3 (0,          0,          bomb.range),   sphereSize);
            Gizmos.DrawSphere (new Vector3 (size.x/2f,  0,          bomb.range/2), sphereSize);
            Gizmos.DrawSphere (new Vector3 (-size.x/2f, 0,          bomb.range/2), sphereSize);
            Gizmos.DrawSphere (new Vector3 (0f,         size.y/2f,  bomb.range/2), sphereSize);
            Gizmos.DrawSphere (new Vector3 (0f,         -size.y/2f, bomb.range/2), sphereSize);
        }

        /// /////////////////////////////////////////////////////////
        /// ReorderableList obstacles
        /// /////////////////////////////////////////////////////////
        
        void DrawObstColListItems (Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = rl_obst_list.serializedProperty.GetArrayElementAtIndex (index);
            EditorGUI.PropertyField (new Rect (rect.x, rect.y + 2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }

        void DrawObstColHeader (Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField (rect, TextBmb.gui_obst_list);
        }

        void AddObstCol (ReorderableList list)
        {
            if (bomb.obst_list == null)
                bomb.obst_list = new List<Collider>();
            bomb.obst_list.Add (null);
            list.index = list.count;
        }

        void RemoveObstCol (ReorderableList list)
        {
            if (bomb.obst_list != null)
            {
                bomb.obst_list.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        void OnSceneGUI()
        {
            if (bomb == null)
                return;

            if (bomb.enabled == true)
            {
                // Set matrix
                Matrix4x4 mat = bomb.transform.localToWorldMatrix;
                mat.SetTRS (bomb.transform.position, bomb.transform.rotation, Vector3.one);
                Handles.matrix = mat;

                // Draw handles
                if (bomb.rangeType == RayfireBomb.RangeType.Spherical)
                    SphericalHandles(bomb);
                else if (bomb.rangeType == RayfireBomb.RangeType.Directional)
                    BoxHandles(bomb);
            }
        }
        
        void SphericalHandles (RayfireBomb bmb)
        {
            Vector3 ho = Vector3.zero;
            ho.y += bmb.heightOffset;

            /*
           EditorGUI.BeginChangeCheck();
           bomb.range = Handles.RadiusHandle (Quaternion.identity, ho, bomb.range);
           if (EditorGUI.EndChangeCheck() == true)
           {
               Undo.RecordObject (bomb, "Change Range");
               SetDirty (bomb);
           }
           */
            
           m_SphHandle.radius = bmb.range;
           m_SphHandle.center = ho;
           EditorGUI.BeginChangeCheck();
           m_SphHandle.DrawHandle();
           if (EditorGUI.EndChangeCheck())
           {
               Undo.RecordObject (bmb, "Change Bounds");
               bmb.range          = m_SphHandle.radius;
               m_SphHandle.center = ho;
               SetDirty (bmb);
           }
        }
        
        void BoxHandles (RayfireBomb bmb)
        {
            // TODO Height offset support
            
            // Set box handle size
            Vector3 boxSize;
            boxSize.x             = bmb.boxSize.x;
            boxSize.y             = bmb.boxSize.y;
            boxSize.z             = bmb.range;
            m_BoxHandle.size   = boxSize;
            m_BoxHandle.center = Vector3.zero + new Vector3 (0, 0, bmb.range / 2f);
            
            EditorGUI.BeginChangeCheck();
            m_BoxHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject (bmb, "Change Bounds");
                bmb.boxSize     = m_BoxHandle.size;
                bmb.range       = m_BoxHandle.size.z;
                m_BoxHandle.center = Vector3.zero + new Vector3 (0, 0, bmb.range / 2f);
                SetDirty (bmb);
            }
            
            // TODO draw arrow
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty (RayfireBomb scr)
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