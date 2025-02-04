using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RayFire;

namespace RayFireEditor
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireRecorder))]
    public class RayfireRecorderEditor : Editor
    {
        RayfireRecorder recorder;
        
        // Minimum & Maximum ranges
        const float duration_min = 1f;
        const float duration_max = 60f;
        const int   rate_min     = 1;
        const int   rate_max     = 60;
        const float thresh_min   = 0;
        const float thresh_max   = 0.05f;
        
        // Serialized properties
        SerializedProperty sp_mode;
        SerializedProperty sp_rec_start;
        SerializedProperty sp_rec_clip;
        SerializedProperty sp_rec_dur;
        SerializedProperty sp_rec_rate;
        SerializedProperty sp_rec_reduce;
        SerializedProperty sp_rec_thresh;
        SerializedProperty sp_rec_str;
        SerializedProperty sp_rec_stop;
        SerializedProperty sp_pla_start;
        SerializedProperty sp_pla_clip;
        SerializedProperty sp_pla_cont;
        SerializedProperty sp_pla_rigid;

        private void OnEnable()
        {
            recorder = (RayfireRecorder)target;
            
            // Find properties
            sp_mode       = serializedObject.FindProperty(nameof(recorder.mode));
            sp_rec_start  = serializedObject.FindProperty(nameof(recorder.recordOnStart));
            sp_rec_clip   = serializedObject.FindProperty(nameof(recorder.clipName));
            sp_rec_dur    = serializedObject.FindProperty(nameof(recorder.duration));
            sp_rec_rate   = serializedObject.FindProperty(nameof(recorder.rate));
            sp_rec_reduce = serializedObject.FindProperty(nameof(recorder.reduceKeys));
            sp_rec_thresh = serializedObject.FindProperty(nameof(recorder.threshold));
            sp_pla_start  = serializedObject.FindProperty(nameof(recorder.playOnStart));
            sp_pla_clip   = serializedObject.FindProperty(nameof(recorder.animationClip));
            sp_pla_cont   = serializedObject.FindProperty(nameof(recorder.controller));
            sp_pla_rigid  = serializedObject.FindProperty(nameof(recorder.rigidAction));
        }


        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            // Update changed properties
            serializedObject.Update();
            
            GUICommon.PropertyField (sp_mode, TextRec.gui_mode);
            if (recorder.mode == RayfireRecorder.AnimatorType.Record)
                GUI_Record();
            if (recorder.mode == RayfireRecorder.AnimatorType.Play)
                GUI_Play();
            
            // Apply changes
            serializedObject.ApplyModifiedProperties();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Record
        /// /////////////////////////////////////////////////////////

        void GUI_Record()
        {
            GUICommon.Caption (TextRec.gui_cap_props);

            GUI_RecordStart();
            GUI_RecordButtons();
            GUICommon.Space ();
                
            GUICommon.PropertyField (sp_rec_clip, TextRec.gui_rec_clip);
            GUICommon.Slider (sp_rec_dur, duration_min, duration_max, TextRec.gui_rec_dur);
            GUICommon.IntSlider (sp_rec_rate, rate_min, rate_max, TextRec.gui_rec_rate);
            GUICommon.PropertyField (sp_rec_reduce, TextRec.gui_rec_reduce);
            
            if (recorder.reduceKeys == true)
            {
                EditorGUI.indentLevel++;
                GUICommon.Slider (sp_rec_thresh, thresh_min, thresh_max, TextRec.gui_rec_thresh);
                EditorGUI.indentLevel--;
            }
        }
        
        void GUI_RecordStart()
        {
            if (Application.isPlaying == false)
                GUICommon.PropertyField (sp_rec_start, TextRec.gui_rec_start);

            if (Application.isPlaying == true && recorder.recordOnStart == true)
            {
                if (recorder.recorder == true)
                {
                    if (GUILayout.Button (TextRec.gui_btn_rec_stop, GUILayout.Height (25)))
                        recorder.StopRecord();
                    GUICommon.Space ();
                    GUILayout.Label (TextRec.rec + recorder.recordedTime.ToString("N1") + "/" + recorder.duration);
                }
            }
        }
        
        void GUI_RecordButtons()
        {
            if (Application.isPlaying == true && recorder.recordOnStart == false)
            {
                GUILayout.BeginHorizontal();
                if (recorder.recorder == false)
                    if (GUILayout.Button (TextRec.gui_btn_rec_start, GUILayout.Height (25)))
                        recorder.StartRecord();
                if (recorder.recorder == true)
                    if (GUILayout.Button (TextRec.gui_btn_rec_stop, GUILayout.Height (25)))
                        recorder.StopRecord();
                EditorGUILayout.EndHorizontal();
            }
            
            if (Application.isPlaying == true && recorder.recordOnStart == false)
            {
                if (recorder.recorder == true)
                {
                    GUICommon.Space ();
                    GUILayout.Label (TextRec.rec + recorder.recordedTime.ToString("N1") + "/" + recorder.duration);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Play
        /// /////////////////////////////////////////////////////////
        
        void GUI_Play()
        {
            GUICommon.Caption (TextRec.gui_cap_props);
            if (Application.isPlaying == false)
                GUICommon.PropertyField (sp_pla_start, TextRec.gui_pla_start);
            
            GUI_PlayButtons();
            
            GUICommon.PropertyField (sp_pla_clip, TextRec.gui_pla_clip);
            GUICommon.PropertyField (sp_pla_cont, TextRec.gui_pla_cont);

            GUICommon.Caption (TextRec.gui_cap_rigid);
            GUICommon.PropertyField (sp_pla_rigid, TextRec.gui_pla_rigid);
        }
        
        void GUI_PlayButtons()
        {
            if (Application.isPlaying == true && recorder.playOnStart == false && recorder.recorder == false)
            {
                if (GUILayout.Button (TextRec.gui_btn_pla_start, GUILayout.Height (25)))
                    recorder.StartPlay();
                GUICommon.Space ();
            }
        }
    }
}