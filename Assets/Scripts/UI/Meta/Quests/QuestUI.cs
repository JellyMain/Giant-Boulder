using System;
using System.Collections.Generic;
using Coffee.UIEffects;
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
        [SerializeField] private UIEffect progressBarEffect;
        [SerializeField] private Button claimButton;
        private PersistentPlayerProgress persistentPlayerProgress;
        public QuestData QuestData { get; private set; }
        public event Action<QuestUI> OnQuestClaimed;
        private bool isCompleted;


        [Inject]
        private void Construct(PersistentPlayerProgress persistentPlayerProgress)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
        }


        public void InitQuestData(QuestData questData)
        {
            QuestData = questData;
            titleText.text = questData.questTitle;
            descriptionText.text = questData.questDescription;
            Instantiate(questData.rewardUIObject, rewardObjectParent);
            claimButton.onClick.AddListener(ClaimReward);

            UpdateQuestProgress(questData);
        }



        private void ClaimReward()
        {
            if (isCompleted)
            {
                OnQuestClaimed?.Invoke(this);
            }
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
            string questId = destroyObjectsQuest.questId;

            QuestProgress questProgress =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary
                    .GetValueOrDefault(questId);

            int destroyedObjects = questProgress?.destroyedObjects ?? 0;

            currentAmount.text = destroyedObjects.ToString();
            targetAmount.text = destroyObjectsQuest.targetObjectAmount.ToString();

            float normalizedProgress = (float)destroyedObjects / destroyObjectsQuest.targetObjectAmount;
            progressFill.fillAmount = normalizedProgress;

            IsQuestCompleted(questProgress);
        }


        private void UpdateCoinsQuest(CollectCoinsQuestData collectCoinsQuest)
        {
            string questId = collectCoinsQuest.questId;

            QuestProgress questProgress =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary[questId];

            int collectedAmount = questProgress?.collectedCoins ?? 0;

            currentAmount.text = collectedAmount.ToString();
            targetAmount.text = collectCoinsQuest.targetCoinsAmount.ToString();

            float normalizedProgress = (float)collectedAmount / collectCoinsQuest.targetCoinsAmount;
            progressFill.fillAmount = normalizedProgress;

            IsQuestCompleted(questProgress);
        }



        private void IsQuestCompleted(QuestProgress questProgress)
        {
            if (questProgress?.questState == QuestState.JustCompleted)
            {
                progressBarEffect.enabled = true;
                isCompleted = true;
                Debug.Log("Play effects");
            }
            else if (questProgress?.questState == QuestState.Completed)
            {
                isCompleted = true;
                Debug.Log("completed");
            }
        }
    }
}
