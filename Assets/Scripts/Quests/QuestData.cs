using Quests.Enums;
using Sirenix.OdinInspector;
using Structures;
using UnityEngine;


namespace Quests
{
    [CreateAssetMenu(menuName = "Quests/Create Quest", fileName = "NewQuest")]
    public class QuestData : ScriptableObject
    {
        public int uniqueId;
        public string questTitle;
        public string questDescription;
        public StructureObject reward;
        public GameObject rewardUIObject;
       [ValidateInput(nameof(ValidateQuestType), "Quest type can't be null")] public QuestType questType;
        public QuestPersistenceProgressType questPersistenceProgressType;

        [Title("Coins Quest"), ShowIf("@questType == QuestType.CollectCoins")]
        public int targetCoinsAmount;
        
        [Title("Objects Quest"), ShowIf("@questType == QuestType.DestroyObjects")]
        public ObjectType targetObjectType;
        [ShowIf("@questType == QuestType.DestroyObjects")]
        public int targetObjectAmount;

        private void OnEnable()
        {
            uniqueId = Mathf.Abs(name.GetHashCode());
        }


        private bool ValidateQuestType()
        {
            return questType != QuestType.None;
        }
    }
}