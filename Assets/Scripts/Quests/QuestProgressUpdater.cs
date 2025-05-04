using System;
using Progress;
using UnityEngine;


namespace Quests
{
    public abstract class QuestProgressUpdater
    {
        private readonly SaveLoadService saveLoadService;
        protected bool isCompleted;
        public event Action<QuestProgressUpdater> OnQuestCompleted;


        protected QuestProgressUpdater(SaveLoadService saveLoadService)
        {
            this.saveLoadService = saveLoadService;
        }


        public virtual void Init()
        {
            saveLoadService.RegisterSceneObject(this);
        }


        protected void QuestCompleted()
        {
            OnQuestCompleted?.Invoke(this);
        }


        public abstract void UpdateQuest();
    }
}
