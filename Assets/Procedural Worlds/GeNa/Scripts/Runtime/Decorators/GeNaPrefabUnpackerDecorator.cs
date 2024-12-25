namespace GeNa.Core
{
    /// <summary>
    /// Decorator for unpacking prefab into Spawner
    /// </summary>
    public class GeNaPrefabUnpackerDecorator : GeNaDecorator
    {
        public override bool UnpackPrefab => true;
    }
}