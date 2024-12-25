using UnityEngine;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for converting GameObject to Terrain Tree
    /// </summary>
    public class GeNaTerrainTreeDecorator : GeNaDecorator
    {
        [SerializeField] protected bool m_enabled = true;
        public bool Enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }
        public override void OnIngest(Resource resource)
        {
            if (Enabled)
            {
                TreePrototype treePrototype = GeNaSpawnerInternal.GetTreePrototype(resource.Prefab);
                int treePrototypeIndex = GeNaSpawnerInternal.GetTreePrototypeIndex(treePrototype);
                if (treePrototypeIndex >= 0)
                {
                    resource.ResourceType = Constants.ResourceType.TerrainTree;
                    resource.TerrainProtoIdx = treePrototypeIndex;
                }
            }
        }
    }
}