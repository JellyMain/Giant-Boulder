using Progress;
using UnityEngine;


namespace Quests
{
    public abstract class QuestProgressUpdater
    {
        private readonly SaveLoadService saveLoadService;
        protected bool isCompleted;


        protected QuestProgressUpdater(SaveLoadService saveLoadService)
        {
            this.saveLoadService = saveLoadService;
        }


        public virtual void Init()
        {
            saveLoadService.RegisterSceneObject(this);
        }


        public abstract void UpdateProgress();
    }
}
