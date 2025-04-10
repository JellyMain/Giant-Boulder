using Factories;
using Quests;
using Zenject;


namespace DataTrackers
{
    public class GameplayQuestTracker
    {
        private readonly QuestService questService;
        private readonly QuestProgressUpdaterFactory questProgressUpdaterFactory;
        private QuestProgressUpdater currentQuestProgressUpdater;
        
        
        public GameplayQuestTracker(QuestService questService, QuestProgressUpdaterFactory questProgressUpdaterFactory)
        {
            this.questService = questService;
            this.questProgressUpdaterFactory = questProgressUpdaterFactory;
        }
        


        public void TrackCurrentQuest()
        {
            currentQuestProgressUpdater = questProgressUpdaterFactory.CreateQuestProgressUpdaterByQuest(questService.CurrentQuest);
            currentQuestProgressUpdater.Init();
        }
    }
}
