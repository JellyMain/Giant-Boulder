using UnityEngine;


namespace Quests
{
    [CreateAssetMenu(menuName = "Quests/CollectCoinsQuest", fileName = "CollectCoinsQuest")]
    public class CollectCoinsQuestData : QuestData
    {
        public int targetCoinsAmount;
    }
}