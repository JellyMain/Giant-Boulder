using System;
using System.Collections.Generic;
using DataTrackers;
using Progress;
using Quests.Enums;
using UnityEngine;


namespace Quests.Implementations
{
    public class CollectCoinsQuestUpdater : QuestProgressUpdater, IDisposable
    {
        private readonly CollectCoinsQuestData collectCoinsQuestData;
        private readonly QuestsService questsService;
        private GameCurrencyTracker gameCurrencyTracker;
        private int collectedCoins;


        public CollectCoinsQuestUpdater(CollectCoinsQuestData collectCoinsQuestData, QuestsService questsService)
        {
            this.collectCoinsQuestData = collectCoinsQuestData;
            this.questsService = questsService;
        }


        public void Dispose()
        {
            gameCurrencyTracker.OnCoinAdded -= OnCoinAdded;
        }


        public override void StartTracking(QuestDependencies questDependencies)
        {
            gameCurrencyTracker = questDependencies.gameCurrencyTracker;
            gameCurrencyTracker.OnCoinAdded += OnCoinAdded;

            UpdateProgress();
        }


        private void OnCoinAdded(int _, Vector3 __)
        {
            collectedCoins++;
            UpdateQuest();
        }


        public override void UpdateQuest()
        {
            questsService.AllQuestsProgresses[collectCoinsQuestData].collectedCoins = collectedCoins;

            if (collectedCoins == collectCoinsQuestData.targetCoinsAmount)
            {
                questsService.AllQuestsProgresses[collectCoinsQuestData].questState = QuestState.JustCompleted;
                QuestCompleted(collectCoinsQuestData);
            }
        }


        private void UpdateProgress()
        {
            if (collectCoinsQuestData.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
            {
                UpdateMultipleSessionProgress();
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



        private void UpdateMultipleSessionProgress()
        {
            QuestProgress progress = questsService.AllQuestsProgresses[collectCoinsQuestData];
            collectedCoins = progress.collectedCoins;
        }


        private void UpdateSingleSessionProgress()
        {
            collectedCoins = 0;
        }
    }
}
