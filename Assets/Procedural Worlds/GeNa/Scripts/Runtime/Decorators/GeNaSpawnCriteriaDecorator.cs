using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for modifying Spawn Criteria overrides
    /// </summary>
    public class GeNaSpawnCriteriaDecorator : GeNaDecorator
    {
        [SerializeField] protected SpawnCriteria m_spawnCriteria = new SpawnCriteria();
        public SpawnCriteria SpawnCriteria
        {
            get => m_spawnCriteria;
            set => m_spawnCriteria = value;
        }
        public override void OnIngest(Resource resource)
        {
            resource.SpawnCriteria = new SpawnCriteria();
            resource.SpawnCriteria.Copy(SpawnCriteria);
        }
    }
}