using System.Collections.Generic;
using DataTrackers;
using Progress;
using Quests.Enums;
using Structures;
using UnityEngine;


namespace Quests.Implementations
{
    public class DestroyObjectsQuest : QuestProgressUpdater, IProgressSaver, IProgressUpdater
    {
        private readonly DestroyObjectsQuestData destroyObjectsQuestData;
        private readonly DestroyedObjectsTracker destroyedObjectsTracker;
        private int objectsCount;


        public DestroyObjectsQuest(DestroyObjectsQuestData destroyObjectsQuestData, SaveLoadService saveLoadService,
            DestroyedObjectsTracker destroyedObjectsTracker) : base(saveLoadService)
        {
            this.destroyObjectsQuestData = destroyObjectsQuestData;
            this.destroyedObjectsTracker = destroyedObjectsTracker;
        }


        public override void Init()
        {
            destroyedObjectsTracker.OnDestroyedObjectAdded += OnDestroyedObjectsAdded;
        }


        private void OnDestroyedObjectsAdded(ObjectType objectType)
        {
            if (objectType == destroyObjectsQuestData.targetObjectType)
            {
                objectsCount++;
                UpdateProgress();
            }
        }


        public override void UpdateProgress()
        {
            if (objectsCount == destroyObjectsQuestData.targetObjectAmount)
            {
                isCompleted = true;
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            QuestsIdProgressDictionary progressDictionary = playerProgress.questsData.questsIdProgressDictionary;

            if (destroyObjectsQuestData.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
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

            if (destroyObjectsQuestData.questPersistenceProgressType == QuestPersistenceProgressType.MultipleSessions)
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
            if (progressDictionary.TryGetValue(destroyObjectsQuestData.uniqueId, out QuestProgress questProgress))
            {
                questProgress.destroyedAmount += objectsCount;
                Debug.Log($"Saved {questProgress.destroyedAmount} of {destroyObjectsQuestData.targetObjectType}");
            }
            else
            {
                progressDictionary[destroyObjectsQuestData.uniqueId] =
                    new QuestProgress
                    {
                        questState = QuestState.InProgress,
                        destroyedAmount = objectsCount
                    };

                Debug.Log(
                    $"Saved {progressDictionary[destroyObjectsQuestData.uniqueId].destroyedAmount} of {destroyObjectsQuestData.targetObjectType}");
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
