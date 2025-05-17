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
        private int destroyedObjects;


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
                destroyedObjects++;
                UpdateQuest();
            }
        }


        public override void UpdateQuest()
        {
            if (destroyedObjects == destroyObjectsQuestData.targetObjectAmount)
            {
                isCompleted = true;
                QuestCompleted(destroyObjectsQuestData);
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            string questId = destroyObjectsQuestData.questId;

            QuestProgress updatedProgress = new QuestProgress()
            {
                destroyedObjects = destroyedObjects,
                questState = isCompleted ? QuestState.JustCompleted : QuestState.InProgress
            };

            progressDictionary[questId] = updatedProgress;
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            if (destroyObjectsQuestData.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
            {
                UpdateMultipleSessionProgress(progressDictionary);
            }
            else if (destroyObjectsQuestData.questPersistenceProgressType == QuestPersistenceProgressType.OneSession)
            {
                UpdateSingleSessionProgress();
            }
            else
            {
                Debug.LogError("Quest persistent progress type is None");
            }
        }


        private void UpdateMultipleSessionProgress(Dictionary<string, QuestProgress> progressDictionary)
        {
            string questId = destroyObjectsQuestData.questId;

            QuestProgress existingProgress = progressDictionary.GetValueOrDefault(questId);

            destroyedObjects = existingProgress?.destroyedObjects ?? 0;
        }


        private void UpdateSingleSessionProgress()
        {
            destroyedObjects = 0;
        }
    }
}
