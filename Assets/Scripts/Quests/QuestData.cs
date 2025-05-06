using Quests.Enums;
using Sirenix.OdinInspector;
using Structures;
using UnityEngine;


namespace Quests
{
    public class QuestData : ScriptableObject
    {
        public int questId;
        public string questTitle;
        public string questDescription;
        public StructureObject reward;

        [AssetList(Path = "Prefabs/UI/Menu/RewardsUIObject")]
        public GameObject rewardUIObject;

        public QuestPersistenceProgressType questPersistenceProgressType;


        private void OnEnable()
        {
            questId = Mathf.Abs(name.GetHashCode());
        }
    }
}
