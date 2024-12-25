#if GAIA_2_PRESENT || GAIA_2023
using System;
using System.Collections.Generic;
using Gaia;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spawner extension to use a gena spawner in a gaia spawner.
    /// Add this script to a spawner prefab and assign the spawner component to the Gena Spawner property.
    /// </summary>
    public class GeNaToGaiaSpawnerExtension : MonoBehaviour, ISpawnExtension
    {
        public string Name => "GeNa Spawner";
        public bool AffectsHeights => true;
        public bool AffectsTextures => false;
        public GeNa.Core.Spawner m_genaSpawner;
        [NonSerialized] private List<UndoRecord> m_undoRecord = new List<UndoRecord>();
        public void Delete(Transform target = null)
        {
            Delete();
        }
        public void Close()
        {
            ///Not required
        }
        public void Delete()
        {
            if (m_genaSpawner != null)
            {
                if (m_undoRecord != null)
                {
                    //
                    for (int i = m_undoRecord.Count - 1; i >= 0; i--)
                    {
                        if (m_undoRecord[i] == null)
                        {
                            continue;
                        }
                        m_undoRecord[i].Undo();
                    }
                }
            }
            m_undoRecord.Clear();
        }
        public void Init(Gaia.Spawner spawner)
        {
            ///Not required
        }
        public void Spawn(Gaia.Spawner spawner, Transform target, int ruleIndex, int instanceIndex, SpawnExtensionInfo spawnInfo)
        {
            if (m_genaSpawner != null)
            {
                m_genaSpawner.SpawnerData.PlacementCriteria.MinRotationY = spawnInfo.m_rotation.eulerAngles.y;
                m_genaSpawner.SpawnerData.PlacementCriteria.MaxRotationY = spawnInfo.m_rotation.eulerAngles.y;
                m_genaSpawner.SpawnerData.PlacementCriteria.MinScale = spawnInfo.m_scale;
                m_genaSpawner.SpawnerData.PlacementCriteria.MaxScale = spawnInfo.m_scale;
                m_genaSpawner.Spawn(spawnInfo.m_position);
                m_undoRecord.Add(m_genaSpawner.SpawnerData.UndoRecord);
            }
        }
    }
}
#endif