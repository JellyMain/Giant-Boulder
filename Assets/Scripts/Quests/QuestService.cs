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
        public List<QuestDataBase> SelectedQuests { get; private set; } = new List<QuestDataBase>();


        public QuestService(StaticDataService staticDataService)
        {
            this.staticDataService = staticDataService;
        }


        public void SetRandomQuests()
        {
            List<QuestDataBase> allQuests = staticDataService.QuestsConfig.quests;

            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, allQuests.Count);
                QuestDataBase randomQuestDataBase = allQuests[randomIndex];
                SelectedQuests.Add(randomQuestDataBase);
            }
        }
        
    }
}
