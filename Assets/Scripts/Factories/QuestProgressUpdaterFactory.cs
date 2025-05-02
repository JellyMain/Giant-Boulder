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


        public QuestProgressUpdater CreateQuestProgressUpdaterByQuest(QuestData questData)
        {
            switch (questData.questType)
            {
                case QuestType.CollectCoins:
                    return new CollectCoinsQuestUpdater(questData, saveLoadService, gameCurrencyTracker);
                case QuestType.DestroyObjects:
                    return new DestroyObjectsQuest(questData, saveLoadService, destroyedObjectsTracker);
                default:
                    return null;
            }
        }
    }
}
