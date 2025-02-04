using System;
using UnityEngine;
namespace GeNa.Core
{
    [ExecuteAlways]
    public class GeNaParticleDecorator : GeNaDecorator
    {
        #region Variables
        [SerializeField] protected bool m_updateChildren = true;
        [SerializeField] protected float m_time = 0f;
        [NonSerialized] protected ParticleSystem[] m_particles;
        #endregion
        #region Properties
        public bool UpdateChildren
        {
            get => m_updateChildren;
            set => m_updateChildren = value;
        }
        public float Time
        {
            get => m_time;
            set => m_time = value;
        }
        public ParticleSystem[] Particles => m_particles;
        #endregion
        #region Methods
        private void OnEnable()
        {
            m_time = 0f;
            if (m_updateChildren)
                m_particles = GetComponentsInChildren<ParticleSystem>();
            else
                m_particles = GetComponents<ParticleSystem>();
        }
        #endregion
    }
}