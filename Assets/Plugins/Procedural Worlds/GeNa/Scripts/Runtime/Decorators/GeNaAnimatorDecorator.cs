using System;
using UnityEngine;
namespace GeNa.Core
{
    [ExecuteAlways]
    public class GeNaAnimatorDecorator : GeNaDecorator
    {
        #region Variables
        [SerializeField] protected bool m_updateChildren = true;
        [NonSerialized] protected Animator[] m_animators;
        #endregion
        #region Properties
        public bool UpdateChildren
        {
            get => m_updateChildren;
            set => m_updateChildren = value;
        }
        public Animator[] Animators => m_animators;
        #endregion
        #region Methods
        private void OnEnable()
        {
            if (m_updateChildren)
                m_animators = GetComponentsInChildren<Animator>();
            else
                m_animators = GetComponents<Animator>();
        }
        private void Reset()
        {
            DestroyAfterSpawn = false;
        }
        #endregion
    }
}