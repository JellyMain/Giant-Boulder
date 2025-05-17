using System;
using Quests.Enums;
using Sirenix.OdinInspector;
using Structures;
using UnityEngine;


namespace Quests
{
    public class QuestData : ScriptableObject
    {
        public string questId;
        public string questTitle;
        public string questDescription;
        public QuestType questType;
        public StructureObject reward;

        [AssetList(Path = "Prefabs/UI/Menu/RewardsUIObject")]
        public GameObject rewardUIObject;

        public QuestPersistenceProgressType questPersistenceProgressType;
        
        

        [Button]
        private void SetNewId()
        {
            if (string.IsNullOrEmpty(questId))
            {
                questId = Guid.NewGuid().ToString();
            }
        }
    }
}
