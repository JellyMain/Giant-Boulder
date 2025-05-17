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
        


        public GameplayQuestTracker(QuestsService questsService,
            LevelCreationWatcher levelCreationWatcher, DestroyedObjectsTracker destroyedObjectsTracker,
            GameCurrencyTracker gameCurrencyTracker)
        {
            this.questsService = questsService;
            this.levelCreationWatcher = levelCreationWatcher;
            this.destroyedObjectsTracker = destroyedObjectsTracker;
            this.gameCurrencyTracker = gameCurrencyTracker;

            this.levelCreationWatcher.OnLevelCreated += TrackQuests;
        }


        private void TrackQuests()
        {
            foreach (KeyValuePair<QuestData, QuestProgressUpdater> questPair in questsService.ActiveQuestsProgressUpdaters)
            {
                QuestData questData = questPair.Key;
                QuestProgressUpdater questProgressUpdater = questPair.Value;

                string questId = questData.questId;
                
                // TODO: Track only uncompleted quests
                
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
