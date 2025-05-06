using System;
using System.Collections.Generic;
using Factories;
using GameLoop;
using Progress;
using Quests;
using Quests.Enums;
using UnityEngine;
using Zenject;


namespace DataTrackers
{
    public class GameplayQuestTracker : IDisposable
    {
        private readonly QuestsService questsService;
        private readonly LevelCreationWatcher levelCreationWatcher;
        private readonly DestroyedObjectsTracker destroyedObjectsTracker;
        private readonly GameCurrencyTracker gameCurrencyTracker;
        private readonly PersistentPlayerProgress persistentPlayerProgress;


        public GameplayQuestTracker(QuestsService questsService,
            LevelCreationWatcher levelCreationWatcher, DestroyedObjectsTracker destroyedObjectsTracker,
            GameCurrencyTracker gameCurrencyTracker, PersistentPlayerProgress persistentPlayerProgress)
        {
            this.questsService = questsService;
            this.levelCreationWatcher = levelCreationWatcher;
            this.destroyedObjectsTracker = destroyedObjectsTracker;
            this.gameCurrencyTracker = gameCurrencyTracker;
            this.persistentPlayerProgress = persistentPlayerProgress;

            this.levelCreationWatcher.OnLevelCreated += TrackQuests;
        }


        private void TrackQuests()
        {
            QuestsIdProgressDictionary questsProgressDictionary =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary;

            List<KeyValuePair<QuestData, QuestProgressUpdater>> questsPairs =
                new List<KeyValuePair<QuestData, QuestProgressUpdater>>(questsService.SelectedQuests);

            foreach (KeyValuePair<QuestData, QuestProgressUpdater> questPair in questsPairs)
            {
                QuestData questData = questPair.Key;
                QuestProgressUpdater questProgressUpdater = questPair.Value;
                int questId = questData.questId;

                QuestProgress questProgress = questsProgressDictionary.GetValueOrDefault(questId);


                if (questProgress?.questState == QuestState.JustCompleted)
                {
                    QuestData newQuest = questsService.ReplaceQuest(questData);
                    questProgressUpdater = questsService.SelectedQuests[newQuest];
                }

                questProgressUpdater.StartTracking(new QuestDependencies
                {
                    destroyedObjectsTracker = destroyedObjectsTracker,
                    gameCurrencyTracker = gameCurrencyTracker
                });
            }
        }


        public void Dispose()
        {
            levelCreationWatcher.OnLevelCreated -= TrackQuests;
        }
    }
}
