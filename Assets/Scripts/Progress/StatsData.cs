using System;


namespace Progress
{
    [Serializable]
    public class StatsData
    {
        public DestroyedObjectsCountDictionary destroyedObjectsCount = new DestroyedObjectsCountDictionary();
        public int coinsCollected;
    }
}
