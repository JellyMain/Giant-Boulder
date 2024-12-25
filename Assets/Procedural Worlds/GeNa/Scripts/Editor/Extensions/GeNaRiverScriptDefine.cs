using UnityEditor;
using UnityEditor.Build;

namespace GeNa.Core
{
    [InitializeOnLoad]
    public class GeNaRiverScriptDefine
    {
        static GeNaRiverScriptDefine()
        {
            SetupGeNaPipelineDefine();
        }

        public static void SetupGeNaPipelineDefine()
        {
            bool updateDefines = false;

            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

#if UNITY_2023_1_OR_NEWER
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            var symbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif

            switch (GeNaUtility.GetActivePipeline())
            {
                case Constants.RenderPipeline.BuiltIn:
                    if (symbols.Contains("GeNa_URP"))
                    {
                        updateDefines = true;
                        symbols = symbols.Replace("GeNa_URP", "");
                    }

                    if (symbols.Contains("GeNa_HDRP"))
                    {
                        updateDefines = true;
                        symbols = symbols.Replace("GeNa_HDRP", "");
                    }

                    break;
                case Constants.RenderPipeline.Universal:
                    if (!symbols.Contains("GeNa_URP"))
                    {
                        updateDefines = true;
                        symbols += ";GeNa_URP";
                    }

                    if (symbols.Contains("GeNa_HDRP"))
                    {
                        updateDefines = true;
                        symbols = symbols.Replace("GeNa_HDRP", "");
                    }

                    break;
                case Constants.RenderPipeline.HighDefinition:
                    if (symbols.Contains("GeNa_URP"))
                    {
                        updateDefines = true;
                        symbols = symbols.Replace("GeNa_URP", "");
                    }

                    if (!symbols.Contains("GeNa_HDRP"))
                    {
                        updateDefines = true;
                        symbols += ";GeNa_HDRP";
                    }

                    break;
            }

            if (updateDefines)
            {
#if UNITY_2023_1_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols);
#else
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
#endif
            }
        }
    }
}