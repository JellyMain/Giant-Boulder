using UnityEditor;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor((typeof(GeNaGaiaDecorator)))]
    public class GeNaGaiaDecoratorEditor : GeNaDecoratorEditor<GeNaGaiaDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Gaia Decorator")]
        public static void AddDecorator(MenuCommand command) => GeNaDecoratorEditorUtility.CreateDecorator<GeNaGaiaDecorator>(command);
        protected override void SettingsPanel(bool helpEnabled)
        {
#if GAIA_2_PRESENT
            EditorGUI.BeginChangeCheck();
            Decorator.Enabled = EditorUtils.Toggle("Enabled", Decorator.Enabled, helpEnabled);
            if (Decorator.Enabled)
            {
                EditorGUI.indentLevel++;
                Decorator.GetSeaLevel = EditorUtils.Toggle("Get Sea Level", Decorator.GetSeaLevel, helpEnabled);
                if (Decorator.GetSeaLevel)
                {
                    EditorGUI.indentLevel++;
                    Decorator.ExtraSeaLevel = EditorUtils.FloatField("ExtraSeaLevel", Decorator.ExtraSeaLevel, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (UnityEngine.Object o in targets)
                {
                    GeNaGaiaDecorator gaiaDecorator = (GeNaGaiaDecorator) o;
                    gaiaDecorator.Enabled = Decorator.Enabled;
                    gaiaDecorator.GetSeaLevel = Decorator.GetSeaLevel;
                    gaiaDecorator.ExtraSeaLevel = Decorator.ExtraSeaLevel;
                    EditorUtility.SetDirty(gaiaDecorator);
                }
            }
#else
            EditorGUILayout.HelpBox("WARNING! Gaia not present in project.", MessageType.Warning);
#endif
        }
    }
}