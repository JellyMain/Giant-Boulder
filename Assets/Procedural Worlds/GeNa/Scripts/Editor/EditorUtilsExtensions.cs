using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PWCommon5;
namespace GeNa.Core
{
    public static class EditorUtilsExtensions
    {
        #region Methods
        public static void Fractal(this EditorUtils editorUtils, Fractal maskFractal, bool helpSwitch, bool showFalloff = true)
        {
            maskFractal.Enabled = editorUtils.Toggle("Noise Enabled", maskFractal.Enabled, helpSwitch);
            if (maskFractal.Enabled)
            {
                EditorGUI.indentLevel++;
                if (showFalloff)
                    maskFractal.NoiseFalloff = editorUtils.CurveField("Noise Falloff", maskFractal.NoiseFalloff, helpSwitch);
                maskFractal.NoiseType = (Constants.NoiseType)editorUtils.EnumPopup("Mask Type", maskFractal.NoiseType, helpSwitch);
                maskFractal.Strength = editorUtils.Slider("Strength", maskFractal.Strength, -2f, 2f, helpSwitch);
                maskFractal.Seed = editorUtils.Slider("Seed", maskFractal.Seed, 1f, 65000f, helpSwitch);
                maskFractal.Octaves = editorUtils.IntSlider("Octaves", maskFractal.Octaves, 1, 15, helpSwitch);
                maskFractal.Frequency = editorUtils.Slider("Frequency", maskFractal.Frequency, 0.0001f, 1f, helpSwitch);
                maskFractal.Persistence = editorUtils.Slider("Persistence", maskFractal.Persistence, 0f, 1f, helpSwitch);
                maskFractal.Lacunarity = editorUtils.Slider("Lacunarity", maskFractal.Lacunarity, 1.5f, 3f, helpSwitch);
                maskFractal.Amplitude = editorUtils.Slider("Amplitude", maskFractal.Amplitude, 0.00001f, 1f, helpSwitch);
                switch (maskFractal.NoiseType)
                {
                    case Constants.NoiseType.Perlin:
                    case Constants.NoiseType.Billow:
                        break;
                    case Constants.NoiseType.Ridged:
                        maskFractal.RidgedOffset = editorUtils.Slider("Ridge Offset", maskFractal.RidgedOffset, 0f, 3f, helpSwitch);
                        break;
                    case Constants.NoiseType.IQ:
                        break;
                    case Constants.NoiseType.Swiss:
                        maskFractal.RidgedOffset = editorUtils.Slider("Ridge Offset", maskFractal.RidgedOffset, 0f, 3f, helpSwitch);
                        maskFractal.Warp = editorUtils.Slider("Warp", maskFractal.Warp, 0f, 5f, helpSwitch);
                        break;
                    case Constants.NoiseType.Jordan:
                        maskFractal.Warp = editorUtils.Slider("Warp", maskFractal.Warp, 0f, 5f, helpSwitch);
                        maskFractal.Warp0 = editorUtils.Slider("Warp0", maskFractal.Warp0, 0f, 15f, helpSwitch);
                        maskFractal.Damp = editorUtils.Slider("Damp", maskFractal.Damp, 0f, 5f, helpSwitch);
                        maskFractal.Damp0 = editorUtils.Slider("Damp0", maskFractal.Damp0, 0f, 5f, helpSwitch);
                        maskFractal.DampScale = editorUtils.Slider("Damp Scale", maskFractal.DampScale, -5, 20f, helpSwitch);
                        break;
                    /*
                     *
                     * CELL NOISE
                        noiseCellType = EditorGUILayout.IntSlider(GetLabel("Cell Type"), noiseCellType, 1, 9);
                        noiseCellDistanceFunction = EditorGUILayout.IntSlider(GetLabel("Cell Dist"), noiseCellDistanceFunction, 1, 7);
                     */
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var offset = maskFractal.Offset;
                offset.x = editorUtils.FloatField("Offset X", offset.x, helpSwitch);
                offset.y = editorUtils.FloatField("Offset Y", offset.y, helpSwitch);
                maskFractal.Offset = offset;

                // Filters
                //maskFractal.Displacement = editorUtils.Slider("Displacement", maskFractal.Displacement, -1f, 1f);
                EditorGUI.indentLevel--;
            }
        }
        public static void ObjectField(this EditorUtils editorUtils, string key, SerializedProperty property, System.Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            property.objectReferenceValue = EditorGUILayout.ObjectField(editorUtils.GetContent(key), property.objectReferenceValue, objType, allowSceneObjects, options);
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUI.BeginProperty(rect, editorUtils.GetContent(key), property);
            EditorGUI.EndProperty();
        }
        public static void Popup(this EditorUtils editorUtils, string key, SerializedProperty intProperty, GUIContent[] optionKeys, bool helpSwitch)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUIContent label = EditorGUI.BeginProperty(rect, editorUtils.GetContent(key), intProperty);
            int selectedIndex = intProperty.intValue;
            intProperty.intValue = EditorGUI.Popup(rect, label, selectedIndex, optionKeys);
            EditorGUI.EndProperty();
            editorUtils.InlineHelp(key, helpSwitch);
        }
        public static void MinMaxSliderWithFields(this EditorUtils editorUtils, SerializedProperty minValueProperty, SerializedProperty maxValueProperty, ref float minValue, ref float maxValue, float minLimit, float maxLimit, string key, bool helpSwitch)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUIContent label = EditorGUI.BeginProperty(rect, editorUtils.GetContent(key), minValueProperty);
            label = EditorGUI.BeginProperty(rect, label, maxValueProperty);
            rect = EditorGUI.PrefixLabel(rect, label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float min = minValue;
            float max = maxValue;
            Rect pos = new Rect(rect.x, rect.y, EditorGUIUtility.fieldWidth, rect.height);
            float value = EditorGUI.DelayedFloatField(pos, min);
            min = Mathf.Clamp(value, minLimit, maxLimit);
            if (!Mathf.Approximately(min, minValue) && max < min)
                max = min;
            pos = new Rect(rect.x + EditorGUIUtility.fieldWidth + 5f, rect.y, rect.width - 10f - 2 * EditorGUIUtility.fieldWidth, rect.height);
            EditorGUI.MinMaxSlider(pos, ref min, ref max, minLimit, maxLimit);
            pos = new Rect(rect.xMax - EditorGUIUtility.fieldWidth, rect.y, EditorGUIUtility.fieldWidth, rect.height);
            value = EditorGUI.DelayedFloatField(pos, max);
            max = Mathf.Clamp(value, minLimit, maxLimit);
            if (!Mathf.Approximately(max, maxValue) && max < min)
                min = max;
            minValue = min;
            maxValue = max;
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
            EditorGUI.EndProperty();
            editorUtils.InlineHelp(key, helpSwitch);
        }
        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="key"></param>
        /// <param name="layerMask"></param>
        /// <param name="editorUtils"></param>
        /// <param name="helpSwitch"></param>
        /// <returns></returns>
        public static LayerMask LayerMaskField(this EditorUtils editorUtils, string key, LayerMask layerMask, bool helpSwitch)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = editorUtils.MaskField(key, maskWithoutEmpty, layers.ToArray(), helpSwitch);
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }
        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="editorUtils"></param>
        /// <param name="key"></param>
        /// <param name="property"></param>
        /// <param name="helpSwitch"></param>
        /// <returns></returns>
        public static void LayerMaskField(this EditorUtils editorUtils, string key, SerializedProperty property, bool helpSwitch)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            GUIContent label = EditorGUI.BeginProperty(rect, editorUtils.GetContent(key), property);
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & property.intValue) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = EditorGUI.MaskField(rect, label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            property.intValue = mask;
            EditorGUI.EndProperty();
            editorUtils.InlineHelp(key, helpSwitch);
        }
        public static bool BeginOverrideToggle(bool @override, EditorApplication.CallbackFunction toggleCallback, EditorApplication.CallbackFunction guiCallback)
        {
            if (BeginControlToggle(@override, out bool enabled))
                toggleCallback();
            @override = enabled;
            if (GUI.enabled == false)
                enabled = false;
            GUI.enabled = enabled;
            guiCallback();
            GUI.enabled = false;
            EndControlToggle();
            return @override;
        }
        /// <summary>
        /// Create toggle to enable/disable a control and returns true if the value of the toggle changes.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="enabled"></param>
        public static bool BeginControlToggle(bool current, out bool enabled)
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            enabled = EditorGUILayout.Toggle(current, GUILayout.Width(60f), GUILayout.ExpandWidth(false));
            bool changed = EditorGUI.EndChangeCheck();
            // Toggle works arkwardly with the clickable area shifting out to the left. It becomes non-clickable with smaller width, so using this trick.
            GUILayout.Space(-48f);
            EditorGUI.BeginDisabledGroup(!enabled);
            return changed;
        }
        /// <summary>
        /// Create toggle to enable/disable a control.
        /// </summary>
        public static void EndControlToggle()
        {
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }
        public static void TerrainModifier(this EditorUtils editorUtils, TerrainModifier terrainModifier, bool helpSwitch,
            Action<Texture2D> addBrushTexture = null,
            Action<int> removeBrushTexture = null,
            Action clearBrushTexture = null)
        {
            if (terrainModifier == null)
                return;
            // Shape
            terrainModifier.Enabled = editorUtils.Toggle("Enabled", terrainModifier.Enabled, helpSwitch);
            GUI.enabled = terrainModifier.Enabled;
            EditorGUI.indentLevel++;
            terrainModifier.BrushIndex = editorUtils.BrushSelectionGrid("TxBrush Shape",
                terrainModifier.BrushIndex,
                out _,
                terrainModifier.BrushTextures.ToArray(),
                addBrushTexture ?? terrainModifier.AddBrushTexture,
                removeBrushTexture ?? terrainModifier.RemoveBrushTexture,
                clearBrushTexture ?? terrainModifier.ClearBrushTextures,
                helpSwitch);
            terrainModifier.EffectType = (EffectType)editorUtils.EnumPopup("Effect Type", terrainModifier.EffectType, helpSwitch);
            EffectType effectType = terrainModifier.EffectType;
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                switch (effectType)
                {
                    case EffectType.Texture:
                        int alphamapLayers = terrainData.alphamapLayers;
                        TerrainLayer[] terrainLayers = terrainData.terrainLayers;
                        GUIContent[] textureChoices = new GUIContent[alphamapLayers];
                        for (int assetIdx = 0; assetIdx < textureChoices.Length; assetIdx++)
                            textureChoices[assetIdx] = new GUIContent(terrainLayers[assetIdx].diffuseTexture.name);
                        terrainModifier.TextureProtoIndex = editorUtils.Popup("Texture", terrainModifier.TextureProtoIndex, textureChoices, helpSwitch);
                        break;
                    case EffectType.Detail:
                        DetailPrototype[] detailPrototypes = terrainData.detailPrototypes;
                        GUIContent[] detailChoices = new GUIContent[detailPrototypes.Length];
                        for (int assetIdx = 0; assetIdx < detailChoices.Length; assetIdx++)
                        {
                            DetailPrototype detailProto = detailPrototypes[assetIdx];
                            string name = "Unknown asset";
                            if (detailProto.prototype != null)
                                name = detailProto.prototype.name;
                            else if (detailProto.prototypeTexture != null)
                                name = detailProto.prototypeTexture.name;
                            detailChoices[assetIdx] = new GUIContent(name);
                        }
                        terrainModifier.DetailProtoIndex = editorUtils.Popup("Details", terrainModifier.DetailProtoIndex, detailChoices, helpSwitch);
                        break;
                }
            }
            switch (effectType)
            {
                case EffectType.Raise:
                case EffectType.Lower:
                    terrainModifier.Strength = editorUtils.FloatField("Strength", terrainModifier.Strength, helpSwitch);
                    break;
                default:
                    terrainModifier.Strength = editorUtils.Slider("Strength", terrainModifier.Strength, 0f, 50f, helpSwitch);
                    break;
            }
            terrainModifier.InvertAlpha = editorUtils.Toggle("Invert Alpha", terrainModifier.InvertAlpha, helpSwitch);
            terrainModifier.AreaOfEffect = editorUtils.IntField("Area Of Effect", terrainModifier.AreaOfEffect, helpSwitch);
            editorUtils.Fractal(terrainModifier.NoiseFractal, helpSwitch);
            if (editorUtils.Button("Apply to Terrain"))
            {
                //if (EditorUtility.DisplayDialog("Warning",
                //    "You are about to apply the current changes to the terrain. This cannot be undone.",
                //    "Okay",
                //    "Cancel"))
                //{
                terrainModifier.ApplyToTerrain();
                //}
            }
            EditorGUI.indentLevel--;
            GUI.enabled = true;
        }
        public static void BoundsModifier(this EditorUtils editorUtils, BoundsModifier boundsModifier, bool helpSwitch)
        {
            if (boundsModifier == null)
                return;
            boundsModifier.Ignore = editorUtils.Toggle("Bounds Ignore", boundsModifier.Ignore, helpSwitch);
            GUI.enabled = !boundsModifier.Ignore;
            boundsModifier.ShapeType = (SdfShape.Type)editorUtils.EnumPopup("Shape Type", boundsModifier.ShapeType, helpSwitch);
            boundsModifier.Center = editorUtils.Vector3Field("Bounds Center", boundsModifier.Center, helpSwitch);
            switch (boundsModifier.ShapeType)
            {
                case SdfShape.Type.Box:
                    boundsModifier.Size = editorUtils.Vector3Field("Bounds Size", boundsModifier.Size, helpSwitch);
                    break;
                case SdfShape.Type.Sphere:
                    boundsModifier.Radius = editorUtils.FloatField("Bounds Radius", boundsModifier.Radius, helpSwitch);
                    break;
                case SdfShape.Type.Capsule:
                case SdfShape.Type.Cylinder:
                    boundsModifier.Radius = editorUtils.FloatField("Bounds Radius", boundsModifier.Radius, helpSwitch);
                    boundsModifier.Height = editorUtils.FloatField("Bounds Height", boundsModifier.Height, helpSwitch);
                    break;
            }
            GUI.enabled = true;
        }
        /// <summary>
        /// Options to Edit the Spawn Criteria Overrides of a prototype
        /// </summary>
        /// <param name="editorUtils"></param>
        /// <param name="crit"></param>
        /// <param name="override"></param>
        /// <param name="helpSwitch"></param>
        /// <param name="useLargeRanges"></param>
        public static SpawnCriteria SpawnCriteriaOverrides(this EditorUtils editorUtils, SpawnCriteria crit, SpawnCriteria @override, bool helpSwitch, bool useLargeRanges = false)
        {
            if (@override == null)
                @override = crit;
            crit.OverrideChildren = EditorGUILayout.Toggle("Override Children", crit.OverrideChildren);
            //crit.ForceSpawn = editorUtils.Toggle("Force Spawn", crit.ForceSpawn);
            //crit.OverrideForceSpawn = crit.ForceSpawn;
            //GUI.enabled = !crit.ForceSpawn;
            crit.OverrideVirginCheckType = BeginOverrideToggle(crit.OverrideVirginCheckType,
                () => crit.CheckCollisionType = @override.CheckCollisionType,
                () => crit.CheckCollisionType = (Constants.VirginCheckType)editorUtils.EnumPopup("Check Collisions", crit.CheckCollisionType));
            editorUtils.InlineHelp("Check Collisions", helpSwitch);
            //crit.ForceSpawn = EditorGUILayout.Toggle(crit.ForceSpawn, "Force Spawn");
            if (crit.CheckCollisionType != Constants.VirginCheckType.None) // && proto.ResourceType < Constants.ResourceType.TerrainGrass)
            {
                if (crit.CheckCollisionType == Constants.VirginCheckType.Bounds)
                {
                    EditorGUI.indentLevel += 1;
                    crit.OverrideSpawnCollisionLayers = BeginOverrideToggle(crit.OverrideSpawnCollisionLayers, () => crit.SpawnCollisionLayers = @override.SpawnCollisionLayers,
                        () => crit.SpawnCollisionLayers = editorUtils.LayerMaskField("Collision Layers", crit.SpawnCollisionLayers, helpSwitch));
                    editorUtils.InlineHelp("Collision Layers", helpSwitch);
                    // Modify Bounds
                    crit.OverrideBoundsBorder = BeginOverrideToggle(crit.OverrideBoundsBorder, () => crit.BlendAmount = @override.BlendAmount, () => crit.BlendAmount = editorUtils.Slider("Blend Amount", crit.BlendAmount, 0.001f, 10f));
                    editorUtils.InlineHelp("Blend Amount", helpSwitch);
                    crit.OverrideRayExtents = BeginOverrideToggle(crit.OverrideRayExtents, () => crit.BoundsExtents = @override.BoundsExtents, () => crit.BoundsExtents = editorUtils.Slider("Bounds Extents", crit.BoundsExtents, 0.0f, 100f));
                    editorUtils.InlineHelp("Bounds Extents", helpSwitch);
                    EditorGUI.indentLevel -= 1;
                }
            }

            // Check Height
            crit.OverrideCheckHeight = BeginOverrideToggle(crit.OverrideCheckHeight,
                () => crit.CheckHeightType = @override.CheckHeightType,
                () => crit.CheckHeightType = (Constants.CriteriaRangeType)editorUtils.EnumPopup("Check Height Type", crit.CheckHeightType));
            editorUtils.InlineHelp("Check Height Type", helpSwitch);
            if (crit.CheckHeightType != Constants.CriteriaRangeType.None)
            {
                EditorGUI.indentLevel++;
                if (crit.CheckHeightType >= Constants.CriteriaRangeType.MinMax)
                {
                    // Min Spawn Slope
                    crit.OverrideMinMaxHeight = BeginOverrideToggle(crit.OverrideMinMaxHeight,
                        () =>
                        {
                            crit.MinHeight = @override.MinHeight;
                            crit.MaxHeight = @override.MaxHeight;
                        },
                        () =>
                        {
                            float minValue = crit.MinHeight;
                            float maxValue = crit.MaxHeight;
                            float minLimit = crit.BottomBoundary;
                            float maxLimit = crit.TopBoundary;
                            bool oldState = GUI.enabled;
                            GUI.enabled = crit.OverrideMinMaxHeight && crit.CheckHeightType >= Constants.CriteriaRangeType.MinMax;
                            editorUtils.MinMaxSliderWithFields("Min Max Height", ref minValue, ref maxValue, minLimit, maxLimit);
                            GUI.enabled = oldState;
                            crit.MinHeight = minValue;
                            crit.MaxHeight = maxValue;
                        });
                    editorUtils.InlineHelp("Min Max Height", helpSwitch);
                }
                if (crit.CheckHeightType != Constants.CriteriaRangeType.MinMax)
                {
                    bool oldState = GUI.enabled;
                    GUI.enabled = false;
                    // Min Spawn Slope
                    crit.OverrideMinMaxSpawnHeight = BeginOverrideToggle(crit.OverrideMinMaxSpawnHeight,
                        () =>
                        {
                            crit.MinSpawnHeight = @override.MinSpawnHeight;
                            crit.MaxSpawnHeight = @override.MaxSpawnHeight;
                        },
                        () =>
                        {
                            float minValue = crit.MinHeight;
                            float maxValue = crit.MaxHeight;
                            float minSpawnValue = crit.MinSpawnHeight;
                            float maxSpawnValue = crit.MaxSpawnHeight;
                            float minLimit = crit.BottomBoundary;
                            float maxLimit = crit.TopBoundary;
                            Color oldColor = GUI.color;
                            if (crit.CheckHeightType == Constants.CriteriaRangeType.Mixed)
                                if (maxSpawnValue <= minValue || minSpawnValue >= maxValue)
                                    GUI.color = Color.red;
                            editorUtils.MinMaxSliderWithFields("Min Max Spawn Height", ref minSpawnValue, ref maxSpawnValue, minLimit, maxLimit);
                            GUI.color = oldColor;
                            crit.MinSpawnHeight = minSpawnValue;
                            crit.MaxSpawnHeight = maxSpawnValue;
                        });
                    editorUtils.InlineHelp("Min Max Spawn Height", helpSwitch);
                    GUI.enabled = oldState;
                }
                switch (crit.CheckHeightType)
                {
                    case Constants.CriteriaRangeType.Range:
                    case Constants.CriteriaRangeType.Mixed:
                        // Height Variance
                        crit.OverrideHeightVariance = BeginOverrideToggle(crit.OverrideHeightVariance,
                            () => crit.HeightRange = @override.HeightRange,
                            () => crit.HeightRange = editorUtils.Slider("Height Range", crit.HeightRange, 0.1f, 200f));
                        editorUtils.InlineHelp("Height Range", helpSwitch);
                        break;
                }
                EditorGUI.indentLevel--;
            }

            // Check Slope
            crit.OverrideCheckSlope = BeginOverrideToggle(crit.OverrideCheckSlope,
                () => crit.CheckSlopeType = @override.CheckSlopeType,
                () => crit.CheckSlopeType = (Constants.CriteriaRangeType)editorUtils.EnumPopup("Check Slope Type", crit.CheckSlopeType));
            editorUtils.InlineHelp("Check Slope Type", helpSwitch);
            if (crit.CheckSlopeType != Constants.CriteriaRangeType.None)
            {
                EditorGUI.indentLevel++;
                if (crit.CheckSlopeType >= Constants.CriteriaRangeType.MinMax)
                {
                    // Min Spawn Slope
                    crit.OverrideMinMaxSlope = BeginOverrideToggle(crit.OverrideMinMaxSlope,
                        () =>
                        {
                            crit.MinSlope = @override.MinSlope;
                            crit.MaxSlope = @override.MaxSlope;
                        },
                        () =>
                        {
                            float minValue = crit.MinSlope;
                            float maxValue = crit.MaxSlope;
                            float minLimit = 0f;
                            float maxLimit = 90f;
                            bool oldState = GUI.enabled;
                            GUI.enabled = crit.OverrideMinMaxSlope && crit.CheckSlopeType >= Constants.CriteriaRangeType.MinMax;
                            editorUtils.MinMaxSliderWithFields("Min Max Slope", ref minValue, ref maxValue, minLimit, maxLimit);
                            GUI.enabled = oldState;
                            crit.MinSlope = minValue;
                            crit.MaxSlope = maxValue;
                        });
                    editorUtils.InlineHelp("Min Max Slope", helpSwitch);
                }
                if (crit.CheckSlopeType != Constants.CriteriaRangeType.MinMax)
                {
                    bool oldState = GUI.enabled;
                    GUI.enabled = false;
                    // Min Spawn Slope
                    crit.OverrideMinMaxSpawnSlope = BeginOverrideToggle(crit.OverrideMinMaxSpawnSlope,
                        () =>
                        {
                            crit.MinSpawnSlope = @override.MinSpawnSlope;
                            crit.MaxSpawnSlope = @override.MaxSpawnSlope;
                        },
                        () =>
                        {
                            float minValue = crit.MinSlope;
                            float maxValue = crit.MaxSlope;
                            float minSpawnValue = crit.MinSpawnSlope;
                            float maxSpawnValue = crit.MaxSpawnSlope;
                            float minLimit = 0f;
                            float maxLimit = 90f;
                            Color oldColor = GUI.color;
                            if (crit.CheckSlopeType == Constants.CriteriaRangeType.Mixed)
                                if (maxSpawnValue <= minValue || minSpawnValue >= maxValue)
                                    GUI.color = Color.red;
                            editorUtils.MinMaxSliderWithFields("Min Max Spawn Slope", ref minSpawnValue, ref maxSpawnValue, minLimit, maxLimit);
                            GUI.color = oldColor;
                            crit.MinSpawnSlope = minSpawnValue;
                            crit.MaxSpawnSlope = maxSpawnValue;
                        });
                    editorUtils.InlineHelp("Min Max Spawn Slope", helpSwitch);
                    GUI.enabled = oldState;
                }
                switch (crit.CheckSlopeType)
                {
                    case Constants.CriteriaRangeType.Range:
                    case Constants.CriteriaRangeType.Mixed:
                        // Slope Range (Variance)
                        crit.OverrideSlopeVariance = BeginOverrideToggle(crit.OverrideSlopeVariance,
                            () => crit.SlopeRange = @override.SlopeRange,
                            () => crit.SlopeRange = editorUtils.Slider("Slope Range", crit.SlopeRange, 0.1f, 180f));
                        editorUtils.InlineHelp("Slope Range", helpSwitch);
                        break;
                }
                EditorGUI.indentLevel--;
            }
            // Check Textures
            crit.OverrideCheckTextures = BeginOverrideToggle(crit.OverrideCheckTextures,
                () => crit.CheckTextures = @override.CheckTextures,
                () => { EditorGUILayout.LabelField("Check Textures"); });
            editorUtils.InlineHelp("Check Textures", helpSwitch);
            if (crit.OverrideCheckTextures)
            {
                EditorGUI.indentLevel++;
                crit.CheckTextures = EditorGUILayout.ToggleLeft("Enabled", crit.CheckTextures);
                GUI.enabled = crit.CheckTextures;
                Terrain terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    var terrainData = terrain.terrainData;
                    var terrainLayers = terrainData.terrainLayers;
                    var alphamapLayers = terrainData.alphamapLayers;
                    GUIContent[] assetChoices = new GUIContent[alphamapLayers];
                    for (int assetIdx = 0; assetIdx < assetChoices.Length; assetIdx++)
                    {
                        assetChoices[assetIdx] = new GUIContent(terrainLayers[assetIdx].diffuseTexture.name);
                    }
                    if (assetChoices.Length == 0)
                    {
                        EditorGUILayout.LabelField("There are no Textures on Terrain");
                    }
                    else
                    {
                        int oldIdx = crit.SelectedTextureIdx;
                        crit.OverrideSelectedTextureIdx = BeginOverrideToggle(crit.OverrideSelectedTextureIdx,
                            () =>
                            {
                                crit.SelectedTextureIdx = @override.SelectedTextureIdx;
                                string name = "No Textures on Terrain";
                                if (crit.SelectedTextureIdx >= 0 && crit.SelectedTextureIdx < terrainLayers.Length)
                                    name = terrainLayers[crit.SelectedTextureIdx].diffuseTexture.name;
                                crit.SelectedTextureName = name;
                            },
                            () => { crit.SelectedTextureIdx = EditorGUILayout.Popup(crit.SelectedTextureIdx, assetChoices); });
                        editorUtils.InlineHelp("Texture Selection", helpSwitch);
                        if (crit.SelectedTextureIdx != oldIdx)
                        {
                            string name = terrainLayers[crit.SelectedTextureIdx].diffuseTexture.name;
                            crit.SelectedTextureName = name;
                        }
                    }
                }

                // Texture Strength
                crit.OverrideTextureStrength = BeginOverrideToggle(crit.OverrideTextureStrength,
                    () => crit.TextureStrength = @override.TextureStrength,
                    () => crit.TextureStrength = editorUtils.Slider("Texture Strength", crit.TextureStrength, 0.0f, 1f));
                editorUtils.InlineHelp("Texture Strength", helpSwitch);
                // Texture Range (Variance)
                crit.OverrideTextureVariance = BeginOverrideToggle(crit.OverrideTextureVariance,
                    () => crit.TextureRange = @override.TextureRange,
                    () => crit.TextureRange = editorUtils.Slider("Texture Range", crit.TextureRange, 0.0f, 1f));
                editorUtils.InlineHelp("Texture Range", helpSwitch);
                EditorGUI.indentLevel--;
                GUI.enabled = true;
            }

            // Check Mask
            crit.OverrideCheckMask = BeginOverrideToggle(crit.OverrideCheckMask,
                () => crit.CheckMask = @override.CheckMask,
                () => EditorGUILayout.LabelField("Check Mask"));
            editorUtils.InlineHelp("Check Mask", helpSwitch);
            if (crit.OverrideCheckMask)
            {
                EditorGUI.indentLevel++;
                crit.CheckMask = EditorGUILayout.ToggleLeft("Enabled", crit.CheckMask);
                GUI.enabled = crit.CheckMask;
                // Check Mask Type
                crit.OverrideCheckMaskType = BeginOverrideToggle(crit.OverrideCheckMaskType,
                    () => crit.CheckMaskType = @override.CheckMaskType,
                    () => crit.CheckMaskType = (Constants.MaskType)editorUtils.EnumPopup("Check Mask", crit.CheckMaskType));
                editorUtils.InlineHelp("Check Mask", helpSwitch);
                if (crit.CheckMaskType != Constants.MaskType.Image)
                {
                    // Mask Fractal
                    Fractal maskFractal = crit.MaskFractal;
                    Fractal overrideMaskFractal = @override.MaskFractal;
                    // Seed
                    crit.OverrideMaskFractalSeed = BeginOverrideToggle(crit.OverrideMaskFractalSeed,
                        () => maskFractal.Seed = overrideMaskFractal.Seed,
                        () => maskFractal.Seed = editorUtils.Slider("Seed", maskFractal.Seed, 0f, 65000f));
                    editorUtils.InlineHelp("Seed", helpSwitch);
                    // Octaves
                    crit.OverrideMaskFractalOctaves = BeginOverrideToggle(crit.OverrideMaskFractalOctaves,
                        () => maskFractal.Octaves = overrideMaskFractal.Octaves,
                        () => maskFractal.Octaves = editorUtils.IntSlider("Octaves", maskFractal.Octaves, 1, 12));
                    editorUtils.InlineHelp("Octaves", helpSwitch);
                    // Frequency
                    crit.OverrideMaskFractalFrequency = BeginOverrideToggle(crit.OverrideMaskFractalFrequency,
                        () => maskFractal.Frequency = overrideMaskFractal.Frequency,
                        () => maskFractal.Frequency = editorUtils.Slider("Frequency", maskFractal.Frequency, 0f, useLargeRanges ? 1f : 0.3f));
                    editorUtils.InlineHelp("Frequency", helpSwitch);
                    // Persistence
                    crit.OverrideMaskFractalPersistence = BeginOverrideToggle(crit.OverrideMaskFractalPersistence,
                        () => maskFractal.Persistence = overrideMaskFractal.Persistence,
                        () => maskFractal.Persistence = editorUtils.Slider("Persistence", maskFractal.Persistence, 0f, 1f));
                    editorUtils.InlineHelp("Persistence", helpSwitch);
                    // Lacunarity
                    crit.OverrideMaskFractalLacunarity = BeginOverrideToggle(crit.OverrideMaskFractalLacunarity,
                        () => maskFractal.Lacunarity = overrideMaskFractal.Lacunarity,
                        () => maskFractal.Lacunarity = editorUtils.Slider("Lacunarity", maskFractal.Persistence, 1.5f, 3.5f));
                    editorUtils.InlineHelp("Lacunarity", helpSwitch);
                    // Midpoint
                    crit.OverrideMidMaskFractal = BeginOverrideToggle(crit.OverrideMidMaskFractal,
                        () => crit.MidMaskFractal = @override.MidMaskFractal,
                        () => crit.MidMaskFractal = editorUtils.Slider("Midpoint", crit.MidMaskFractal, 0f, 1f));
                    editorUtils.InlineHelp("Midpoint", helpSwitch);
                    // Range
                    crit.OverrideMaskFractalRange = BeginOverrideToggle(crit.OverrideMaskFractalRange,
                        () => crit.MaskFractalRange = @override.MaskFractalRange,
                        () => crit.MaskFractalRange = editorUtils.Slider("Range", crit.MaskFractalRange, 0f, 1f));
                    editorUtils.InlineHelp("Range", helpSwitch);
                    // Invert Mask
                    crit.OverrideMaskInvert = BeginOverrideToggle(crit.OverrideMaskInvert,
                        () => crit.MaskInvert = @override.MaskInvert,
                        () => crit.MaskInvert = editorUtils.Toggle("Invert Mask", crit.MaskInvert));
                    editorUtils.InlineHelp("Invert Mask", helpSwitch);
                }
                else
                {
                    // Mask Image
                    crit.OverrideMaskImage = BeginOverrideToggle(crit.OverrideMaskImage,
                        () => crit.MaskImage = @override.MaskImage,
                        () => crit.MaskImage = (Texture2D)editorUtils.ObjectField("Image Mask", crit.MaskImage, typeof(Texture2D), false));
                    editorUtils.InlineHelp("Image Mask", helpSwitch);
                    // Image Filter Color
                    crit.OverrideImageFilterColor = BeginOverrideToggle(crit.OverrideImageFilterColor,
                        () => crit.ImageFilterColor = @override.ImageFilterColor,
                        () => crit.ImageFilterColor = editorUtils.ColorField("Selection Color", crit.ImageFilterColor));
                    editorUtils.InlineHelp("Selection Color", helpSwitch);
                    // Image Filter Fuzzy Match
                    crit.OverrideImageFilterFuzzyMatch = BeginOverrideToggle(crit.OverrideImageFilterFuzzyMatch,
                        () => crit.ImageFilterFuzzyMatch = @override.ImageFilterFuzzyMatch,
                        () => crit.ImageFilterFuzzyMatch = editorUtils.Slider("Selection Accuracy", crit.ImageFilterFuzzyMatch, 0f, 1f));
                    editorUtils.InlineHelp("Selection Accuracy", helpSwitch);
                    // Constrain Within Masked Bounds
                    crit.OverrideConstrainWithinMaskedBounds = BeginOverrideToggle(crit.OverrideConstrainWithinMaskedBounds,
                        () => crit.ConstrainWithinMaskedBounds = @override.ConstrainWithinMaskedBounds,
                        () => crit.ConstrainWithinMaskedBounds = editorUtils.Toggle("Fit Within Mask", crit.ConstrainWithinMaskedBounds));
                    editorUtils.InlineHelp("Fit Within Mask", helpSwitch);
                    // Invert Masked Alpha
                    crit.OverrideInvertMaskedAlpha = BeginOverrideToggle(crit.OverrideInvertMaskedAlpha,
                        () => crit.InvertMaskedAlpha = @override.InvertMaskedAlpha,
                        () => crit.InvertMaskedAlpha = editorUtils.Toggle("Invert Alpha", crit.InvertMaskedAlpha));
                    editorUtils.InlineHelp("Invert Alpha", helpSwitch);
                    // Success On Masked Alpha
                    crit.OverrideSuccessOnMaskedAlpha = BeginOverrideToggle(crit.OverrideSuccessOnMaskedAlpha,
                        () => crit.SuccessOnMaskedAlpha = @override.SuccessOnMaskedAlpha,
                        () => crit.SuccessOnMaskedAlpha = editorUtils.Toggle("Success By Alpha", crit.SuccessOnMaskedAlpha));
                    editorUtils.InlineHelp("Success By Alpha", helpSwitch);
                    // Scale On Masked Alpha
                    crit.OverrideScaleOnMaskedAlpha = BeginOverrideToggle(crit.OverrideScaleOnMaskedAlpha,
                        () => crit.ScaleOnMaskedAlpha = @override.ScaleOnMaskedAlpha,
                        () => crit.ScaleOnMaskedAlpha = editorUtils.Toggle("Scale By Alpha", crit.ScaleOnMaskedAlpha));
                    editorUtils.InlineHelp("Scale By Alpha", helpSwitch);
                    if (crit.ScaleOnMaskedAlpha)
                    {
                        EditorGUI.indentLevel++;
                        // Min Scale On Masked Alpha
                        crit.OverrideMinScaleOnMaskedAlpha = BeginOverrideToggle(crit.OverrideMinScaleOnMaskedAlpha,
                            () => crit.MinScaleOnMaskedAlpha = @override.MinScaleOnMaskedAlpha,
                            () => crit.MinScaleOnMaskedAlpha = editorUtils.Slider("Mask Alpha Min Scale", crit.MinScaleOnMaskedAlpha, 0f, 10f));
                        editorUtils.InlineHelp("Mask Alpha Min Scale", helpSwitch);
                        // Max Scale On Masked Alpha
                        crit.OverrideMaxScaleOnMaskedAlpha = BeginOverrideToggle(crit.OverrideMaxScaleOnMaskedAlpha,
                            () => crit.MaxScaleOnMaskedAlpha = @override.MaxScaleOnMaskedAlpha,
                            () => crit.MaxScaleOnMaskedAlpha = editorUtils.Slider("Mask Alpha Max Scale", crit.MaxScaleOnMaskedAlpha, 0f, 10f));
                        editorUtils.InlineHelp("Mask Alpha Max Scale", helpSwitch);
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }
            GUI.enabled = true;
            return crit;
        }
        public static void SpawnFlags(this EditorUtils editorUtils, SpawnFlags spawnFlags, bool canUseColliders, bool helpEnabled)
        {
            GUIStyle spawnFlagsPanel = new GUIStyle(GUI.skin.window)
            {
                normal = { textColor = GUI.skin.label.normal.textColor },
                alignment = TextAnchor.UpperCenter,
                margin = new RectOffset(0, 0, 5, 7),
                padding = new RectOffset(10, 10, 3, 3),
                stretchWidth = true,
                stretchHeight = false
            };
            //resFlagsPanel.fontStyle = FontStyle.Bold;
            // Need a bunch of weird stuff here
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth -= 5f + 10f * indent;
            float fieldsWidth = EditorGUIUtility.labelWidth;
            float indentWidth = 12f * indent;
            GUILayoutOption fieldOption = GUILayout.Width(fieldsWidth + 15f);
            spawnFlags.ApplyToChildren = editorUtils.Toggle("ApplyToChildren", spawnFlags.ApplyToChildren, helpEnabled);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(indentWidth + 12f);
                GUILayout.BeginVertical(spawnFlagsPanel, fieldOption);
                {
                    spawnFlags.FlagBatchingStatic = editorUtils.Toggle("Static Batching", spawnFlags.FlagBatchingStatic, helpEnabled, fieldOption);
                    spawnFlags.FlagLightmapStatic = editorUtils.Toggle("Static Lightmap", spawnFlags.FlagLightmapStatic, helpEnabled, fieldOption);
                    spawnFlags.FlagOccludeeStatic = editorUtils.Toggle("Static Occludee", spawnFlags.FlagOccludeeStatic, helpEnabled, fieldOption);
                    spawnFlags.FlagOccluderStatic = editorUtils.Toggle("Static Occluder", spawnFlags.FlagOccluderStatic, helpEnabled, fieldOption);
#if !UNITY_2022_2_OR_NEWER
                    spawnFlags.FlagNavigationStatic = editorUtils.Toggle("Static Navigation", spawnFlags.FlagNavigationStatic, helpEnabled, fieldOption);
                    //spawnFlags.FlagOffMeshLinkGeneration = editorUtils.Toggle("Offmesh Link Gen", spawnFlags.FlagOffMeshLinkGeneration, helpEnabled, fieldOption);
#endif
                }
                GUILayout.EndVertical();
                GUILayout.Space(20f);
                GUILayout.BeginVertical(spawnFlagsPanel, fieldOption);
                {
                    spawnFlags.FlagReflectionProbeStatic = editorUtils.Toggle("Static Ref Probe", spawnFlags.FlagReflectionProbeStatic, helpEnabled, fieldOption);
                    //spawnFlags.FlagMovingObject = editorUtils.Toggle("Moving Object", spawnFlags.FlagMovingObject, helpEnabled, fieldOption);
                    spawnFlags.FlagIsOutdoorObject = editorUtils.Toggle("Outdoor Object", spawnFlags.FlagIsOutdoorObject, helpEnabled, fieldOption);
                    spawnFlags.FlagForceOptimise = editorUtils.Toggle("Force Optimise", spawnFlags.FlagForceOptimise, helpEnabled, fieldOption);
                    spawnFlags.FlagCanBeOptimised = editorUtils.Toggle("Can Optimise", spawnFlags.FlagCanBeOptimised, helpEnabled, fieldOption);
                    EditorGUI.BeginDisabledGroup(!canUseColliders);
                    spawnFlags.UseColliderBounds = editorUtils.Toggle("Use Collider Bounds", spawnFlags.UseColliderBounds, helpEnabled, fieldOption);
                    EditorGUI.EndDisabledGroup();
                    if (spawnFlags.FlagForceOptimise)
                        spawnFlags.FlagCanBeOptimised = true;
                }
                GUILayout.EndVertical();
                GUILayout.Space(-40f);
            }
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
        }
        #endregion
    }
}