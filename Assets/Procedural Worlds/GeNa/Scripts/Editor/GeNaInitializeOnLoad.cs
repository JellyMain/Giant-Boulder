using System.Collections.Generic;
using GeNa.Core;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class GeNaInitializeOnLoad
{
    private static Constants.RenderPipeline previousRenderPipeline;
    private static bool inAnimationMode = false;
    private static List<GeNaAnimatorDecorator> m_animators = new List<GeNaAnimatorDecorator>();
    private static List<GeNaParticleDecorator> m_particles = new List<GeNaParticleDecorator>();

    static GeNaInitializeOnLoad()
    {
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.quitting -= OnEditorQuit;
        EditorApplication.quitting += OnEditorQuit;
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReloads;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReloads;
        AssemblyReloadEvents.afterAssemblyReload -= OnBeforeAssemblyReloads;
        AssemblyReloadEvents.afterAssemblyReload += OnBeforeAssemblyReloads;
        GeNaEvents.onDecoratorSpawned -= OnDecoratorSpawned;
        GeNaEvents.onDecoratorSpawned += OnDecoratorSpawned;
        InAnimationMode(false);
        previousRenderPipeline = GeNaUtility.GetActivePipeline();
        GeNaManagerEditor.LoadPreferences();
        ProcessScriptDefine();
        GeNaRuntimeEvents.Initialize();
    }

    private static void Update()
    {
        RunAnimations();
        RunParticles();
    }

    #region Scripting Defines

    private static void ProcessScriptDefine()
    {
        //Make sure we inject GAIA_2_PRESENT
        bool updateScripting = false;
        var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

#if UNITY_2023_1_OR_NEWER
        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
        var symbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
        var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
#endif
        
        if (symbols.Contains("GENA_PRO"))
        {
            updateScripting = true;
            symbols = symbols.Replace("GENA_PRO", "GENA_PRO");
        }
        if (!symbols.Contains("GENA_PRO"))
        {
            updateScripting = true;
            symbols += ";" + "GENA_PRO";
        }
        if (updateScripting)
        {
#if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
#endif
        }
    }

    #endregion

    #region Events

    private static void OnEditorQuit()
    {
        GeNaFactory.Dispose();
    }

    private static void OnBeforeAssemblyReloads()
    {
        GeNaFactory.Dispose();
    }

    private static void OnAfterAssemblyReloads()
    {
        GeNaUtility.UpdateSplinesRenderPipeline(previousRenderPipeline, out previousRenderPipeline);
    }

    private static void OnDecoratorSpawned(IDecorator decorator)
    {
        switch (decorator)
        {
            case GeNaAnimatorDecorator animatorDecorator:
                m_animators.Add(animatorDecorator);
                break;
            case GeNaParticleDecorator particleDecorator:
                m_particles.Add(particleDecorator);
                break;
        }
    }

    #endregion

    #region Editor Animations + Particles

    public static void InAnimationMode(bool value)
    {
        if (value == inAnimationMode)
            return;
        inAnimationMode = value;
        if (inAnimationMode)
            AnimationMode.StartAnimationMode();
        else
            AnimationMode.StopAnimationMode();
    }

    private static void RunAnimations()
    {
        bool decoratorsEmpty = m_animators.Count == 0;
        if (decoratorsEmpty)
            return;
        bool repaint = false;
        bool isPlaying = false;
        List<GeNaAnimatorDecorator> animatorsToDelete = new List<GeNaAnimatorDecorator>();
        foreach (GeNaAnimatorDecorator decorator in m_animators)
        {
            if (decorator == null)
                continue;
            if (!decorator.isActiveAndEnabled)
                continue;
            Animator[] animators = decorator.Animators;
            if (animators == null)
                continue;
            bool finished = true;
            foreach (Animator animator in animators)
            {
                if (!animator.enabled)
                    continue;
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                float normalizedTime = stateInfo.normalizedTime;
                // var loop = stateInfo.loop;
                if (normalizedTime >= 1f)
                {
                    decorator.enabled = false;
                    continue;
                }

                finished = false;
                repaint = true;
                isPlaying = true;
                animator.Update(EditorTime.deltaTime);
            }

            if (finished)
                animatorsToDelete.Add(decorator);
        }

        m_animators.RemoveAll(animator => animator == null || animatorsToDelete.Contains(animator));
        if (repaint)
            SceneView.RepaintAll();
        InAnimationMode(isPlaying);
    }

    private static void RunParticles()
    {
        bool decoratorsEmpty = m_particles.Count == 0;
        if (decoratorsEmpty)
            return;
        bool repaint = false;
        List<GeNaParticleDecorator> particlesToDelete = new List<GeNaParticleDecorator>();
        foreach (GeNaParticleDecorator decorator in m_particles)
        {
            if (decorator == null)
                continue;
            if (!decorator.enabled)
                continue;
            ParticleSystem[] particles = decorator.Particles;
            if (particles == null)
                continue;
            bool finished = true;
            foreach (ParticleSystem particle in particles)
            {
                ParticleSystem.MainModule main = particle.main;
                decorator.Time += EditorTime.deltaTime;
                if (decorator.Time >= main.duration)
                {
                    decorator.enabled = false;
                    continue;
                }

                finished = false;
                repaint = true;
                particle.Stop();
                particle.Clear();
                particle.randomSeed = 5;
                particle.Simulate(decorator.Time, true, true);
            }

            if (finished)
                particlesToDelete.Add(decorator);
        }

        m_particles.RemoveAll(particle => particle == null || particlesToDelete.Contains(particle));
        if (repaint)
            SceneView.RepaintAll();
    }

    #endregion
}