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


        public void SetQuestData(QuestData questData)
        {
            titleText.text = questData.questTitle;
            descriptionText.text = questData.questDescription;
            Instantiate(questData.rewardUIObject, rewardObjectParent);

            UpdateQuestProgress(questData);
        }


        private void UpdateQuestProgress(QuestData questData)
        {
            switch (questData)
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
                    Debug.LogWarning($"Unhandled quest type: {questData.GetType()}");
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
