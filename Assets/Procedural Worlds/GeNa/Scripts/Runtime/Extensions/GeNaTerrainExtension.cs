using System;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for Clearing Terrain Trees along a Spline
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Terrain", menuName = "Procedural Worlds/GeNa/Extensions/Terrain", order = 0)]
    public class GeNaTerrainExtension : GeNaSplineExtension
    {
        #region Variables
        // Compute Shader
        [SerializeField] protected float m_width = 5f;
        [SerializeField] protected float m_heightOffset = 0f;
        [SerializeField] protected float m_noiseStrength = 1.0f;
        [SerializeField] protected bool m_roadLike = false;
        // Smoothness
        [SerializeField] protected float m_shoulder = 1.5f;
        // Noise
        [SerializeField] protected Fractal m_maskFractal = new Fractal();
        [SerializeField] protected AnimationCurve m_shoulderFalloff = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));
        [SerializeField] protected TerrainModifier m_terrainModifier = new TerrainModifier();

        // Non Serialized
        [NonSerialized] private bool m_isDirty = false;
        [NonSerialized] private TerrainEntity m_terrainEntity;
        #endregion
        #region Properties
        public EffectType EffectType
        {
            get => m_terrainModifier.EffectType;
            set => m_terrainModifier.EffectType = value;
        }
        public float Strength
        {
            get => m_terrainModifier.Strength;
            set => m_terrainModifier.Strength = value;
        }
        public int TextureProtoIndex
        {
            get => m_terrainModifier.TextureProtoIndex;
            set => m_terrainModifier.TextureProtoIndex = value;
        }
        public int DetailProtoIndex
        {
            get => m_terrainModifier.DetailProtoIndex;
            set => m_terrainModifier.DetailProtoIndex = value;
        }
        public float Width
        {
            get => m_width;
            set => m_width = Mathf.Max(0f, value);
        }
        public float HeightOffset
        {
            get => m_heightOffset;
            set => m_heightOffset = value;
        }
        public bool RoadLike
        {
            get => m_roadLike;
            set => m_roadLike = value;
        }
        // Noise
        public float NoiseStrength
        {
            get => m_noiseStrength;
            set => m_noiseStrength = value;
        }
        // Smoothness
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
        public TerrainTools GetTerrainTools()
        {
            GeNaManager geNaManager = GeNaManager.GetInstance();
            if (geNaManager == null)
                return null;
            var tools = geNaManager.TerrainTools;
            tools.RoadLike = RoadLike;
            tools.Width = Width;
            tools.Shoulder = Shoulder;
            tools.Strength = Strength;
            tools.HeightOffset = HeightOffset;
            tools.ShoulderFalloff = ShoulderFalloff;
            tools.MaskFractal = MaskFractal;
            return tools;
        }
        public void Visualize()
        {
            if (!m_isSelected)
                return;
            TerrainTools tools = GetTerrainTools();
            if (m_terrainModifier != null && m_terrainModifier.EffectType != EffectType)
            {
                m_terrainModifier.EffectType = this.EffectType;
                this.m_isDirty = true;
            }
            if (m_isDirty)
            {
                if (m_terrainEntity != null)
                {
                    m_terrainEntity.Dispose();
                }
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
            TerrainEntity terrainEntity = tools.GenerateTerrainEntity(m_terrainModifier, Spline);
            if (terrainEntity != null)
            {
                if (recordUndo)
                {
                    terrainEntity.name = EffectType.ToString();
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