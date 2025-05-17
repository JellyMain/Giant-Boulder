using System;
using Progress;
using UnityEngine;


namespace Quests
{
    public abstract class QuestProgressUpdater
    {
        public event Action<QuestData> OnQuestCompleted;
        
        
        protected void QuestCompleted(QuestData questData)
        {
            OnQuestCompleted?.Invoke(questData);
        }


        public abstract void StartTracking(QuestDependencies questDependencies);
        

        
        public abstract void UpdateQuest();
    }
}