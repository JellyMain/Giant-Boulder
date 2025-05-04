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
        private readonly QuestService questService;
        private readonly QuestProgressUpdaterFactory questProgressUpdaterFactory;
        private readonly LevelCreationWatcher levelCreationWatcher;
        public Dictionary<QuestProgressUpdater, QuestDataBase > TrackedQuests { get; private set; }
        

        public GameplayQuestTracker(QuestService questService, QuestProgressUpdaterFactory questProgressUpdaterFactory,
            LevelCreationWatcher levelCreationWatcher)
        {
            this.questService = questService;
            this.questProgressUpdaterFactory = questProgressUpdaterFactory;
            this.levelCreationWatcher = levelCreationWatcher;

            this.levelCreationWatcher.OnLevelCreated += TrackQuests;
        }


        private void TrackQuests()
        {
            TrackedQuests = new Dictionary<QuestProgressUpdater, QuestDataBase>();
            
            foreach (QuestDataBase quest in questService.SelectedQuests)
            {
                
                QuestProgressUpdater questProgressUpdater =
                    questProgressUpdaterFactory.CreateQuestProgressUpdaterByQuest(quest);

                TrackedQuests[questProgressUpdater] = quest;
                
                questProgressUpdater.Init();
            }
        }


        public void Dispose()
        {
            levelCreationWatcher.OnLevelCreated -= TrackQuests;
            TrackedQuests.Clear();
        }
    }
}
