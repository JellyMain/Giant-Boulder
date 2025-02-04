using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for modifying Bounds of Object when Spawning
    /// </summary>
    public class GeNaBoundsDecorator : GeNaDecorator
    {
        [SerializeField] protected BoundsModifier m_boundsModifier = new BoundsModifier();
        public BoundsModifier BoundsModifier => m_boundsModifier;
        public override void OnIngest(Resource resource)
        {
            resource.AddColliderToAabb = false;
        }
        public override BoundsModifier GetBoundsModifier()
        {
            return m_boundsModifier;
        }
    }
}