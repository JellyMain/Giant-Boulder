using System;
using System.Collections.Generic;
using System.Linq;
using Factories;
using Progress;
using Quests.Implementations;
using StaticData.Services;
using Structures;
using Zenject;
using Random = UnityEngine.Random;


namespace Quests
{
    public class QuestsService
    {
        private readonly StaticDataService staticDataService;
        private readonly SaveLoadService saveLoadService;

        public Dictionary<QuestData, QuestProgressUpdater> SelectedQuests { get; private set; }



        public QuestsService(StaticDataService staticDataService, SaveLoadService saveLoadService)
        {
            this.staticDataService = staticDataService;
            this.saveLoadService = saveLoadService;
        }



        public void SetRandomQuests()
        {
            SelectedQuests = new Dictionary<QuestData, QuestProgressUpdater>();

            List<QuestData> allQuests = staticDataService.QuestsConfig.quests;

            List<QuestData> allQuestsCopy = new List<QuestData>(allQuests);

            for (int i = allQuestsCopy.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (allQuestsCopy[i], allQuestsCopy[j]) = (allQuestsCopy[j], allQuestsCopy[i]);
            }

            for (int i = 0; i < 3; i++)
            {
                QuestData questData = allQuestsCopy[i];
                QuestProgressUpdater questProgressUpdater = CreateQuestProgressUpdaterByQuest(questData);

                SelectedQuests[questData] = questProgressUpdater;
            }
        }



        private QuestProgressUpdater CreateQuestProgressUpdaterByQuest(QuestData questData)
        {
            switch (questData)
            {
                case CollectCoinsQuestData collectCoinsQuestData:
                {
                    return new CollectCoinsQuestUpdater(collectCoinsQuestData, saveLoadService);
                }
                case DestroyObjectsQuestData destroyObjectsQuestData:
                {
                    return new DestroyObjectsQuestUpdater(destroyObjectsQuestData, saveLoadService);
                }
                default:
                {
                    return null;
                }
            }
        }
    }
}
