using UnityEditor;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaPrefabUnpackerDecorator))]
    public class GeNaPrefabUnpackerDecoratorEditor : GeNaDecoratorEditor<GeNaPrefabUnpackerDecorator>
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            m_hideUnpackPrefabOption = true;
        }
        [MenuItem("GameObject/GeNa/Decorators/Prefab Unpacker Decorator")]
        public static void AddDecorator(MenuCommand command) => GeNaDecoratorEditorUtility.CreateDecorator<GeNaPrefabUnpackerDecorator>(command);
        protected override void RenderSettingsPanel()
        {
            // Don't Render
        }
    }
}