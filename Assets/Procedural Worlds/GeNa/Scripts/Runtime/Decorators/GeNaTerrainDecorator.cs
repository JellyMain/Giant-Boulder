using System;
using UnityEngine;
using System.Collections;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for modifying Terrain
    /// </summary>
    [ExecuteAlways]
    public class GeNaTerrainDecorator : GeNaDecorator
    {
        [SerializeField] protected TerrainModifier m_terrainModifier = new TerrainModifier();
        [NonSerialized] private bool m_isSelected = false;
        public TerrainModifier TerrainModifier => m_terrainModifier;
        public bool IsSelected
        {
            get => m_isSelected;
            set => m_isSelected = value;
        }
        private void UpdateTerrainModifier()
        {
            m_terrainModifier.Position = transform.position;
            m_terrainModifier.RotationY = transform.eulerAngles.y;
        }
        public void Update()
        {
            if (!m_isSelected)
                return;
            UpdateTerrainModifier();
            m_terrainModifier.UpdateTerrain = false;
        }
        public override void OnIngest(Resource resource)
        {
            resource.HasHeights = true;
            Palette palette = resource.Palette;
            m_terrainModifier.AddBrushTextures(m_terrainModifier.BrushTextures, palette);
        }
        public override IEnumerator OnSelfSpawned(Resource resource)
        {
            if (TerrainModifier != null && TerrainModifier.Enabled)
            {
                UpdateTerrainModifier();
                TerrainModifier.UpdateTerrain = true;
                TerrainEntity terrainEntity = TerrainModifier.GenerateTerrainEntity();
                GeNaSpawnerData spawnerData = resource.SpawnerData;
                UndoRecord undoRecord = spawnerData.UndoRecord;
                if (terrainEntity != null)
                {
                    // Append the Undo Record (if there is one)
                    undoRecord.Record(terrainEntity);
                    terrainEntity.Perform();
                }
            }
            yield break;
        }
        public override void LoadReferences(Palette palette)
        {          
            m_terrainModifier.BrushTextures.Clear();
            foreach (int id in m_terrainModifier.BrushTextureIDs)
            {
                Texture2D texture2D = palette.GetObject<Texture2D>(id);
                if (texture2D != null)
                    m_terrainModifier.BrushTextures.Add(texture2D);
            }
        }
    }
}