using System;
using Progress;
using Quests;
using Quests.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace UI.Meta.Quests
{
    public class QuestUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text targetAmount;
        [SerializeField] private TMP_Text currentAmount;
        [SerializeField] private Transform rewardObjectParent;
        [SerializeField] private Image progressFill;
        private PersistentPlayerProgress persistentPlayerProgress;



        [Inject]
        private void Construct(PersistentPlayerProgress persistentPlayerProgress)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
        }


        public void SetQuestData(QuestDataBase questDataBase)
        {
            titleText.text = questDataBase.questTitle;
            descriptionText.text = questDataBase.questDescription;
            Instantiate(questDataBase.rewardUIObject, rewardObjectParent);

            UpdateQuestProgress(questDataBase);
        }


        private void UpdateQuestProgress(QuestDataBase questDataBase)
        {
            switch (questDataBase)
            {
                case CollectCoinsQuestData collectCoinsQuestData:
                {
                    UpdateCoinsQuest(collectCoinsQuestData);
                    break;
                }
                case DestroyObjectsQuestData destroyObjectsQuestData:
                {
                    UpdateObjectsQuest(destroyObjectsQuestData);
                    break;
                }

                default:
                {
                    Debug.LogWarning($"Unhandled quest type: {questDataBase.GetType()}");
                    break;
                }
                    
            }
        }


        private void UpdateObjectsQuest(DestroyObjectsQuestData destroyObjectsQuest)
        {
            int destroyedAmount = 0;

            if (persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary.TryGetValue(
                    destroyObjectsQuest.uniqueId, out QuestProgress questProgress))
            {
                destroyedAmount = questProgress.destroyedAmount;
            }

            currentAmount.text = destroyedAmount.ToString();
            targetAmount.text = destroyObjectsQuest.targetObjectAmount.ToString();

            float normalizedProgress = (float)destroyedAmount / destroyObjectsQuest.targetObjectAmount;
            progressFill.fillAmount = normalizedProgress;
        }


        private void UpdateCoinsQuest(CollectCoinsQuestData collectCoinsQuest)
        {
            int collectedAmount = 0;

            if (persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary.TryGetValue(
                    collectCoinsQuest.uniqueId, out QuestProgress questProgress))
            {
                collectedAmount = questProgress.collectedCoins;
            }

            currentAmount.text = collectedAmount.ToString();
            targetAmount.text = collectCoinsQuest.targetCoinsAmount.ToString();

            float normalizedProgress = (float)collectedAmount / collectCoinsQuest.targetCoinsAmount;
            progressFill.fillAmount = normalizedProgress;
        }
    }
}
