using UnityEditor;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaParticleDecorator))]
    public class GeNaParticleDecoratorEditor : GeNaDecoratorEditor<GeNaParticleDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Particle Decorator")]
        public static void AddDecorator(MenuCommand command) => GeNaDecoratorEditorUtility.CreateDecorator<GeNaParticleDecorator>(command);
        /// <summary>
        /// See if this is an origami structure with a valid builder
        /// </summary>
        protected override void SettingsPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                Decorator.UpdateChildren = EditorUtils.Toggle("Update Children", Decorator.UpdateChildren);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Object o in targets)
                {
                    GeNaParticleDecorator animatorDecorator = (GeNaParticleDecorator) o;
                    animatorDecorator.UpdateChildren = Decorator.UpdateChildren;
                    EditorUtility.SetDirty(animatorDecorator);
                }
            }
        }
    }
}