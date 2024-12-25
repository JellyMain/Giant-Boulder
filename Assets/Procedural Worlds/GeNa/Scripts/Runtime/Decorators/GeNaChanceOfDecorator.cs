using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for introducing the element of chance into Spawning
    /// </summary>
    public class GeNaChanceOfDecorator : GeNaDecorator
    {
        [SerializeField] protected float m_successRate = 1.0f;
        public float SuccessRate
        {
            get => m_successRate;
            set => m_successRate = Mathf.Clamp01(value);
        }
        public override void OnIngest(Resource resource)
        {
            Prototype prototype = resource.Prototype;
            resource.SetStatic(prototype, Constants.ResourceStatic.Dynamic);
            resource.SuccessRate = m_successRate;
        }
    }
}