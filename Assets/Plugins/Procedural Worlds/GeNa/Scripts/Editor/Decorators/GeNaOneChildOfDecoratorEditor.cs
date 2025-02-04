using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaOneChildOfDecorator))]
    public class GeNaOneChildOfDecoratorEditor : GeNaDecoratorEditor<GeNaOneChildOfDecorator>
    {
        public override bool HideInSpawner => true;
        [MenuItem("GameObject/GeNa/Decorators/Child Of Decorator")]
        public static void AddDecorator(MenuCommand command) => GeNaDecoratorEditorUtility.CreateDecorator<GeNaOneChildOfDecorator>(command);
        protected override void OnEnable()
        {
            base.OnEnable(); 
            if (Decorator != null)
                Decorator.RefreshChildren();
        }
        protected override void SettingsPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                List<GeNaOneChildOfDecorator.Child> children = Decorator.Children;
                if (children.Count > 0)
                {
                    EditorUtils.LabelField("Weights", helpEnabled);
                    EditorGUI.indentLevel++;
                    foreach (GeNaOneChildOfDecorator.Child child in children)
                        child.weight = EditorGUILayout.Slider(child.transform.name, child.weight, 0.0f, 1.0f);
                    EditorGUI.indentLevel--;
                }
                else
                    GUILayout.Label("There are no Children attached to this GameObject.");
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(Decorator);
            }
        }
    }
}