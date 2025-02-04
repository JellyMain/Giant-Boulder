using UnityEngine;
namespace GeNa.Core
{
    [ExecuteAlways]
    [RequireComponent(typeof(Terrain))]
    public class GeNaTerrainEvents : MonoBehaviour
    {
        private Terrain m_terrain;
        public Terrain Terrain
        {
            get
            {
                if (m_terrain == null)
                    m_terrain = GetComponent<Terrain>();
                return m_terrain;
            }
        }
        private void OnTerrainChanged(TerrainChangedFlags flags)
        {
            GeNaEvents.onTerrainChanged?.Invoke(Terrain, flags);
        }
    }
}