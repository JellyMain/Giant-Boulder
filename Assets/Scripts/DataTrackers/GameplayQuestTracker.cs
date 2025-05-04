using System;
using System.Collections.Generic;
using Factories;
using GameLoop;
using Quests;
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
            foreach (QuestProgressUpdater questProgressUpdater in questsService.SelectedQuests.Values)
            {
                questProgressUpdater.StartTracking(new QuestDependencies()
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
