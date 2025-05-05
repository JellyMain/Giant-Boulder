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
        private PersistentPlayerProgress persistentPlayerProgress;
        public QuestData QuestData { get; private set; }


        [Inject]
        private void Construct(PersistentPlayerProgress persistentPlayerProgress)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
        }


        public void SetQuestData(QuestData questData)
        {
            QuestData = questData;
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
            int questId = destroyObjectsQuest.uniqueId;

            QuestProgress questProgress =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary
                    .GetValueOrDefault(questId);

            int destroyedObjects = questProgress?.destroyedObjects ?? 0;

            currentAmount.text = destroyedObjects.ToString();
            targetAmount.text = destroyObjectsQuest.targetObjectAmount.ToString();

            float normalizedProgress = (float)destroyedObjects / destroyObjectsQuest.targetObjectAmount;
            progressFill.fillAmount = normalizedProgress;
        }




        private void UpdateCoinsQuest(CollectCoinsQuestData collectCoinsQuest)
        {
            int questId = collectCoinsQuest.uniqueId;

            QuestProgress questProgress =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary[questId];

            int collectedAmount = questProgress?.collectedCoins ?? 0;

            currentAmount.text = collectedAmount.ToString();
            targetAmount.text = collectCoinsQuest.targetCoinsAmount.ToString();

            float normalizedProgress = (float)collectedAmount / collectCoinsQuest.targetCoinsAmount;
            progressFill.fillAmount = normalizedProgress;
        }
    }
}
