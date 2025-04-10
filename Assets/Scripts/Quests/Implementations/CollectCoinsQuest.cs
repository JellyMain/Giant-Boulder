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
        private readonly GameCurrencyTracker gameCurrencyTracker;
        private int collectedCoins;


        public CollectCoinsQuestUpdater(Quest quest, SaveLoadService saveLoadService,
            GameCurrencyTracker gameCurrencyTracker) : base(quest, saveLoadService)
        {
            this.gameCurrencyTracker = gameCurrencyTracker;
        }


        public override void Init()
        {
            base.Init();
            gameCurrencyTracker.OnCoinAdded += OnCoinAdded;
        }


        public void Dispose()
        {
            gameCurrencyTracker.OnCoinAdded -= OnCoinAdded;
        }


        private void OnCoinAdded(int _, Vector3 __)
        {
            collectedCoins++;
            UpdateProgress();
        }


        public override void UpdateProgress()
        {
            if (collectedCoins == quest.targetCoinsAmount)
            {
                isCompleted = true;
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            if (quest.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
            {
                SaveMultipleSessionProgress(progressDictionary);
            }
            else
            {
                SaveSingleSessionProgress(progressDictionary);
            }
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            if (quest.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
            {
                UpdateMultipleSessionProgress(progressDictionary);
            }
            else
            {
                UpdateSingleSessionProgress();
            }
        }



        private void SaveMultipleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (progressDictionary.TryGetValue(quest.uniqueId, out QuestProgress questProgress))
            {
                questProgress.collectedCoins += collectedCoins;
            }
            else
            {
                progressDictionary[quest.uniqueId] =
                    new QuestProgress
                    {
                        questState = QuestState.InProgress,
                        collectedCoins = collectedCoins
                    };
            }
        }


        private void SaveSingleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (isCompleted)
            {
                progressDictionary[quest.uniqueId] = new QuestProgress
                {
                    questState = QuestState.Completed,
                    collectedCoins = quest.targetCoinsAmount
                };
            }
            else
            {
                if (progressDictionary.TryGetValue(quest.uniqueId, out QuestProgress questProgress))
                {
                    questProgress.collectedCoins = collectedCoins;
                }
                else
                {
                    progressDictionary[quest.uniqueId] = new QuestProgress
                    {
                        questState = QuestState.InProgress,
                        collectedCoins = collectedCoins
                    };
                }
            }
        }



        private void UpdateMultipleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (progressDictionary.TryGetValue(quest.uniqueId, out QuestProgress questProgress))
            {
                collectedCoins = questProgress.collectedCoins;
            }
            else
            {
                collectedCoins = 0;
            }
        }


        private void UpdateSingleSessionProgress()
        {
            collectedCoins = 0;
        }
    }
}
