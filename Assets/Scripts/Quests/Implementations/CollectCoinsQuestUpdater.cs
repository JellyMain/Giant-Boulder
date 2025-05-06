using System;
using System.Collections.Generic;
using DataTrackers;
using Progress;
using Quests.Enums;
using UnityEngine;


namespace Quests.Implementations
{
    public class CollectCoinsQuestUpdater : QuestProgressUpdater, IDisposable, IProgressSaver, IProgressUpdater
    {
        private readonly CollectCoinsQuestData collectCoinsQuestData;
        private GameCurrencyTracker gameCurrencyTracker;
        private int collectedCoins;


        public CollectCoinsQuestUpdater(CollectCoinsQuestData collectCoinsQuestData, SaveLoadService saveLoadService) :
            base(saveLoadService)
        {
            this.collectCoinsQuestData = collectCoinsQuestData;
        }


        public void Dispose()
        {
            gameCurrencyTracker.OnCoinAdded -= OnCoinAdded;
        }


        public override void StartTracking(QuestDependencies questDependencies)
        {
            base.StartTracking(questDependencies);
            gameCurrencyTracker = questDependencies.gameCurrencyTracker;
            gameCurrencyTracker.OnCoinAdded += OnCoinAdded;
        }


        private void OnCoinAdded(int _, Vector3 __)
        {
            collectedCoins++;
            UpdateQuest();
        }

        
        public override void UpdateQuest()
        {
            if (collectedCoins == collectCoinsQuestData.targetCoinsAmount)
            {
                isCompleted = true;
                QuestCompleted(collectCoinsQuestData);
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            int questId = collectCoinsQuestData.questId;

            QuestProgress updatedProgress = new QuestProgress()
            {
                collectedCoins = collectedCoins,
                questState = isCompleted ? QuestState.JustCompleted : QuestState.InProgress
            };

            progressDictionary[questId] = updatedProgress;
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            if (collectCoinsQuestData.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
            {
                UpdateMultipleSessionProgress(progressDictionary);
            }
            else if (collectCoinsQuestData.questPersistenceProgressType == QuestPersistenceProgressType.OneSession)
            {
                UpdateSingleSessionProgress();
            }
            else
            {
                Debug.LogError("Quest persistent progress type is None");
            }
        }



        private void UpdateMultipleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            int questId = collectCoinsQuestData.questId;

            QuestProgress existingProgress = progressDictionary.GetValueOrDefault(questId);

            collectedCoins = existingProgress?.collectedCoins ?? 0;
        }


        private void UpdateSingleSessionProgress()
        {
            collectedCoins = 0;
        }
    }
}
