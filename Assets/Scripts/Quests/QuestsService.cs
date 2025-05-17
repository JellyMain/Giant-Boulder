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
    public class QuestsService : IInitializable, IProgressSaver
    {
        private readonly StaticDataService staticDataService;
        private readonly SaveLoadService saveLoadService;
        private readonly PersistentPlayerProgress persistentPlayerProgress;
        private Dictionary<QuestType, List<QuestData>> allQuests;

        public Dictionary<QuestData, QuestProgressUpdater> ActiveQuestsProgressUpdaters { get; private set; } =
            new Dictionary<QuestData, QuestProgressUpdater>();

        public Dictionary<QuestType, List<QuestData>> SortedActiveQuests { get; private set; } =
            new Dictionary<QuestType, List<QuestData>>();

        public Dictionary<QuestData, QuestProgress> AllQuestsProgresses { get; private set; } =
            new Dictionary<QuestData, QuestProgress>();


        //TODO: Get all quests progress data from this service and not from PlayerProgress


        public QuestsService(StaticDataService staticDataService, SaveLoadService saveLoadService,
            PersistentPlayerProgress persistentPlayerProgress)
        {
            this.staticDataService = staticDataService;
            this.saveLoadService = saveLoadService;
            this.persistentPlayerProgress = persistentPlayerProgress;
        }


        public void Initialize()
        {
            saveLoadService.RegisterGlobalObject(this);
        }


        public void GetQuestsProgresses()
        {
            QuestsIdProgressDictionary questsProgressDictionary =
                persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary;

            allQuests = staticDataService.QuestsConfig.quests;

            foreach (List<QuestData> quests in allQuests.Values)
            {
                foreach (QuestData questData in quests)
                {
                    string questId = questData.questId;

                    QuestProgress questProgress = new QuestProgress()
                    {
                        questState = QuestState.Inactive
                    };

                    if (questsProgressDictionary.TryGetValue(questId, out QuestProgress progress))
                    {
                        questProgress = progress;
                    }

                    AllQuestsProgresses[questData] = questProgress;
                }
            }

            List<QuestData> savedQuests = GetSavedQuests();
            
            SetSavedQuestsOrCreateNew(savedQuests);
        }


        private void SetSavedQuestsOrCreateNew(List<QuestData> savedQuests)
        {
            if (savedQuests.Count != 0)
            {
                SetSavedQuests(savedQuests);
            }
            else
            {
                SetNewQuests();
            }
        }


        private void SetSavedQuests(List<QuestData> savedQuests)
        {
            foreach (QuestData questData in savedQuests)
            {
                QuestProgressUpdater questProgressUpdater = CreateQuestProgressUpdaterByQuest(questData);

                ActiveQuestsProgressUpdaters[questData] = questProgressUpdater;
            }

            SortActiveQuests();
        }


        private void SetNewQuests()
        {
            foreach (QuestType questType in allQuests.Keys)
            {
                for (int i = 0; i < 3; i++)
                {
                    QuestData questData = TakeNewQuest(questType);
                    QuestProgressUpdater questProgressUpdater = CreateQuestProgressUpdaterByQuest(questData);

                    ActiveQuestsProgressUpdaters[questData] = questProgressUpdater;
                    AllQuestsProgresses[questData].questState = QuestState.InProgress;
                }
            }

            SortActiveQuests();
        }



        private List<QuestData> GetSavedQuests()
        {
            List<QuestData> questsInProgress = new List<QuestData>(9);

            foreach (KeyValuePair<QuestData, QuestProgress> questProgressPair in AllQuestsProgresses)
            {
                if (questProgressPair.Value.questState == QuestState.InProgress)
                {
                    questsInProgress.Add(questProgressPair.Key);
                }
            }
            
            return questsInProgress;
        }


        public QuestData ReplaceQuest(QuestData oldQuestData)
        {
            QuestType questType = oldQuestData.questType;

            QuestData newQuest = TakeNewQuest(questType);
            ActiveQuestsProgressUpdaters.Remove(oldQuestData);
            SortedActiveQuests[questType].Remove(oldQuestData);

            QuestProgressUpdater newQuestProgressUpdater = CreateQuestProgressUpdaterByQuest(newQuest);
            ActiveQuestsProgressUpdaters[newQuest] = newQuestProgressUpdater;
            SortedActiveQuests[questType].Add(newQuest);

            return newQuest;
        }


        private QuestData TakeNewQuest(QuestType questType)
        {
            foreach (QuestData questData in allQuests[questType])
            {
                QuestProgress questProgress = AllQuestsProgresses[questData];

                if (questProgress.questState == QuestState.Inactive)
                {
                    questProgress.questState = QuestState.InProgress;
                    AllQuestsProgresses[questData] = questProgress;

                    return questData;
                }
            }

            Debug.LogError("No free quests were found");
            return null;
        }


        private void SortActiveQuests()
        {
            foreach (QuestData questData in ActiveQuestsProgressUpdaters.Keys)
            {
                if (!SortedActiveQuests.ContainsKey(questData.questType))
                {
                    SortedActiveQuests[questData.questType] = new List<QuestData>();
                }

                SortedActiveQuests[questData.questType].Add(questData);
            }
        }


        private QuestProgressUpdater CreateQuestProgressUpdaterByQuest(QuestData questData)
        {
            switch (questData)
            {
                case CollectCoinsQuestData collectCoinsQuestData:
                {
                    return new CollectCoinsQuestUpdater(collectCoinsQuestData, this);
                }
                case DestroyObjectsQuestData destroyObjectsQuestData:
                {
                    return new DestroyObjectsQuestUpdater(destroyObjectsQuestData, this);
                }
                default:
                {
                    return null;
                }
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary questsProgressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            foreach (KeyValuePair<QuestData, QuestProgress> questProgressPair in AllQuestsProgresses)
            {
                string questId = questProgressPair.Key.questId;

                questsProgressDictionary[questId] = questProgressPair.Value;
            }
        }
    }
}
