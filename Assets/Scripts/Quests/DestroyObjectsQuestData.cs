using Structures;
using UnityEngine;


namespace Quests
{
    [CreateAssetMenu(menuName = "Quests/DestroyObjectsQuest", fileName = "DestroyObjectsQuest")]
    public class DestroyObjectsQuestData : QuestData
    {
        public ObjectType targetObjectType;
        public int targetObjectAmount;
    }
}
