using System;
using System.Collections.Generic;
using System.Linq;
using Factories;
using Progress;
using Quests.Enums;
using Quests.Implementations;
using StaticData.Services;
using Structures;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


namespace Quests
{
    public class QuestsService
    {
        private readonly StaticDataService staticDataService;
        private readonly SaveLoadService saveLoadService;
        private readonly PersistentPlayerProgress persistentPlayerProgress;
        private List<QuestData> allQuests;

        public Dictionary<QuestData, QuestProgressUpdater> SelectedQuests { get; private set; } =
            new Dictionary<QuestData, QuestProgressUpdater>();



        public QuestsService(StaticDataService staticDataService, SaveLoadService saveLoadService,
            PersistentPlayerProgress persistentPlayerProgress)
        {
            this.staticDataService = staticDataService;
            this.saveLoadService = saveLoadService;
            this.persistentPlayerProgress = persistentPlayerProgress;
        }


        public void SetSavedQuests()
        {
            List<QuestData> savedQuests = TakeSavedQuests();

            foreach (QuestData questData in savedQuests)
            {
                QuestProgressUpdater questProgressUpdater = CreateQuestProgressUpdaterByQuest(questData);

                SelectedQuests[questData] = questProgressUpdater;
            }
        }

        
        public void SetNewQuests()
        {
            allQuests = staticDataService.QuestsConfig.quests;

            for (int i = 0; i < 3; i++)
            {
                QuestData questData = TakeNewQuest();
                QuestProgressUpdater questProgressUpdater = CreateQuestProgressUpdaterByQuest(questData);

                SelectedQuests[questData] = questProgressUpdater;
            }
        }


        
        private List<QuestData> TakeSavedQuests()
        {
            QuestsIdProgressDictionary questsProgressDictionary =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary;

            allQuests = staticDataService.QuestsConfig.quests;

            List<QuestData> questsInProgress = new List<QuestData>(3);

            foreach (QuestData questData in allQuests)
            {
                int questId = questData.uniqueId;
                QuestProgress questProgress = questsProgressDictionary.GetValueOrDefault(questId);

                if (questProgress?.questState == QuestState.InProgress)
                {
                    questsInProgress.Add(questData);

                    if (questsInProgress.Count == 3)
                    {
                        return questsInProgress;
                    }
                }
            }

            Debug.LogError($"There was found only {questsInProgress.Count} quests in progress");
            return null;
        }
        
        
        public QuestData ReplaceQuest(QuestData questToReplace)
        {
            QuestData newQuest = TakeNewQuest();
            SelectedQuests.Remove(questToReplace);

            QuestProgressUpdater newQuestProgressUpdater = CreateQuestProgressUpdaterByQuest(newQuest);
            SelectedQuests[newQuest] = newQuestProgressUpdater;

            return newQuest;
        }


        private QuestData TakeNewQuest()
        {
            QuestsIdProgressDictionary questsProgressDictionary =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary;

            foreach (QuestData questData in allQuests)
            {
                int questId = questData.uniqueId;
                QuestProgress questProgress = questsProgressDictionary.GetValueOrDefault(questId);

                if (questProgress == null)
                {
                    questProgress = new QuestProgress()
                    {
                        questState = QuestState.InProgress
                    };
                    questsProgressDictionary[questId] = questProgress;

                    return questData;
                }
            }

            Debug.LogError("No free quests were found");
            return null;
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
