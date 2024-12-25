using UnityEditor;
using Object = UnityEngine.Object;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaAnimatorDecorator))]
    public class GeNaAnimatorDecoratorEditor : GeNaDecoratorEditor<GeNaAnimatorDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Animator Decorator")]
        public static void AddDecorator(MenuCommand command) => GeNaDecoratorEditorUtility.CreateDecorator<GeNaAnimatorDecorator>(command);
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
                    GeNaAnimatorDecorator animatorDecorator = (GeNaAnimatorDecorator) o;
                    animatorDecorator.UpdateChildren = Decorator.UpdateChildren;
                    EditorUtility.SetDirty(animatorDecorator);
                }
            }
        }
    }
}