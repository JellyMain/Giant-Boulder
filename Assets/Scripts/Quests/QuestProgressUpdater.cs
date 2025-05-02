using Progress;


namespace Quests
{
    public abstract class QuestProgressUpdater
    {
        protected readonly QuestData questData;
        private readonly SaveLoadService saveLoadService;
        protected bool isCompleted;
        

        protected QuestProgressUpdater(QuestData questData, SaveLoadService saveLoadService)
        {
            this.questData = questData;
            this.saveLoadService = saveLoadService;
        }


        public virtual void Init()
        {
            saveLoadService.RegisterSceneObject(this);
        }

        public abstract void UpdateProgress();
        
    }
}
