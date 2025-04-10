using System;
using Factories;
using StaticData.Services;
using Structures;
using Zenject;


namespace Quests
{
    public class QuestService 
    {
        public Quest CurrentQuest { get; private set; }
        public event Action<ObjectType> OnObjectDestroyed;
        public event Action OnCoinCollected;
        
        
        public void SetCurrentQuest(Quest quest)
        {
            CurrentQuest = quest;
        }
    }
}
