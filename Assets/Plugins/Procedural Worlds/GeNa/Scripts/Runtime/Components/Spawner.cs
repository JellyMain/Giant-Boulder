using UnityEngine;
namespace GeNa.Core
{
    public class Spawner : GeNaSpawner
    {
        public override GeNaSpawnerData SpawnerData
        {
            get
            {
                if (m_spawnerData == null)
                {
                    m_spawnerData = ScriptableObject.CreateInstance<SpawnerData>();
                    m_spawnerData.name = "Spawner Data (Temp)";
                }
                return m_spawnerData;
            }
        }
    }
}