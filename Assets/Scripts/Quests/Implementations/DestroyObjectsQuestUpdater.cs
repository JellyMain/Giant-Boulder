using System;
using DataTrackers;
using Progress;
using Quests.Enums;
using Structures;
using UnityEngine;


namespace Quests.Implementations
{
    public class DestroyObjectsQuestUpdater : QuestProgressUpdater, IDisposable
    {
        private readonly DestroyObjectsQuestData destroyObjectsQuestData;
        private readonly QuestsService questsService;
        private DestroyedObjectsTracker destroyedObjectsTracker;
        private int destroyedObjects;


        public DestroyObjectsQuestUpdater(DestroyObjectsQuestData destroyObjectsQuestData, QuestsService questsService)
        {
            this.destroyObjectsQuestData = destroyObjectsQuestData;
            this.questsService = questsService;
        }


        public override void StartTracking(QuestDependencies questDependencies)
        {
            destroyedObjectsTracker = questDependencies.destroyedObjectsTracker;
            destroyedObjectsTracker.OnDestroyedObjectAdded += OnDestroyedObjectsAdded;

            UpdateProgress();
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
            questsService.AllQuestsProgresses[destroyObjectsQuestData].destroyedObjects = destroyedObjects;

            if (destroyedObjects == destroyObjectsQuestData.targetObjectAmount)
            {
                questsService.AllQuestsProgresses[destroyObjectsQuestData].questState = QuestState.JustCompleted;
                QuestCompleted(destroyObjectsQuestData);
            }
        }



        private void UpdateProgress()
        {
            if (destroyObjectsQuestData.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
            {
                UpdateMultipleSessionProgress();
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


        private void UpdateMultipleSessionProgress()
        {
            QuestProgress progress = questsService.AllQuestsProgresses[destroyObjectsQuestData];
            destroyedObjects = progress.destroyedObjects;
        }


        private void UpdateSingleSessionProgress()
        {
            destroyedObjects = 0;
        }


        public void Dispose()
        {
            destroyedObjectsTracker.OnDestroyedObjectAdded -= OnDestroyedObjectsAdded;
        }
    }
}
