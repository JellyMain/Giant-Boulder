//Copyright(c)2020 Procedural Worlds Pty Limited 
using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaRoadExtension))]
    public class GeNaRoadExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaRoadExtension m_extension;
        private bool m_isAsset;

        protected Editor m_roadProfileEditor;

        protected void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            m_extension = target as GeNaRoadExtension;
            if (m_extension == null)
                return;
            m_isAsset = AssetDatabase.Contains(m_extension);
        }

        public override void OnSceneGUI()
        {
            if (m_extension == null)
                m_extension = target as GeNaRoadExtension;
            if (m_extension.Spline.Settings.Advanced.DebuggingEnabled == false)
                return;
            Handles.color = Color.red;
            foreach (GeNaCurve curve in m_extension.Spline.Curves)
            {
                DrawCurveDirecton(curve);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (m_extension == null)
                m_extension = target as GeNaRoadExtension;
            EditorGUI.BeginChangeCheck();
            {
                bool defaultGUIEnabled = GUI.enabled;
                EditorGUILayout.BeginHorizontal();
                m_editorUtils.LabelField("Tag", GUILayout.MaxWidth(40));
                m_extension.Tag = EditorGUILayout.TagField(m_extension.Tag);
                m_editorUtils.LabelField("Layer", GUILayout.MaxWidth(40));
                m_extension.Layer = EditorGUILayout.LayerField(m_extension.Layer);
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("TagAndLayerHelp", HelpEnabled);
                EditorGUILayout.BeginHorizontal();
                m_extension.CastShadows = m_editorUtils.Toggle("CastShadows", m_extension.CastShadows);
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    var camera = sceneView.camera;
                    if (camera != null)
                    {
                        if (camera.actualRenderingPath != RenderingPath.Forward)
                            GUI.enabled = false;
                    }
                }

                m_extension.ReceiveShadows = m_editorUtils.Toggle("ReceiveShadows", m_extension.ReceiveShadows);
                GUI.enabled = defaultGUIEnabled;
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("ShadowsHelp", HelpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.Heading("RoadMeshSettings");
                m_editorUtils.InlineHelp("RoadMeshSettings", HelpEnabled);
                EditorGUI.indentLevel++;
                m_extension.Width = m_editorUtils.FloatField("MeshWidth", m_extension.Width, HelpEnabled);
                m_extension.SeparateYScale = m_editorUtils.Toggle("SeparateYScale", m_extension.SeparateYScale, HelpEnabled);
                EditorGUI.indentLevel++;
                if (m_extension.SeparateYScale)
                    m_extension.Height = m_editorUtils.Slider("HeightScale", m_extension.Height, 1.001f, 15.0f, HelpEnabled);
                EditorGUI.indentLevel--;
                m_extension.IntersectionSize = m_editorUtils.Slider("IntersectionSize", m_extension.IntersectionSize, 0.8f, 1.2f, HelpEnabled);
                m_extension.UseSlopedCrossSection = m_editorUtils.Toggle("UseSlopedCrossSection", m_extension.UseSlopedCrossSection, HelpEnabled);
                m_extension.CrossSectionOverride = m_editorUtils.ObjectField("CrossSectionOverride", m_extension.CrossSectionOverride, typeof(RoadCrossSectionOverride), true) as RoadCrossSectionOverride;
                m_extension.SplitAtTerrains = m_editorUtils.Toggle("SplitMeshesAtTerrains", m_extension.SplitAtTerrains, HelpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                m_editorUtils.Heading("RoadBehaviourSettings");
                m_editorUtils.InlineHelp("RoadBehaviourSettings", HelpEnabled);
                EditorGUI.indentLevel++;
                m_extension.AddRoadCollider = m_editorUtils.Toggle("AddCollider", m_extension.AddRoadCollider, HelpEnabled);
                m_extension.RaycastTerrainOnly = m_editorUtils.Toggle("RaycastTerrainOnly", m_extension.RaycastTerrainOnly, HelpEnabled);
                m_extension.ConformToGround = m_editorUtils.Toggle("ConformToGround", m_extension.ConformToGround, HelpEnabled);
                if (!m_extension.ConformToGround)
                {
                    EditorGUI.indentLevel++;
                    m_extension.GroundAttractDistance = m_editorUtils.FloatField("GroundSnapDistance", m_extension.GroundAttractDistance, HelpEnabled);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

                if (!m_isAsset)
                {
                    EditorGUILayout.Space();
                    m_editorUtils.Heading("RoadTools");
                    m_editorUtils.InlineHelp("RoadTools", HelpEnabled);
                    EditorGUI.indentLevel++;
                    if (m_editorUtils.Button("LevelIntersectionTangents", GUILayout.Width(300)))
                    {
                        m_extension.LevelIntersectionTangents();
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();

                bool showRoadSettings = true;
                var spline = m_extension.Spline;
                if (spline != null)
                {
                    showRoadSettings |= spline.gameObject.activeInHierarchy;
                }

                GUI.enabled = showRoadSettings;
                {
                    m_editorUtils.Heading("RoadRenderingSettings");
                    m_editorUtils.InlineHelp("RoadRenderingSettings", HelpEnabled);
                    EditorGUI.indentLevel++;
                    m_extension.RoadProfile = (GeNaRoadProfile)m_editorUtils.ObjectField("RoadProfile", m_extension.RoadProfile, typeof(GeNaRoadProfile), true, HelpEnabled);
                    if (m_extension.RoadProfile != null)
                    {
                        if (m_roadProfileEditor == null)
                            m_roadProfileEditor = CreateEditor(m_extension.RoadProfile);
                        GeNaRoadProfileEditor.SetProfile(m_extension.RoadProfile, (GeNaRoadProfileEditor)m_roadProfileEditor);
                        m_roadProfileEditor.OnInspectorGUI();
                    }

                    EditorGUI.indentLevel--;
                }
                GUI.enabled = defaultGUIEnabled;

                if (!m_isAsset)
                {
                    if (m_editorUtils.Button("BakeRoad", HelpEnabled))
                    {
                        if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("BakeTitleRoad"), m_editorUtils.GetTextValue("BakeMessageRoad"), "Ok"))
                            m_extension.Bake(true);
                        GUIUtility.ExitGUI();
                    }

                    if (m_extension.HasBakedRoads())
                    {
                        EditorGUILayout.Space(3);
                        if (m_editorUtils.Button("DeleteBakedRoad", HelpEnabled))
                        {
                            if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("DeleteBakeTitleRoad"), m_editorUtils.GetTextValue("DeleteBakeMessageRoad"), "Ok", "Cancel"))
                            {
                                m_extension.DeleteBakedRoad(true);
                            }

                            GUIUtility.ExitGUI();
                        }
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_extension);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawCurveDirecton(GeNaCurve geNaCurve)
        {
            Vector3 forward = (geNaCurve.P3 - geNaCurve.P0).normalized;
            GeNaSample geNaSample = geNaCurve.GetSample(0.45f);
            DrawArrow(geNaSample.Location, forward);
            geNaSample = geNaCurve.GetSample(0.5f);
            DrawArrow(geNaSample.Location, forward);
            geNaSample = geNaCurve.GetSample(0.55f);
            DrawArrow(geNaSample.Location, forward);
        }

        private void DrawArrow(Vector3 position, Vector3 direction)
        {
            direction.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Handles.DrawLine(position, position + (-direction + right) * 0.75f);
            Handles.DrawLine(position, position + (-direction - right) * 0.75f);
        }

        [MenuItem("GameObject/GeNa/Add Road Spline", false, 17)]
        public static void AddRoadSpline(MenuCommand command)
        {
            Spline spline = Spline.CreateSpline("Road Spline");
            if (spline != null)
            {
                Undo.RegisterCreatedObjectUndo(spline.gameObject, $"[{PWApp.CONF.Name}] Created '{spline.gameObject.name}'");
                GeNaCarveExtension carve = spline.AddExtension<GeNaCarveExtension>();
                carve.name = "Carve";
                GeNaClearCollidersExtension clearColliders = spline.AddExtension<GeNaClearCollidersExtension>();
                GeNaClearDetailsExtension clearDetails = spline.AddExtension<GeNaClearDetailsExtension>();
                GeNaClearTreesExtension clearTrees = spline.AddExtension<GeNaClearTreesExtension>();
                GeNaTerrainExtension terrainTexture = spline.AddExtension<GeNaTerrainExtension>();
                GeNaRoadExtension roads = spline.AddExtension<GeNaRoadExtension>();
                roads.GroundAttractDistance = 0.0f;
                roads.name = "Road";
                Selection.activeGameObject = spline.gameObject;
                carve.Width = roads.Width * 1.2f;
                clearDetails.Width = roads.Width;
                clearTrees.Width = roads.Width;
                terrainTexture.Width = roads.Width;
                if (terrainTexture != null)
                {
                    terrainTexture.name = "Texture";
                    terrainTexture.EffectType = EffectType.Texture;
                    terrainTexture.Width = roads.Width;
                }

                if (clearDetails != null)
                {
                    clearDetails.name = "Clear Details/Grass";
                    clearDetails.Width = roads.Width;
                }

                if (clearTrees != null)
                {
                    clearTrees.name = "Clear Trees";
                    clearTrees.Width = roads.Width;
                }

                if (clearColliders != null)
                {
                    clearColliders.name = "Clear Colliders";
                    clearColliders.Width = roads.Width * 2.0f;
                }
            }
        }
    }
}