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
    private Dictionary<QuestType, List<QuestData>> allQuests;

    public Dictionary<QuestData, QuestProgressUpdater> ActiveQuestsProgressUpdaters { get; private set; } =
        new Dictionary<QuestData, QuestProgressUpdater>();

    public Dictionary<QuestType, List<QuestData>> SortedActiveQuests { get; private set; } =
        new Dictionary<QuestType, List<QuestData>>();

    public Dictionary<QuestData, QuestProgress> ActiveQuestsProgresses { get; private set; } =
        new Dictionary<QuestData, QuestProgress>();



    public QuestsService(StaticDataService staticDataService, SaveLoadService saveLoadService,
        PersistentPlayerProgress persistentPlayerProgress)
    {
        this.staticDataService = staticDataService;
        this.saveLoadService = saveLoadService;
        this.persistentPlayerProgress = persistentPlayerProgress;
    }


    public void SetSavedQuests()
    {
        List<QuestData> savedQuests = GetSavedQuests();

        foreach (QuestData questData in savedQuests)
        {
            QuestProgressUpdater questProgressUpdater = CreateQuestProgressUpdaterByQuest(questData);

            ActiveQuestsProgressUpdaters[questData] = questProgressUpdater;
        }

        SortActiveQuests();
    }


    public void SetNewQuests()
    {
        allQuests = staticDataService.QuestsConfig.quests;

        foreach (QuestType questType in allQuests.Keys)
        {
            for (int i = 0; i < 3; i++)
            {
                QuestData questData = TakeNewQuest(questType);
                QuestProgressUpdater questProgressUpdater = CreateQuestProgressUpdaterByQuest(questData);

                ActiveQuestsProgressUpdaters[questData] = questProgressUpdater;
            }
        }

        SortActiveQuests();
    }



    private List<QuestData> GetSavedQuests()
    {
        QuestsIdProgressDictionary questsProgressDictionary =
            persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary;

        allQuests = staticDataService.QuestsConfig.quests;

        List<QuestData> questsInProgress = new List<QuestData>(9);

        foreach (List<QuestData> quests in allQuests.Values)
        {
            foreach (QuestData questData in quests)
            {
                string questId = questData.questId;
                QuestProgress questProgress = questsProgressDictionary.GetValueOrDefault(questId);

                if (questProgress?.questState == QuestState.InProgress)
                {
                    questsInProgress.Add(questData);
                }
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
        QuestsIdProgressDictionary questsProgressDictionary =
            persistentPlayerProgress.PlayerProgress.questsData.questsIdProgressDictionary;

        foreach (QuestData questData in allQuests[questType])
        {
            string questId = questData.questId;
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
