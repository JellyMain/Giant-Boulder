using Factories;
using Quests;
using UnityEngine;
using Zenject;


namespace DataTrackers
{
    public class GameplayQuestTracker
    {
        private readonly QuestService questService;
        private readonly QuestProgressUpdaterFactory questProgressUpdaterFactory;


        public GameplayQuestTracker(QuestService questService, QuestProgressUpdaterFactory questProgressUpdaterFactory)
        {
            this.questService = questService;
            this.questProgressUpdaterFactory = questProgressUpdaterFactory;
        }



        public void TrackQuests()
        {
            foreach (QuestDataBase quest in questService.SelectedQuests)
            {
                QuestProgressUpdater currentQuestProgressUpdater =
                    questProgressUpdaterFactory.CreateQuestProgressUpdaterByQuest(quest);
                currentQuestProgressUpdater.Init();
                Debug.Log("Inited");
            }
        }
    }
}
