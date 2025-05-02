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
            switch (questData.questType)
            {
                case QuestType.CollectCoins:
                {
                    UpdateCoinsQuest(questData);
                    break;
                }
                case QuestType.DestroyObjects:
                {
                    UpdateObjectsQuest(questData);
                    break;
                }
                case QuestType.None:
                    Debug.LogError("Quest has type None");
                    break;
            }
        }


        private void UpdateObjectsQuest(QuestData questData)
        {
            int destroyedAmount = 0;

            if (persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary.TryGetValue(
                    questData.uniqueId, out QuestProgress questProgress))
            {
                destroyedAmount = questProgress.destroyedAmount;
            }

            currentAmount.text = destroyedAmount.ToString();
            targetAmount.text = questData.targetObjectAmount.ToString();

            float normalizedProgress = (float)destroyedAmount / questData.targetObjectAmount;
            progressFill.fillAmount = normalizedProgress;
        }


        private void UpdateCoinsQuest(QuestData questData)
        {
            int collectedAmount = 0;

            if (persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary.TryGetValue(
                    questData.uniqueId, out QuestProgress questProgress))
            {
                collectedAmount = questProgress.collectedCoins;
            }

            currentAmount.text = collectedAmount.ToString();
            targetAmount.text = questData.targetCoinsAmount.ToString();
            
            float normalizedProgress = (float)collectedAmount / questData.targetCoinsAmount;
            progressFill.fillAmount = normalizedProgress;
        }
    }
}
