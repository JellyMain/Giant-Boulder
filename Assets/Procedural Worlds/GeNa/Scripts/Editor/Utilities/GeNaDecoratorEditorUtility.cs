using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    public static class GeNaDecoratorEditorUtility
    {
        public static void CreateDecorator<T>(MenuCommand menuCommand) where T : MonoBehaviour
        {
            GameObject gameObject = menuCommand.context as GameObject;
            if (gameObject == null)
                gameObject = new GameObject(typeof(T).Name);
            if (gameObject != null)
            {
                T decorator = gameObject.AddComponent<T>();
                RegisterDecorator(gameObject, decorator);
            }
        }
        public static void RegisterDecorator(GameObject context, Object decorator)
        {
            Undo.RegisterCreatedObjectUndo(decorator, "Added Decorator Component");
            Selection.activeObject = context;
        }
    }
}