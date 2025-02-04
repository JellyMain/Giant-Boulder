using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for modifying Spawn Flags of Prefab Prototype
    /// </summary>
    public class GeNaSpawnFlagsDecorator : GeNaDecorator
    {
        [SerializeField] protected SpawnFlags m_spawnFlags = new SpawnFlags();
        public SpawnFlags SpawnFlags => m_spawnFlags;
    }
}