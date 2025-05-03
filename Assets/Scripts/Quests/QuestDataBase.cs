using Quests.Enums;
using Sirenix.OdinInspector;
using Structures;
using UnityEngine;


namespace Quests
{
    public class QuestDataBase : ScriptableObject
    {
        public int uniqueId;
        public string questTitle;
        public string questDescription;
        public StructureObject reward;

        [AssetList(Path = "Prefabs/UI/Menu/RewardsUIObject")]
        public GameObject rewardUIObject;

        public QuestPersistenceProgressType questPersistenceProgressType;


        private void OnEnable()
        {
            uniqueId = Mathf.Abs(name.GetHashCode());
        }
    }
}
