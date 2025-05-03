using DataTrackers;
using Progress;
using Quests;
using Quests.Enums;
using Quests.Implementations;


namespace Factories
{
    public class QuestProgressUpdaterFactory
    {
        private readonly GameCurrencyTracker gameCurrencyTracker;
        private readonly SaveLoadService saveLoadService;
        private readonly DestroyedObjectsTracker destroyedObjectsTracker;


        public QuestProgressUpdaterFactory(GameCurrencyTracker gameCurrencyTracker, SaveLoadService saveLoadService,
            DestroyedObjectsTracker destroyedObjectsTracker)
        {
            this.gameCurrencyTracker = gameCurrencyTracker;
            this.saveLoadService = saveLoadService;
            this.destroyedObjectsTracker = destroyedObjectsTracker;
        }


        public QuestProgressUpdater CreateQuestProgressUpdaterByQuest(QuestDataBase questDataBase)
        {
            switch (questDataBase)
            {
                case CollectCoinsQuestData collectCoinsQuestData:
                {
                    return new CollectCoinsQuestUpdater(collectCoinsQuestData, saveLoadService, gameCurrencyTracker);
                }
                case DestroyObjectsQuestData destroyObjectsQuestData:
                {
                    return new DestroyObjectsQuest(destroyObjectsQuestData, saveLoadService, destroyedObjectsTracker);
                }
                default:
                {
                    return null;
                }
            }
        }
    }
}
