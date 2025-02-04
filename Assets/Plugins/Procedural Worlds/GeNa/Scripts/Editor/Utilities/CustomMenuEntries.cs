using UnityEngine;
using UnityEditor;
namespace GeNa.Core
{
    public static class CustomMenuEntries
    {
        public static string PathToTemplateFolder => $"{Application.dataPath}/Procedural Worlds/GeNa/Scripts/Templates";
        [MenuItem("Assets/Create/Procedural Worlds/GeNa/Templates/Decorator")]
        public static void CreateDecoratorTemplate()
        {
            string decoratorTemplate = $"{PathToTemplateFolder}/GeNaDecoratorTemplate.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(decoratorTemplate, "NewCustomDecorator.cs");
        }
        
        [MenuItem("Assets/Create/Procedural Worlds/GeNa/Templates/Spline Extension")]
        public static void CreateSplineExtensionTemplate()
        {
            string spawnerExtensionTemplate = $"{PathToTemplateFolder}/GeNaSplineExtensionTemplate.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(spawnerExtensionTemplate, "NewCustomSplineExtension.cs");
        }
        
        [MenuItem("Assets/Create/Procedural Worlds/GeNa/Templates/Builder")]
        public static void CreateBuilderTemplate()
        {
            string spawnerExtensionTemplate = $"{PathToTemplateFolder}/GeNaBuilderTemplate.cs.txt";
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(spawnerExtensionTemplate, "NewCustomBuilder.cs");
        }
    }
}