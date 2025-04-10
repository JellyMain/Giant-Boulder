using System.Collections.Generic;
using DataTrackers;
using Progress;
using Quests.Enums;
using Structures;


namespace Quests.Implementations
{
    public class DestroyObjectsQuest : QuestProgressUpdater, IProgressSaver, IProgressUpdater
    {
        private readonly Quest quest;
        private readonly DestroyedObjectsTracker destroyedObjectsTracker;
        private int objectsCount;


        public DestroyObjectsQuest(Quest quest, SaveLoadService saveLoadService,
            DestroyedObjectsTracker destroyedObjectsTracker) : base(quest, saveLoadService)
        {
            this.quest = quest;
            this.destroyedObjectsTracker = destroyedObjectsTracker;
        }


        public override void Init()
        {
            destroyedObjectsTracker.OnDestroyedObjectAdded += OnDestroyedObjectsAdded;
        }


        private void OnDestroyedObjectsAdded(ObjectType objectType)
        {
            if (objectType == quest.targetObjectType)
            {
                objectsCount++;
                UpdateProgress();
            }
        }


        public override void UpdateProgress()
        {
            if (objectsCount == quest.targetObjectAmount)
            {
                isCompleted = true;
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            if (quest.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
            {
                SaveMultipleSessionProgress(progressDictionary);
            }
            else
            {
                SaveSingleSessionProgress(progressDictionary);
            }
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            if (quest.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
            {
                UpdateMultipleSessionProgress(progressDictionary);
            }
            else
            {
                UpdateSingleSessionProgress();
            }
        }



        private void SaveMultipleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (progressDictionary.TryGetValue(quest.uniqueId, out QuestProgress questProgress))
            {
                questProgress.collectedCoins += objectsCount;
            }
            else
            {
                progressDictionary[quest.uniqueId] =
                    new QuestProgress
                    {
                        questState = QuestState.InProgress,
                        objectAmountDestroyed = objectsCount
                    };
            }
        }


        private void SaveSingleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (isCompleted)
            {
                progressDictionary[quest.uniqueId] = new QuestProgress
                {
                    questState = QuestState.Completed,
                    objectAmountDestroyed = quest.targetObjectAmount
                };
            }
            else
            {
                if (progressDictionary.TryGetValue(quest.uniqueId, out QuestProgress questProgress))
                {
                    questProgress.objectAmountDestroyed = objectsCount;
                }
                else
                {
                    progressDictionary[quest.uniqueId] = new QuestProgress
                    {
                        questState = QuestState.InProgress,
                        objectAmountDestroyed = objectsCount
                    };
                }
            }
        }



        private void UpdateMultipleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (progressDictionary.TryGetValue(quest.uniqueId, out QuestProgress questProgress))
            {
                objectsCount = questProgress.objectAmountDestroyed;
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
