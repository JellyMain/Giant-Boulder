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
        private readonly DestroyObjectsQuestData destroyObjectsQuestData;
        private DestroyedObjectsTracker destroyedObjectsTracker;
        private int objectsCount;


        public DestroyObjectsQuestUpdater(DestroyObjectsQuestData destroyObjectsQuestData,
            SaveLoadService saveLoadService) : base(saveLoadService)
        {
            this.destroyObjectsQuestData = destroyObjectsQuestData;
        }


        public override void StartTracking(QuestDependencies questDependencies)
        {
            base.StartTracking(questDependencies);
            destroyedObjectsTracker = questDependencies.destroyedObjectsTracker;
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
                QuestCompleted(destroyObjectsQuestData);
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
                questProgress.destroyedAmount = objectsCount;
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
            if (progressDictionary.TryGetValue(destroyObjectsQuestData.uniqueId, out QuestProgress questProgress))
            {
                objectsCount = questProgress.destroyedAmount;
            }
            else
            {
                objectsCount = 0;
            }
        }


        private void UpdateSingleSessionProgress()
        {
            objectsCount = 0;
        }
    }
}
