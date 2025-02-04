using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for getting the Sea Level from Gaia (requires Gaia 2 / Gaia Pro)
    /// </summary>
    public class GeNaGaiaDecorator : GeNaDecorator
    {
        [SerializeField] protected bool m_enabled = true;
        [SerializeField] protected bool m_getSeaLevel = true;
        [SerializeField] protected float m_extraSeaLevel = 0f;
        public bool Enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }
        public bool GetSeaLevel
        {
            get => m_getSeaLevel;
            set => m_getSeaLevel = value;
        }
        public float ExtraSeaLevel
        {
            get => m_extraSeaLevel;
            set => m_extraSeaLevel = value;
        }
        public override void OnIngest(Resource resource)
        {
            if (Enabled)
            {
                Prototype prototype = resource.Prototype;
                GeNaSpawnerData spawnerData = prototype.SpawnerData;
                spawnerData.SetSeaLevel(GetSeaLevel, ExtraSeaLevel);
                if (GetSeaLevel)
                    GeNaEvents.SetSeaLevel(spawnerData);
            }
        }
    }
}