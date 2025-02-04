using System;
using System.Collections.Generic;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for Clearing Terrain Trees along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Clear Trees", menuName = "Procedural Worlds/GeNa/Extensions/Clear Trees", order = 0)]
    public class GeNaClearTreesExtension : GeNaSplineExtension
    {
        #region Variables
        // Compute Shader
        [SerializeField] protected float m_width = 5f;
        // Smoothness
        [SerializeField] protected float m_shoulder = 1.5f;
        // Noise
        [SerializeField] protected Fractal m_maskFractal = new Fractal();
        [SerializeField] protected AnimationCurve m_shoulderFalloff = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));
        // Non Serialized
        [NonSerialized] protected TerrainTools m_terrainTools;
        [NonSerialized] private bool m_isDirty = false;
        [NonSerialized] protected Stack<TerrainEntity> m_undoStack = new Stack<TerrainEntity>();
        [SerializeField] protected TerrainModifier m_terrainModifier = new TerrainModifier();
        #endregion
        #region Properties
        public float Width
        {
            get => m_width;
            set => m_width = Mathf.Max(0f, value);
        }
        public float Shoulder
        {
            get => m_shoulder;
            set => m_shoulder = Mathf.Max(0f, value);
        }
        public Fractal MaskFractal
        {
            get => m_maskFractal;
            set => m_maskFractal = value;
        }
        public AnimationCurve ShoulderFalloff
        {
            get => m_shoulderFalloff;
            set => m_shoulderFalloff = value;
        }
        #endregion
        protected override void OnSceneGUI()
        {
            Visualize();
        }

        protected void UpdateTerrainModifier()
        {
            m_terrainModifier.EffectType = EffectType.ClearTrees;
        }
        public TerrainTools GetTerrainTools()
        {
            if (m_terrainTools == null)
            {
                GeNaManager geNaManager = GeNaManager.GetInstance();
                if (geNaManager != null)
                {
                    m_terrainTools = geNaManager.TerrainTools;
                }
            }
            return m_terrainTools;
        }
        private TerrainEntity m_terrainEntity;
        public void Visualize()
        {
            if (!m_isSelected)
                return;
            TerrainTools tools = GetTerrainTools();
            tools.Width = Width;
            tools.Shoulder = Shoulder;
            tools.HeightOffset = 0f;
            tools.MaskFractal = MaskFractal;
            tools.ShoulderFalloff = ShoulderFalloff;
            UpdateTerrainModifier();
            if (m_isDirty)
            {
                m_terrainEntity = tools.GenerateTerrainEntity(m_terrainModifier, Spline);
                m_isDirty = false;
            }
            if (m_terrainEntity != null)
            {
                tools.Visualize(m_terrainEntity);
            }
        }
        private void Modify(bool recordUndo = true)
        {
            TerrainTools tools = GetTerrainTools();
            tools.Width = Width;
            tools.Shoulder = Shoulder;
            tools.HeightOffset = 0f;
            tools.MaskFractal = MaskFractal;
            tools.ShoulderFalloff = ShoulderFalloff;
            UpdateTerrainModifier();
            TerrainEntity terrainEntity = tools.GenerateTerrainEntity(m_terrainModifier, Spline);
            if (terrainEntity != null)
            {
                if (recordUndo)
                {
                    terrainEntity.name = "Clear Trees";
                    GeNaUndoRedo.RecordUndo(terrainEntity);
                }
                terrainEntity.Perform();
            }
        }
        public void Clear()
        {
            Modify();
        }
        protected override GameObject OnBake(GeNaSpline spline)
        {
            Modify(false);
            return null;
        }
        public override void Execute()
        {
            if (!m_isSelected)
                return;
            Visualize();
        }
        protected override void OnSelect()
        {
            Visualize();
            m_isSelected = true;
            OnSplineDirty();
        }
        protected override void OnDeselect()
        {
            m_isSelected = false;
        }
        protected override void OnDelete()
        {
        }
        protected override void OnSplineDirty()
        {
            m_isDirty = true;
        }
    }
}