using System;
using Progress;
using UnityEngine;


namespace Quests
{
    public abstract class QuestProgressUpdater
    {
        private readonly SaveLoadService saveLoadService;
        protected bool isCompleted;
        public event Action<QuestData> OnQuestCompleted;


        protected QuestProgressUpdater(SaveLoadService saveLoadService)
        {
            this.saveLoadService = saveLoadService;
        }
        

        protected void QuestCompleted(QuestData questData)
        {
            OnQuestCompleted?.Invoke(questData);
        }


        public virtual void StartTracking(QuestDependencies questDependencies)
        {
            saveLoadService.RegisterSceneObject(this);
        }

        
        public abstract void UpdateQuest();
    }
}