using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireUnyielding))]
    public class RayfireUnyieldingEditor : Editor
    {
        RayfireUnyielding uny;
        Vector3           centerWorldPos;
        BoxBoundsHandle   m_BoundsHandle = new BoxBoundsHandle();

        // Foldout
        static bool fld_ali;
        
        // Serialized properties
        SerializedProperty sp_uny;
        SerializedProperty sp_act;
        SerializedProperty sp_sim;
        SerializedProperty sp_show;
        SerializedProperty sp_size;
        SerializedProperty sp_cent;
        SerializedProperty sp_center;
        SerializedProperty sp_al_sz;
        
        void OnEnable()
        {
            // Get component
            uny = (RayfireUnyielding)target;
            
            // Find properties
            sp_uny    = serializedObject.FindProperty(nameof(uny.unyielding));
            sp_act    = serializedObject.FindProperty(nameof(uny.activatable));
            sp_sim    = serializedObject.FindProperty(nameof(uny.simulationType));
            sp_show   = serializedObject.FindProperty(nameof(uny.showGizmo));
            sp_size   = serializedObject.FindProperty(nameof(uny.size));
            sp_center = serializedObject.FindProperty(nameof(uny.centerPosition));
            sp_al_sz  = serializedObject.FindProperty(nameof(uny.alSz));
            sp_cent   = serializedObject.FindProperty(nameof(uny.showCenter));
            
            // Foldout
            if (EditorPrefs.HasKey (TextKeys.uny_fld_ali) == true) fld_ali = EditorPrefs.GetBool (TextKeys.uny_fld_ali);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            GUICommon.Space();
            
            GUI_Button();
            GUI_Properties();
            GUI_Gizmo();
            // GUI_Align();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }

        /// /////////////////////////////////////////////////////////
        /// Button
        /// /////////////////////////////////////////////////////////

        void GUI_Button()
        {
            if (Application.isPlaying == true)
            {
                if (GUILayout.Button (TextUny.gui_btn_act, GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireUnyielding != null)
                            (targ as RayfireUnyielding).Activate();
                GUICommon.Space();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void GUI_Properties()
        {
            GUICommon.CaptionBox (TextUny.gui_cap_prp);
            GUICommon.PropertyField (sp_uny, TextUny.gui_uny);
            GUICommon.PropertyField (sp_act, TextUny.gui_act);
            GUICommon.PropertyField (sp_sim, TextUny.gui_sim);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Gizmo
        /// /////////////////////////////////////////////////////////
        
        void GUI_Gizmo()
        {
            GUICommon.CaptionBox (TextUny.gui_cap_giz);
            GUICommon.PropertyField (sp_show,   TextUny.gui_show);
            GUICommon.PropertyField (sp_size,   TextUny.gui_size);
            GUICommon.PropertyField (sp_center, TextUny.gui_center);
            EditorGUILayout.BeginHorizontal ();
            GUICommon.Toggle (sp_cent, TextUny.gui_btn_cnt);
            if (GUILayout.Button (TextUny.gui_btn_res, GUILayout.Height (22)))
            {
                Undo.RecordObjects (targets, TextUny.gui_btn_res.text);
                foreach (RayfireUnyielding scr in targets)
                {
                    scr.centerPosition = Vector3.zero;
                    SetDirty (scr);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void GUI_Align()
        {
            SetFoldoutPref (ref fld_ali, TextKeys.uny_fld_ali, "Align");
            if (fld_ali == true)
            {
                GUICommon.Space();
                
                EditorGUI.BeginChangeCheck();
                
                if (GUILayout.Button ("Up", GUILayout.Height (22)))
                {
                    Undo.RecordObjects (targets, "Align");
                    SetUnyGizmo (1);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button ("Left   -X  ", GUILayout.Height (22)))
                {
                    Undo.RecordObjects (targets, "Align");
                    SetUnyGizmo (4);
                }
                if (GUILayout.Button ("Right    X", GUILayout.Height (22)))
                {
                    Undo.RecordObjects (targets, "Align");
                    SetUnyGizmo (3);
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button ("  Back -Z   ", GUILayout.Height (22)))
                {
                    Undo.RecordObjects (targets, "Align");
                    SetUnyGizmo (6);
                }
                if (GUILayout.Button ("Forward Z", GUILayout.Height (22)))
                {
                    Undo.RecordObjects (targets, "Align");
                    SetUnyGizmo (5);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button ("Down", GUILayout.Height (22)))
                {
                    Undo.RecordObjects (targets, "Align");
                    SetUnyGizmo (2);
                }

                if (EditorGUI.EndChangeCheck())
                    SceneView.RepaintAll();
                
                GUICommon.Space();

                GUICommon.PropertyField (sp_al_sz, TextUny.gui_al_sz);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////
        
        [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
        static void DrawGizmosSelected (RayfireUnyielding targ, GizmoType gizmoType)
        {
            if (targ.enabled && targ.showGizmo == true)
            {
                Gizmos.color  = GUICommon.color_blue;
                Gizmos.matrix = targ.transform.localToWorldMatrix;
                Gizmos.DrawWireCube (targ.centerPosition, targ.size);
            }
        }

        private void OnSceneGUI()
        {
            if (uny.enabled && uny.showGizmo == true)
            {
                Transform transform      = uny.transform;
                centerWorldPos  = transform.TransformPoint (uny.centerPosition);

                // Point3 handle
                if (uny.showCenter == true)
                {
                    EditorGUI.BeginChangeCheck();
                    centerWorldPos = Handles.PositionHandle (centerWorldPos, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck() == true)
                        Undo.RecordObject (uny, TextUny.rec_move);
                                    
                    uny.centerPosition = transform.InverseTransformPoint (centerWorldPos);
                }
                
                Handles.matrix = uny.transform.localToWorldMatrix;
                m_BoundsHandle.wireframeColor = GUICommon.color_blue;
                m_BoundsHandle.center         = uny.centerPosition;
                m_BoundsHandle.size           = uny.size;

                // Draw the handle
                EditorGUI.BeginChangeCheck();
                m_BoundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject (uny, TextUny.rec_bnd);
                    uny.size = m_BoundsHandle.size;
                }
            }
        }
        
        void SetUnyGizmo (int state)
        {
            // Get maximum bound
            Transform tm               = uny.transform;
            Bounds    bound            = RFCluster.GetChildrenBound (tm);
            Vector3   localBoundCenter = tm.InverseTransformPoint(bound.center);
            float     al_size          = Mathf.Abs (uny.alSz);
            
            uny.size.x = bound.size.x / tm.localScale.x;
            uny.size.y = bound.size.y / tm.localScale.y;
            uny.size.z = bound.size.z / tm.localScale.z;
            
            if (state == 1 || state == 2)
            {
                // Set size in units
                uny.size.y = al_size;
                
                // Set center position in local space to center
                uny.centerPosition.x = localBoundCenter.x;
                uny.centerPosition.z = localBoundCenter.z;
                
                if (state == 1)
                    uny.centerPosition.y = localBoundCenter.y + Mathf.Abs(bound.size.y / 2f / tm.localScale.y) - al_size / 2f;
                if (state == 2)
                    uny.centerPosition.y = localBoundCenter.y - Mathf.Abs(bound.size.y / 2f / tm.localScale.y) + al_size / 2f;
            }
            
            if (state == 3 || state == 4)
            {
                // Set size in units
                uny.size.x = al_size;
                
                // Set center position in local space to center
                uny.centerPosition.z = localBoundCenter.z;
                uny.centerPosition.y = localBoundCenter.y;
                if (state == 3)
                    uny.centerPosition.x = localBoundCenter.x + Mathf.Abs(bound.size.x / 2f / tm.localScale.x) - al_size / 2f;
                if (state == 4)
                    uny.centerPosition.x = localBoundCenter.x - Mathf.Abs(bound.size.x / 2f / tm.localScale.x) + al_size / 2f;
            }
            
            if (state == 5 || state == 6)
            {
                // Set size in units
                uny.size.z = al_size;

                // Set center position in local space to center
                uny.centerPosition.x = localBoundCenter.x;
                uny.centerPosition.y = localBoundCenter.y;
                                
                if (state == 5)
                    uny.centerPosition.z = localBoundCenter.z + Mathf.Abs(bound.size.z / 2f / tm.localScale.z) - al_size / 2f;
                if (state == 6)
                    uny.centerPosition.z = localBoundCenter.x - Mathf.Abs(bound.size.z / 2f / tm.localScale.z) + al_size / 2f;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////
        
        void SetDirty (RayfireUnyielding scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
                SceneView.RepaintAll();
            }
        }
        
        void SetFoldoutPref (ref bool val, string pref, string caption) 
        {
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Foldout (val, caption, true);
            if (EditorGUI.EndChangeCheck() == true)
                EditorPrefs.SetBool (pref, val);
        }
    }
}