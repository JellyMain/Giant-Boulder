using System;
using System.Collections;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for performing Sub-Spawner operations
    /// </summary>
    [ExecuteAlways]
    public class GeNaSubSpawnerDecorator : GeNaDecorator
    {
        [SerializeField] protected int m_maxSubSpawnerDepth = 5;
        [SerializeField] protected GeNaSpawner m_subSpawner;
        [SerializeField] protected Color m_subSpawnerColor = Color.blue;
        public int MaxSubSpawnerDepth
        {
            get => m_maxSubSpawnerDepth;
            set => m_maxSubSpawnerDepth = value;
        }
        public GeNaSpawner SubSpawner
        {
            get => m_subSpawner;
            set => m_subSpawner = value;
        }
        public GeNaSpawnerData SubSpawnerData => m_subSpawner != null ? m_subSpawner.SpawnerData : SubSpawnerData;
        public Color SubSpawnerColor
        {
            get => m_subSpawnerColor;
            set => m_subSpawnerColor = value;
        }
        public override void OnIngest(Resource resource)
        {
            resource.HasSubSpawner = true;
            GeNaSpawner subSpawner = SubSpawner;
            if (subSpawner != null)
            {
                Palette palette = resource.Palette;
                resource.AddSubSpawner(subSpawner.gameObject, palette);
                GeNaSpawnerData subSpawnerData = resource.SubSpawnerData;
                if (subSpawnerData != null)
                {
                    SpawnerSettings settings = resource.SubSpawnerData.Settings;
                    settings.MaxSubSpawnerDepth = MaxSubSpawnerDepth;
                    subSpawnerData.RotationY = transform.eulerAngles.y;
                }
            }
        }
        public override IEnumerator OnSelfSpawned(Resource resource)
        {
            var subSpawnerData = resource.SubSpawnerData;
            if (subSpawnerData != null)
            {
                subSpawnerData.RotationY = transform.eulerAngles.y;
            }
            yield return null;
        }
    }
}