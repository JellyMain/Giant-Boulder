using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaSpawnCriteriaDecorator))]
    public class GeNaSpawnCriteriaDecoratorEditor : GeNaDecoratorEditor<GeNaSpawnCriteriaDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Spawn Criteria Decorator")]
        public static void AddDecorator(MenuCommand command) => GeNaDecoratorEditorUtility.CreateDecorator<GeNaSpawnCriteriaDecorator>(command);
        protected override void SettingsPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                Decorator.SpawnCriteria = EditorUtils.SpawnCriteriaOverrides(Decorator.SpawnCriteria, Decorator.SpawnCriteria, helpEnabled);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object @object in targets) 
                {
                    if (@object is GeNaSpawnCriteriaDecorator decorator)
                        decorator.SpawnCriteria.Copy(Decorator.SpawnCriteria);

                    EditorUtility.SetDirty(@object);
                }
            }
        }
    }
}