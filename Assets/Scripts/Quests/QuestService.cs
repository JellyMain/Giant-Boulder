using System;
using System.Collections.Generic;
using Factories;
using StaticData.Services;
using Structures;
using Zenject;
using Random = UnityEngine.Random;


namespace Quests
{
    public class QuestService
    {
        private readonly StaticDataService staticDataService;
        public List<QuestData> CurrentQuests { get; private set; } = new List<QuestData>();
        public QuestData CurrentQuestData { get; private set; }


        public QuestService(StaticDataService staticDataService)
        {
            this.staticDataService = staticDataService;
        }


        public void SetRandomQuests()
        {
            List<QuestData> allQuests = staticDataService.QuestsConfig.quests;

            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, allQuests.Count);
                QuestData randomQuestData = allQuests[randomIndex];
                CurrentQuests.Add(randomQuestData);
            }
        }


        public void SetCurrentQuest(QuestData questData)
        {
            CurrentQuestData = questData;
        }
    }
}
