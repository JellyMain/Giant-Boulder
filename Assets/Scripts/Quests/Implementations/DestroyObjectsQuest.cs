using System.Collections.Generic;
using DataTrackers;
using Progress;
using Quests.Enums;
using Structures;


namespace Quests.Implementations
{
    public class DestroyObjectsQuest : QuestProgressUpdater, IProgressSaver, IProgressUpdater
    {
        private readonly DestroyedObjectsTracker destroyedObjectsTracker;
        private int objectsCount;


        public DestroyObjectsQuest(QuestData questData, SaveLoadService saveLoadService,
            DestroyedObjectsTracker destroyedObjectsTracker) : base(questData, saveLoadService)
        {
            this.destroyedObjectsTracker = destroyedObjectsTracker;
        }


        public override void Init()
        {
            destroyedObjectsTracker.OnDestroyedObjectAdded += OnDestroyedObjectsAdded;
        }


        private void OnDestroyedObjectsAdded(ObjectType objectType)
        {
            if (objectType == questData.targetObjectType)
            {
                objectsCount++;
                UpdateProgress();
            }
        }


        public override void UpdateProgress()
        {
            if (objectsCount == questData.targetObjectAmount)
            {
                isCompleted = true;
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            if (questData.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
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

            if (questData.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
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
            if (progressDictionary.TryGetValue(questData.uniqueId, out QuestProgress questProgress))
            {
                questProgress.collectedCoins += objectsCount;
            }
            else
            {
                progressDictionary[questData.uniqueId] =
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
                progressDictionary[questData.uniqueId] = new QuestProgress
                {
                    questState = QuestState.Completed,
                    destroyedAmount = questData.targetObjectAmount
                };
            }
            else
            {
                if (progressDictionary.TryGetValue(questData.uniqueId, out QuestProgress questProgress))
                {
                    questProgress.destroyedAmount = objectsCount;
                }
                else
                {
                    progressDictionary[questData.uniqueId] = new QuestProgress
                    {
                        questState = QuestState.InProgress,
                        destroyedAmount = objectsCount
                    };
                }
            }
        }



        private void UpdateMultipleSessionProgress(Dictionary<int, QuestProgress> progressDictionary)
        {
            if (progressDictionary.TryGetValue(questData.uniqueId, out QuestProgress questProgress))
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
