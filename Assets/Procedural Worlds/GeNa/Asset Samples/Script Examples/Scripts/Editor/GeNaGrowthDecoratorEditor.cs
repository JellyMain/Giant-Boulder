using GeNa.Core;
using UnityEditor;

// Custom Editor Attribute for GeNaGrowthDecorator
[CustomEditor(typeof(GeNaGrowthDecorator))]
public class GeNaGrowthDecoratorEditor : GeNaDecoratorEditor<GeNaGrowthDecorator> // Inherits from Custom Editor that Supports GeNa Pro
{
    protected override void SettingsPanel(bool helpEnabled)
    {
        EditorGUI.BeginChangeCheck();
        {
            Decorator.Speed = EditorUtils.Slider("GrowthSpeed", Decorator.Speed, 1f, 10f, helpEnabled);
            Decorator.StartScale = EditorUtils.Vector3Field("GrowthStartScale", Decorator.StartScale, helpEnabled);
            Decorator.ScaleToGameObject = EditorUtils.Toggle("GrowthScaleToGameObject", Decorator.ScaleToGameObject,helpEnabled);
            if (!Decorator.ScaleToGameObject)
            {
                Decorator.EndScale = EditorUtils.Vector3Field("GrowthEndScale", Decorator.EndScale, helpEnabled);
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(Decorator);
        }
    }
}