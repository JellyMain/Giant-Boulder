using System;
using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Spline Extension for performing Terrain Carve operations (note: only works on Terrains)
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Carve", menuName = "Procedural Worlds/GeNa/Extensions/Carve", order = 0)]
    public class GeNaCarveExtension : GeNaSplineExtension
    {
        #region Constants
        /// <summary>
        /// Maximum length of Spline before Preview defaults to off (on selection)
        /// </summary>
        private const float MAX_PREVIEW_LENGTH = 5000.0f;
        #endregion
        #region Variables
        // Compute Shader
        [SerializeField] protected float m_width = 5f;
        [SerializeField] protected float m_heightOffset = 0.0f;
        [SerializeField] protected float m_shoulder = 10.0f;
        [SerializeField] protected bool m_roadLike = false;
        [SerializeField] protected bool m_showPreview = true;
        [SerializeField] protected Fractal m_maskFractal = new Fractal
        {
            Enabled = true,
            NoiseFalloff = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)),
            Strength = 1f,
            Seed = 1,
            Octaves = 8,
            Frequency = 1f,
            Persistence = .5f,
            Lacunarity = 2,
            Amplitude = 1
        };
        [SerializeField] protected AnimationCurve m_shoulderFalloff = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));
        [SerializeField] protected TerrainModifier m_terrainModifier = new TerrainModifier();
        [NonSerialized] protected TerrainTools m_terrainTools;
        [NonSerialized] protected TerrainEntity m_terrainEntity;
        #endregion
        #region Properties
        public bool ShowPreview
        {
            get => m_showPreview;
            set => m_showPreview = value;
        }
        public float Width
        {
            get => m_width;
            set => m_width = value;
        }
        public float HeightOffset
        {
            get => m_heightOffset;
            set => m_heightOffset = value;
        }
        // Smoothness
        public float Shoulder
        {
            get => m_shoulder;
            set => m_shoulder = value;
        }
        public bool RoadLike
        {
            get => m_roadLike;
            set => m_roadLike = value;
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
        #region Methods
        private TerrainTools GetTerrainTools()
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
        protected override void OnSceneGUI()
        {
            Visualize();
        }
        protected void UpdateTerrainModifier()
        {
            m_terrainModifier.EffectType = EffectType.Flatten;
        }
        public TerrainEntity GetTerrainEntity()
        {
            TerrainTools tools = GetTerrainTools();
            if (tools == null)
                return null;
            tools.Width = Width;
            tools.Shoulder = Shoulder;
            tools.RoadLike = RoadLike;
            tools.HeightOffset = HeightOffset;
            tools.MaskFractal = MaskFractal;
            tools.ShoulderFalloff = ShoulderFalloff;
            UpdateTerrainModifier();
            return tools.GenerateTerrainEntity(m_terrainModifier, Spline);
        }
        private void Modify(bool recordUndo = true)
        {
            TerrainEntity terrainEntity = GetTerrainEntity();
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
        public void Visualize()
        {
            if (!m_isSelected || !m_showPreview)
                return;
            TerrainTools tools = GetTerrainTools();
            if (tools == null)
                return;
            tools.Width = Width;
            tools.Shoulder = Shoulder;
            tools.RoadLike = RoadLike;
            tools.HeightOffset = HeightOffset;
            tools.MaskFractal = MaskFractal;
            tools.ShoulderFalloff = ShoulderFalloff;
            UpdateTerrainModifier();
            TerrainEntity terrainEntity = tools.GenerateTerrainEntity(m_terrainModifier, Spline);
            if (terrainEntity != null)
            {
                tools.Visualize(terrainEntity);
                terrainEntity.Dispose();
            }
        }
        public void Carve()
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
            // Show the preview if the Spline length is below the maximum preview length
            m_showPreview = Spline.Length <= MAX_PREVIEW_LENGTH;
        }
        protected override void OnDeselect()
        {
            m_isSelected = false;
        }
        #endregion
    }
}