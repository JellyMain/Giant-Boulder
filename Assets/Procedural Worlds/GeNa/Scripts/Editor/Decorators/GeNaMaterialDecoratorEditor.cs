using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaMaterialDecorator))]
    public class GeNaMaterialDecoratorEditor : GeNaDecoratorEditor<GeNaMaterialDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Material Decorator")]
        public static void AddDecorator(MenuCommand command) => GeNaDecoratorEditorUtility.CreateDecorator<GeNaMaterialDecorator>(command);
        /// <summary>
        /// Creates Material Profile asset
        /// </summary>
        [MenuItem("Assets/Create/Procedural Worlds/GeNa/Create Material Profile")]
        public static void CreateMaterialProfile()
        {
            GeNaMaterialDecoratorDatabase asset = ScriptableObject.CreateInstance<GeNaMaterialDecoratorDatabase>();
            AssetDatabase.CreateAsset(asset, "Assets/GeNa Material Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        protected override void SettingsPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(Styles.box);
            Decorator.Enabled = EditorUtils.Toggle("Enabled", Decorator.Enabled, helpEnabled);
            if (Decorator.Enabled)
            {
                EditorGUILayout.BeginHorizontal();
                GeNaMaterialDecoratorDatabase data = Decorator.MaterialData;
                data = (GeNaMaterialDecoratorDatabase)EditorUtils.ObjectField("MaterialData", data, typeof(GeNaMaterialDecoratorDatabase), false);
                if (data != Decorator.MaterialData)
                {
                    Decorator.MaterialData = data;
                    Decorator.MaterialData.GetPresets();
                }
                if (EditorUtils.Button("Edit", GUILayout.MaxWidth(40f)))
                {
                    if (Decorator.MaterialData != null)
                    {
                        Selection.activeObject = Decorator.MaterialData;
                        EditorGUIUtility.PingObject(Decorator.MaterialData);
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorUtils.InlineHelp("MaterialData", helpEnabled);

                if (Decorator.MaterialData != null)
                {
                    Decorator.MaterialData.GetPresets();
                    if (Decorator.MaterialData.m_presets.Count > 0)
                    {
                        if (Decorator.MaterialData.m_overridePreset)
                        {
                            EditorGUILayout.HelpBox(EditorUtils.GetTextValue("OverrideMaterialPresetEnabled"), MessageType.Warning);
                            GUI.enabled = false;
                        }

                        EditorGUILayout.BeginVertical(Styles.box);
                        Decorator.SelectedPreset = EditorGUILayout.Popup("Preset", Decorator.SelectedPreset, Decorator.MaterialData.m_presets.ToArray());
                        EditorUtils.InlineHelp("Preset", helpEnabled);

                        GUI.enabled = true;
                        Decorator.GetMaterialType = (GetMaterialIDType)EditorUtils.EnumPopup("MaterialGetType", Decorator.GetMaterialType, helpEnabled);
                        switch (Decorator.GetMaterialType)
                        {
                            case GetMaterialIDType.ID:
                            {
                                Decorator.MaterialInstanceID = EditorUtils.IntField("MaterialID", Decorator.MaterialInstanceID, helpEnabled);
                                break;
                            }
                            case GetMaterialIDType.Name:
                            {
                                if (Decorator.MaterialNames.Count > 0)
                                {
                                    for (int i = 0; i < Decorator.MaterialNames.Count; i++)
                                    {
                                        EditorGUILayout.BeginVertical(Styles.box);
                                        EditorGUILayout.BeginHorizontal();
                                        Decorator.MaterialNames[i] = EditorUtils.TextField("MaterialName", Decorator.MaterialNames[i]);
                                        if (GUILayout.Button("-", GUILayout.MaxWidth(25f)))
                                        {
                                            Decorator.MaterialNames.RemoveAt(i);
                                            EditorGUIUtility.ExitGUI();
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorUtils.InlineHelp("MaterialName", helpEnabled);
                                        EditorGUILayout.EndVertical();
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.HelpBox("Please add a material name below this can be part of a name as the check is to look if the mateiral name is contained within the full name. Example: Leaf, instead of Tree_Leaf_Pine_01.", MessageType.Info);
                                }
                                if (EditorUtils.Button("AddNewMaterialName"))
                                {
                                    Decorator.MaterialNames.Add(null);
                                }
                                break;
                            }
                        }
                        EditorGUILayout.EndVertical();

                        /*EditorGUILayout.BeginVertical(Styles.box);
                        EditorGUILayout.BeginHorizontal();
                        EditorUtils.Heading("Renderer Setup", GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth - 100f));
                        if (EditorUtils.Button("ClearRenderers", GUILayout.MaxWidth(100f)))
                        {
                            Decorator.RendererData.Clear();
                            EditorGUIUtility.ExitGUI();
                        }
                        EditorGUILayout.EndHorizontal();

                        Decorator.ShowRenderData = EditorGUILayout.BeginFoldoutHeaderGroup(Decorator.ShowRenderData, "Renderers");
                        if (Decorator.ShowRenderData)
                        {
                            if (Decorator.RendererData.Count > 0)
                            {
                                for (int i = 0; i < Decorator.RendererData.Count; i++)
                                {
                                    EditorGUILayout.BeginVertical(Styles.box);
                                    if (Decorator.RendererData[i].m_meshRenderer == null)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        Decorator.RendererData[i].m_meshRenderer =
                                            (MeshRenderer) EditorUtils.ObjectField("Renderer",
                                                Decorator.RendererData[i].m_meshRenderer, typeof(MeshRenderer), true,
                                                helpEnabled);
                                        if (GUILayout.Button("Get", GUILayout.MaxWidth(50f)))
                                        {
                                            Decorator.RendererData[i].m_meshRenderer =
                                                Decorator.GetComponent<MeshRenderer>();
                                            if (Decorator.RendererData[i].m_meshRenderer == null)
                                            {
                                                Decorator.RendererData[i].m_meshRenderer =
                                                    Decorator.GetComponentInChildren<MeshRenderer>();
                                            }

                                            EditorGUIUtility.ExitGUI();
                                        }

                                        if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                                        {
                                            Decorator.RendererData.RemoveAt(i);
                                            EditorGUIUtility.ExitGUI();
                                        }

                                        EditorGUILayout.EndHorizontal();
                                    }
                                    else
                                    {
                                        Decorator.RendererData[i].m_meshRenderer =
                                            (MeshRenderer) EditorUtils.ObjectField("Renderer",
                                                Decorator.RendererData[i].m_meshRenderer, typeof(MeshRenderer), true,
                                                helpEnabled);

                                        EditorGUILayout.BeginHorizontal();
                                        Decorator.RendererData[i].m_materialID = EditorUtils.IntSlider("MaterialID",
                                            Decorator.RendererData[i].m_materialID, 0,
                                            Decorator.RendererData[i].m_meshRenderer.sharedMaterials.Length - 1);
                                        if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                                        {
                                            Decorator.RendererData.RemoveAt(i);
                                            EditorGUIUtility.ExitGUI();
                                        }

                                        EditorGUILayout.EndHorizontal();
                                    }

                                    EditorGUILayout.EndVertical();
                                }
                            }

                            if (EditorUtils.Button("AddNewRendererData"))
                            {
                                Decorator.RendererData.Add(new MaterialDecoratorRendererData());
                            }
                        }

                        EditorGUILayout.EndFoldoutHeaderGroup();

                        EditorGUILayout.BeginVertical(Styles.box);
                        m_editorUtils.Heading("Drag and Drop Setup");
                        DragAndDropWindow();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();*/
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("No data has been found in the material data. Please edit the material data and add at least 1 material data.", MessageType.Info);
                    }
                }
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object o in targets)
                {
                    GeNaMaterialDecorator decorator = (GeNaMaterialDecorator) o;
                    decorator.Enabled = Decorator.Enabled;
                    decorator.MaterialData = Decorator.MaterialData;
                    EditorUtility.SetDirty(o);
                }
            }
        }

        /// <summary>
        /// Handle drop area for new objects
        /// </summary>
        public bool DragAndDropWindow()
        {
            // Ok - set up for drag and drop
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            string dropMsg = m_editorUtils.GetTextValue("Drag And Drop GameObjects in the scene to fetch the renderer data.");
            GUI.Box(dropArea, dropMsg, EditorStyles.helpBox);
            if (evt.type == EventType.DragPerform || evt.type == EventType.DragUpdated)
            {
                if (!dropArea.Contains(evt.mousePosition))
                {
                    return false;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    // Handle prefabs / detail scriptable object
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject go)
                        {
                            /*MeshRenderer[] renderers = go.GetComponents<MeshRenderer>();
                            if (renderers.Length < 1)
                            {
                                renderers = go.GetComponentsInChildren<MeshRenderer>();
                            }

                            if (renderers.Length > 0)
                            {
                                foreach (MeshRenderer renderer in renderers)
                                {
                                    Decorator.RendererData.Add(new MaterialDecoratorRendererData
                                    {
                                        m_meshRenderer = renderer, 
                                        m_materialID = Decorator.MaterialData.GetMaterialIDFromType(renderer, Decorator.MaterialNames, Decorator.MaterialInstanceID, Decorator.GetMaterialType)
                                    });
                                }
                            }*/
                        }
                    }

                    return true;
                }
            }
            return false;
        }
    }
}