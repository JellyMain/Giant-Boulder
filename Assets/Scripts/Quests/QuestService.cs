using System;
using System.Collections.Generic;
using System.Linq;
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

            List<QuestDataBase> allQuestsCopy = new List<QuestDataBase>(allQuests);

            for (int i = allQuestsCopy.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (allQuestsCopy[i], allQuestsCopy[j]) = (allQuestsCopy[j], allQuestsCopy[i]);
            }
            
            SelectedQuests = allQuestsCopy.Take(3).ToList();
        }
    }
}
