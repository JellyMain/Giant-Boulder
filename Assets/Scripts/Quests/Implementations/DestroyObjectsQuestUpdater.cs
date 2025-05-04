using System.Collections.Generic;
using DataTrackers;
using Progress;
using Quests.Enums;
using Structures;
using UnityEngine;


namespace Quests.Implementations
{
    public class DestroyObjectsQuestUpdater : QuestProgressUpdater, IProgressSaver, IProgressUpdater
    {
        private readonly DestroyedObjectsTracker destroyedObjectsTracker;
        private readonly DestroyObjectsQuestData destroyObjectsQuestData;
        private int objectsCount;


        public DestroyObjectsQuestUpdater(DestroyObjectsQuestData destroyObjectsQuestData, SaveLoadService saveLoadService,
            DestroyedObjectsTracker destroyedObjectsTracker) : base(saveLoadService)
        {
            this.destroyObjectsQuestData = destroyObjectsQuestData;
            this.destroyedObjectsTracker = destroyedObjectsTracker;
        }


        public override void Init()
        {
            base.Init();
            destroyedObjectsTracker.OnDestroyedObjectAdded += OnDestroyedObjectsAdded;
        }


        private void OnDestroyedObjectsAdded(ObjectType objectType)
        {
            if (objectType == destroyObjectsQuestData.targetObjectType)
            {
                objectsCount++;
                UpdateQuest();
            }
        }


        public override void UpdateQuest()
        {
            if (objectsCount == destroyObjectsQuestData.targetObjectAmount)
            {
                isCompleted = true;
                QuestCompleted();
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            switch (destroyObjectsQuestData.questPersistenceProgressType)
            {
                case QuestPersistenceProgressType.MultipleSessions:
                {
                    SaveMultipleSessionProgress(progressDictionary);
                    break;
                }
                case QuestPersistenceProgressType.OneSession:
                {
                    SaveSingleSessionProgress(progressDictionary);
                    break;
                }
            }
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            switch (destroyObjectsQuestData.questPersistenceProgressType)
            {
                case QuestPersistenceProgressType.MultipleSessions:
                {
                    UpdateMultipleSessionProgress(progressDictionary);
                    break;
                }
                case QuestPersistenceProgressType.OneSession:
                {
                    UpdateSingleSessionProgress();
                    break;
                }
            }
        }



        private void SaveMultipleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (progressDictionary.TryGetValue(destroyObjectsQuestData.uniqueId, out QuestProgress questProgress))
            {
                questProgress.destroyedAmount += objectsCount;
            }
            else
            {
                progressDictionary[destroyObjectsQuestData.uniqueId] =
                    new QuestProgress
                    {
                        questState = QuestState.InProgress,
                        destroyedAmount = objectsCount
                    };
            }
        }


        private void SaveSingleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (isCompleted)
            {
                progressDictionary[destroyObjectsQuestData.uniqueId] = new QuestProgress
                {
                    questState = QuestState.Completed,
                    destroyedAmount = destroyObjectsQuestData.targetObjectAmount
                };
            }
            else
            {
                if (progressDictionary.TryGetValue(destroyObjectsQuestData.uniqueId, out QuestProgress questProgress))
                {
                    questProgress.destroyedAmount = objectsCount;
                }
                else
                {
                    progressDictionary[destroyObjectsQuestData.uniqueId] = new QuestProgress
                    {
                        questState = QuestState.InProgress,
                        destroyedAmount = objectsCount
                    };
                }
            }
        }



        private void UpdateMultipleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            objectsCount = 0;
        }


        private void UpdateSingleSessionProgress()
        {
            objectsCount = 0;
        }
    }
}
