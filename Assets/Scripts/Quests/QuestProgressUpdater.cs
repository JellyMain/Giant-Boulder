using Progress;


namespace Quests
{
    public abstract class QuestProgressUpdater
    {
        protected readonly Quest quest;
        private readonly SaveLoadService saveLoadService;
        protected bool isCompleted;
        

        protected QuestProgressUpdater(Quest quest, SaveLoadService saveLoadService)
        {
            this.quest = quest;
            this.saveLoadService = saveLoadService;
        }


        public virtual void Init()
        {
            saveLoadService.RegisterSceneObject(this);
        }

        public abstract void UpdateProgress();
        
    }
}
