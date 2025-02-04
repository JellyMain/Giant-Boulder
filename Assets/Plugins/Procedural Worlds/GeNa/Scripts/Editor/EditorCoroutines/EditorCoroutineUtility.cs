using System.Collections;
namespace GeNa.Core
{
    public static class EditorCoroutineUtility
    {
        public static EditorCoroutine StartCoroutine(IEnumerator routine, object owner) => new EditorCoroutine(routine, owner);
    }
}