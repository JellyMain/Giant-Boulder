using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace GeNa.Core
{
    public static class EditorExtension
    {
        #region SerializedProperty
        /// <summary>
        /// Gets all children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.Next(false);
            }
            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;
                    yield return currentProperty;
                } while (currentProperty.Next(false));
            }
        }
        /// <summary>
        /// Gets visible children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }
            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;
                    yield return currentProperty;
                } while (currentProperty.NextVisible(false));
            }
        }
        #endregion
        #region List<Transform>
        public static List<Transform> FilterByPrefabInstanceRoots(this List<Transform> tree)
        {
            return tree.Where(item => PrefabUtility.IsAnyPrefabInstanceRoot(item.gameObject)).ToList();
        }
        public static List<Transform> FilterByMissingUnpackDecorator(this List<Transform> tree)
        {
            return tree.Where(item =>
            {
                GeNaDecorator[] decorator = item.GetComponents<GeNaDecorator>();
                if (decorator != null && decorator.Length > 0)
                {
                    if (decorator.Any(transform => transform.UnpackPrefab))
                        return false;
                }
                return true;
            }).ToList();
        }
        public static bool IsValidUnpackerChain(this List<Transform> tree)
        {
            foreach (Transform transform in tree)
            {
                GeNaDecorator[] decorator = transform.GetComponents<GeNaDecorator>();
                if (decorator != null && decorator.Length > 0)
                {
                    if (decorator.Any(item => item.UnpackPrefab))
                        continue;
                    return false;
                }
                else
                    return false;
            }
            return true;
        }
        #endregion
    }
}