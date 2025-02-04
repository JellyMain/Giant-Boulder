/*using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using GeNa.Core;
using UnityEngine.Rendering;

//[ExecuteInEditMode]
public class FlowCaptureHelper : EditorWindow
{
    [MenuItem("Window/FlowCapture")]
    private static void ShowWindow()
    {
        var window = GetWindow<FlowCaptureHelper>();
        window.titleContent = new GUIContent("Backup Data");
        window.Show();
    }

    static void Init()
    {
        FlowCaptureHelper window = (FlowCaptureHelper)EditorWindow.GetWindow(typeof(FlowCaptureHelper));
        window.Show();
    }

    //Varaibles
    GeNaRiverProfile riverProfile;

    [SerializeField] public Material waterMaterial;

    private void OnGUI()
    {
        var serializedObject = new SerializedObject(this);

        if(GUILayout.Button("CaptureFlow"))
        {
            CaptureFlowz();
        }
        riverProfile = EditorGUILayout.ObjectField("River Profile", riverProfile, typeof(GeNaRiverProfile), false) as GeNaRiverProfile;
    }


    private void CaptureFlowz()
    {

        GameObject newOcean = GameObject.Find("Water Surface");
        CaptureFlow captureFlow = new CaptureFlow();

        Material riverMat = Selection.activeGameObject.GetComponent<Renderer>().sharedMaterial;
        string filePath = Application.dataPath + "/Procedural Worlds/GeNa/Scripts/Core/Extensions/RiverFlow/GeNa River/Textures/" + "tFlow" + ".png";

        if (newOcean != null)
        {
            captureFlow.CaptureRiverFlow(Selection.activeGameObject.transform.parent, filePath, 1024, riverMat, newOcean.transform);
        }
        else
        {
            captureFlow.CaptureRiverFlow(Selection.activeGameObject.transform.parent, filePath, 1024, riverMat, null);
        }
       
    }


}*/
